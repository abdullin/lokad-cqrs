#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Feature.Logging;

// ReSharper disable UnusedMethodReturnValue.Global

namespace Lokad.Cqrs.Build.Engine
{
    /// <summary>
    /// Fluent API for creating and configuring <see cref="CloudEngineHost"/>
    /// </summary>
    public class CloudEngineBuilder : BuildSyntaxHelper
    {
        readonly HashSet<IModule> _moduleEnlistments = new HashSet<IModule>();

        bool IsEnlisted<TModule>() where TModule : IModule
        {
            return _moduleEnlistments.Count(x => x is TModule) > 0;
        }

        public void Enlist<TModule>(Action<TModule> config) where TModule : IModule, new()
        {
            var m = new TModule();
            config(m);
            _moduleEnlistments.Add(m);
        }

        public void Enlist(IModule module)
        {
            _moduleEnlistments.Add(module);
        }


        /// <summary>
        /// Configures the message domain for the instance of <see cref="CloudEngineHost"/>.
        /// </summary>
        /// <param name="config">configuration syntax.</param>
        /// <returns>same builder for inline multiple configuration statements</returns>
        public CloudEngineBuilder DomainIs(Action<ModuleForMessageDirectory> config)
        {
            Enlist(config);
            return this;
        }

        protected readonly ContainerBuilder Builder = new ContainerBuilder();

        public CloudEngineBuilder Advanced(Action<ContainerBuilder> build)
        {
            build(Builder);
            return this;
        }

        public CloudEngineBuilder Serialization(Action<ModuleForSerialization> config)
        {
            var m = new ModuleForSerialization();
            config(m);
            Enlist(m);
            return this;
        }

        public CloudEngineBuilder EnlistObserver(IObserver<ISystemEvent> observer)
        {

            Builder.RegisterInstance(observer);
            return this;
        }

        public CloudEngineBuilder EnlistObserver<TObserver>() where TObserver : IObserver<ISystemEvent>
        {
            Builder.RegisterType<TObserver>().As<IObserver<ISystemEvent>>().SingleInstance();
            return this;
        }

        public CloudEngineBuilder Memory(Action<MemoryModule> configure)
        {
            var m = new MemoryModule();
            configure(m);
            Builder.RegisterModule(m);
            return this;
        }

        /// <summary>
        /// Builds this <see cref="CloudEngineHost"/>.
        /// </summary>
        /// <returns>new instance of cloud engine host</returns>
        public CloudEngineHost Build()
        {
            // nonconditional registrations
            // System presets
            InnerSystemRegisterObservations();

            Builder.RegisterType<DispatcherProcess>();
            Builder.RegisterType<MessageDuplicationManager>().SingleInstance();

            // some defaults
            Builder.RegisterType<CloudEngineHost>().SingleInstance();

            // conditional registrations and defaults
            if (!IsEnlisted<ModuleForMessageDirectory>())
            {
                DomainIs(m => m.InUserAssemblies());
            }
            if (!IsEnlisted<ModuleForSerialization>())
            {
                Serialization(x => x.UseDataContractSerializer());
            }


            foreach (var module in _moduleEnlistments)
            {
                Builder.RegisterModule(module);
            }


            var container = Builder.Build();
            var host = container.Resolve<CloudEngineHost>(TypedParameter.From(container));
            host.Initialize();
            return host;
        }

        void InnerSystemRegisterObservations()
        {
            Builder.RegisterType<ReactiveSystemObserverAdapter>().SingleInstance().As<ISystemObserver>();
            Builder.RegisterCallback(ci =>
                {
                    var service = new TypedService(typeof(IObserver<ISystemEvent>));
                    if (!ci.IsRegistered(service))
                    {
                        var builder = new ContainerBuilder();
                        builder.RegisterType<ImmediateTracingObserver>()
                            .As<IObserver<ISystemEvent>>()
                            .SingleInstance();
                        builder.Update(ci);
                    }
                });
        }
    }
}