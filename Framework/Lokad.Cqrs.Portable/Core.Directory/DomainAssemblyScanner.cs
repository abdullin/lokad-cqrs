#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lokad.Cqrs.Core.Directory
{
    public sealed class DomainAssemblyScanner
    {
        readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();
        readonly HashSet<Predicate<Type>> _handlerSelector = new HashSet<Predicate<Type>>();
        readonly HashSet<Predicate<Type>> _serializableSelector = new HashSet<Predicate<Type>>();

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


        public static bool IsUserAssembly(Assembly a)
        {
            if (string.IsNullOrEmpty(a.FullName))
                return false;

            var prefixes = new[]
                {
                    "System", "Microsoft", "nunit", "JetBrains", "Autofac", "mscorlib"
                };

            foreach (var prefix in prefixes)
            {
                if (a.FullName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            return true;
        }

        public void Constrain(MethodInvokerHint hint)
        {
            WhereMessages(t => hint.MessageInterface.IsAssignableFrom(t));
            WhereConsumers(type => type.GetInterfaces().Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == hint.ConsumerTypeDefinition));
        }


        public IEnumerable<MessageMapping> Build(Type consumerInterfaceDefinition)
        {
            if (!_assemblies.Any())
            {
                var userAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(IsUserAssembly);
                foreach (var userAssembly in userAssemblies)
                {
                    _assemblies.Add(userAssembly);
                }
            }


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
                    ListMessagesConsumedByInterfaces(consumerType, consumerInterfaceDefinition)
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

        static IEnumerable<Type> ListMessagesConsumedByInterfaces(Type consumerType, Type consumerTypeDefinition)
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