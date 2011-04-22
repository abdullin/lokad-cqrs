namespace Lokad.Cqrs
{
	public sealed class EnvelopeReference
	{
		public readonly string EnvelopeId;
		public readonly string StorageReference;
		public readonly string StorageContainer;

		public EnvelopeReference(string envelopeId, string storageContainer, string storageReference)
		{
			EnvelopeId = envelopeId;
			StorageReference = storageReference;
			StorageContainer = storageContainer;
		}
	}
}