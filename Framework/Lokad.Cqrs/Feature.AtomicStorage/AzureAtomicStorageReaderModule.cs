#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AzureAtomicStorageReaderModule : Module
    {
        readonly IAzureAtomicStorageStrategy _strategy;

        public AzureAtomicStorageReaderModule(IAzureAtomicStorageStrategy strategy)
        {
            _strategy = strategy;
        }


        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterGeneric(typeof (AzureAtomicEntityReader<,>))
                .As(typeof (IAtomicEntityReader<,>))
                .SingleInstance();
            builder
                .RegisterGeneric(typeof (AzureAtomicSingletonReader<>))
                .As(typeof (IAtomicSingletonReader<>))
                .SingleInstance();

            builder.RegisterInstance(_strategy);
        }
    }
}