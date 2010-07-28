using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using jabber;
using jabber.client;
using jabber.protocol.client;
using Lokad.Messaging;
using Lokad.Quality;

namespace Lokad.Cqrs.Xmpp
{
	[UsedImplicitly]
	public class XmppNotifier : IStartable, IDisposable, IRealtimeNotifier
	{
		readonly JabberClient _client;
		
		readonly ManualResetEvent _authenticated = new ManualResetEvent(false);
		readonly ILog _log;

		public event XmppCertificateValidationCallback OnInvalidCertificate = (certificate, chain, errors) => false;
		
		public XmppNotifier(IXmppSettings settings, ILogProvider provider)
		{
			var jid = new JID(settings.JabberId);

			_client = new JabberClient
				{
					User = jid.User,
					Server = jid.Server,
					NetworkHost = settings.NetworkHost,
					Password = settings.Password,
					AutoLogin = settings.Has(XmppOptions.AutoLogin),
					AutoPresence = settings.Has(XmppOptions.AutoPresence),
					AutoReconnect = 60,
					Resource = jid.Resource,
					KeepAlive = 60,
					
					};
			_client.OnAuthenticate += OnAuthenticate;
			_client.OnDisconnect += OnDisconnect;
			_client.OnInvalidCertificate += OnClientInvalidCertificate;
			
			_log = provider.Get(typeof(XmppNotifier).FullName);
 
		}

		bool OnClientInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return OnInvalidCertificate(certificate, chain, sslPolicyErrors);
		}


		void OnAuthenticate(object sender)
		{
			_authenticated.Set();
		}
		void OnDisconnect(object sender)
		{
			_authenticated.Reset();
		}

		public void StartUp()
		{
			_client.Connect();
			if (!_authenticated.WaitOne(6000))
			{
				throw new InvalidOperationException("Failed to authenticate");
			}
		}
		public void Dispose()
		{
			_client.Close();
			_client.OnDisconnect -= OnDisconnect;
			_client.OnAuthenticate -= OnAuthenticate;
			_client.OnInvalidCertificate -= OnClientInvalidCertificate;
			_authenticated.Close();
			_client.Dispose();
		}
		
		public void Notify(string recipient, string body, RealtimeNotificationType options)
		{
			var type = EnumUtil<MessageType>.ConvertSafelyFrom(options);
			try
			{
				_client.Message(type, recipient, body);
			}
			catch (Exception ex)
			{
				_log.ErrorFormat(ex, "Failed to send message to '{0}'", recipient);
			}
		}
	}
}