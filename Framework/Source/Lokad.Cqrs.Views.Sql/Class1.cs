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

		void Write(Type view, object item, object partition, Maybe<object> identity);
		void Update(Type view, Action<object> patch, object partition, Maybe<object> identity);
		void Delete(Type type, object partition, Maybe<object> identity);
	}

	public static class ExtendIPublishViews
	{
		public static void Write<TView>(this IPublishViews self, TView view, object partition, Maybe<object> identity)
		{
			self.Write(typeof(TView), view, partition, identity);
		}
		public static void Delete<TView>(this IPublishViews self, object partition, Maybe<object> identity)
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

		public void Write(Type view, object item, object partition, Maybe<object> identity)
		{
			using (var conn = _factory())
			{
				conn.Open();
				using (var tx = conn.BeginTransaction(_level))
				{
					using (var cmd = new SqlCommand("", conn, tx))
					{
						SqlViewDialect.WriteRecord(cmd, partition, identity, view, s => _serializer.Serialize(item, s));
					}
				}
			}
		}

		public void Update(Type view, Action<object> patch, object partition, Maybe<object> identity)
		{
			using (var conn = _factory())
			{
				conn.Open();
				using (var tx = conn.BeginTransaction(_level))
				{
					using (var cmd = new SqlCommand("", conn, tx))
					{
						SqlViewDialect.PatchRecord(cmd, partition, identity, view, _serializer, patch);
					}
				}
			}
		}

		public void Delete(Type type, object partition, Maybe<object> identity)
		{
			using (var conn = _factory())
			{
				conn.Open();
				using (var tx = conn.BeginTransaction(_level))
				{
					using (var cmd = new SqlCommand("", conn, tx))
					{
						SqlViewDialect.DeleteRecord(cmd, type, partition, identity);
					}
				}
			}
		}
	}


	public interface IQueryViews<in TQuery>
	{
		Maybe<object> Load(Type type, object partition);
		Maybe<object> Load(Type type, object partition, object identity);

		Maybe<TView> Load<TView>(object partition);
		Maybe<TView> Load<TView>(object partition, object identity);

		void List<TView>(TQuery query, Action<ViewEntity<TView>> process);
		void List(Type type, TQuery query, Action<ViewEntity> process);
	}
	
	public sealed class SqlViewQuery
	{
		public readonly Maybe<IndexQuery> IndexQuery = Maybe<IndexQuery>.Empty;
		public readonly object PartitionKey;
		public readonly Maybe<int> RecordLimit = Maybe<int>.Empty;

		public SqlViewQuery(object partitionKey)
		{
			PartitionKey = partitionKey;
		}

		public SqlViewQuery(Maybe<int> recordLimit, object partitionKey, Maybe<IndexQuery> indexQuery)
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