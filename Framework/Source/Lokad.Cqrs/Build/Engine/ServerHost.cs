using System;
using System.Threading;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Lokad.Cqrs
{
	public abstract class CloudEngineRole : RoleEntryPoint
	{
		ICloudEngineHost _host;
		volatile bool _shouldStop;

		/// <summary>
		/// Implement in the inheriting class to configure the bus host.
		/// </summary>
		/// <returns></returns>
		protected abstract ICloudEngineHost BuildHost();

		protected event Action<ICloudEngineHost> WhenEngineStarts = host => { }; 

		public override void Run()
		{
			_host.Start();

			WhenEngineStarts(_host);

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
			_host.Stop();
			_shouldStop = true;

			base.OnStop();
		}
	}
}