#region (c) 2010-2011 Lokad. New BSD License

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Dispatch;
using NHibernate;
using NHibernate.ByteCode.Castle;
using NHibernate.Cfg;

namespace Sample_03.Worker
{
    public sealed class NHibernateModule : IModule
    {
        readonly ContainerBuilder _builder = new ContainerBuilder();
        readonly Configuration _config;

        public NHibernateModule(Configuration configuration)
        {
            _config = configuration;
        }

        readonly string _proxyFactoryClass = typeof(ProxyFactoryFactory).FullName;

        void ApplyProxyFactory(Configuration instance)
        {
            // this is needed to work properly in merged Dll scenarios
            if (!string.IsNullOrEmpty(_proxyFactoryClass))
            {
                instance.Properties["proxyfactory.factory_class"] = _proxyFactoryClass;
            }
        }


        static ISession ComposeSession(IComponentContext c)
        {
            var session = c.Resolve<ISessionFactory>().OpenSession();
            session.FlushMode = FlushMode.Commit;
            return session;
        }

        void IModule.Configure(IComponentRegistry componentRegistry)
        {
            ApplyProxyFactory(_config);
            _builder.RegisterInstance(_config.BuildSessionFactory());

            // Session is created for the entire message batch
            _builder.Register(ComposeSession)
                .InstancePerMatchingLifetimeScope(DispatchLifetimeScopeTags.MessageEnvelopeScopeTag);
            _builder.Update(componentRegistry);
        }
    }
}