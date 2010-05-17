#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using Autofac;
using Lokad;

namespace CloudBus.Build.Cloud
{
	public sealed class DefaultCloudBusHost : ICloudBusHost
	{
		readonly IContainer _container;
		readonly ILog _log;
		readonly IEnumerable<IBusProcess> _serverThreads;

		public DefaultCloudBusHost(
			IContainer container,
			ILogProvider provider,
			IEnumerable<IBusProcess> serverThreads)
		{
			_container = container;
			_serverThreads = serverThreads;
			_log = provider.CreateLog<DefaultCloudBusHost>();
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
	}
}