using System.Diagnostics;
using System.Web;

namespace Sample_05.Web
{
	/// <summary>
	/// Static class that provides strongly-typed access to the session (user)-
	/// specific objects and variables. This includes session-specific container
	/// </summary>
	public static class GlobalState
	{
		const string SessionIdentityKey = "GlobalSetup_SIK";
		const string ContextResolverKey = "GlobalSetup_CRK";

		/// <summary>
		/// Gets a value indicating whether this instance is authenticated.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is authenticated; otherwise, <c>false</c>.
		/// </value>
		public static bool IsAuthenticated
		{
			get { return HttpContext.Current.Session[SessionIdentityKey] != null; }
		}

		/// <summary>
		/// Gets the account associated with the current session (may be null in unauth context)
		/// </summary>
		/// <value>The account associated with the current session.</value>
		public static SessionIdentity Identity
		{
			get
			{
				// session recovery is handled by the global handler
				return (SessionIdentity)HttpContext.Current.Session[SessionIdentityKey];
			}
		}

		public static string Username
		{
			get
			{
				if (null == Identity)
				{
					return "User";
				}
				return Identity.UserName;
			}
		}


		public static void Clear()
		{
			var session = HttpContext.Current.Session;
			session.Clear();
		}

		/// <summary>
		/// Single point of entry to initialize the session
		/// </summary>
		/// <param name="identity">The account session.</param>
		public static void InitializeSession(SessionIdentity identity)
		{
			var session = HttpContext.Current.Session;
			session[SessionIdentityKey] = identity;
		}

		/// <summary>
		/// Initializes the session, using the auth information
		/// associated with the current request
		/// </summary>
		public static void InitializeSession()
		{
			var context = HttpContext.Current;
			// we are fine
			if (context.Session[SessionIdentityKey] != null)
				return;

			// unauthenticated session here
			if (!context.Request.IsAuthenticated)
				return;

			// authenticated session but without our data.
			// recover expired session (or use cookie)

			Trace.WriteLine("Session initialization attempt");
			GlobalAuth
				.GetCurrentIdentity()
				.Apply(s => InitializeSession(s));
		}
	}
}