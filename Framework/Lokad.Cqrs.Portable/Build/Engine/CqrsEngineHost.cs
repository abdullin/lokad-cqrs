#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Lokad.Cqrs.Build.Engine.Events;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Build.Engine
{
    public sealed class CqrsEngineHost : IDisposable
    {
        public ILifetimeScope Container { get; private set; }
        readonly ISystemObserver _observer;
        readonly IEnumerable<IEngineProcess> _serverProcesses;

        public CqrsEngineHost(
            ILifetimeScope container,
            ISystemObserver observer,
            IEnumerable<IEngineProcess> serverProcesses)
        {
            Container = container;
            _serverProcesses = serverProcesses;
            _observer = observer;
        }

        public CancellationTokenSource StartAndRun()
        {
            var token = new CancellationTokenSource();
            Start(token.Token);
            return token;
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

            _observer.Notify(new EngineStarted(names));

            return Task.Factory.StartNew(() =>
                {
                    var watch = Stopwatch.StartNew();
                    try
                    {
                        Task.WaitAll(tasks, token);
                    }
                    catch(OperationCanceledException)
                    {}
                    _observer.Notify(new EngineStopped(watch.Elapsed));
                });
        }


        internal void Initialize()
        {
         
            _observer.Notify(new EngineInitializationStarted());
            foreach (var process in _serverProcesses)
            {
                process.Initialize();
            }
            _observer.Notify(new EngineInitialized());
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