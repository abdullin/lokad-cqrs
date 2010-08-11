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
	public sealed class EntityStorageTests
	{
		// ReSharper disable InconsistentNaming

		[ProtoContract]
		public sealed class UserView
		{
			[ProtoMember(1)]
			public string Name { get; set; }
		}

		IReadEntity Read { get; set; }
		IWriteEntity Write { get; set; }
		IStorageContainer Container { get; set; }

		public EntityStorageTests()
		{
			var serializer = new ProtoBufMessageSerializer(new[] {typeof (UserView)});
			Container = new FileStorage().GetContainer("views");

			var views = new EntityStorage(Container, serializer, serializer);
			Read = views;
			Write = views;
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
			// TODO: add volatile rollbacks?

			Write.Write("1", new UserView
				{
					Name = "John"
				});

			Write.Patch<UserView>("1", v =>
				{
					v.Name = v.Name + " Doe";
				});

			Read.Load<UserView>("1").ShouldPassCheck(v => v.Name == "John Doe");
			Write.Delete<UserView>("1");
			Read.Load<UserView>("1").ShouldFail();
		}

		[Test, ExpectedException(typeof(OptimisticConcurrencyException))]
		public void Concurrency()
		{
			Write.Write("1", new UserView { Name = "John" });

			Write.Patch<UserView>("1", v =>
			{
				SystemUtil.Sleep(1.Seconds());
				Write.Write("1", new UserView { Name = "John" });

				v.Name = v.Name + " Doe";
			});
		}
	}
}