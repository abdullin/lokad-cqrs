#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System.Diagnostics;
using Lokad;
using Lokad.Cqrs;
using Sample_05.Contracts;

namespace Sample_05.Web
{
	public static class GlobalSetup
	{
		internal static readonly ICloudClient Client;
		
		internal static readonly ViewReader Views;

		static GlobalSetup()
		{
			Client = Build();
			Views = Client.Resolve<ViewReader>();
		}

		public static ICloudClient Build()
		{
			var builder = new CloudClientBuilder();
			builder.Serialization.UseProtocolBuffers();
			builder.Domain(db =>
				{
					db.WithDefaultInterfaces();
					db.InAssemblyOf<RegisterUserCommand>();
				});

			builder.Views(x =>
				{
					x.WithDefaultInterfaces();
					x.InAssemblyOf<LoginView>();
					x.ViewContainer = "sample-05-views";
				});
			
			builder.Azure.LoadStorageAccountFromSettings("StorageConnectionString");
			
			

			builder.AddMessageClient(sm => sm.DefaultToQueue("sample-05-queue"));

			return builder.Build();
		}

		internal static void InitIfNeeded()
		{
			Trace.WriteLine("Bus running: " + Client.GetHashCode());
		}
	}
}