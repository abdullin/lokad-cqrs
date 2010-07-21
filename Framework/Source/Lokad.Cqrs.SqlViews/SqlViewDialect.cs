#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Lokad.Serialization;

namespace Lokad.Cqrs.SqlViews
{
	public sealed class SqlViewDialect
	{
		readonly IDbPartitionManager _manager;
		readonly IDataSerializer _serializer;

		public SqlViewDialect(IDataSerializer serializer, IDbPartitionManager manager)
		{
			_serializer = serializer;
			_manager = manager;
		}

		static string OperandToSql(ConstraintOperand operand)
		{
			switch (operand)
			{
				case ConstraintOperand.Equal:
					return "=";
				case ConstraintOperand.GreaterThan:
					return ">";
				case ConstraintOperand.GreaterThanOrEqual:
					return ">=";
				case ConstraintOperand.LessThan:
					return "<";
				case ConstraintOperand.LessThanOrEqual:
					return "<=";
				case ConstraintOperand.NotEqual:
					return "<>";
				default:
					throw new ArgumentOutOfRangeException("operand");
			}
		}

		public void PatchRecord(IDbCommand cmd, Type type, string partition, string identity, Action<object> patch)
		{
			var query = new IdentityConstraint(ConstraintOperand.Equal, identity);
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
			AddFilterClause(cmd, builder, type, partition, new IdentityConstraint(ConstraintOperand.Equal, identity));
			cmd.CommandText = builder.ToString();
			cmd.ExecuteNonQuery();
		}


		public void DeleteRecord(IDbCommand cmd, Type type, string partition, string identity)
		{
			var builder = new StringBuilder();
			builder.Append("DELETE " + _manager.GetTable(type, partition));
			AddFilterClause(cmd, builder, type, partition, new IdentityConstraint(ConstraintOperand.Equal, identity));
			cmd.CommandText = builder.ToString();
			cmd.ExecuteNonQuery();
		}

		public void DeletePartition(IDbCommand cmd, Type type, string partition)
		{
			var builder = new StringBuilder();
			builder.Append("DELETE " + _manager.GetTable(type, partition));
			AddFilterClause(cmd, builder, type, partition, Maybe<IdentityConstraint>.Empty);
			cmd.CommandText = builder.ToString();
			cmd.ExecuteNonQuery();
		}


		void AddFilterClause(IDbCommand cmd, StringBuilder sb, Type type, string partition, Maybe<IdentityConstraint> index)
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


		public void ReadList(IDbCommand cmd, Type type, string partition, Maybe<IViewQuery> query,
			Action<ViewEntity> processor)
		{
			var txt = new StringBuilder("SELECT ");

			query.Combine(q => q.RecordLimit).Apply(i => txt.Append(" TOP " + i + " "));

			txt.Append("Data, Id FROM " + _manager.GetTable(type, partition));

			AddFilterClause(cmd, txt, type, partition, query.Combine(q => q.Constraint));

			cmd.CommandText = txt.ToString();

			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var id = (string) reader[1];
					using (var memory = new MemoryStream((byte[]) reader[0]))
					{
						var value = _serializer.Deserialize(memory, type);
						var e = new ViewEntity(partition, id, value);
						processor(e);
					}
				}
			}
		}
	}
}