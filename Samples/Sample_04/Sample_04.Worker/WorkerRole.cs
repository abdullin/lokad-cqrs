#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.IO;
using Lokad;
using Lokad.Cqrs;
using Lokad.Quality;
using Microsoft.WindowsAzure.Diagnostics;
using Newtonsoft.Json;

namespace Sample_04.Worker
{
	[UsedImplicitly]
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
						// let's use Protocol Buffers!
						d.UseProtocolBuffers();
						d.InCurrentAssembly();
						d.WithDefaultInterfaces();
					})
				// we'll handle all messages incoming to this queue
				.HandleMessages(mc =>
					{
						mc.ListenTo("sample-01");
						mc.WithSingleConsumer();

						// let's record failures to the specified blob
						mc.LogExceptionsToBlob("sample-01-errors", RenderAdditionalContent);
					})
				// when we send message - default it to this queue as well
				.SendMessages(m => m.DefaultToQueue("sample-01"))
				.Build();


			return host;
		}

		static void RenderAdditionalContent(UnpackedMessage message, Exception exception, TextWriter builder)
		{
			
			builder.WriteLine("Content");
			builder.WriteLine("=======");
			builder.WriteLine(message.ContractType.Name);
			try
			{
				// we'll use JSON serializer for printing messages nicely
				builder.WriteLine(JsonConvert.SerializeObject(message.Content, Formatting.Indented));
			}
			catch (Exception ex)
			{
				builder.WriteLine(ex.ToString());
			}
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