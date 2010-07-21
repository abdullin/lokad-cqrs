using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Lokad.Serialization;
using System.Linq;

namespace Lokad.Cqrs.Views.Sql
{
	public enum PartitionPolicy
	{
		/// <summary>
		/// All records are stored within one table
		/// </summary>
		SingleTable,
		/// <summary>
		/// Each
		/// </summary>
		PartitionAndType,
		Partition

	}

	public interface IDbPartitionManager
	{
		string GetTable(Type type, string partition);
		string GetViewName(Type type);
		bool ImplicitPartition { get; }
		bool ImplicitView { get; }
		void Execute(Type type, string partition, Action<IDbCommand> exec);

	}

	public sealed class DbPartitionManagerForSimpleSql : IDbPartitionManager
	{
		readonly string _connection;
		readonly IsolationLevel _isolationLevel;
		readonly IDataContractMapper _mapper;
		readonly string _tableName;
		

		public DbPartitionManagerForSimpleSql(string connection, IsolationLevel isolationLevel, IDataContractMapper mapper, string tableName)
		{
			_connection = connection;
			_tableName = tableName;
			_mapper = mapper;
			_isolationLevel = isolationLevel;
		}

		static string Create(string name)
		{
			const string s = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}]') AND type in (N'U'))
BEGIN
	CREATE TABLE [{0}](
		[Partition] [nvarchar](64) NOT NULL,
		[Id] [nvarchar](64) NOT NULL,
		[View] [nvarchar](256) NOT NULL,
		[Data] [varbinary](max) NOT NULL,
	 CONSTRAINT [IX_{0}] PRIMARY KEY CLUSTERED 
	(
		[Partition] ASC,
		[View] ASC,
		[Id] ASC
	))

END";

			return string.Format(s, name);
		}
		static string CreateDropSql(string name)
		{
			const string s =
				@"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{0}]') AND type in (N'U'))
DROP TABLE [{0}]
";
			return string.Format(s, name);
		}

		public void CreateIfNotExist()
		{
			Execute(null, null, db =>
				{
					db.CommandText = Create(_tableName);
					db.ExecuteNonQuery();
				});
		}

		public void DropTables()
		{
			Execute(null, null, db =>
			{
				db.CommandText = CreateDropSql(_tableName);
				db.ExecuteNonQuery();
			});
		}

		public void Execute(Type type, string partition, Action<IDbCommand> exec)
		{
			using (var conn = new SqlConnection(_connection))
			{
				conn.Open();
				using (var tx = conn.BeginTransaction(_isolationLevel))
				{
					using (var cmd = new SqlCommand("", conn, tx))
					{
						exec(cmd);
					}
					tx.Commit();
				}
			}
		}

		public string GetTable(Type type, string partition)
		{
			return _tableName;
		}

		public string GetViewName(Type type)
		{
			return _mapper.GetContractNameByType(type).GetValue(() => type.Name);
		}

		public bool ImplicitPartition
		{
			get { return true; }
		}

		public bool ImplicitView
		{
			get { return true; }
		}
	}

	public sealed class SqlViewDialect
	{
		readonly IDataSerializer _serializer;
		readonly IDbPartitionManager _manager;

		public SqlViewDialect(IDataSerializer serializer, IDbPartitionManager manager)
		{
			_serializer = serializer;
			_manager = manager;
		}

		public static string OperandToSql(QueryViewOperand operand)
		{
			switch (operand)
			{
				case QueryViewOperand.Equal:
					return "=";
				case QueryViewOperand.GreaterThan:
					return ">";
				case QueryViewOperand.GreaterThanOrEqual:
					return ">=";
				case QueryViewOperand.LessThan:
					return "<";
				case QueryViewOperand.LessThanOrEqual:
					return "<=";
				case QueryViewOperand.NotEqual:
					return "<>";
				default:
					throw new ArgumentOutOfRangeException("operand");
			}
		}

		public void PatchRecord(IDbCommand cmd, Type type, string partition, string identity, Action<object> patch)
		{
			var query = new IndexQuery(QueryViewOperand.Equal, identity);
			var list = new List<ViewEntity>();
			ReadList(cmd, type, partition, new ViewQuery(1, query), list.Add);

			foreach (var item in list)
			{
				var entity = item;
				patch(entity.Value);
				cmd.Parameters.Clear();
				// no batch support in this scenario for now
				UpdateRecord(cmd, type, partition, entity.Identity, s => _serializer.Serialize(entity.Value, s));
			}

		}

		public void WriteRecord(IDbCommand cmd, Type type, string partition, string identity, object data)
		{
			DeleteRecord(cmd, type, partition, identity);
			cmd.Parameters.Clear();
			InsertRecord(cmd, type, partition, identity, data);
		}

		void InsertRecord(IDbCommand cmd, Type type, string partition, string identity, object data)
		{
			var paramList = new List<Pair<string, string>>();
			{
				paramList.AddTuple("[Id]", "@id");
				AddStringParam(cmd, "@id", identity);
			}
			using (var mem = new MemoryStream())
			{
				_serializer.Serialize(data, mem);
				AddByteParam(cmd, "@data", mem.ToArray());
				paramList.AddTuple("[Data]", "@data");
			}
			if (_manager.ImplicitPartition)
			{
				paramList.AddTuple("[Partition]", "@part");
				AddStringParam(cmd, "@part", partition);
			}
			if (_manager.ImplicitView)
			{
				paramList.AddTuple("[View]", "@view");
				AddStringParam(cmd, "@view", _manager.GetViewName(type));
			}

			var builder = new StringBuilder();
			var columns = paramList.Select(p => p.Key).Join(",");
			var args = paramList.Select(p => p.Value).Join(",");
			builder
				.Append("INSERT INTO " + _manager.GetTable(type, partition))
				.Append(" (" + columns + ") ")
				.Append("VALUES (" + args + ")");
			cmd.CommandText = builder.ToString();
			cmd.ExecuteNonQuery();
		}

		void UpdateRecord(IDbCommand cmd, Type type, string partition, string identity, Action<Stream> data)
		{
			var builder = new StringBuilder("UPDATE " + _manager.GetTable(type, partition));
			builder.Append(" SET Data=@data");
			using (var mem = new MemoryStream())
			{
				data(mem);
				AddByteParam(cmd, "@data", mem.ToArray());
			}
			AddFilterClause(cmd, builder, type, partition, new IndexQuery(QueryViewOperand.Equal, identity));
			cmd.CommandText = builder.ToString();
			cmd.ExecuteNonQuery();
		}





		public void DeleteRecord(IDbCommand cmd, Type type, string partition, string identity)
		{
			var builder = new StringBuilder();
			builder.Append("DELETE " + _manager.GetTable(type, partition));
			AddFilterClause(cmd, builder, type, partition, new IndexQuery(QueryViewOperand.Equal, identity));
			cmd.CommandText = builder.ToString();
			cmd.ExecuteNonQuery();
		}

		public void DeletePartition(IDbCommand cmd, Type type, string partition)
		{
			var builder = new StringBuilder();
			builder.Append("DELETE " + _manager.GetTable(type, partition));
			AddFilterClause(cmd, builder, type, partition, Maybe<IndexQuery>.Empty);
			cmd.CommandText = builder.ToString();
			cmd.ExecuteNonQuery();
		}



		void AddFilterClause(IDbCommand cmd, StringBuilder sb, Type type, string partition, Maybe<IndexQuery> index)
		{
			var clause = new List<string>();

			if (_manager.ImplicitPartition)
			{
				clause.Add("Partition=@part");
				AddStringParam(cmd, "@part", partition);
			}
			if (_manager.ImplicitView)
			{
				clause.Add("[View]=@view");
				AddStringParam(cmd, "@view", _manager.GetViewName(type));
			}

			index.Apply(iq =>
			{
				clause.Add("[Id]" + OperandToSql(iq.Operand) + " @id ");
				AddStringParam(cmd, "@id", iq.Value);
			});
			if (clause.Count > 0)
			{
				sb.AppendLine(" WHERE " + clause.Join(" AND ") + " ");
			}
		}

		static void AddStringParam(IDbCommand cmd, string name, string value)
		{
			var parameter = cmd.CreateParameter();
			parameter.ParameterName = name;
			parameter.Value = value;
			parameter.Direction = ParameterDirection.Input;
			parameter.DbType = DbType.String;
			cmd.Parameters.Add(parameter);
		}

		static void AddByteParam(IDbCommand cmd, string name, byte[] value)
		{
			var parameter = cmd.CreateParameter();
			parameter.ParameterName = name;
			parameter.Value = value;
			parameter.Direction = ParameterDirection.Input;
			cmd.Parameters.Add(parameter);
		}


		public void ReadList(IDbCommand cmd, Type type, string partition, Maybe<ViewQuery> query, Action<ViewEntity> processor)
		{
			var txt = new StringBuilder("SELECT ");

			query.Combine(q => q.RecordLimit).Apply(i => txt.Append(" TOP " + i + " "));

			txt.Append("Data, Id FROM " + _manager.GetTable(type, partition));

			AddFilterClause(cmd, txt, type, partition, query.Combine(q => q.IndexQuery));
			
			cmd.CommandText = txt.ToString();

			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var id = (string)reader[1];
					using (var memory = new MemoryStream((byte[])reader[0]))
					{
						var value = _serializer.Deserialize(memory, type);
						var e = new ViewEntity(partition, id, value);
						processor(e);
					}
				}
			}
		}
	}

	public sealed class ViewEntity
	{
		public readonly string Partition;
		public readonly string Identity;
		public readonly object Value;

		public ViewEntity(string partition, string identity, object value)
		{
			Partition = partition;
			Identity = identity;
			Value = value;
		}

		public ViewEntity<TView> Cast<TView>()
		{
			return new ViewEntity<TView>(Partition, Identity, (TView)Value);
		}
	}
	public sealed class ViewEntity<TView> : IEquatable<ViewEntity<TView>>
	{
		public readonly string Partition;
		public readonly string Identity;
		public readonly TView Value;

		public ViewEntity(string partition, string identity, TView value)
		{
			Partition = partition;
			Identity = identity;
			Value = value;
		}

		public bool Equals(ViewEntity<TView> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Partition, Partition) && Equals(other.Identity, Identity) && Equals(other.Value, Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (ViewEntity<TView>)) return false;
			return Equals((ViewEntity<TView>) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (Partition != null ? Partition.GetHashCode() : 0);
				result = (result*397) ^ (Identity != null ? Identity.GetHashCode() : 0);
				result = (result*397) ^ Value.GetHashCode();
				return result;
			}
		}

		public override string ToString()
		{
			return string.Format("[{0}/{1}]: {2}", Partition, Identity, Value);
		}
	}
}