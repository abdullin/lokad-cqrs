#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Extensions;
using Lokad.Cqrs.Logging;


namespace Lokad.Cqrs
{
	
	public sealed class CloudEngineHost : IDisposable
	{
		public ILifetimeScope Container { get; private set; }
		readonly ILog _log;
		readonly IEnumerable<IEngineProcess> _serverProcesses;

		public CloudEngineHost(
			ILifetimeScope container,
			ILog log,
			IEnumerable<IEngineProcess> serverProcesses)
		{
			Container = container;
			_serverProcesses = serverProcesses;
			_log = log;
		}

		public Task Start(CancellationToken token)
		{
			var tasks = _serverProcesses.ToArray(p => p.Start(token));
			_log.Log(new HostStarted());
			return Task.Factory.ContinueWhenAll(tasks, t => _log.Log(new HostStopped()));
		}

		public void Initialize()
		{
			_log.Log(new HostInitializationStarted());
			foreach (var process in _serverProcesses)
			{
				process.Initialize();
			}
			_log.Log(new HostInitialized());
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