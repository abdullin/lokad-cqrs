namespace Lokad.Cqrs
{
	public sealed class MessageReference
	{
		public readonly string EnvelopeId;
		public readonly string StorageReference;
		public readonly string StorageContainer;

		public MessageReference(string envelopeId, string storageContainer, string storageReference)
		{
			EnvelopeId = envelopeId;
			StorageReference = storageReference;
			StorageContainer = storageContainer;
		}
	}
}