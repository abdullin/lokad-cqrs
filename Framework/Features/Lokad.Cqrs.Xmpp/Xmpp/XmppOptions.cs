using System;

namespace Lokad.Cqrs.Xmpp
{
	[Flags]
	public enum XmppOptions
	{
		None = 0x00,
		/// <summary>
		/// Automatically log in on connection. 
		/// </summary>
		AutoLogin = 0x01,
		/// <summary>
		/// Automatically send presence on connection.
		/// </summary>
		AutoPresence = 0x02,
		/// <summary>
		/// Retrieves the roster on connection.
		/// </summary>
		AutoRoster = 0x04,

		/// <summary>
		/// Automatically send back 501/feature-not-implemented to IQs that have not been handled.
		/// </summary>
		AutoIQErrors = 0x08,
	}
}