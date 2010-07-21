#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Quality;

namespace Lokad.Cqrs.SqlViews
{
	/// <summary>
	/// Simple query engine for views stored in a single SQL table
	/// </summary> 
	[UsedImplicitly]
	public sealed class QuerySqlViews : IQueryViews
	{
		readonly SqlViewDialect _dialect;
		readonly IDbPartitionManager _manager;

		public QuerySqlViews(IDbPartitionManager manager, SqlViewDialect dialect)
		{
			_manager = manager;
			_dialect = dialect;
		}

		public void QueryPartition(Type type, string partition, Maybe<IViewQuery> query, Action<ViewEntity> process)
		{
			_manager.Execute(type, partition, cmd => _dialect.ReadList(cmd, type, partition, query, process));
		}
	}
}