#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using Autofac;
using Lokad.Quality;

namespace Lokad.Cqrs
{
	[UsedImplicitly]
	public sealed class DefaultCloudEngineHost : ICloudEngineHost
	{
		readonly IContainer _container;
		readonly ILog _log;
		readonly IEnumerable<IEngineProcess> _serverThreads;

		

		public DefaultCloudEngineHost(
			IContainer container,
			ILogProvider provider,
			IEnumerable<IEngineProcess> serverThreads)
		{
			_container = container;
			_serverThreads = serverThreads;
			_log = provider.CreateLog<DefaultCloudEngineHost>();
		}

		public void Start()
		{
			_log.Info("Starting host");

			foreach (var thread in _serverThreads)
			{
				thread.Start();
			}
		}

		public void Initialize()
		{
			_log.Info("Initializing host");
		}

		public void Stop()
		{
			_log.Info("Stopping host");

			foreach (var thread in _serverThreads)
			{
				thread.Dispose();
			}

			_container.Dispose();
		}

		public TService Resolve<TService>()
		{
			return _container.Resolve<TService>();
		}
	}
}