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

		IEntityReader Reader { get; set; }
		IEntityWriter Writer { get; set; }
		IStorageContainer Container { get; set; }

		public EntityStorageTests()
		{
			var serializer = new ProtoBufMessageSerializer(new[] {typeof (UserView)});
			Container = new FileStorage().GetContainer("views");

			var views = new EntityStorage(Container, serializer, serializer);
			Reader = views;
			Writer = views;
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

			Writer.Upsert("1", new UserView
				{
					Name = "John"
				});

			Writer.Update<UserView>("1", v =>
				{
					v.Name = v.Name + " Doe";
				});

			Reader.Read<UserView>("1").ShouldPassCheck(v => v.Name == "John Doe");
			Writer.Remove<UserView>("1");
			Reader.Read<UserView>("1").ShouldFail();
		}

		[Test, ExpectedException(typeof(OptimisticConcurrencyException))]
		public void Concurrency()
		{
			
			Writer.Upsert("1", new UserView { Name = "John" });

			Writer.Update<UserView>("1", v =>
			{
				SystemUtil.Sleep(1.Seconds());
				Writer.Upsert("1", new UserView { Name = "John" });
				v.Name = v.Name + " Doe";
			});
		}
	}
}