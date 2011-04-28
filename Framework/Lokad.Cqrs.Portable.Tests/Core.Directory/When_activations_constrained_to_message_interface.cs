using System.Linq;
using NUnit.Framework;

namespace Lokad.Cqrs.Core.Directory
{
    [TestFixture]
    public sealed class When_activations_constrained_to_message_interface : MessageDirectoryFixture
    {
        // ReSharper disable InconsistentNaming

        // ReSharper disable InconsistentNaming
        MessageActivationMap Map { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Map = Builder.BuildActivationMap(mm => typeof(ISomethingHappenedEvent) == mm.Message);
        }

        [Test]
        public void Only_derived_messages_are_allowed()
        {
            var derivedMessages = TestMessageTypes
                .Where(t => typeof (ISomethingHappenedEvent).IsAssignableFrom(t));

            CollectionAssert.IsSubsetOf(Map.QueryAllMessageTypes(), derivedMessages);
        }

        [Test]
        public void Non_handled_derived_messages_are_prohibited()
        {
            CollectionAssert.DoesNotContain(Map.QueryAllMessageTypes(), typeof(SomethingUnexpectedHandled));
        }
        
    }
}