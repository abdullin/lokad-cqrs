using System.Runtime.Serialization;
using Lokad.Quality;
using NUnit.Framework;

namespace Lokad.Cqrs.ProtoBuf.Tests
{
	[TestFixture]
	public sealed class ProtoBufDataTests : Fixture
	{
		// ReSharper disable InconsistentNaming

		[Test]
		public void Roundtrip_Data()
		{
			var result = RoundTrip(new SimpleDataClass("Some"));
			Assert.AreEqual("Some", result.Field);

		}


		[Test]
		public void Default_reference_is_type_name()
		{
			var contractReference = ProtoBufUtil.GetContractReference(typeof(SimpleDataClass));
			Assert.AreEqual("SimpleDataClass", contractReference);
		}

		[Test]
		public void Class_can_override()
		{
			var contractReference = ProtoBufUtil.GetContractReference(typeof(CustomDataClass));
			Assert.AreEqual("Custom/Type", contractReference);
		}

		[DataContract]
		public sealed class SimpleDataClass
		{
			[DataMember(Order = 1)]
			public string Field { get; private set; }

			public SimpleDataClass(string field)
			{
				Field = field;
			}

			[UsedImplicitly]
			SimpleDataClass()
			{
			}
		}

		[DataContract(Namespace = "Custom", Name = "Type")]
		public sealed class CustomDataClass
		{
			
		}
	}
}