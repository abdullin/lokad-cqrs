using NUnit.Framework;
using System.Linq;

// ReSharper disable InconsistentNaming
namespace Lokad.Cqrs.Core.Directory
{
    [TestFixture]
    public sealed class When_activations_constrained_to_handler_type_with_interface : MessageDirectoryFixture
    {
        
        MessageActivationMap Map { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Map = Builder.BuildActivationMap(mm => typeof(WhenSomethingGenericHappened) == mm.Consumer);
        }

        [Test]
        public void Only_derived_messages_are_allowed()
        {
            var expected = TestMessageTypes
                .Where(t => typeof (ISomethingHappenedEvent).IsAssignableFrom(t))
                .ToArray();

            CollectionAssert.AreEquivalent(expected, Map.QueryAllMessageTypes());
        }

        [Test]
        public void Only_single_consumer_is_allowed()
        {
            CollectionAssert.AreEquivalent(new[] {typeof(WhenSomethingGenericHappened)}, Map.QueryDistinctConsumingTypes());
        }
    }
}