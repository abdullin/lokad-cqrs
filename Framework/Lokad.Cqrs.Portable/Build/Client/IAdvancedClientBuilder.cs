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
        void RegisterModule(IModule module);
        CqrsClientBuilder RegisterObserver(IObserver<ISystemEvent> observer);
        void DataSerializer(Func<Type[], IDataSerializer> serializer);
        void EnvelopeSerializer(IEnvelopeSerializer serializer);
        CqrsClientBuilder ConfigureContainer(Action<ContainerBuilder> build);
        IList<IObserver<ISystemEvent>> Observers { get; }
    }
}