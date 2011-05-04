#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;
using Autofac;
using Autofac.Core;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    public sealed class FileStreamingStorageModule : BuildSyntaxHelper, IModule
    {
        readonly string _localPath;

        public FileStreamingStorageModule(string localPath)
        {
            _localPath = localPath;
        }

        public bool WipeStorageAtStartUp { get; set; }

        public void Configure(IComponentRegistry componentRegistry)
        {
            var directory = new DirectoryInfo(_localPath);
            if (WipeStorageAtStartUp && directory.Exists)
            {
                directory.Delete(true);
            }

            
            directory.Create();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new FileStorageContainer(directory)).As<IStorageRoot>();
            builder.Update(componentRegistry);
        }
    }
}