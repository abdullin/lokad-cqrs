#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// Autofac module for Lokad.CQRS engine. It initializes view containers 
    /// on start-up and wires writers
    /// </summary>
    public sealed class AzureAtomicStorageWriterModule : Module
    {
        readonly IAzureAtomicStorageStrategy _strategy;

        public AzureAtomicStorageWriterModule(IAzureAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterGeneric(typeof (AzureAtomicEntityWriter<,>))
                .WithParameter(TypedParameter.From(_strategy))
                .As(typeof (IAtomicEntityWriter<,>))
                .SingleInstance();
            builder
                .RegisterGeneric(typeof (AzureAtomicSingletonWriter<>))
                .As(typeof (IAtomicSingletonWriter<>))
                .WithParameter(TypedParameter.From(_strategy))
                .SingleInstance();
            builder
                .RegisterType(typeof (AtomicStorageInitialization))
                .As<IEngineProcess>()
                .SingleInstance();

            builder
                .RegisterType<AzureAtomicStorageFactory>()
                .As<IAtomicStorageFactory>()
                .WithParameter(TypedParameter.From(_strategy))
                .SingleInstance();

            builder
                .RegisterType<NuclearStorage>()
                .SingleInstance();
        }
    }
}