#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Core.Serialization;
using NUnit.Framework;
using ProtoBuf;


namespace Lokad.Cqrs.ProtoBuf.Tests
{
	[TestFixture]
	public sealed class ProtoBufNativeTests : Fixture
	{
		// ReSharper disable InconsistentNaming


		[Test]
		public void Roundtrip_Data()
		{
			var result = RoundTrip(new SimpleProtoClass("Some"));
			Assert.AreEqual("Some", result.Field);
		}

		[Test]
		public void Roundtrip_Via()
		{
			var result = RoundTrip(new SimpleProtoClass("Some"), typeof(CustomProtoClass));
			Assert.AreEqual("Some", result.Field);
		}


		[Test]
		public void Default_reference_is_type_name()
		{
			var contractReference = ProtoBufUtil.GetContractReference(typeof (SimpleProtoClass));
			Assert.AreEqual("SimpleProtoClass", contractReference);
		}

		[Test]
		public void Class_can_override()
		{
			var contractReference = ProtoBufUtil.GetContractReference(typeof (CustomProtoClass));
			Assert.AreEqual("Custom", contractReference);
		}

		[ProtoContract]
		public sealed class SimpleProtoClass : IExtensible
		{
			[ProtoMember(1)]
			public string Field { get; private set; }

			public SimpleProtoClass(string field)
			{
				Field = field;
			}

			
			SimpleProtoClass()
			{
			}

			IExtension _extensible;


			public IExtension GetExtensionObject(bool createIfMissing)
			{
				return Extensible.GetExtensionObject(ref _extensible, createIfMissing);
			}
		}

		[ProtoContract(Name = "Custom")]
		public sealed class CustomProtoClass : IExtensible
		{

			IExtension _extensible;


			public IExtension GetExtensionObject(bool createIfMissing)
			{
				return Extensible.GetExtensionObject(ref _extensible, createIfMissing);
			}
		}
	}
}