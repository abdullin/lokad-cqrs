using System;
using System.IO;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.FilePartition;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class FileModule : HideObjectMembersFromIntelliSense, IModule
    {
        Action<IComponentRegistry> _funqlets = registry => { };
        public void Configure(IComponentRegistry componentRegistry)
        {
            _funqlets(componentRegistry);
        }

        public void AddFileProcess(FileStorageConfig folder, string[] queues, Action<FilePartitionModule> config)
        {
            var module = new FilePartitionModule(folder, queues);
            config(module);
            _funqlets += module.Configure;
        }

        public void AddFileProcess(FileStorageConfig folder, params string[] queues)
        {
            AddFileProcess(folder, queues, m => { });
        }

        public void AddFileSender(FileStorageConfig folder, string queueName)
        {
            AddFileSender(folder, queueName, module => { });
        }

        public void AddFileProcess(FileStorageConfig folder, string queueName, Action<FilePartitionModule> config)
        {
            AddFileProcess(folder, new[] { queueName }, config);
        }

        public void AddFileRouter(FileStorageConfig folder, string queueName, Func<ImmutableEnvelope, string> config)
        {
            AddFileProcess(folder, queueName, m => m.DispatchToRoute(config));
        }

        public void AddFileRouter(FileStorageConfig folder, string[] queueNames, Func<ImmutableEnvelope, string> config)
        {
            AddFileProcess(folder, queueNames, m => m.DispatchToRoute(config));
        }

        public void AddFileSender(FileStorageConfig directory, string queueName, Action<SendMessageModule> config)
        {
            var module = new SendMessageModule((context, s) => new FileQueueWriterFactory(directory, context.Resolve<IEnvelopeStreamer>()), directory.AccountName, queueName);
            config(module);
            _funqlets += module.Configure;
        }
    }
}