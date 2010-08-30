#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence
// 
// Lokad.CQRS for Windows Azure: http://code.google.com/p/lokad-cqrs/

#endregion

using Lokad;
using Lokad.Cqrs;
using Microsoft.WindowsAzure.Diagnostics;

namespace Sample_01.Worker
{
	public class WorkerRole : CloudEngineRole
	{
		protected override ICloudEngineHost BuildHost()
		{
			// for more detail about this sample see:
			// http://code.google.com/p/lokad-cqrs/wiki/GuidanceSeries

			var builder = new CloudEngineBuilder();
			builder.Serialization.UseDataContractSerializer();
			builder.DomainIs(d =>
				{
					d.InCurrentAssembly();
					d.WithDefaultInterfaces();
				});
			
			builder.AddMessageHandler(mc =>
				{
					mc.ListenToQueue("sample-01");
					mc.WithSingleConsumer();
				});

			builder.AddMessageClient(m => m.DefaultToQueue("sample-01"));

			return builder.Build();
		}

		public override bool OnStart()
		{
			DiagnosticMonitor.Start("DiagnosticsConnectionString");
			// we send first ping message, when host starts
			WhenEngineStarts += SendFirstMessage;

			return base.OnStart();
		}

		static void SendFirstMessage(ICloudEngineHost host)
		{
			var sender = host.Resolve<IMessageClient>();
			var game = Rand.String.NextWord();
			sender.Send(new PingPongCommand(0, game));
		}
	}


}