#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

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