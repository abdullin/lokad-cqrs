using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using jabber;
using jabber.client;
using jabber.protocol.client;
using Lokad.Messaging;
using Lokad.Quality;

namespace Lokad.Cqrs.Xmpp
{
	[UsedImplicitly]
	public class XmppCommunicator : IStartable, IDisposable, IRealtimeNotifier
	{
		readonly JabberClient _client;
		
		readonly ManualResetEvent _authPending = new ManualResetEvent(false);
		readonly ILog _log;

		public event XmppCertificateValidationCallback OnInvalidCertificate = (certificate, chain, errors) => false;
		
		public XmppCommunicator(IXmppSettings settings, ILogProvider provider)
		{
			var jid = new JID(settings.JabberId);

			_client = new JabberClient
				{
					User = jid.User,
					Server = jid.Server,
					Resource = jid.Resource,
					NetworkHost = settings.NetworkHost,
					Password = settings.Password,
					AutoRoster = settings.Has(XmppOptions.AutoRoster),
					AutoLogin = settings.Has(XmppOptions.AutoLogin),
					AutoPresence = settings.Has(XmppOptions.AutoPresence),
					AutoIQErrors = settings.Has(XmppOptions.AutoIQErrors),
					AutoReconnect = 60,
					KeepAlive = 60,
				};

			_log = provider.Get(typeof(XmppCommunicator).FullName);

			DebugLogSettings(settings);

			_client.OnAuthenticate += OnAuthenticate;
			_client.OnAuthError += OnClientAuthError;
			_client.OnDisconnect += OnDisconnect;
			_client.OnInvalidCertificate += OnClientInvalidCertificate;
		}

		void DebugLogSettings(IXmppSettings settings)
		{
			var pwd = string.IsNullOrEmpty(settings.Password) ? "<empty>" : (settings.Password.Length + " chars");
			_log.DebugFormat("XMPP connecting. JID: '{0}', Priority: {1}, Host: '{2}', Pwd: {3}, Option: {4}",
				settings.JabberId,
				settings.Priority,
				settings.NetworkHost,
				pwd,settings.Options
				);
		}

		void OnClientAuthError(object sender, XmlElement rp)
		{
			_authPending.Set();
			var exception = new InvalidOperationException("Authentication failure");
			exception.Data.Add("xml", rp);
			throw exception;
		}

		bool OnClientInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return OnInvalidCertificate(certificate, chain, sslPolicyErrors);
		}


		void OnAuthenticate(object sender)
		{
			_authPending.Set();
		}
		void OnDisconnect(object sender)
		{
			_authPending.Reset();
		}

		public void StartUp()
		{
			_client.Connect();
			if (!_authPending.WaitOne(6000))
			{
				throw new InvalidOperationException("Failed to authenticate in time");
			}
		}
		public void Dispose()
		{
			_client.Close();
			_client.OnDisconnect -= OnDisconnect;
			_client.OnAuthenticate -= OnAuthenticate;
			_client.OnInvalidCertificate -= OnClientInvalidCertificate;
			_client.OnAuthError -= OnClientAuthError;
			_authPending.Close();
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