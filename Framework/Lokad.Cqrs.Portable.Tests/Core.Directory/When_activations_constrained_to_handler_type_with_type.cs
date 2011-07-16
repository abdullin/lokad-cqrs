using Lokad.Cqrs.Feature.DirectoryDispatch;
using NUnit.Framework;

namespace Lokad.Cqrs.Core.Directory
{
    [TestFixture]
    public sealed class When_activations_constrained_to_handler_type_with_type : MessageDirectoryFixture
    {
        // ReSharper disable InconsistentNaming
        MessageActivationInfo[] Map { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Map = Builder.BuildActivationMap(mm => typeof(WhenSomethingSpecificHappened) == mm.Consumer);
        }

        [Test]
        public void Only_single_consumer_is_allowed()
        {
            var expected = new[] { typeof(WhenSomethingSpecificHappened) };
            CollectionAssert.AreEquivalent(expected, QueryDistinctConsumingTypes(Map));
        }

        [Test]
        public void Only_specific_message_is_allowed()
        {
            var expected = new[] { typeof(SomethingSpecificHappenedEvent) };
            CollectionAssert.AreEquivalent(expected, QueryAllMessageTypes(Map));
        }
    }
}