#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Data;
using System.Data.SqlClient;
using Lokad.Cqrs.Views.Sql;
using Lokad.Serialization;

namespace Lokad.Cqrs
{
	public interface IPublishViews
	{
		// different variations of the view go to different tables
		//void Write<TView>(TView view, object partition, Maybe<object> identity);
		//void Update<TView>(Action<TView> patch, object partition, Maybe<object> identity);
		//void Delete<TView>(object partition, Maybe<object> identity);

		void Write(Type view, object item, string partition, string identity);
		void Patch(Type type, string partition, string identity, Action<object> patch);
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

		public static void Patch<TView>(this IPublishViews self, string partition, string identity, Action<TView> patch)
		{
			self.Patch(typeof(TView), partition, identity, o => patch((TView)o));
		}

		public static void DeletePartition<TView>(this IPublishViews self, string partition)
		{
			self.DeletePartition(typeof(TView), partition);
		}
	}



	public sealed class PublishSqlViews : IPublishViews
	{
		readonly IDbPartitionManager _manager;
		readonly SqlViewDialect _dialect;

		public PublishSqlViews(IDbPartitionManager manager, SqlViewDialect dialect)
		{
			_manager = manager;
			_dialect = dialect;
		}

		public void Write(Type type, object view, string partition, string identity)
		{
			_manager.Execute(type, partition, cmd => _dialect.WriteRecord(cmd, type, partition, identity, view));
		}

		public void Patch(Type type, string partition, string identity, Action<object> patch)
		{
			_manager.Execute(type, partition, cmd => _dialect.PatchRecord(cmd, type, partition, identity, patch));
		}

		public void Delete(Type type, string partition, string identity)
		{
			_manager.Execute(type, partition, cmd => _dialect.DeleteRecord(cmd, type, partition, identity));
		}

		public void DeletePartition(Type type, string partition)
		{
			_manager.Execute(type, partition, cmd => _dialect.DeletePartition(cmd, type, partition));
		}

	}

	public interface IQueryViews
	{
		void Query(Type type, string partition, ViewQuery query, Action<ViewEntity> process);
	}


	public sealed class ViewQuery
	{
		public readonly Maybe<IndexQuery> IndexQuery = Maybe<IndexQuery>.Empty;
		public readonly Maybe<int> RecordLimit = Maybe<int>.Empty;

		public static readonly ViewQuery Empty = new ViewQuery();

		public ViewQuery()
		{
		}

		public ViewQuery(Maybe<int> recordLimit, Maybe<IndexQuery> indexQuery)
		{
			RecordLimit = recordLimit;
			IndexQuery = indexQuery;
		}

		public ViewQuery SetMaxRecords(int limit)
		{
			return new ViewQuery(limit, IndexQuery);
		}

		public ViewQuery WithIndexQuery(QueryViewOperand operand, string value)
		{
			return new ViewQuery(RecordLimit, new IndexQuery(operand, value));
		}
	}

	public sealed class IndexQuery
	{
		public readonly QueryViewOperand Operand;
		public readonly string Value;

		public IndexQuery(QueryViewOperand operand, string value)
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