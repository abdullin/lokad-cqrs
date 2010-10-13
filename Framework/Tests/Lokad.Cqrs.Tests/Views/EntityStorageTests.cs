#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Threading;
using Lokad.Cqrs.Serialization;
using Lokad.Cqrs.Storage;
using Lokad.Cqrs.Tests.Storage;
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
			Container.Remove();
		}


		[Test]
		public void Test()
		{
			// TODO: add volatile rollbacks?
			Writer.Add("1", new UserView
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

			Writer.Add("1", new UserView { Name = "John" });

			Writer.Update<UserView>("1", v =>
			{
				Thread.Sleep(1.Seconds());
				Writer.Update<UserView>("1", e => { e.Name = "Jonny"; });
				v.Name = v.Name + " Doe";
			});
		}
	}
}