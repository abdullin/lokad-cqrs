#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Data;

namespace Lokad.Cqrs.SqlViews
{
	public interface IDbPartitionManager
	{
		bool ImplicitPartition { get; }
		bool ImplicitView { get; }
		string GetTable(Type type, string partition);
		string GetViewName(Type type);
		void Execute(Type type, string partition, Action<IDbCommand> exec);
	}
}