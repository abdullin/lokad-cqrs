#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory.Default;

namespace Lokad.Cqrs.Core.Directory
{
    /// <summary>
    /// Module for building CQRS domains.
    /// </summary>
    public class ModuleForMessageDirectory : IModule
    {
        readonly DomainAssemblyScanner _scanner = new DomainAssemblyScanner();
        readonly ContainerBuilder _builder;
        MethodInvokerHint _hint;

        Func<MessageEnvelope, MessageItem, object> _contextFactory;
        Type _contextFactoryType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleForMessageDirectory"/> class.
        /// </summary>
        public ModuleForMessageDirectory()
        {
            _builder = new ContainerBuilder();

            InvocationHandlerBySample<IConsume<IMessage>>(a => a.Consume(null, null));
            ContextFactory((envelope, item) => new MessageDetail(envelope.EnvelopeId));
 
        }

        public void ContextFactory<TResult>(Func<MessageEnvelope,MessageItem, TResult> result)
		{
            _contextFactory = (envelope, item) => result(envelope, item);
            _contextFactoryType = typeof (TResult);
		}



		public void InvocationHandlerBySample<THandler>(Expression<Action<THandler>> action)
		{
			_hint = MethodInvokerHint.FromConsumerSample(action);
		}

        /// <summary>
        /// <para>Specifies custom rule for finding messages - where they derive from the provided interface. </para>
        /// <para>By default we expect messages to derive from <see cref="IMessage"/>.</para>
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public ModuleForMessageDirectory WhereMessagesAre<TInterface>()
        {
            _scanner.WhereMessages(type =>
                typeof (TInterface).IsAssignableFrom(type)
                    && type.IsAbstract == false);
            _scanner.WithAssemblyOf<TInterface>();

            return this;
        }


        /// <summary>
        /// <para>Specifies custom rule for finding message consumers - where they derive from the provided interface. </para>
        /// <para>By default we expect consumers to derive from <see cref="IConsumeMessage"/>.</para>
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public ModuleForMessageDirectory WhereConsumersAre<TInterface>()
        {
            _scanner.WhereConsumers(type =>
                typeof (TInterface).IsAssignableFrom(type)
                    && type.IsAbstract == false);
            _scanner.WithAssemblyOf<TInterface>();
            return this;
        }

        /// <summary>
        /// Specifies custom lookup rule for the consumers
        /// </summary>
        /// <param name="customFilterForConsumers">The custom filter for consumers.</param>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public ModuleForMessageDirectory WhereConsumers(Predicate<Type> customFilterForConsumers)
        {
            _scanner.WhereConsumers(customFilterForConsumers);
            return this;
        }

        /// <summary>
        /// Specifies custom lookup rule for the messages.
        /// </summary>
        /// <param name="customFilterForMessages">The custom filter for messages.</param>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public ModuleForMessageDirectory WhereMessages(Predicate<Type> customFilterForMessages)
        {
            _scanner.WhereMessages(customFilterForMessages);
            return this;
        }

        /// <summary>
        /// Includes assemblies of the specified types into the discovery process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>same module instance for chaining fluent configurations</returns>
        public ModuleForMessageDirectory InAssemblyOf<T>()
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
        public ModuleForMessageDirectory InAssemblyOf<T1, T2>()
        {
            _scanner.WithAssemblyOf<T1>();
            _scanner.WithAssemblyOf<T2>();
            return this;
        }


        public ModuleForMessageDirectory InUserAssemblies()
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
        public ModuleForMessageDirectory InAssemblyOf<T1, T2, T3>()
        {
            _scanner.WithAssemblyOf<T1>();
            _scanner.WithAssemblyOf<T2>();
            _scanner.WithAssemblyOf<T3>();

            return this;
        }

        /// <summary>
        /// Includes the current assembly in the discovery
        /// </summary>
        /// same module instance for chaining fluent configurations
        public ModuleForMessageDirectory InCurrentAssembly()
        {
            _scanner.WithAssembly(Assembly.GetCallingAssembly());

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