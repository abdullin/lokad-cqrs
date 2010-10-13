#region Copyright (c) 2010 Lokad. New BSD License

// Copyright (c) Lokad 2010 SAS 
// Company: http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD licence

#endregion

using System;

namespace Sample_05.Web
{
	/// <summary>
	/// Simple class for holding in session information about the current account
	/// in a strongly-typed manner
	/// </summary>
	/// <remarks>
	/// We'll have to keep this in session. So it must be serializable 
	/// for the out-of-proc sessions
	/// </remarks>
	[Serializable]
	public sealed class SessionIdentity
	{
		public readonly Guid UserId;
		public readonly string UserName;
		public readonly string SessionTitle;
		public readonly AuthInfo Info;

		public SessionIdentity(AuthInfo info, string userName)
		{
			Info = info;
			UserId = info.UserId;
			UserName = userName;

			SessionTitle = string.Format("{0} ({1})", UserName, UserId);
		}
	}
}