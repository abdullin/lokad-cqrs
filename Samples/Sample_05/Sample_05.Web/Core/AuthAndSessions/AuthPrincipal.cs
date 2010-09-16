using System;
using System.Security.Principal;

namespace Sample_05.Web
{
	/// <summary>
	/// Implementation of <see cref="IPrincipal"/> that provides
	/// backward compatibility for legacy authorization rules and ELMAH.
	/// </summary>
	[Serializable]
	public sealed class AuthPrincipal : MarshalByRefObject, IPrincipal
	{
		public readonly SessionIdentity Identity;
		readonly IIdentity _identity;


		public AuthPrincipal(SessionIdentity account)
		{
			Identity = account;
			_identity = new GenericIdentity(account.SessionTitle);
		}

		public bool IsInRole(string role)
		{
			return false;
		}

		IIdentity IPrincipal.Identity
		{
			get { return _identity; }
		}
	}
}