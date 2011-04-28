#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using Lokad.Cqrs.Core.Directory;
using NUnit.Framework;

namespace Lokad.Cqrs.Legacy
{
    [TestFixture]
    public sealed class MessageDirectoryTests
    {
        // ReSharper disable InconsistentNaming

        #region Classes

        public interface IMessage
        {
        }

        public interface IDomainCommand : IMessage
        {
        }

        public interface IDomainEvent : IMessage
        {
        }

        public interface IConsumeMessage
        {
        }

        public interface Handle<TMessage> : IConsumeMessage<TMessage>
            where TMessage : IDomainCommand
        {
        }

        public interface ConsumerOf<TMessage> : IConsumeMessage<TMessage>
            where TMessage : IDomainEvent
        {
        }


        public interface IConsumeMessage<TMessage> : IConsumeMessage
            where TMessage : IMessage
        {
            void Consume(TMessage message);
        }

        public sealed class ListenToAll : IConsumeMessage<IMessage>
        {
            public void Consume(IMessage message)
            {
                throw new NotImplementedException();
            }
        }

        public interface ISomethingHappenedEvent : IDomainEvent
        {
        }

        public sealed class SomethingHappenedEvent : ISomethingHappenedEvent
        {
        }


        public sealed class WhenSomethingHappened : ConsumerOf<ISomethingHappenedEvent>
        {
            public void Consume(ISomethingHappenedEvent message)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class DoSomethingCommand : IDomainCommand
        {
        }

        public sealed class DoSomethingHandler : Handle<DoSomethingCommand>
        {
            public void Consume(DoSomethingCommand message)
            {
                throw new NotImplementedException();
            }
        }

        public sealed class OrphanCommand : IDomainCommand
        {
        }

        #endregion

        MessageDirectoryBuilder Builder { get; set; }
        MessageMapping[] Mappings { get; set; }
        

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var hint = MethodInvokerHint.FromConsumerSample<IConsumeMessage<IMessage>>(m => m.Consume(null));
            var scanner = new DomainAssemblyScanner();

            scanner.Constrain(hint);

            Mappings = scanner
                .WithAssemblyOf<MessageDirectoryTests>()
                .Build(hint.ConsumerTypeDefinition).ToArray();


            Builder = new MessageDirectoryBuilder(Mappings);
        }

        [Test]
        public void Scan()
        {
            //Directory.Consumers.ToArray();
        }

        [Test]
        public void Filtered()
        {
            var map = Builder.BuildActivationMap(mm => typeof (ListenToAll) == mm.Consumer);

            CollectionAssert.AreEquivalent(new[] {typeof(ListenToAll)}, map.QueryAllConsumingTypes());
            var expected = new[]
                {
                    typeof(ISomethingHappenedEvent), 
                    typeof(SomethingHappenedEvent), 
                    typeof(DoSomethingCommand),
                    typeof(IDomainCommand),
                    typeof(IDomainEvent),
                    typeof(IMessage),
                    typeof(OrphanCommand)
                };
            CollectionAssert.AreEquivalent(expected, map.QueryAllMessageTypes());


            //CollectionAssert.Contains(consumer.MessageTypes, typeof (ISomethingHappenedEvent));
            //CollectionAssert.Contains(consumer.MessageTypes, typeof (SomethingHappenedEvent));
            //CollectionAssert.Contains(consumer.MessageTypes, typeof (DoSomethingCommand));
        }

        //[Test]
        //public void Orphans_are_kept()
        //{
        //    var directory = Builder.BuildDirectory(mm => typeof (ListenToAll) != mm.Consumer);

        //    CollectionAssert.Contains(directory.Messages.Select(m => m.MessageType).ToArray(), typeof (OrphanCommand));
        //}
    }
}