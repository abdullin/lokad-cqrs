#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using System.Linq;
using System.Net;
using Lokad.Cqrs;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Sample_05.Contracts;
using Sample_05.Domain;

namespace Sample_05.Worker
{
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
			builder.DomainIs(x =>
				{
					x.WithDefaultInterfaces();
					x.InAssemblyOf<RegisterUserCommand>();
					x.InAssemblyOf<RegisterUser>();
				});

			builder.Views(x =>
				{
					x.WithDefaultInterfaces();
					x.InAssemblyOf<LoginView>();
					x.ViewContainer = "sample-05-views";
				});
			builder.Azure.LoadStorageAccountFromSettings("StorageConnectionString");
			builder.AddMessageClient(sm => sm.DefaultToQueue("sample-05-queue"));


			// we'll handle all messages incoming to this queue
			builder.AddMessageHandler(x =>
				{
					x.ListenToQueue("sample-05-queue");
					x.WithSingleConsumer();
					// let's record failures to the specified blob 
					// container using the pretty printer
					x.LogExceptionsToBlob(c => { c.ContainerName = "sample-05-errors"; });

					// set XmppRecipient in your config and enable this
					// to send notifications to the specified Jabber
					//
					// mc.LogExceptionsToCommunicator(c => { c.ConfigKeyForRecipient = "XmppRecipient"; });
				});


			return builder.Build();
		}

		public override bool OnStart()
		{
			// Set the maximum number of concurrent connections 
			ServicePointManager.DefaultConnectionLimit = 12;

			DiagnosticMonitor.Start("DiagnosticsConnectionString");
			RoleEnvironment.Changing += RoleEnvironmentChanging;

			return base.OnStart();
		}

		void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
		{
			// If a configuration setting is changing
			if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
			{
				// Set e.Cancel to true to restart this role instance
				e.Cancel = true;
			}
		}
	}
}