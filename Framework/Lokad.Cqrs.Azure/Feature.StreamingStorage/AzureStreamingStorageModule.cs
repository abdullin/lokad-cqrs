#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;
using Autofac.Core;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    public sealed class AzureStreamingStorageModule : HideObjectMembersFromIntelliSense, IModule
    {
        readonly string _accountName;

        public AzureStreamingStorageModule(string accountName)
        {
            _accountName = accountName;
        }

        public void Configure(IComponentRegistry componentRegistry)
        {
            var builder = new ContainerBuilder();
            builder.Register(c =>
                {
                    var config = c.ResolveNamed<IAzureAccessConfiguration>(_accountName);
                    return new BlobStreamingRoot(config.CreateBlobClient());
                }).As<IStreamingRoot>().SingleInstance();
            builder.Update(componentRegistry);
        }
    }
}