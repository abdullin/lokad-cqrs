using NUnit.Framework;

namespace Lokad.Cqrs.ProtoBuf.Tests
{
	[TestFixture]
	public sealed class ExtentSyntaxForProtoBufTests
	{
		// ReSharper disable InconsistentNaming
		[Test]
		public void Namespace_is_consistent()
		{
			Assert.AreEqual("Lokad.Cqrs", typeof(ExtendSyntaxWithProtoBuf).Namespace, "namespace should not change");
		}
	}
}