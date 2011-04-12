#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
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
			_observer.Notify(new HostStarted());
			return Task.Factory.ContinueWhenAll(tasks, t => _observer.Notify(new HostStopped()));
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
				throw Errors.Inner(e);
			}
		}

		public void Dispose()
		{
			Container.Dispose();
		}
	}
}