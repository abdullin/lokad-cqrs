#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

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