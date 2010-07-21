using NUnit.Framework;
using Lokad.Testing;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.SqlViews.Tests
{
	[TestFixture]
	public sealed class When_view_updated : SimpleSqlFixture
	{
		public When_view_updated()
		{
			WithView<TestView>();
		}

		public override void OnFixtureSetup()
		{
			Publish.Write(new TestView()
			{
				Value = "Asd"
			}, "Root", "Id");

			Publish.Patch<TestView>("Root", "Id", tv => tv.Value = "Updated");
		}

		[Test]
		public void Patched_value_is_present()
		{
			Query.Load<TestView>("Root", "Id").ShouldPassCheck(t => t.Value == "Updated");
		}
	}
}