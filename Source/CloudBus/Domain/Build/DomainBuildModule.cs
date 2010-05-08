using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using Bus2.Serialization;
using Module = Autofac.Module;

namespace Bus2.Domain.Build
{
	public class DomainBuildModule : Module
	{
		readonly HashSet<Assembly> _assemblies = new HashSet<Assembly>();
		Action<ContainerBuilder> _registerSerializer;
		readonly MessageDirectoryBuilder _directoryBuilder = new MessageDirectoryBuilder();

		MethodInfo _consumingMethod;
		Func<Type, bool> _messageSelector;

		public DomainBuildModule()
		{
			_directoryBuilder.LoadSystemMessages();
			UseDataContractSerializer();
			ConsumeMethodSample<IConsumeMessage<string>>(i => i.Consume(""));
		}

		public DomainBuildModule MessagesInherit<TInterface>()
		{
			_messageSelector = type =>
				typeof (TInterface).IsAssignableFrom(type)
					&& type.IsAbstract == false;
			InAssemblyOf<TInterface>();
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
			_assemblies.Add(typeof (T).Assembly);
			return this;
		}

		public DomainBuildModule ConsumeMethodSample<THandler>(Expression<Action<THandler>> expression)
		{
			_consumingMethod = MessageReflectionUtil.ExpressConsumer(expression);
			InAssemblyOf<THandler>();
			return this;
		}

		static bool IsUserAssembly(Assembly a)
		{
			if (string.IsNullOrEmpty(a.FullName))
				return false;
			if (a.FullName.StartsWith("System."))
				return false;
			return true;
		}

		public DomainBuildModule ConsumerInterfaceHas<TAttribute>()
			where TAttribute : Attribute
		{
			var methods = AppDomain
				.CurrentDomain
				.GetAssemblies()
				.Where(IsUserAssembly)
				.SelectMany(e => e.GetTypes().Where(t => t.IsPublic))
				.Where(t => t.IsInterface)
				.Where(t => t.IsGenericTypeDefinition)
				.SelectMany(t => t.GetMethods())
				.Where(t => t.ContainsGenericParameters)
				.Where(t => t.IsDefined(typeof (TAttribute), false))
				.ToArray();

			if (methods.Length == 0)
				throw new InvalidOperationException("Was not able to find any generic methods marked with the attribute");
			if (methods.Length > 1)
				throw new InvalidOperationException("Only one method has to be marked with the attribute");

			InAssemblyOf<TAttribute>();

			_consumingMethod = methods.First();

			return this;
		}


		protected override void Load(ContainerBuilder builder)
		{
			if (!_assemblies.Any())
			{
				throw new InvalidOperationException("Domain assemblies should be referenced. See InAssemblyOf");
			}

			_directoryBuilder.LoadDomainMessagesAndConsumers(
				_assemblies,
				_consumingMethod.DeclaringType,
				_messageSelector);

			var directory = _directoryBuilder.BuildDirectory(_consumingMethod);
			foreach (var consumer in directory.Consumers)
			{
				builder.RegisterType(consumer.ConsumerType);
			}
			builder.RegisterInstance(directory);

			_registerSerializer(builder);
		}
	}
}