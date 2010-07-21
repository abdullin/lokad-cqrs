#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.SqlViews
{
	public sealed class PublishSqlViews : IPublishViews
	{
		readonly SqlViewDialect _dialect;
		readonly IDbPartitionManager _manager;

		public PublishSqlViews(IDbPartitionManager manager, SqlViewDialect dialect)
		{
			_manager = manager;
			_dialect = dialect;
		}

		public void Write(Type type, string partition, string identity, object view)
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
}