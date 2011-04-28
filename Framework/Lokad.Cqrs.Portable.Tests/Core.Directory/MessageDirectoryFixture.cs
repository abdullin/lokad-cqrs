#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;

namespace Lokad.Cqrs.Core.Directory
{
    public class MessageDirectoryFixture
    {
        protected readonly Type[] TestMessageTypes;
        protected readonly Type[] TestConsumerTypes;
        protected MessageDirectoryBuilder Builder { get; set; }
        protected MessageMapping[] Mappings { get; set; }

        public MessageDirectoryFixture()
        {
            var hint = MethodInvokerHint.FromConsumerSample<IConsumeMessage<IMessage>>(m => m.Consume(null));
            var scanner = new DomainAssemblyScanner();

            scanner.Constrain(hint);

            Mappings = scanner
                .WithAssemblyOf<When_activation_map_constrained_to_catch_all_consumer>()
                .Build(hint.ConsumerTypeDefinition).ToArray();


            Builder = new MessageDirectoryBuilder(Mappings);

            var nestedTypes = typeof (MessageDirectoryFixture).GetNestedTypes();

            TestMessageTypes = nestedTypes
                .Where(t => typeof (IMessage).IsAssignableFrom(t)).ToArray();
            TestConsumerTypes = nestedTypes
                .Where(t => typeof (IConsumeMessage).IsAssignableFrom(t)).ToArray();
        }

        #region Classes

        public interface IMessage {}

        public interface IDomainCommand : IMessage {}

        public interface IDomainEvent : IMessage {}

        public interface IConsumeMessage {}

        public interface Handle<TMessage> : IConsumeMessage<TMessage>
            where TMessage : IDomainCommand {}

        public interface ConsumerOf<TMessage> : IConsumeMessage<TMessage>
            where TMessage : IDomainEvent {}


        public interface IConsumeMessage<TMessage> : IConsumeMessage
            where TMessage : IMessage
        {
            void Consume(TMessage message);
        }

        public sealed class ListenToAll : IConsumeMessage<IMessage>
        {
            public void Consume(IMessage message) {}
        }

        public interface ISomethingHappenedEvent : IDomainEvent {}

        public sealed class SomethingSpecificHappenedEvent : ISomethingHappenedEvent {}

        public sealed class SomethingElseHappened : ISomethingHappenedEvent {}

        public sealed class SomethingUnexpectedHandled : ISomethingHappenedEvent{}


        public sealed class WhenSomethingGenericHappened : ConsumerOf<ISomethingHappenedEvent>
        {
            public void Consume(ISomethingHappenedEvent message) {}
        }

        public sealed class WhenSomethingSpecificHappened : ConsumerOf<SomethingSpecificHappenedEvent>
        {
            public void Consume(SomethingSpecificHappenedEvent message) {}
        }

        public sealed class DoSomethingCommand : IDomainCommand {}

        public sealed class DoSomethingHandler : Handle<DoSomethingCommand>
        {
            public void Consume(DoSomethingCommand message) {}
        }

        public sealed class OrphanCommand : IDomainCommand {}

        #endregion
    }
}