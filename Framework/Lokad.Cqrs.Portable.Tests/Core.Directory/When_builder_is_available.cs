using System.Linq;
using NUnit.Framework;

namespace Lokad.Cqrs.Core.Directory
{
    [TestFixture]
    public sealed class When_builder_is_available : MessageDirectoryFixture
    {
        // ReSharper disable InconsistentNaming

        [Test]
        public void All_concrete_messages_discovered_for_serialization()
        {
            var expected = TestMessageTypes.Where(t => !t.IsAbstract).ToArray();
            CollectionAssert.AreEquivalent(expected, Builder.ListMessagesToSerialize());
        }

        [Test]
        public void Al_concrete_handlers_discovered_for_activation()
        {
            var expected = TestConsumerTypes.Where(t => !t.IsAbstract).ToArray();
            CollectionAssert.AreEquivalent(expected, Builder.ListConsumersToActivate());
        }
    }
}