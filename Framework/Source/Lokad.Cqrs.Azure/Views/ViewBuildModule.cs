using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Storage;
using Lokad.Default;
using Lokad.Serialization;

namespace Lokad.Cqrs.Views
{

	public enum ViewModuleRole
	{
		Writer, Reader
	}
	public class ViewBuildModule : IModule
	{
		readonly ViewModuleRole _role;
		readonly ContainerBuilder _builder = new ContainerBuilder();
		readonly ViewAssemblyScanner _scanner = new ViewAssemblyScanner();

		public string ViewContainer { get; set; }

		

		public ViewBuildModule(ViewModuleRole role)
		{
			_role = role;
			ViewContainer = "cqrs-views";
		}

		public void Configure(IComponentRegistry componentRegistry)
		{
			var mappings = _scanner.Build();
			var discovery = new FixedTypeDiscovery(mappings.ToArray());
			_builder.RegisterInstance(discovery).As<IKnowSerializationTypes>();
			
			// configure view reader/writer based on the role
			switch (_role)
			{
				case ViewModuleRole.Writer:
					_builder.Register(c => new ViewWriter(ComposeStorage(c, ViewContainer))).SingleInstance();
					break;
				case ViewModuleRole.Reader:
					_builder.Register(c => new ViewReader(ComposeStorage(c, ViewContainer))).SingleInstance();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			_builder.Update(componentRegistry);
		}

		EntityStorage ComposeStorage(IComponentContext c, string containerName)
		{
			var root = c.Resolve<IStorageRoot>();
			var container = root.GetContainer(containerName).Create();
			var serializer = c.Resolve<IDataSerializer>();
			var mapper = c.Resolve<IDataContractMapper>();
			return new EntityStorage(container, serializer, mapper);
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
}