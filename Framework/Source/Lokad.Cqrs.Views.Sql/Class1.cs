#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Data;
using System.Data.SqlClient;
using Lokad.Serialization;

namespace Lokad.Cqrs.Views.Sql
{
	public interface IPublishViews
	{
		// different variations of the view go to different tables
		//void Write<TView>(TView view, object partition, Maybe<object> identity);
		//void Update<TView>(Action<TView> patch, object partition, Maybe<object> identity);
		//void Delete<TView>(object partition, Maybe<object> identity);

		void Write(Type view, object item, string partition, string identity);
		void Patch(Type view, Action<object> patch, string partition, string identity);
		void Delete(Type type, string partition, string identity);
		
		void DeletePartition(Type type, string partition);
		// secondary indexes to be added here
	}

	public static class ExtendIPublishViews
	{
		public static void Write<TView>(this IPublishViews self, TView view, string partition, string identity)
		{
			self.Write(typeof(TView), view, partition, identity);
		}
		public static void Delete<TView>(this IPublishViews self, string partition, string identity)
		{
			self.Delete(typeof(TView), partition,identity);
		}
	}

	public sealed class PublishSqlViews : IPublishViews
	{
		readonly IsolationLevel _level;
		readonly Func<SqlConnection> _factory;
		readonly IDataSerializer _serializer;

		public PublishSqlViews(IsolationLevel level, Func<SqlConnection> factory, IDataSerializer serializer)
		{
			_level = level;
			_factory = factory;
			_serializer = serializer;
		}

		public void Write(Type type, object view, string partition, string identity)
		{
			Execute(cmd => SqlViewDialect.WriteRecord(cmd, partition, identity, type, s => _serializer.Serialize(view, s)));
		}

		public void Patch(Type view, Action<object> patch, string partition, string identity)
		{
			Execute(cmd => SqlViewDialect.PatchRecord(cmd, partition, identity, view, _serializer, patch));
		}

		public void Delete(Type type, string partition, string identity)
		{
			Execute(cmd => SqlViewDialect.DeleteRecord(cmd, type, partition, identity));
		}

		public void DeletePartition(Type type, string partition)
		{
			Execute(cmd => SqlViewDialect.DeletePartition(cmd, type, partition));
		}

		void Execute(Action<SqlCommand> exec)
		{
			using (var conn = _factory())
			{
				conn.Open();
				using (var tx = conn.BeginTransaction(_level))
				{
					using (var cmd = new SqlCommand("", conn, tx))
					{
						exec(cmd);
					}
					tx.Commit();
				}

			}
		}
	}


	public interface IQueryViews<in TQuery>
	{
		Maybe<object> Load(Type type, string partition, string identity);
		Maybe<TView> Load<TView>(string partition, string identity);

		void List<TView>(TQuery query, Action<ViewEntity<TView>> process);
		void List(Type type, TQuery query, Action<ViewEntity> process);
	}
	
	public sealed class SqlViewQuery
	{
		public readonly Maybe<IndexQuery> IndexQuery = Maybe<IndexQuery>.Empty;
		public readonly string PartitionKey;
		public readonly Maybe<int> RecordLimit = Maybe<int>.Empty;

		public SqlViewQuery(string partitionKey)
		{
			PartitionKey = partitionKey;
		}

		public SqlViewQuery(Maybe<int> recordLimit, string partitionKey, Maybe<IndexQuery> indexQuery)
		{
			RecordLimit = recordLimit;
			IndexQuery = indexQuery;
			PartitionKey = partitionKey;
		}

		public SqlViewQuery SetMaxRecords(int limit)
		{
			return new SqlViewQuery(limit, PartitionKey, IndexQuery);
		}

		public SqlViewQuery WithIndexQuery(QueryViewOperand operand, object value)
		{
			return new SqlViewQuery(RecordLimit, PartitionKey, new IndexQuery(operand, value));
		}
	}

	public sealed class IndexQuery
	{
		public readonly QueryViewOperand Operand;
		public readonly object Value;

		public IndexQuery(QueryViewOperand operand, object value)
		{
			Operand = operand;
			Value = value;
		}
	}

	public enum QueryViewOperand
	{
		Equal,
		GreaterThan,
		GreaterThanOrEqual,
		LessThan,
		LessThanOrEqual,
		NotEqual
	}
}