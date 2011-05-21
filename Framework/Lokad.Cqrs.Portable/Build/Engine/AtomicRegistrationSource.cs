#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Lifetime;
using Lokad.Cqrs.Feature.AtomicStorage;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class AtomicRegistrationCore
    {
        readonly IAtomicStorageFactory _factory;

        public AtomicRegistrationCore(IAtomicStorageFactory factory)
        {
            _factory = factory;
        }

        public bool TryResolve(Type serviceType, out object instance)
        {
            instance = null;
            if (!serviceType.IsGenericType)
                return false;

            var definition = serviceType.GetGenericTypeDefinition();
            var arguments = serviceType.GetGenericArguments();
            
            if (definition == typeof(IAtomicSingletonReader<>))
            {
                instance = typeof(IAtomicStorageFactory)
                    .GetMethod("GetSingletonReader")
                    .MakeGenericMethod(arguments)
                    .Invoke(_factory, null);
                return true;
            }
            if (definition == typeof(IAtomicSingletonWriter<>))
            {
                instance = typeof(IAtomicStorageFactory)
                    .GetMethod("GetSingletonWriter")
                    .MakeGenericMethod(arguments)
                    .Invoke(_factory, null);
                return true;
            }
            if (definition == typeof(IAtomicEntityReader<,>))
            {
                instance = typeof(IAtomicStorageFactory)
                    .GetMethod("GetEntityReader")
                    .MakeGenericMethod(arguments)
                    .Invoke(_factory, null);
                return true;
            }
            if (definition == typeof(IAtomicEntityWriter<,>))
            {
                instance = typeof(IAtomicStorageFactory)
                    .GetMethod("GetEntityWriter")
                    .MakeGenericMethod(arguments)
                    .Invoke(_factory, null);
                return true;
            }
            return false;
        }
    }
    public sealed class AtomicRegistrationSource : IRegistrationSource
    {
        readonly AtomicRegistrationCore _core;

        public AtomicRegistrationSource(AtomicRegistrationCore core)
        {
            _core = core;
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service,
            Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var serviceWithType = service as IServiceWithType;
            if (serviceWithType != null)
            {
                object instance;
                if (_core.TryResolve(serviceWithType.ServiceType, out instance))
                {
                    var data = new RegistrationData(service)
                    {
                        Sharing = InstanceSharing.Shared,
                        Lifetime = new RootScopeLifetime()
                    };

                    yield return
                        RegistrationBuilder.CreateRegistration(Guid.NewGuid(), data,
                            new ProvidedInstanceActivator(instance), new[] { service });
                }
            }
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }
    }
}