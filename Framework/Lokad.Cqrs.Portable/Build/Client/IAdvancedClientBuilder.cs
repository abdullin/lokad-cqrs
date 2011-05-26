#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace Lokad.Cqrs.Build.Client
{
    public interface IAdvancedClientBuilder : IHideObjectMembersFromIntelliSense
    {
        /// <summary>
        /// Registers custom container module
        /// </summary>
        /// <param name="module">The module to register.</param>
        void RegisterModule(IModule module);
        /// <summary>
        /// Registers custom Reactive observer.
        /// </summary>
        /// <param name="observer">The observer.</param>
        /// <returns></returns>
        void RegisterObserver(IObserver<ISystemEvent> observer);
        /// <summary>
        /// Overrides custom data serializer with a provided factory
        /// </summary>
        /// <param name="serializer">The serializer factory (taking collection of message types as inputs).</param>
        void DataSerializer(Func<Type[], IDataSerializer> serializer);
        /// <summary>
        /// Overrides custom envelope serializer with a provided factory
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        void EnvelopeSerializer(IEnvelopeSerializer serializer);
        /// <summary>
        /// Applies custom configuration to the container
        /// </summary>
        /// <param name="build">The build.</param>
        void ConfigureContainer(Action<ContainerBuilder> build);
        /// <summary>
        /// Lists currect reactive observers
        /// </summary>
        IList<IObserver<ISystemEvent>> Observers { get; }
    }
}