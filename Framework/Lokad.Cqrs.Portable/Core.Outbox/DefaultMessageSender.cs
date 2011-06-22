#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.Transactions;
using Lokad.Cqrs.Core.Envelope;

namespace Lokad.Cqrs.Core.Outbox
{
    sealed class DefaultMessageSender : IMessageSender
    {
        readonly IQueueWriter[] _queues;
        readonly ISystemObserver _observer;
        readonly Func<string> _idGenerator;

        public DefaultMessageSender(IQueueWriter[] queues, ISystemObserver observer, Func<string> idGenerator)
        {
            _queues = queues;
            _observer = observer;
            _idGenerator = idGenerator;

            if (queues.Length == 0)
                throw new InvalidOperationException("There should be at least one queue");
        }

        public void SendOne(object content)
        {
            InnerSendBatch(cb => { }, new[] {content});
        }

        public void SendOne(object content, Action<EnvelopeBuilder> configure)
        {
            InnerSendBatch(configure, new[] {content});
        }


        public void SendBatch(object[] content)
        {
            InnerSendBatch(cb => { }, content);
        }

        public void SendBatch(object[] content, Action<EnvelopeBuilder> builder)
        {
            InnerSendBatch(builder, content);
        }

        
        Random _random = new Random();


        void InnerSendBatch(Action<EnvelopeBuilder> configure, object[] messageItems) {
            if (messageItems.Length == 0)
                return;

            var id = _idGenerator();

            var builder = new EnvelopeBuilder(id);
            foreach (var item in messageItems)
            {
                builder.AddItem(item);
            }
            
            configure(builder);
            var envelope = builder.Build();

            var queue = GetOutboundQueue();

            if (Transaction.Current == null)
            {
                queue.PutMessage(envelope);
                _observer.Notify(new EnvelopeSent(queue.Name, envelope.EnvelopeId, false,
                    envelope.Items.Select(x => x.MappedType.Name).ToArray()));
            }
            else
            {
                var action = new CommitActionEnlistment(() =>
                    {
                        queue.PutMessage(envelope);
                        _observer.Notify(new EnvelopeSent(queue.Name, envelope.EnvelopeId, true,
                            envelope.Items.Select(x => x.MappedType.Name).ToArray()));
                    });
                Transaction.Current.EnlistVolatile(action, EnlistmentOptions.None);
            }
        }

        IQueueWriter GetOutboundQueue()
        {
            if (_queues.Length == 1)
                return _queues[0];
            var random = _random.Next(_queues.Length);
            return _queues[random];
        }
    }
}