#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Directory;
using Lokad.Default;
using Lokad.Serialization;

namespace Lokad.Cqrs.Domain
{
	/// <summary>
	/// Module for building CQRS domains.
	/// </summary>
	public class DomainBuildModule : IModule
	{
		readonly DomainAssemblyScanner _scanner = new DomainAssemblyScanner();
		readonly ContainerBuilder _builder;
		InvocationHint _hint;
		Func<MessageAttributesContract, object> _contextFactory;

		/// <summary>
		/// Initializes a new instance of the <see cref="DomainBuildModule"/> class.
		/// </summary>
		public DomainBuildModule()
		{
			_builder = new ContainerBuilder();
			// default settings
			InvocationHandlerBySample<IConsume<IMessage>>(a => a.Consume(null, null));
			ContextFactory(ac => new MessageDetail(ac.GetAttributeString(MessageAttributeTypeContract.Identity).Value));
		}



		/// <summary>
		/// Specifies custom lookup rule for the consumers
		/// </summary>
		/// <param name="customFilterForConsumers">The custom filter for consumers.</param>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public DomainBuildModule WhereConsumers(Func<Type, bool> customFilterForConsumers)
		{
			_scanner.WhereConsumers(customFilterForConsumers);
			return this;
		}

		/// <summary>
		/// Specifies custom lookup rule for the messages.
		/// </summary>
		/// <param name="customFilterForMessages">The custom filter for messages.</param>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public DomainBuildModule WhereMessages(Func<Type, bool> customFilterForMessages)
		{
			_scanner.WhereMessages(customFilterForMessages);
			return this;
		}

		/// <summary>
		/// Includes assemblies of the specified types into the discovery process
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public DomainBuildModule InAssemblyOf<T>()
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
		public DomainBuildModule InAssemblyOf<T1,T2>()
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
		public DomainBuildModule InAssemblyOf<T1, T2, T3>()
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
		public DomainBuildModule InCurrentAssembly()
		{
			_scanner.WithAssembly(Assembly.GetCallingAssembly());
			
			return this;
		}


		void IModule.Configure(IComponentRegistry componentRegistry)
		{
			_scanner.IncludeSystemMessages = true;

			// add implicit filters
			
			_scanner.Constrain(_hint);
			var mappings = _scanner.Build(_hint.ConsumerTypeDefinition);



			var handler = new InvocationHandler(_hint, _contextFactory);
			var directoryBuilder = new MessageDirectoryBuilder(mappings, handler);
			var directory = directoryBuilder.BuildDirectory(m => true);


			foreach (var consumer in directory.Consumers)
			{
				if (!consumer.ConsumerType.IsAbstract)
				{
					_builder.RegisterType(consumer.ConsumerType);
				}
			}

			_builder.RegisterInstance(directoryBuilder).As<IMessageDirectoryBuilder>();
			_builder.RegisterInstance(directory).As<IMessageDirectory, IKnowSerializationTypes>();

			_builder
				.RegisterType<DomainAwareMessageProfiler>()
				.As<IMessageProfiler>()
				.SingleInstance();

			_builder.Update(componentRegistry);
		}

		

		

		public DomainBuildModule InvocationHandlerBySample<THandler>(Expression<Action<THandler>> action)
		{
			_hint = InvocationHint.FromConsumerSample(action);
			return this;
		}

		public DomainBuildModule ContextFactory<TResult>(Func<MessageAttributesContract,TResult> result)
		{
			if (!_hint.HasContext)
			{
				throw new InvalidOperationException("Declaring interface type does not have context parameter");
			}
			if (!_hint.MessageContextType.Value.IsAssignableFrom(typeof(TResult)))
			{
				throw new InvalidOperationException("Passed lambda returns object instance that is not assignable to: " + _hint.MessageContextType.Value);
			}

			_contextFactory = contract => result(contract);
			return this;
		}


		

		public ContainerBuilder Target
		{
			get { return _builder; }
		}
	}
}