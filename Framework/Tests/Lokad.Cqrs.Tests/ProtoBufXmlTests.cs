using System.Xml.Serialization;
using Lokad.Cqrs.Serialization;
using NUnit.Framework;

namespace Lokad.Cqrs.ProtoBuf.Tests
{
	[TestFixture]
	public sealed class ProtoBufXmlTests : Fixture
	{

		// ReSharper disable InconsistentNaming

		[Test]
		public void Roundtrip_Xml()
		{
			var result = RoundTrip(new SimpleXmlClass()
				{
					Value = "value"
				});

			Assert.AreEqual("value", result.Value);
		}

		[Test]
		public void Default_reference_is_type_name()
		{
			var contractReference = ProtoBufUtil.GetContractReference(typeof(SimpleXmlClass));
			Assert.AreEqual("SimpleXmlClass", contractReference);
		}

		[Test]
		public void Xml_class_can_override()
		{
			var contractReference = ProtoBufUtil.GetContractReference(typeof(CustomXmlClass));
			Assert.AreEqual("Custom/Type", contractReference);
		}



		[XmlType()]
		public sealed class SimpleXmlClass
		{
			[XmlElement(Order = 1)]
			public string Value { get; set; }
		}

		[XmlType(Namespace = "Custom", TypeName = "Type")]
		public sealed class CustomXmlClass
		{
			[XmlElement(Order = 1)]
			public string Value { get; set; }
		}
	}
}