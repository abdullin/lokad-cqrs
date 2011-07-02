using System;
using System.ComponentModel;
using System.IO;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.FilePartition;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class FileClientModule : HideObjectMembersFromIntelliSense, IModule
    {
        Action<IComponentRegistry> _modules = context => { };




        public void AddFileSender(FileStorageConfig config, string queueName, Action<SendMessageModule> configure)
        {
            var module = new SendMessageModule((context, endpoint) => new FileQueueWriterFactory(config, context.Resolve<IEnvelopeStreamer>()), config.AccountName, queueName);
            configure(module);
            _modules += module.Configure;
        }



        public void AddFileSender(FileStorageConfig config, string queueName)
        {
            AddFileSender(config, queueName, m => { });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Configure(IComponentRegistry container)
        {
            _modules(container);
        }
    }
}