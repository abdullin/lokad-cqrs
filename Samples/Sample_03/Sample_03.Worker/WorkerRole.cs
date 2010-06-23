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
						mc.ListenTo("sample-03");
						mc.WithMultipleConsumers();
					})
				.WithNHibernate(m => m.WithConfiguration("MyDbFile", BuildNHibernateConfig))
				// create IMessageClient that will send to this queue by default
				.SendMessages(m => m.DefaultToQueue("sample-03"))
				// enable and auto-wire scheduled tasks feature
				.RunTasks(m => { m.WithDefaultInterfaces().InCurrentAssembly(); })
				.Build();
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