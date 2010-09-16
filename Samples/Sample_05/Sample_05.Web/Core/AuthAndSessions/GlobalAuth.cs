using System;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using Lokad;
using Sample_05.Contracts;

namespace Sample_05.Web
{
	public static class GlobalAuth
	{

		public static Maybe<SessionIdentity> AuthenticateOpenId(string identity)
		{
			
			return AzureViews
				.Get<LoginView>(LoginView.CalculateSHA1(identity))
				.Convert(uv => new SessionIdentity(new AuthInfo(uv.UserId), uv.Username));
		}

		public static void HandleLogin(SessionIdentity account, bool persistCookie)
		{
			GlobalState.InitializeSession(account);
			string username = account.Info.ToCookieString();
			FormsAuthentication.SetAuthCookie(username, persistCookie);
		}


		/// <summary>
		/// Performs the HTTP Request initialization
		/// </summary>
		public static void InitializeRequest()
		{
			var context = HttpContext.Current;

			if (!context.Request.IsAuthenticated)
				return;

			var formsPrincipal = context.User;
			var username = formsPrincipal.Identity.Name;

			GetOrLoadRealPrincipal(username)
				.Apply(p => context.User = p)
				.Handle(() => context.User = CreateAnonymous());
		}
		static IPrincipal CreateAnonymous()
		{
			// implementation is anonymous when name is empty
			var identity = new GenericIdentity("");
			return new GenericPrincipal(identity, new string[0]);
		}

		static Maybe<AuthPrincipal> GetOrLoadRealPrincipal(string username)
		{
			var cache = HttpContext.Current.Cache;

			var cacheKey = "GlobalAuth_" + username;
			var principal = cache[cacheKey] as Maybe<AuthPrincipal>;

			if (principal == null)
			{
				// abdullin: this is not the global resolution cache
				// (that one is located down in the gateway logic)
				// but rather a small lookup one
				principal = LoadPrincipalInner(username);

				cache.Insert(
					cacheKey,
					principal,
					null,
					DateTime.UtcNow.AddMinutes(10),
					Cache.NoSlidingExpiration);

				cache[cacheKey] = principal;
			}
			return principal;
		}

		static Maybe<AuthPrincipal> LoadPrincipalInner(string formsUserName)
		{
			try
			{
				return AuthInfo.Parse(formsUserName)
					.Combine(i => AzureViews.Get<LoginView>(i.UserId).Convert(uv => new SessionIdentity(new AuthInfo(uv.UserId), uv.Username)))
					.Convert(sid => new AuthPrincipal(sid));

			}
			catch (KeyInvalidException)
			{
				return Maybe<AuthPrincipal>.Empty;
			}
		}

		/// <summary>
		/// Performs the logout
		/// </summary>
		public static void LogoutAndRedirectToLogin()
		{
			GlobalState.Clear();

			FormsAuthentication.SignOut();

			var context = HttpContext.Current;

			context.Session.Abandon();
			var response = context.Response;
			// to avoid getting empty page
			response.Redirect("~/");
			// to avoid processing session-bound code that would still fire.
			response.End();
		}

		/// <summary>
		/// Gets the account associated with the current request.
		/// </summary>
		/// <returns>account that is associated with the current request</returns>
		public static Maybe<SessionIdentity> GetCurrentIdentity()
		{
			var principal = HttpContext.Current.User as AuthPrincipal;
			if (null == principal)
				return Maybe<SessionIdentity>.Empty;
			return principal.Identity;
		}
	}
}