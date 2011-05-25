#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using System;
using System.Linq;
using System.Net;
using System.Threading;
using Lokad.Cqrs;
using Lokad.Cqrs.Build.Engine;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Sample_04.Worker
{
    public class WorkerRole : RoleEntryPoint
    {
        CqrsEngineHost _host;

        public override void Run()
        {
            _host.Start(_source.Token);
            _source.Token.WaitHandle.WaitOne();
        }

        readonly CancellationTokenSource _source = new CancellationTokenSource();

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 48;
            RoleEnvironment.Changing += RoleEnvironmentChanging;

            _host = BuildBusWorker.Configure().Build();

            SendFirstMessage(_host);

            return base.OnStart();
        }

        public override void OnStop()
        {
            _source.Cancel();
            base.OnStop();
        }

        static void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }

        private static void SendFirstMessage(CqrsEngineHost host)
        {
            var sender = host.Resolve<IMessageSender>();
            var game = new Random().NextDouble().ToString();
            sender.SendOne(new PingPongCommand(0, game));
        }
    }

}