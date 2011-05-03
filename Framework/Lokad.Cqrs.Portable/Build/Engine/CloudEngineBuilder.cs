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
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Core.Serialization;

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

        int Count<TModule>() where TModule : IModule
        {
            return _moduleEnlistments.Count(x => x is TModule);
        }

        public void EnlistModule(IModule module)
        {
            _moduleEnlistments.Add(module);
        }

        /// <summary>
        /// Configures the message domain for the instance of <see cref="CloudEngineHost"/>.
        /// </summary>
        /// <param name="config">configuration syntax.</param>
        /// <returns>same builder for inline multiple configuration statements</returns>
        public CloudEngineBuilder Domain(Action<ModuleForMessageDirectory> config)
        {
            var directory = new ModuleForMessageDirectory();
            config(directory);
            EnlistModule(directory);
            return this;
        }

        readonly ContainerBuilder _builder = new ContainerBuilder();

        public CloudEngineBuilder Advanced(Action<ContainerBuilder> build)
        {
            build(_builder);
            return this;
        }

        public CloudEngineBuilder Serialization(Action<ModuleForSerialization> config)
        {
            var m = new ModuleForSerialization();
            config(m);
            EnlistModule(m);
            return this;
        }


        public CloudEngineBuilder EnlistObserver(IObserver<ISystemEvent> observer)
        {
            _builder.RegisterInstance(observer);
            return this;
        }

        public CloudEngineBuilder EnlistObserver<TObserver>() where TObserver : IObserver<ISystemEvent>
        {
            _builder.RegisterType<TObserver>().As<IObserver<ISystemEvent>>().SingleInstance();
            return this;
        }

        public CloudEngineBuilder Memory(Action<MemoryModule> configure)
        {
            var m = new MemoryModule();
            configure(m);
            _builder.RegisterModule(m);
            return this;
        }

        public CloudEngineBuilder Storage(Action<StorageModule> configure)
        {
            var m = new StorageModule();
            configure(m);
            EnlistModule(m);
            return this;
        }

        public bool DisableDefaultObserver { get; set; }

        /// <summary>
        /// Builds this <see cref="CloudEngineHost"/>.
        /// </summary>
        /// <returns>new instance of cloud engine host</returns>
        public CloudEngineHost Build()
        {
            // nonconditional registrations
            // System presets
            InnerSystemRegisterObservations();

            _builder.RegisterType<DispatcherProcess>();
            _builder.RegisterType<MessageDuplicationManager>().SingleInstance();

            // some defaults
            _builder.RegisterType<CloudEngineHost>().SingleInstance();

            // conditional registrations and defaults
            if (!IsEnlisted<ModuleForMessageDirectory>())
            {
                Domain(m => m.InUserAssemblies());
            }
            if (!IsEnlisted<ModuleForSerialization>())
            {
                Serialization(x => x.UseDataContractSerializer());
            }

            if (Count<StorageModule>() == 0)
            {
                EnlistModule(new StorageModule());
            }
            if (Count<StorageModule>()> 1)
            {
                throw new InvalidOperationException("Only one storage module can be configured!");
            }

            //if (_moduleEnlistments.Count(m => m is ))


            foreach (var module in _moduleEnlistments)
            {
                _builder.RegisterModule(module);
            }


            var container = _builder.Build();
            var host = container.Resolve<CloudEngineHost>(TypedParameter.From(container));
            host.Initialize();
            return host;
        }

        void InnerSystemRegisterObservations()
        {
            _builder.RegisterType<ReactiveSystemObserverAdapter>().SingleInstance().As<ISystemObserver>();
            if (!DisableDefaultObserver)
            {
                _builder.RegisterType<ImmediateTracingObserver>().As<IObserver<ISystemEvent>>().SingleInstance();
            }

            //Builder.RegisterCallback(ci =>
            //    {
            //        var service = new TypedService(typeof(IObserver<ISystemEvent>));
            //        if (ci.IsRegistered(service)) return;
            //        var builder = new ContainerBuilder();
            //        builder.RegisterType<ImmediateTracingObserver>()
            //            .As<IObserver<ISystemEvent>>()
            //            .SingleInstance();
            //        builder.Update(ci);
            //    });
        }
    }
}