#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Directory;
using Lokad.Default;

namespace Lokad.Cqrs.Domain
{
	/// <summary>
	/// Module for building CQRS domains.
	/// </summary>
	public class DomainBuildModule : IModule
	{
		readonly DomainAssemblyScanner _scanner = new DomainAssemblyScanner();
		readonly ContainerBuilder _builder;

		/// <summary>
		/// Initializes a new instance of the <see cref="DomainBuildModule"/> class.
		/// </summary>
		public DomainBuildModule()
		{
			_builder = new ContainerBuilder();
		}

		/// <summary>
		/// Uses default interfaces and conventions.
		/// </summary>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public DomainBuildModule WithDefaultInterfaces()
		{
			ConsumerMethodSample<IConsume<IMessage>>(i => i.Consume(null));
			WhereMessagesAre<IMessage>();
			WhereConsumersAre<IConsumeMessage>();
			
			return this;
		}

		/// <summary>
		/// Provides sample of the custom consuming method expression. By default we expect it to be <see cref="IConsume{TMessage}.Consume"/>.
		/// </summary>
		/// <typeparam name="THandler">The type of the handler.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public DomainBuildModule ConsumerMethodSample<THandler>(Expression<Action<THandler>> expression)
		{
			_scanner.ConsumerMethodSample(expression);
			return this;
		}

		/// <summary>
		/// <para>Specifies custom rule for finding messages - where they derive from the provided interface. </para>
		/// <para>By default we expect messages to derive from <see cref="IMessage"/>.</para>
		/// </summary>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public DomainBuildModule WhereMessagesAre<TInterface>()
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
		public DomainBuildModule WhereConsumersAre<TInterface>()
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
			var mappings = _scanner.Build();

			var directoryBuilder = new MessageDirectoryBuilder(mappings, _scanner.ConsumingMethod.Name);

			var directory = directoryBuilder.BuildDirectory(m => true);


			foreach (var consumer in directory.Consumers)
			{
				if (!consumer.ConsumerType.IsAbstract)
				{
					_builder.RegisterType(consumer.ConsumerType);
				}
			}

			_builder.RegisterInstance(directoryBuilder).As<MessageDirectoryBuilder>();
			_builder.RegisterInstance(directory).As<MessageDirectory, IKnowSerializationTypes>();

			_builder
				.RegisterType<DomainAwareMessageProfiler>()
				.As<IMessageProfiler>()
				.SingleInstance();

			_builder.Update(componentRegistry);
		}
	}
}