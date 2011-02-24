using System;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Tests.Storage
{
	public abstract class When_copying_items_in<T> : StorageItemFixture<T> where T : ITestStorage, new()
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