#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence
// 
// Lokad.CQRS for Windows Azure: http://code.google.com/p/lokad-cqrs/

#endregion

using System;
using System.IO;
using Lokad;
using Lokad.Cqrs;
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

			var builder = new CloudEngineBuilder();

			// let's use Protocol Buffers!
			builder.Serialization.UseProtocolBuffers();

			// this tells the server about the domain
			builder.DomainIs(d =>
				{
					d.InCurrentAssembly();
					d.WithDefaultInterfaces();
				});

			// we'll handle all messages incoming to this queue
			builder.AddMessageHandler(mc =>
				{
					mc.ListenToQueue("sample-04");
					mc.WithSingleConsumer();
					// let's record failures to the specified blob 
					// container using the pretty printer
					mc.LogExceptionsToBlob(c =>
						{
							c.ContainerName = "sample-04-errors";
							c.WithTextAppender(RenderAdditionalContent);
						});

					// set XmppRecipient in your config and enable this
					// to send notifications to the specified Jabber
					//
					// mc.LogExceptionsToCommunicator(c => { c.ConfigKeyForRecipient = "XmppRecipient"; });
				});


			// using XMPP communicator. Make sure to put proper values in your config before enabling:
			// XmppIdentity
			// XmppPassword
			// XmppNetworkHost (optional, talk.l.google.com for GTalk)

			//builder.CommunicateWithXmpp(x =>
			//    {
			//        x.OnCertificateError((certificate, chain, errors) => true);
			//        x.Resource = InstanceName;
			//    });


			// when we send message - default it to this queue as well
			builder.AddMessageClient(m => m.DefaultToQueue("sample-04"));

			return builder.Build();
		}

		static void RenderAdditionalContent(UnpackedMessage message, Exception exception, TextWriter builder)
		{
			builder.WriteLine("Content");
			builder.WriteLine("=======");
			builder.WriteLine(message.ContractType.Name);
			try
			{
				// we'll use JSON serializer for printing messages nicely
				var text = JsonConvert.SerializeObject(message.Content, Formatting.Indented);
				builder.WriteLine(text);
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