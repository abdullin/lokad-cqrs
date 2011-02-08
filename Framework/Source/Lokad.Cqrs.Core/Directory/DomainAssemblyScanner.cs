#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Directory
{
	public sealed class DomainAssemblyScanner
	{
		readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();
		readonly Filter<Type> _handlerSelector = new Filter<Type>();
		readonly Filter<Type> _serializableSelector = new Filter<Type>();
		
		
		public bool IncludeSystemMessages { get; set; }

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

		public DomainAssemblyScanner WhereMessages(Func<Type, bool> filter)
		{
			_serializableSelector.Where(filter);
			return this;
		}

		public DomainAssemblyScanner WhereConsumers(Func<Type, Boolean> filter)
		{
			_handlerSelector.Where(filter);
			return this;
		}
		//public sealed 

		static bool IsUserAssembly(Assembly a)
		{
			if (string.IsNullOrEmpty(a.FullName))
				return false;
			if (a.FullName.StartsWith("System."))
				return false;
			if (a.FullName.StartsWith("Microsoft."))
				return false;
			return true;
		}

		public IEnumerable<MessageMapping> GetSystemMessages()
		{
			return Assembly
				.GetExecutingAssembly()
				.GetTypes()
				.Where(t => t.IsPublic)
				.Where(t => t.IsDefined(typeof (DataContractAttribute), false))
				.Select(message => new MessageMapping(typeof (MessageMapping.BusSystem), message, false));
		}

		
		public DomainAssemblyScanner Constrain(InvocationHint hint)
		{
			WhereMessages(t => hint.MessageInterface.IsAssignableFrom(t));
			WhereConsumers(type => type.GetInterfaces().Where(i => i.IsGenericType)
				.Any(i => i.GetGenericTypeDefinition() == hint.ConsumerTypeDefinition));

			return this;
		}

		public IEnumerable<MessageMapping> Build(Type consumerInterfaceDefinition)
		{
			if (consumerInterfaceDefinition == null) throw new ArgumentNullException("consumerInterfaceDefinition");

			if (!_assemblies.Any())
				throw new InvalidOperationException("There are no assemblies to scan");

			

			var types = _assemblies
				.SelectMany(a => a.GetExportedTypes())
				.ToList();

			var messageTypes = _serializableSelector
				.Apply(types)
				.ToArray();

			var consumerTypes = _handlerSelector
				.Apply(types)
				.Where(t => !t.IsGenericType)
				.ToArray();

			var consumingDirectly = consumerTypes
				.SelectMany(consumerType => ListMessagesConsumedByInterfaces(consumerType, consumerInterfaceDefinition).Select(messageType => new MessageMapping(consumerType, messageType, true)))
				.ToArray();

			var consumingIndirectly = consumingDirectly
				.SelectMany(mm => messageTypes
					.Where(t => mm.Message.IsAssignableFrom(t))
					.Where(t => mm.Message != t)
					.Select(t => new MessageMapping(mm.Consumer, t, false)))
				.ToArray();


			var result = new HashSet<MessageMapping>();

			result.AddRange(consumingDirectly);
			result.AddRange(consumingIndirectly);

			if (IncludeSystemMessages)
			{
				result.AddRange(GetSystemMessages());
			}

			var allMessages = result.ToSet(m => m.Message);

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