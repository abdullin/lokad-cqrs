using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Build.Engine;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Core.Serialization;

namespace Lokad.Cqrs.Build.Client
{
    public class CqrsClientBuilder : BuildSyntaxHelper
    {
        readonly List<IModule> _enlistments = new List<IModule>();

        bool IsEnlisted<TModule>() where TModule : IModule
        {
            return _enlistments.Count(x => x is TModule) > 0;
        }

        int Count<TModule>() where TModule : IModule
        {
            return _enlistments.Count(x => x is TModule);
        }

        public void EnlistModule(IModule module)
        {
            _enlistments.Add(module);
        }

        public bool DisableDefaultObserver { get; set; }

        public CqrsClientBuilder EnlistObserver(IObserver<ISystemEvent> observer)
        {
            _builder.RegisterInstance(observer);
            return this;
        }

        public CqrsClientBuilder EnlistObserver<TObserver>() where TObserver : IObserver<ISystemEvent>
        {
            _builder.RegisterType<TObserver>().As<IObserver<ISystemEvent>>().SingleInstance();
            return this;
        }

        public CqrsClientBuilder Serialization(Action<SerializationModule> config)
        {
            var m = new SerializationModule();
            config(m);
            EnlistModule(m);
            return this;
        }

        readonly ContainerBuilder _builder = new ContainerBuilder();

        public CqrsClientBuilder Advanced(Action<ContainerBuilder> build)
        {
            build(_builder);
            return this;
        }

        public CqrsClientBuilder Storage(Action<StorageModule> configure)
        {
            var m = new StorageModule();
            configure(m);
            EnlistModule(m);
            return this;
        }

        /// <summary>
        /// Configures the message domain for the instance of <see cref="CqrsEngineHost"/>.
        /// </summary>
        /// <param name="config">configuration syntax.</param>
        /// <returns>same builder for inline multiple configuration statements</returns>
        public CqrsClientBuilder Domain(Action<ModuleForMessageDirectory> config)
        {
            var directory = new ModuleForMessageDirectory();
            config(directory);
            EnlistModule(directory);
            return this;
        }

        void InnerSystemRegisterObservations()
        {
            _builder.RegisterType<ReactiveSystemObserverAdapter>().SingleInstance().As<ISystemObserver>();
            if (!DisableDefaultObserver)
            {
                _builder.RegisterType<ImmediateTracingObserver>().As<IObserver<ISystemEvent>>().SingleInstance();
            }
        }

        public CqrsClient Build()
        {
            InnerSystemRegisterObservations();
            // conditional registrations and defaults
            if (!IsEnlisted<ModuleForMessageDirectory>())
            {
                Domain(m => m.InUserAssemblies());
            }
            if (!IsEnlisted<SerializationModule>())
            {
                Serialization(x => x.UseDataContractSerializer());
            }

            if (Count<StorageModule>() == 0)
            {
                EnlistModule(new StorageModule());
            }
            if (Count<StorageModule>() > 1)
            {
                throw new InvalidOperationException("Only one storage module can be configured!");
            }
            foreach (var module in _enlistments)
            {
                _builder.RegisterModule(module);
            }

            var container = _builder.Build();
            return new CqrsClient(container);
        }
    }
}