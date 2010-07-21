using System.Collections.Generic;
using Lokad.Cqrs.Views.Sql;
using Lokad.Testing;
using NUnit.Framework;
using System.Linq;


// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.SqlViews.Tests
{

	[TestFixture]
	public sealed class When_view_published : SimpleSqlFixture
	{
		public When_view_published()
		{
			WithView<TestView>();
			WithView<AnotherView>();
		}

		public TestView[] Expected { get; set; }

		public override void OnFixtureSetup()
		{
			var view = new TestView()
				{
					Value = "Asd"
				};
			Publish.Write(view, "Root", "Id");
			Expected = new[] { view };
		}

		[Test]
		public void It_could_be_Loaded()
		{
			Query
				.Load<TestView>("Root", "Id")
				.ShouldPassCheck(p => p.Value == "Asd");
		}

		[Test]
		public void It_could_be_listed()
		{
			var actual = Query.List<TestView>("Root").ToArray(s => s.Value);
			CollectionAssert.AreEqual(Expected, actual);
		}

		[Test]
		public void It_could_be_filtered()
		{
			var query = new ViewQuery().WithIndexQuery(QueryViewOperand.Equal, "Id");
			var actual = Query.List<TestView>("Root", query).ToArray(s => s.Value);
			CollectionAssert.AreEqual(Expected, actual);
		}

		[Test]
		public void It_is_partition_scoped()
		{
			Query.Load<TestView>("Root2", "Id").ShouldFail();
		}

		[Test]
		public void It_is_identity_scoped()
		{
			Query.Load<TestView>("Root", "Id2").ShouldFail();
		}

		[Test]
		public void It_is_type_scoped()
		{
			Query.Load<AnotherView>("Root", "Id").ShouldFail();
		}
	}
}