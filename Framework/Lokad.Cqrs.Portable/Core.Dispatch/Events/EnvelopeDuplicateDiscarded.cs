namespace Lokad.Cqrs.Core.Dispatch.Events
{
    public sealed class EnvelopeDuplicateDiscarded : ISystemEvent
    {
        public string QueueName { get; private set; }
        public string EnvelopeId { get; private set; }

        public EnvelopeDuplicateDiscarded(string queueName, string envelopeId)
        {
            QueueName = queueName;
            EnvelopeId = envelopeId;
        }

        public override string ToString()
        {
            return string.Format("[{0}] duplicate discarded '{1}'", EnvelopeId, QueueName);
        }
    }
}