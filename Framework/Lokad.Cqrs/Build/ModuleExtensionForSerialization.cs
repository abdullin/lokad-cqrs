using Lokad.Cqrs.Core.Serialization;

namespace Lokad.Cqrs.Build
{
	public static class ModuleExtensionForSerialization
	{
		public static void AutoDetectSerializer(this ModuleForSerialization @this)
		{
			@this.RegisterDataSerializer<DataSerializerWithAutoDetection>();
			@this.RegisterEnvelopeSerializer<EnvelopeSerializerWithProtoBuf>();
		}

		public static void UseProtoBufSerialization(this ModuleForSerialization self)
		{
			self.RegisterDataSerializer<DataSerializerWithProtoBuf>();
			self.RegisterEnvelopeSerializer<EnvelopeSerializerWithProtoBuf>();
		}
	}
}