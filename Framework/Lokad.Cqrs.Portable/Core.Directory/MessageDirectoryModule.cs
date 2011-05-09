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
        MethodInvokerHint _hint;

        Func<ImmutableEnvelope, ImmutableMessage, object> _contextFactory;
        Type _contextFactoryType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDirectoryModule"/> class.
        /// </summary>
        public MessageDirectoryModule()
        {
            _builder = new ContainerBuilder();

            HandlerSample<IConsume<IMessage>>(a => a.Consume(null, null));
            ContextFactory((envelope, item) => new MessageContext(envelope.EnvelopeId, envelope.CreatedOnUtc));
 
        }

        public void ContextFactory<TResult>(Func<ImmutableEnvelope,ImmutableMessage, TResult> result)
		{
            _contextFactory = (envelope, item) => result(envelope, item);
            _contextFactoryType = typeof (TResult);
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


        public MessageDirectoryModule InUserAssemblies()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.IsNullOrEmpty(assembly.FullName))
                    continue;
                if (assembly.FullName.StartsWith("System."))
                    continue;
                if (assembly.FullName.StartsWith("Microsoft."))
                    continue;
                _scanner.WithAssembly(assembly);
            }
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
            if (_hint.HasContext)
            {
                if (!_hint.MessageContextType.Value.IsAssignableFrom(_contextFactoryType))
                {
                    throw new InvalidOperationException("Passed lambda returns object instance that is not assignable to: " + _hint.MessageContextType.Value);
                }
            }
            
            

            var handler = new MethodInvoker(_contextFactory, _hint);

            _scanner.Constrain(_hint);
            var mappings = _scanner.Build(_hint.ConsumerTypeDefinition);
            var builder = new MessageDirectoryBuilder(mappings);
            var messages = builder.ListMessagesToSerialize();

            foreach (var consumer in builder.ListConsumersToActivate())
            {
                _builder.RegisterType(consumer);
            }

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