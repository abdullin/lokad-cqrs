using NUnit.Framework;

namespace Lokad.Cqrs.Core.Directory
{
    [TestFixture]
    public sealed class When_activations_constrained_to_handler_type_with_type : MessageDirectoryFixture
    {
        // ReSharper disable InconsistentNaming
        MessageActivationMap Map { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Map = Builder.BuildActivationMap(mm => typeof(WhenSomethingSpecificHappened) == mm.Consumer);
        }

        [Test]
        public void Only_single_consumer_is_allowed()
        {
            CollectionAssert.AreEquivalent(new[] { typeof(WhenSomethingSpecificHappened) }, Map.QueryDistinctConsumingTypes());
        }

        [Test]
        public void Only_specific_message_is_allowed()
        {
            CollectionAssert.AreEquivalent(new[] { typeof(SomethingSpecificHappenedEvent) }, Map.QueryAllMessageTypes());
        }
    }
}