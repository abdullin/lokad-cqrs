#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lokad.Cqrs.Core.Directory
{
    public sealed class DomainAssemblyScanner
    {
        readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();
        readonly HashSet<Predicate<Type>> _handlerSelector = new HashSet<Predicate<Type>>();
        readonly HashSet<Predicate<Type>> _serializableSelector = new HashSet<Predicate<Type>>();
        MethodInfo _consumingMethod;

        public MethodInfo ConsumingMethod
        {
            get { return _consumingMethod; }
        }

        public DomainAssemblyScanner WithAssemblyOf<T>()
        {
            _assemblies.Add(typeof (T).Assembly);
            return this;
        }

        public DomainAssemblyScanner WithAssembly(Assembly assembly)
        {
            _assemblies.Add(assembly);
            return this;
        }

        public DomainAssemblyScanner WhereMessages(Predicate<Type> filter)
        {
            _serializableSelector.Add(filter);
            return this;
        }

        public DomainAssemblyScanner WhereConsumers(Predicate<Type> filter)
        {
            _handlerSelector.Add(filter);
            return this;
        }

        public DomainAssemblyScanner ConsumerMethodSample<THandler>(Expression<Action<THandler>> expression)
        {
            _consumingMethod = MessageReflectionUtil.ExpressConsumer(expression);
            return this;
        }

        public static bool IsUserAssembly(Assembly a)
        {
            if (string.IsNullOrEmpty(a.FullName))
                return false;
            if (a.FullName.StartsWith("System."))
                return false;
            if (a.FullName.StartsWith("Microsoft."))
                return false;
            return true;
        }

        public IEnumerable<MessageMapping> Build()
        {
            if (null == _consumingMethod)
                throw new InvalidOperationException("Consuming method has not been defined");

            if (!_assemblies.Any())
                throw new InvalidOperationException("There are no assemblies to scan");

            var types = _assemblies
                .SelectMany(a => a.GetExportedTypes())
                .ToList();

            var messageTypes = types
                .Where(t => !_serializableSelector.Any(x => !x(t)))
                .ToArray();

            var consumerTypes = types
                .Where(t => !_handlerSelector.Any(x => !x(t)))
                .Where(t => !t.IsGenericType)
                .ToArray();

            var consumingDirectly = consumerTypes
                .SelectMany(consumerType =>
                    GetConsumedMessages(consumerType, _consumingMethod.DeclaringType)
                        .Select(messageType => new MessageMapping(consumerType, messageType, true)))
                .ToArray();

            var consumingIndirectly = consumingDirectly
                .SelectMany(mm => messageTypes
                    .Where(t => mm.Message.IsAssignableFrom(t))
                    .Where(t => mm.Message != t)
                    .Select(t => new MessageMapping(mm.Consumer, t, false)))
                .ToArray();


            var result = new HashSet<MessageMapping>();

            foreach (var m in consumingDirectly)
            {
                result.Add(m);
            }
            foreach (var m in consumingIndirectly)
            {
                result.Add(m);
            }

            var allMessages = new HashSet<Type>(result.Select(m => m.Message));

            foreach (var messageType in messageTypes)
            {
                if (!allMessages.Contains(messageType))
                {
                    allMessages.Add(messageType);
                    result.Add(new MessageMapping(typeof (MessageMapping.BusNull), messageType, true));
                }
            }


            return result;
        }

        static IEnumerable<Type> GetConsumedMessages(Type consumerType, Type consumerTypeDefinition)
        {
            var interfaces = consumerType
                .GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(t => t.GetGenericTypeDefinition() == consumerTypeDefinition);

            foreach (var consumerInterface in interfaces)
            {
                var argument = consumerInterface.GetGenericArguments()[0];

                yield return argument;
            }
        }
    }
}