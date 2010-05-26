#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq.Expressions;
using Autofac;
using CloudBus.Serialization;

namespace CloudBus.Domain.Build
{
	public class DomainBuildModule : Module
	{
		readonly AssemblyScanner _builder = new AssemblyScanner();
		Action<ContainerBuilder> _registerSerializer;

		public DomainBuildModule()
		{
			//_messageScanner.LoadSystemMessages();

			UseDataContractSerializer();

			ConsumerMethodSample<IConsumeMessage<string>>(i => i.Consume(""));
		}

		public DomainBuildModule ConsumerMethodSample<THandler>(Expression<Action<THandler>> expression)
		{
			_builder.ConsumerMethodSample(expression);
			return this;
		}

		public DomainBuildModule MessagesInherit<TInterface>()
		{
			_builder.WhereMessages(type =>
				typeof (TInterface).IsAssignableFrom(type)
					&& type.IsAbstract == false);
			_builder.WithAssemblyOf<TInterface>();

			return this;
		}

		public DomainBuildModule ConsumersInherit<TInterface>()
		{
			_builder.WhereConsumers(type =>
				typeof (TInterface).IsAssignableFrom(type)
					&& type.IsAbstract == false);
			_builder.WithAssemblyOf<TInterface>();
			return this;
		}

		public DomainBuildModule UseDataContractSerializer()
		{
			_registerSerializer = (builder) =>
				{
					builder
						.RegisterType<DataContractMessageSerializer>()
						.As<IMessageSerializer>()
						.SingleInstance();
				};
			return this;
		}

		public DomainBuildModule UseBinarySerializer()
		{
			_registerSerializer = (builder) =>
				builder
					.RegisterType<BinaryMessageSerializer>()
					.As<IMessageSerializer>()
					.SingleInstance();
			return this;
		}

		public DomainBuildModule InAssemblyOf<T>()
		{
			_builder.WithAssemblyOf<T>();
			return this;
		}

		protected override void Load(ContainerBuilder builder)
		{
			_builder.IncludeSystemMessages = true;
			var mappings = _builder.Build();

			var directoryBuilder = new MessageDirectoryBuilder(mappings, _builder.ConsumingMethod.Name);

			var directory = directoryBuilder.BuildDirectory(m => true);


			foreach (var consumer in directory.Consumers)
			{
				if (!consumer.ConsumerType.IsAbstract)
				{
					builder.RegisterType(consumer.ConsumerType);
				}
			}

			builder.RegisterInstance(directoryBuilder).As<IMessageDirectoryBuilder>();
			builder.RegisterInstance(directory);

			builder
				.RegisterType<DomainAwareMessageProfiler>()
				.As<IMessageProfiler>()
				.SingleInstance();

			_registerSerializer(builder);
		}
	}
}