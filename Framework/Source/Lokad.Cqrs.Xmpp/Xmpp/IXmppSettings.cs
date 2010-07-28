namespace Lokad.Cqrs.Xmpp
{
	public interface IXmppSettings
	{
		string JabberId { get; }
		/// <summary>
		/// Gets the connecting resource.  Used to identify a unique connection.
		/// </summary>
		/// <value>The resource.</value>
		string Password { get;}
		string NetworkHost { get; }
		XmppOptions Options { get; }
		/// <summary>
		/// Priority for this connection
		/// </summary>
		/// <value>The priority.</value>
		int Priority { get; }
	}
}