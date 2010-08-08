using System;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace CloudBus.Tests.Storage
{
	[TestFixture]
	public sealed class When_copying_blob_to_blob : StorageItemFixture
	{
		[Test]
		public void Test()
		{
			TestContainer.Create();

			var source = GetItem("source");
			var target = GetItem("target");

			Write(source, Guid.Empty);

			target.CopyFrom(source);

			ShouldHaveGuid(target, Guid.Empty);

		}
		
	}
}