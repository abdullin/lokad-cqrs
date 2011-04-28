namespace Lokad.Cqrs.Core.Directory.Default
{
    public sealed class MessageDetail
    {
        public readonly string EnvelopeId;

        public MessageDetail(string envelopeId)
        {
            EnvelopeId = envelopeId;
        }
    }
}