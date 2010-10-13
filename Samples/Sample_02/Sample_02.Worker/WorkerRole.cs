#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using Lokad.Cqrs;
using Microsoft.WindowsAzure.Diagnostics;

namespace Sample_02.Worker
{
	public class WorkerRole : CloudEngineRole
	{
		protected override ICloudEngineHost BuildHost()
		{
			// for more detail about this sample see:
			// http://code.google.com/p/lokad-cqrs/wiki/GuidanceSeries

			var builder = new CloudEngineBuilder();
			builder.Serialization.UseDataContractSerializer();

			// this tells the server about the domain
			builder.DomainIs(d =>
				{
					d.WithDefaultInterfaces();
					d.InCurrentAssembly();
				});

			// we'll handle all messages incoming to this queue
			builder.AddMessageHandler(mc =>
				{
					mc.ListenToQueue("sample-02");
					mc.WithMultipleConsumers();
				});

			// create IMessageClient that will send to sample-02 by default
			builder.AddMessageClient(m => m.DefaultToQueue("sample-02"));

			// enable and auto-wire scheduled tasks feature
			builder.AddScheduler(m => m.WithDefaultInterfaces().InCurrentAssembly());

			return builder.Build();
		}

		public override bool OnStart()
		{
			DiagnosticMonitor.Start("DiagnosticsConnectionString");
			return base.OnStart();
		}
	}
}