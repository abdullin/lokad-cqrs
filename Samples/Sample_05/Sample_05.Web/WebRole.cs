using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
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

		private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
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
