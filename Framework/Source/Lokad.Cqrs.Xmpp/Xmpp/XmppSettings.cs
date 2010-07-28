using jabber;

namespace Lokad.Cqrs.Xmpp
{
	public sealed class XmppSettings : IXmppSettings
	{

		/// <summary>
		/// The username to connect as.
		/// </summary>
		/// <value>The name of the user.</value>
		public string UserName { get; set; }

		public int Priority { get; set; }

		public string Resource { get; set; }
		public string Server { get; set; }
		public string JabberId
		{
			get { return new JID(UserName, Server, Resource); }
		}

		/// <summary>
		/// The password to use for connecting.  This may be sent across the wire plaintext, if the server doesn't support digest, and PlaintextAuth is true.
		/// </summary>
		/// <value>The password.</value>
		public string Password { get; set; }

		public string NetworkHost { get; set; }
		public XmppOptions Options { get; set; }
	}
}