#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

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
			return new CloudEngineBuilder()
			// this tells the server about the domain
				.Domain(d =>
					{
						d.InCurrentAssembly();
						d.WithDefaultInterfaces();
					})
				// we'll handle all messages incoming to this queue
				.HandleMessages(mc =>
					{
						mc.ListenTo("sample-01");
						mc.WithSingleConsumer();
					})
				// when we send message - default it to this queue as well
				.SendMessages(m => m.DefaultToQueue("sample-01"))
				.Build();
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