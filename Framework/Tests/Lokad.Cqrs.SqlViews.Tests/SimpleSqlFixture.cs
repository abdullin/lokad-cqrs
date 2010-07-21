using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Lokad.Serialization;
using NUnit.Framework;

namespace Lokad.Cqrs.SqlViews.Tests
{

	[TestFixture]
	public abstract class SimpleSqlFixture
	{

		public DbPartitionManagerForSimpleSql Manager { get; private set; }
		public IPublishViews Publish { get; private set; }
		public IQueryViews Query { get; private set; }

		readonly HashSet<Type> _views = new HashSet<Type>();

		public void WithView<TView>()
		{
			_views.Add(typeof (TView));
		}

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			var conn = @"Data Source=.\SQLExpress;Initial Catalog=LokadDev;Integrated Security=true";
			var name = "View_Table";

			var serializer = new ProtoBufSerializer(_views);
			Manager = new DbPartitionManagerForSimpleSql(conn, IsolationLevel.ReadCommitted, serializer, name);
			Manager.CreateIfNotExist();

			var dialect = new SqlViewDialect(serializer, Manager);
			Publish = new PublishSqlViews(Manager, dialect);
			Query = new QuerySqlViews(Manager, dialect);

			OnFixtureSetup();
		}

		public virtual void OnFixtureSetup()
		{
			
		}

		[TestFixtureTearDown]
		public void FixtureTearDown()
		{
			Manager.DropTables();
		}
	}
}
