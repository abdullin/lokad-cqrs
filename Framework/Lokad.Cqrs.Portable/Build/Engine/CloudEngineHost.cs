#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Lokad.Cqrs.Build.Engine.Events;
using Lokad.Cqrs.Core.Evil;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class CloudEngineHost : IDisposable
    {
        public ILifetimeScope Container { get; private set; }
        readonly ISystemObserver _observer;
        readonly IEnumerable<IEngineProcess> _serverProcesses;

        public CloudEngineHost(
            ILifetimeScope container,
            ISystemObserver observer,
            IEnumerable<IEngineProcess> serverProcesses)
        {
            Container = container;
            _serverProcesses = serverProcesses;
            _observer = observer;
        }

        public Task Start(CancellationToken token)
        {
            var tasks = _serverProcesses.Select(p => p.Start(token)).ToArray();

            if (tasks.Length == 0)
            {
                throw new InvalidOperationException(string.Format("There were no instances of '{0}' registered", typeof(IEngineProcess).Name));
            }

            var names =
                _serverProcesses.Select(p => string.Format("{0}({1:X8})", p.GetType().Name, p.GetHashCode())).ToArray();

            _observer.Notify(new HostStarted(names));

            return Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Task.WaitAll(tasks, token);
                    }
                    catch(OperationCanceledException)
                    {}
                    _observer.Notify(new HostStopped());
                });
        }

        public void Initialize()
        {
            _observer.Notify(new HostInitializationStarted());
            foreach (var process in _serverProcesses)
            {
                process.Initialize();
            }
            _observer.Notify(new HostInitialized());
        }

        public TService Resolve<TService>()
        {
            try
            {
                return Container.Resolve<TService>();
            }
            catch (TargetInvocationException e)
            {
                throw InvocationUtil.Inner(e);
            }
        }

        public void Dispose()
        {
            Container.Dispose();
        }
    }
}