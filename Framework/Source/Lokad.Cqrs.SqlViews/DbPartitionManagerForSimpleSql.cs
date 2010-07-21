#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Data;
using System.Data.SqlClient;
using Lokad.Serialization;

namespace Lokad.Cqrs.SqlViews
{
	public sealed class DbPartitionManagerForSimpleSql : IDbPartitionManager
	{
		readonly string _connection;
		readonly IsolationLevel _isolationLevel;
		readonly IDataContractMapper _mapper;
		readonly string _tableName;


		public DbPartitionManagerForSimpleSql(string connection, IsolationLevel isolationLevel, IDataContractMapper mapper,
			string tableName)
		{
			_connection = connection;
			_tableName = tableName;
			_mapper = mapper;
			_isolationLevel = isolationLevel;
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

		static string Create(string name)
		{
			const string s =
				@"
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
	}
}