using ProtoBuf;

namespace Lokad.Cqrs.Core.Transport.Contracts_v2
{
	[ProtoContract]
	public sealed class Schema2ReferenceContract
	{
		[ProtoMember(1)]
		public readonly string EnvelopeId;
		[ProtoMember(2)]
		public readonly string StorageContainer;
		[ProtoMember(3)]
		public readonly string StorageReference;

		public Schema2ReferenceContract(string envelopeId, string storageContainer, string storageReference)
		{
			EnvelopeId = envelopeId;
			StorageContainer = storageContainer;
			StorageReference = storageReference;
		}


		Schema2ReferenceContract()
		{
		}

	}
}