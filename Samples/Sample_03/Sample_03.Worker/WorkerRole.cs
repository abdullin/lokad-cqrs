#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence
// 
// Lokad.CQRS for Windows Azure: http://code.google.com/p/lokad-cqrs/

#endregion

using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Lokad.Cqrs;
using Microsoft.WindowsAzure.Diagnostics;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Lokad;

namespace Sample_03.Worker
{
	public class WorkerRole : CloudEngineRole
	{
		protected override ICloudEngineHost BuildHost()
		{
			// for more detail about this sample see:
			// http://code.google.com/p/lokad-cqrs/wiki/GuidanceSeries

			// Important: this project is to be run on x64 by default (with SQLite)
			// if you are running it locally on x86 bit machine, go to /Samples/Library 
			// and replace System.Data.SQLite.DLL with  System.Data.SQLite.x86.DLL from the same folder.


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
					mc.ListenToQueue("sample-03");
					mc.WithMultipleConsumers();
				});

			builder.WithNHibernate(m => m.WithConfiguration("MyDbFile", BuildNHibernateConfig));

			builder.AddMessageClient(m => m.DefaultToQueue("sample-03"));

			builder.AddScheduler(m => m.WithDefaultInterfaces().InCurrentAssembly());

			return builder.Build();
		}

		static Configuration BuildNHibernateConfig(string fileName)
		{
			// your automapping setup here
			var autoMap = AutoMap
				.AssemblyOf<AccountEntity>(type => type.Name.EndsWith("Entity"));

			// we use SQLite database that is kept in file and recreated on startup
			// and is created on start-up
			return Fluently.Configure()
				// use SQLite file
				.Database(SQLiteConfiguration.Standard.UsingFile(fileName))
				// Generate automappings
				.Mappings(m => m.AutoMappings.Add(autoMap))
				// regenerate database on startup
				.ExposeConfiguration(cfg => new SchemaExport(cfg).Execute(false, true, false))
				.BuildConfiguration();
		}

		public override bool OnStart()
		{
			DiagnosticMonitor.Start("DiagnosticsConnectionString");
			return base.OnStart();
		}
	}
}