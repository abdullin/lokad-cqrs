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
        readonly ContainerBuilder _builder;
        IMethodContextManager _contextManager;
        MethodInvokerHint _hint;
        Action<ContainerBuilder> _actionReg;

        
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDirectoryModule"/> class.
        /// </summary>
        public MessageDirectoryModule()
        {
            _builder = new ContainerBuilder();

            HandlerSample<IConsume<IMessage>>(a => a.Consume(null));
            ContextFactory((envelope, message) => new MessageContext(envelope.EnvelopeId, message.Index, envelope.CreatedOnUtc));
        }


        public void ContextFactory<TContext>(Func<ImmutableEnvelope,ImmutableMessage, TContext> manager)
            where TContext : class 
        {
            var instance = new MethodContextManager<TContext>(manager);
            _contextManager = instance;
            _actionReg = builder => builder.RegisterInstance<Func<TContext>>(instance.Get);
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

        /// <summary>
        /// Includes assemblies of the specified types into the discovery process
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns>
        /// same module instance for chaining fluent configurations
        /// </returns>
        public MessageDirectoryModule InAssemblyOf<T1, T2>()
        {
            _scanner.WithAssemblyOf<T1>();
            _scanner.WithAssemblyOf<T2>();
            return this;
        }

        /// <summary>
        /// Includes assemblies of the specified types into the discovery process
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns>
        /// same module instance for chaining fluent configurations
        /// </returns>
        public MessageDirectoryModule InAssemblyOf<T1, T2, T3>()
        {
            _scanner.WithAssemblyOf<T1>();
            _scanner.WithAssemblyOf<T2>();
            _scanner.WithAssemblyOf<T3>();

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


        void IModule.Configure(IComponentRegistry componentRegistry)
        {
            var handler = new MethodInvoker(_hint, _contextManager);

            _scanner.Constrain(_hint);
            var mappings = _scanner.Build(_hint.ConsumerTypeDefinition);
            var builder = new MessageDirectoryBuilder(mappings);
            var messages = builder.ListMessagesToSerialize();

            foreach (var consumer in builder.ListConsumersToActivate())
            {
                _builder.RegisterType(consumer);
            }
            _actionReg(_builder);
            _builder.RegisterInstance(builder).As<MessageDirectoryBuilder>();
            _builder.RegisterInstance(new SerializationList(messages)).As<IKnowSerializationTypes>();
            _builder.RegisterInstance(handler).As<IMethodInvoker>();
            _builder.Update(componentRegistry);
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