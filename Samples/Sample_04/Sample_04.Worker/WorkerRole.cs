using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lokad;
using Lokad.Cqrs;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Autofac;

namespace Sample_04.Worker
{
	public class WorkerRole : CloudEngineRole
	{
		protected override ICloudEngineHost BuildHost()
		{
			// for more detail about this sample see:
			// http://code.google.com/p/lokad-cqrs/wiki/GuidanceSeries

			var host = new CloudEngineBuilder()
				// this tells the server about the domain
				.Domain(d =>
					{
						d.InCurrentAssembly();
						d.WithDefaultInterfaces();
						d.UseProtocolBuffers();
					})
				// we'll handle all messages incoming to this queue
				.HandleMessages(mc =>
					{
						mc.ListenTo("sample-01");
						mc.WithSingleConsumer();
						mc.WhenMessageHandlerFails((message, exception) => Logger.Handle(message, exception));
					})
				// when we send message - default it to this queue as well
				.SendMessages(m => m.DefaultToQueue("sample-01"))
				.WithAction(cb => cb.RegisterType<CustomExceptionLogger>().SingleInstance())
				.Build();

			Logger = host.Resolve<CustomExceptionLogger>();

			return host;
		}

		CustomExceptionLogger Logger { get; set; }

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
