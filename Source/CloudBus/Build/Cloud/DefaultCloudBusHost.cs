using System.Collections.Generic;
using Autofac;
using Lokad;

namespace Bus2.Build.Cloud
{
	public sealed class DefaultCloudBusHost : ICloudBusHost
	{
		readonly IContainer _container;
		readonly IEnumerable<IBusProcess> _serverThreads;
		readonly ILog _log;

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