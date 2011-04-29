#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Lokad.Cqrs.Build.Engine
{
    public abstract class CloudEngineRole : RoleEntryPoint
    {
        /// <summary>
        /// Implement in the inheriting class to configure the bus host.
        /// </summary>
        /// <returns></returns>
        protected abstract CloudEngineHost BuildHost();

        CloudEngineHost _host;
        readonly CancellationTokenSource _source = new CancellationTokenSource();

        Task _task;

        public override bool OnStart()
        {
            // this is actually azure initialization;
            _host = BuildHost();
            _host.Initialize();
            return base.OnStart();
        }

        public override void Run()
        {
            _task = _host.Start(_source.Token);
            _source.Token.WaitHandle.WaitOne();
        }

        public string InstanceName
        {
            get
            {
                return string.Format("{0}/{1}",
                    RoleEnvironment.CurrentRoleInstance.Role.Name,
                    RoleEnvironment.CurrentRoleInstance.Id);
            }
        }

        public override void OnStop()
        {
            _source.Cancel(true);

            _task.Wait(TimeSpan.FromSeconds(10));
            _host.Dispose();

            base.OnStop();
        }
    }
}