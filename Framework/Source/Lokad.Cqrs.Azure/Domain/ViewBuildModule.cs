using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Default;
using Lokad.Cqrs.Storage;
using Lokad.Serialization;
using System.Linq;

namespace Lokad.Cqrs.Domain
{
	public class ViewBuildModule : IModule
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();
		readonly ViewAssemblyScanner _scanner = new ViewAssemblyScanner();

		public void Configure(IComponentRegistry componentRegistry)
		{
			var mappings = _scanner.Build();
			_builder.RegisterInstance(new FixedTypeDiscovery(mappings.ToArray())).As<IKnowSerializationTypes>();
			_builder.RegisterType<EntityStorage>().As<IEntityReader, IEntityWriter>().SingleInstance();

			_builder.Update(componentRegistry);
		}

		/// <summary>
		/// Uses default interfaces and conventions.
		/// </summary>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public ViewBuildModule WithDefaultInterfaces()
		{
			WhereEntitiesAre<IEntity>();
			return this;
		}


		/// <summary>
		/// <para>Specifies custom rule for finding views - where they derive from the provided interface. </para>
		/// <para>By default we expect messages to derive from <see cref="IMessage"/>.</para>
		/// </summary>
		/// <typeparam name="TInterface">The type of the interface.</typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public ViewBuildModule WhereEntitiesAre<TInterface>()
		{
			_scanner.WhereEntities(type =>
				typeof(TInterface).IsAssignableFrom(type)
					&& type.IsAbstract == false);
			_scanner.WithAssemblyOf<TInterface>();

			return this;
		}

		/// <summary>
		/// Includes assemblies of the specified types into the discovery process
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public ViewBuildModule InAssemblyOf<T>()
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
		public ViewBuildModule InAssemblyOf<T1, T2>()
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
		public ViewBuildModule InAssemblyOf<T1, T2, T3>()
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
		public ViewBuildModule InCurrentAssembly()
		{
			_scanner.WithAssembly(Assembly.GetCallingAssembly());

			return this;
		}

	}


	public sealed class FixedTypeDiscovery : IKnowSerializationTypes
	{
		readonly Type[] _types;

		public FixedTypeDiscovery(Type[] types)
		{
			_types = types;
		}

		public IEnumerable<Type> GetKnownTypes()
		{
			return _types;
		}
	}
}