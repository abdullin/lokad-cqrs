#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Default;
using Lokad.Cqrs.Serialization;

namespace Lokad.Cqrs.Domain.Build
{
	public class DomainBuildModule : IModule, ISyntax<ContainerBuilder>
	{
		readonly MessageAssemblyScanner _scanner = new MessageAssemblyScanner();
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public DomainBuildModule()
		{
			//_messageScanner.LoadSystemMessages();

			UseDataContractSerializer();
		}

		public DomainBuildModule WithDefaultInterfaces()
		{
			ConsumerMethodSample<IConsume<IMessage>>(i => i.Consume(null));
			MessagesInherit<IMessage>();
			ConsumersInherit<IConsumeMessage>();
			return this;
		}

		public DomainBuildModule ConsumerMethodSample<THandler>(Expression<Action<THandler>> expression)
		{
			_scanner.ConsumerMethodSample(expression);
			return this;
		}

		public DomainBuildModule MessagesInherit<TInterface>()
		{
			_scanner.WhereMessages(type =>
				typeof (TInterface).IsAssignableFrom(type)
					&& type.IsAbstract == false);
			_scanner.WithAssemblyOf<TInterface>();

			return this;
		}

		public DomainBuildModule ConsumersInherit<TInterface>()
		{
			_scanner.WhereConsumers(type =>
				typeof (TInterface).IsAssignableFrom(type)
					&& type.IsAbstract == false);
			_scanner.WithAssemblyOf<TInterface>();
			return this;
		}

		public DomainBuildModule UseDataContractSerializer()
		{
			_builder
				.RegisterType<DataContractMessageSerializer>()
				.As<IMessageSerializer>()
				.SingleInstance();
			return this;
		}

		public DomainBuildModule UseBinarySerializer()
		{
			_builder
				.RegisterType<BinaryMessageSerializer>()
				.As<IMessageSerializer>()
				.SingleInstance();
			return this;
		}

		public DomainBuildModule InAssemblyOf<T>()
		{
			_scanner.WithAssemblyOf<T>();
			return this;
		}

		public DomainBuildModule InCurrentAssembly()
		{
			_scanner.WithAssembly(Assembly.GetCallingAssembly());
			return this;
		}

		

		public void Configure(IComponentRegistry componentRegistry)
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

			_builder.RegisterInstance(directoryBuilder).As<IMessageDirectoryBuilder>();
			_builder.RegisterInstance(directory);

			_builder
				.RegisterType<DomainAwareMessageProfiler>()
				.As<IMessageProfiler>()
				.SingleInstance();

			_builder.Update(componentRegistry);
		}

		public ContainerBuilder Target
		{
			get { return _builder; }
		}
	}
}