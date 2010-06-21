#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Settings;
using NHibernate;
using NHibernate.Bytecode;
using NHibernate.ByteCode.Castle;
using NHibernate.Cfg;

namespace Lokad.Cqrs.NHibernate
{
	public sealed class NHibernateModule : Module
	{
		Action<ContainerBuilder> _registrations = builder => { };

		public NHibernateModule()
		{
			WithProxyFactory<ProxyFactoryFactory>();
		}


		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => c.Resolve<Configuration>().BuildSessionFactory()).SingleInstance();
			builder.Register(ComposeSession).InstancePerLifetimeScope();
			builder.RegisterType<NHibernateStarter>().As<IStartable>();
		}


		string _proxyFactoryClass;

		public NHibernateModule WithProxyFactory<TFactory>()
			where TFactory : IProxyFactoryFactory
		{
			_proxyFactoryClass = typeof (TFactory).FullName;
			return this;
		}

		public NHibernateModule WithConfiguration(Configuration config)
		{
			ApplyProxyFactory(config);
			_registrations += builder => builder.RegisterInstance(config);
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

		public NHibernateModule WithConfiguration(string connection, Func<string, Configuration> config)
		{
			_registrations += builder => builder
				.Register(c => ComposeConfiguration(c, connection, config))
				.SingleInstance();
			return this;
		}

		static ISession ComposeSession(IComponentContext c)
		{
			var session = c.Resolve<ISessionFactory>().OpenSession();
			session.FlushMode = FlushMode.Commit;

			return session;
		}
	}
}