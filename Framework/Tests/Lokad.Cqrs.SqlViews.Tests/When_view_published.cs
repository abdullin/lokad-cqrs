#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Linq;
using Lokad.Testing;
using NUnit.Framework;

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
			var view = new TestView
				{
					Value = "Asd"
				};
			Publish.Write("Root", "Id", view);
			Expected = new[] {view};
		}

		[Test]
		public void It_could_be_Loaded()
		{
			Query
				.Load<TestView>("Root", "Id")
				.ShouldPassCheck(p => p.Value == "Asd");
		}

		[Test]
		public void It_could_be_filtered()
		{
			var query = new ViewQuery().SetIdentityConstraint(ConstraintOperand.Equal, "Id");
			var actual = Query.ListPartition<TestView>("Root", query).ToArray(s => s.Value);
			CollectionAssert.AreEqual(Expected, actual);
		}

		[Test]
		public void It_could_be_listed()
		{
			var actual = Query.ListPartition<TestView>("Root").ToArray(s => s.Value);
			CollectionAssert.AreEqual(Expected, actual);
		}

		[Test]
		public void It_is_identity_scoped()
		{
			Query.Load<TestView>("Root", "Id2").ShouldFail();
		}

		[Test]
		public void It_is_partition_scoped()
		{
			Query.Load<TestView>("Root2", "Id").ShouldFail();
		}

		[Test]
		public void It_is_type_scoped()
		{
			Query.Load<AnotherView>("Root", "Id").ShouldFail();
		}
	}
}