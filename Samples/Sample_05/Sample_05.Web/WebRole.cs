#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using System.Linq;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Sample_05.Web
{
	public class WebRole : RoleEntryPoint
	{
		public override bool OnStart()
		{
			DiagnosticMonitor.Start("DiagnosticsConnectionString");
			RoleEnvironment.Changing += RoleEnvironmentChanging;

			GlobalSetup.InitIfNeeded();

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