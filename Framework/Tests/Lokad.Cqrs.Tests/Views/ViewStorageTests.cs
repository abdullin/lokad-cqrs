#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Storage;
using Lokad.Cqrs.Tests.Storage;
using Lokad.Cqrs.Views;
using Lokad.Serialization;
using Lokad.Testing;
using NUnit.Framework;
using ProtoBuf;

namespace Lokad.Cqrs.Tests.Views
{
	[TestFixture]
	public sealed class ViewStorageTests
	{
		// ReSharper disable InconsistentNaming

		[ProtoContract]
		public sealed class UserView
		{
			[ProtoMember(1)]
			public string Name { get; set; }
		}

		IReadState Query { get; set; }
		IWriteState Publish { get; set; }
		IStorageContainer Container { get; set; }

		public ViewStorageTests()
		{
			var serializer = new ProtoBufMessageSerializer(new[] {typeof (UserView)});
			Container = new BlobStorage().GetContainer("views");

			var views = new ViewStorage(Container, serializer, serializer);
			Query = views;
			Publish = views;
		}

		[SetUp]
		public void SetUp()
		{
			Container.Create();
		}

		[TearDown]
		public void TearDown()
		{
			Container.Delete();
		}


		[Test]
		public void Test()
		{
			Publish.Write("1", new UserView
				{
					Name = "John"
				});

			Publish.Patch<UserView>("1", v =>
				{
					v.Name = v.Name + " Doe";
				});

			Query.Load<UserView>("1").ShouldPassCheck(v => v.Name == "John Doe");
			Publish.Delete<UserView>("1");
			Query.Load<UserView>("1").ShouldFail();
		}
	}
}