using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Lokad;
using Lokad.Quality;
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
				new { controller = "user", action = "index", id = "" } // Parameter defaults
				);
		}


		protected void Application_Start()
		{
			GlobalSetup.InitIfNeeded();
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