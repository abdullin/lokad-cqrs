using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Lokad.Cqrs.Views.Sql;
using Lokad.Serialization;
using NUnit.Framework;
using ProtoBuf;

namespace Lokad.Cqrs.SqlViews.Tests
{
	[TestFixture]
	public sealed class ClassTests
	{
		[Test]
		public void Test()
		{
			var conn = @"Data Source=.\SQLExpress;Initial Catalog=Salescast.Beta;Integrated Security=true";

			var pub = new PublishSqlViews(IsolationLevel.ReadCommitted, () => new SqlConnection(conn), new ProtoBufSerializer(new[] {typeof(MyView)}));

			pub.Write(new MyView()
				{
					Value = "Asd"
				}, "Test", "Id");
		}


		
	}

	[ProtoContract]
	public sealed class MyView
	{
		[ProtoMember(1)]
		public string Value { get; set; }
	}
}
