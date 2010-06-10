using System;
using System.Linq;
using System.Net;
using System.Threading;
using Autofac;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace CloudBus.Build.Cloud
{
	public abstract class CloudServerHost : RoleEntryPoint
	{
		ICloudBusHost _host;
		volatile bool _shouldStop;

		protected abstract ICloudBusHost BuildHost();

		protected event Action<ICloudBusHost> WhenHostStarts = host => { }; 

		public override void Run()
		{
			_host.Start();

			WhenHostStarts(_host);

			while (false == _shouldStop)
			{
				Thread.Sleep(1000);
			}
		}

		public override bool OnStart()
		{
			var onStart = base.OnStart();
			_host = BuildHost();
			_host.Initialize();

			return onStart;
		}

		public override void OnStop()
		{
			_host.Stop();
			_shouldStop = true;

			base.OnStop();
		}
	}
}