#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using System;
using System.Diagnostics;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Lokad;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

// ReSharper disable InconsistentNaming

namespace Sample_05.Web
{
	[UsedImplicitly]
	public class MvcApplication : HttpApplication
	{
		static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new {controller = "user", action = "index", id = ""} // Parameter defaults
				);
		}


		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			RegisterRoutes(RouteTable.Routes);

			if (RoleEnvironment.IsAvailable)
			{
				Trace.Listeners.Add(new DiagnosticMonitorTraceListener
					{
						Name = "AzureDiagnostics"
					});
			}
		}


		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
			// we check if cookie is valid and update the identity
			// otherwise unauthenticated identity is used
			GlobalAuth.InitializeRequest();
		}

		protected void Session_Start(object sender, EventArgs e)
		{
			GlobalState.InitializeSession();
		}
	}
}