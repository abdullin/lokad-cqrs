using Lokad.Cqrs.Core.Serialization;

namespace Lokad.Cqrs.Build
{
	public static class ModuleExtensionForSerialization
	{
		public static void AutoDetectSerializer(this AutofacBuilderForSerialization @this)
		{
			@this.RegisterDataSerializer<DataSerializerWithAutoDetection>();
			@this.RegisterEnvelopeSerializer<EnvelopeSerializerWithProtoBuf>();
		}

		public static void UseProtoBufSerialization(this AutofacBuilderForSerialization self)
		{
			self.RegisterDataSerializer<DataSerializerWithProtoBuf>();
			self.RegisterEnvelopeSerializer<EnvelopeSerializerWithProtoBuf>();
		}
	}
}