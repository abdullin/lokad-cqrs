using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Lokad.Serialization;

namespace Lokad.Cqrs.Views.Sql
{
	static class SqlViewDialect
	{
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

		public const string UniformViewTableName = "[View_BigTable]";

		public static void PatchRecord(SqlCommand cmd, object partition, Maybe<object> identity, Type type, IDataSerializer ser, Action<object> patch)
		{
			var query = identity.Convert(i => new IndexQuery(QueryViewOperand.Equal, i));


			var list = new List<ViewEntity>();
			ReadList(cmd, new SqlViewQuery(1, partition, query), (s,p,i) => list.Add(new ViewEntity(p,i,ser.Deserialize(s, type))));

			foreach (var item in list)
			{
				var entity = item;
				patch(entity.Value);
				cmd.Parameters.Clear();
				// no batch support in this scenario for now
				UpdateRecord(cmd, partition, entity.Identity, type, s => ser.Serialize(entity.Value, s));
			}

		}

		public static void WriteRecord(SqlCommand cmd, object partition, Maybe<object> identity, Type type, Action<Stream> data)
		{
			DeleteRecord(cmd, type, partition, identity);
			cmd.Parameters.Clear();
			InsertRecord(cmd, partition, identity, data);
		}

		static void InsertRecord(SqlCommand cmd, object partition, Maybe<object> identity, Action<Stream> data)
		{
			cmd.CommandText = "INSERT INTO " + UniformViewTableName + " (Partition, Id, Data) VALUES (@part,@id,@data)";
			cmd.Parameters.AddWithValue("@part", partition);
			cmd.Parameters.AddWithValue("@id", identity.GetValue((object)DBNull.Value));
			using (var mem = new MemoryStream())
			{
				data(mem);
				cmd.Parameters.AddWithValue("@data", mem.ToArray());
			}
			cmd.ExecuteNonQuery();
		}

		static void UpdateRecord(SqlCommand cmd, object partition, Maybe<object> identity, Type type, Action<Stream> data)
		{
			var builder = new StringBuilder("UPDATE " + UniformViewTableName);

			var id = identity.GetValue((object) DBNull.Value);
			AddFilterClause(cmd, builder, partition, new IndexQuery(QueryViewOperand.Equal, id));
			builder.Append(" SET Data=@data");
			using (var mem = new MemoryStream())
			{
				data(mem);
				cmd.Parameters.AddWithValue("@data", mem.ToArray());
			}
			cmd.ExecuteNonQuery();
		}

		public static void DeleteRecord(SqlCommand cmd, Type type, object partition, Maybe<object> identity)
		{
			var builder = new StringBuilder();
			builder.Append("DELETE " + UniformViewTableName);
			var id = identity.GetValue((object) DBNull.Value);
			AddFilterClause(cmd, builder, partition, new IndexQuery(QueryViewOperand.Equal, id));
			cmd.ExecuteNonQuery();
		}

		public static void DeletePartition(SqlCommand cmd, Type type, object partition)
		{
			var builder = new StringBuilder();
			builder.Append("DELETE " + UniformViewTableName);
			AddFilterClause(cmd, builder, partition, Maybe<IndexQuery>.Empty);
			cmd.ExecuteNonQuery();
		}

		

		static void AddFilterClause(SqlCommand cmd, StringBuilder sb, object partition, Maybe<IndexQuery> index)
		{
			sb.Append(" WHERE Partition=@part");
			cmd.Parameters.AddWithValue("@part", partition);

			index.Apply(iq =>
			{
				sb.Append(" AND [Id] " + OperandToSql(iq.Operand) + " @id ");
				cmd.Parameters.AddWithValue("@id", iq.Value);
			});
		}

		public delegate void Reader(Stream stream, object partition, Maybe<object> identity);

		public static void ReadList(SqlCommand cmd, SqlViewQuery query, Reader processor)
		{
			var txt = new StringBuilder("SELECT ");

			query.RecordLimit.Apply(i => txt.Append(" TOP " + i + " "));
			//.Append(_directory.GetStorageReferenceByType(type))


			// in this implementation there is no splitting between 
			// tables by type of partition 
			txt.Append("Data, Partition, Id FROM " + UniformViewTableName);

			AddFilterClause(cmd, txt, query.PartitionKey, query.IndexQuery);
			
			cmd.CommandText = txt.ToString();

			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var partition = reader[1];
					var id = reader.IsDBNull(2) ? Maybe<object>.Empty : Maybe.From(reader[2]);
					using (var memory = new MemoryStream((byte[])reader[0]))
					{
						processor(memory, partition, id);
					}
				}
			}
		}
	}

	public sealed class ViewEntity
	{
		public readonly object Partition;
		public readonly Maybe<object> Identity;
		public readonly object Value;

		public ViewEntity(object partition, Maybe<object> identity, object value)
		{
			Partition = partition;
			Identity = identity;
			Value = value;
		}
	}
	public sealed class ViewEntity<TView>
	{
		public readonly object Partition;
		public readonly Maybe<object> Identity;
		public readonly TView Value;

		public ViewEntity(object partition, Maybe<object> identity, TView value)
		{
			Partition = partition;
			Identity = identity;
			Value = value;
		}
	}
}