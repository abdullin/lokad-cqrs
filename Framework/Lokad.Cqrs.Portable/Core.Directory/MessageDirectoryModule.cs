#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory.Default;

namespace Lokad.Cqrs.Core.Directory
{
    /// <summary>
    /// Module for building CQRS domains.
    /// </summary>
    public class MessageDirectoryModule : IModule
    {
        readonly DomainAssemblyScanner _scanner = new DomainAssemblyScanner();
        IMethodContextManager _contextManager;
        MethodInvokerHint _hint;
        Action<IComponentRegistry> _actionReg;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDirectoryModule"/> class.
        /// </summary>
        public MessageDirectoryModule()
        {
            HandlerSample<IConsume<IMessage>>(a => a.Consume(null));
            ContextFactory((envelope, message) => new MessageContext(envelope.EnvelopeId, message.Index, envelope.CreatedOnUtc));
        }


        public void ContextFactory<TContext>(Func<ImmutableEnvelope,ImmutableMessage, TContext> manager)
            where TContext : class 
        {
            var instance = new MethodContextManager<TContext>(manager);
            _contextManager = instance;
            _actionReg = c => c.Register<Func<TContext>>(instance.Get);
        }

		public void HandlerSample<THandler>(Expression<Action<THandler>> action)
		{
		    if (action == null) throw new ArgumentNullException("action");
		    _hint = MethodInvokerHint.FromConsumerSample(action);
		}


        /// <summary>
        /// Includes assemblies of the specified types into the discovery process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public MessageDirectoryModule InAssemblyOf<T>()
        {
            _scanner.WithAssemblyOf<T>();
            return this;
        }

        sealed class SerializationList : IKnowSerializationTypes
        {
            public SerializationList(ICollection<Type> types)
            {
                _types = types;
            }

            readonly ICollection<Type> _types;
            public IEnumerable<Type> GetKnownTypes()
            {
                return _types;
            }
        }


        void IModule.Configure(IComponentRegistry container)
        {
            _scanner.Constrain(_hint);
            var mappings = _scanner.Build(_hint.ConsumerTypeDefinition);
            var builder = new MessageDirectoryBuilder(mappings);
            var messages = builder.ListMessagesToSerialize();

            var cb = new ContainerBuilder();
            foreach (var consumer in builder.ListConsumersToActivate())
            {
                cb.RegisterType(consumer);
            }
            cb.Update(container);
            _actionReg(container);
            container.Register(builder);
            container.Register<IKnowSerializationTypes>(new SerializationList(messages));
            var invoker = new MethodInvoker(_hint, _contextManager);
            container.Register<IMethodInvoker>(invoker);
        }

        public sealed class DomainAwareMessageProfiler
        {
            //readonly IDictionary<Type, GetInfoDelegate> _delegates;

            //public DomainAwareMessageProfiler(MessageDirectory directory)
            //{
            //    _delegates = BuildFrom(directory);
            //}


            //public string GetReadableMessageInfo(UnpackedMessage message)
            //{
            //    GetInfoDelegate value;

            //    if (_delegates.TryGetValue(message.ContractType, out value))
            //    {
            //        return value(message);
            //    }
            //    return GetDefaultInfo(message);
            //}

            //static string GetDefaultInfo(UnpackedMessage message)
            //{
            //    var contract = message.ContractType.Name;
            //    return message
            //        .GetState<CloudQueueMessage>()
            //        .Convert(s => contract + " - " + s.Id, contract);
            //}

            //static IDictionary<Type, GetInfoDelegate> BuildFrom(MessageDirectory directory)
            //{
            //    var delegates = new Dictionary<Type, GetInfoDelegate>();
            //    foreach (var message in directory.Messages)
            //    {
            //        if (message.MessageType.IsInterface)
            //            continue;

            //        var type = message.MessageType;
            //        var hasStringOverride = type.GetMethod("ToString").DeclaringType != typeof (object);

            //        if (hasStringOverride)
            //        {
            //            delegates.Add(type, m => m.Content.ToString());
            //        }
            //        else
            //        {
            //            delegates.Add(type, GetDefaultInfo);
            //        }
            //    }
            //    return delegates;
            //}

            //delegate string GetInfoDelegate(UnpackedMessage message);
        }
    }
}