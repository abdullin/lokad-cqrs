#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs;
using Microsoft.WindowsAzure.Diagnostics;

namespace Sample_02.Worker
{
	public class WorkerRole : CloudEngineRole
	{
		protected override ICloudEngineHost BuildHost()
		{
			return new CloudEngineBuilder()
				// this tells the server about the domain
				.Domain(d =>
					{
						d.WithDefaultInterfaces();
						d.InCurrentAssembly();
					})
				// we'll handle all messages incoming to this queue
				.HandleMessages(mc =>
					{
						mc.ListenTo("sample-02");
						mc.WithMultipleConsumers();
					})
				// create IMessageClient that will send to sample-02 by default
				.SendMessages(m => m.DefaultToQueue("sample-02"))
				// enable and auto-wire scheduled tasks feature
				.RunTasks(m => { m.WithDefaultInterfaces().InCurrentAssembly(); })
				.Build();
		}

		public override bool OnStart()
		{
			DiagnosticMonitor.Start("DiagnosticsConnectionString");
			return base.OnStart();
		}
	}
}