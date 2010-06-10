using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using CloudBus;
using CloudBus.Build.Cloud;
using Lokad;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Autofac;

namespace Sample_02.Worker
{
	public class WorkerRole : CloudServerHost
	{
		protected override ICloudBusHost BuildHost()
		{
			return new CloudBusBuilder()
				.Domain(d =>
				{
					d.WithDefaultInterfaces();
					d.InCurrentAssembly();
				})
				.HandleMessages(mc =>
					{
						mc.ListenTo("sample-02");
						mc.WithMultipleConsumers();
					})
				.SendMessages(m => m.DefaultToQueue("sample-02"))
				.RunTasks(m =>
					{
						m.WithDefaultInterfaces().InCurrentAssembly();
					})
				.Build();
		}

		public override bool OnStart()
		{
			DiagnosticMonitor.Start("DiagnosticsConnectionString");
			return base.OnStart();
		}
	}
}
