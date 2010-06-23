#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using Lokad.Settings;
using NHibernate;
using NHibernate.Bytecode;
using NHibernate.ByteCode.Castle;
using NHibernate.Cfg;

namespace Lokad.Cqrs.NHibernate
{
	public sealed class NHibernateModule : IModule
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public NHibernateModule()
		{
			WithProxyFactory<ProxyFactoryFactory>();
		}
		string _proxyFactoryClass;

		/// <summary>
		/// Specifies custom proxy factory to use
		/// </summary>
		/// <typeparam name="TFactory">The type of the factory.</typeparam>
		/// <returns>same module instance for inlining in fluent configs</returns>
		public NHibernateModule WithProxyFactory<TFactory>()
			where TFactory : IProxyFactoryFactory
		{
			_proxyFactoryClass = typeof (TFactory).FullName;
			return this;
		}

		/// <summary>
		/// Configures module to use NHibernate with the provided configuration
		/// </summary>
		/// <param name="config">The config.</param>
		/// <returns>same module instance for inlining in fluent configs</returns>
		public NHibernateModule WithConfiguration(Configuration config)
		{
			ApplyProxyFactory(config);
			_builder.RegisterInstance(config);
			return this;
		}

		Configuration ComposeConfiguration(IComponentContext context, string key, Func<string, Configuration> config)
		{
			var provider = context.Resolve<ISettingsProvider>();
			var connectionValue = provider
				.GetValue(key)
				.ExposeException("Could not load setting '{0}'", key);

			var instance = config(connectionValue);
			ApplyProxyFactory(instance);
			return instance;
		}

		void ApplyProxyFactory(Configuration instance)
		{
			// this is needed to work properly in merged Dll scenarios
			if (!string.IsNullOrEmpty(_proxyFactoryClass))
			{
				instance.Properties["proxyfactory.factory_class"] = _proxyFactoryClass;
			}
		}
		/// <summary>
		///Configures module to use NHibernate with the connection string looked up by the key
		/// from the application configs
		/// </summary>
		/// <param name="connectionKeyName">The connection key name to use.</param>
		/// <param name="configFactory">The configuration Factoru.</param>
		/// <returns>same module for usage in fluent configurations</returns>
		public NHibernateModule WithConfiguration(string connectionKeyName, Func<string, Configuration> configFactory)
		{
			_builder
				.Register(c => ComposeConfiguration(c, connectionKeyName, configFactory))
				.SingleInstance();
			return this;
		}

		static ISession ComposeSession(IComponentContext c)
		{
			var session = c.Resolve<ISessionFactory>().OpenSession();
			session.FlushMode = FlushMode.Commit;
			return session;
		}

		void IModule.Configure(IComponentRegistry componentRegistry)
		{
			_builder.Register(c => c.Resolve<Configuration>().BuildSessionFactory()).SingleInstance();
			_builder.Register(ComposeSession).InstancePerLifetimeScope();
			_builder.RegisterType<NHibernateStarter>().As<IStartable>();
			_builder.Update(componentRegistry);
		}
	}
}