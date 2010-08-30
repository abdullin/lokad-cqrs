#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using jabber;
using jabber.client;
using jabber.protocol.client;
using Lokad.Cqrs.Messaging;
using Lokad.Messaging;
using Lokad.Quality;

namespace Lokad.Cqrs.Xmpp
{
	[UsedImplicitly]
	public class XmppCommunicator : IEngineProcess, ICommunicator
	{
		readonly ManualResetEventSlim _authPending = new ManualResetEventSlim(false);
		readonly JabberClient _client;
		readonly ILog _log;
		readonly Subject<ICommunicatorMessage> _subject = new Subject<ICommunicatorMessage>();

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

			_log = provider.LogForName(this);

			DebugLogSettings(settings);
		}

		public void Notify(string recipient, string body, CommunicationType options)
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

		public IDisposable Subscribe(IObserver<ICommunicatorMessage> observer)
		{
			return _subject.Subscribe(observer);
		}

		public void Dispose()
		{
			_subject.OnCompleted();
			_client.Dispose();
		}

		public void Initialize()
		{
			_log.Debug("Initialize");
		}

		public Task Start(CancellationToken token)
		{
			_log.Debug("Start");
			return Task.Factory
				.StartNew(() => RunBody(token))
				.ContinueWith(t => _log.Debug("XMPP Exited"));
		}

		public event XmppCertificateValidationCallback OnInvalidCertificate = (certificate, chain, errors) => false;


		void ClientOnOnMessage(object sender, Message msg)
		{
			_subject.OnNext(new XmppMessageWrapper(msg));
		}

		void DebugLogSettings(IXmppSettings settings)
		{
			var pwd = string.IsNullOrEmpty(settings.Password) ? "<empty>" : (settings.Password.Length + " chars");
			_log.DebugFormat("XMPP connecting. JID: '{0}', Priority: {1}, Host: '{2}', Pwd: {3}, Option: {4}",
				settings.JabberId,
				settings.Priority,
				settings.NetworkHost,
				pwd, settings.Options
				);
		}

		void OnClientAuthError(object sender, XmlElement rp)
		{
			_authPending.Set();
			var exception = new InvalidOperationException("Authentication failure");
			exception.Data.Add("xml", rp);
			throw exception;
		}

		bool OnClientInvalidCertificate(object sender, X509Certificate certificate, X509Chain chain,
			SslPolicyErrors sslPolicyErrors)
		{
			return OnInvalidCertificate(certificate, chain, sslPolicyErrors);
		}


		void OnAuthenticate(object sender)
		{
			_log.Debug("OnAuthenticate");
			_authPending.Set();
		}

		void OnDisconnect(object sender)
		{
			_authPending.Reset();
		}


		void RunBody(CancellationToken token)
		{
			try
			{
				_authPending.Reset();

				_client.OnAuthenticate += OnAuthenticate;
				_client.OnAuthError += OnClientAuthError;
				_client.OnDisconnect += OnDisconnect;
				_client.OnInvalidCertificate += OnClientInvalidCertificate;
				_client.OnMessage += ClientOnOnMessage;
				_client.OnError += ClientOnOnError;

				_client.Connect();
				if (!_authPending.Wait(6.Seconds(), token))
				{
					throw new InvalidOperationException("Failed to authenticate in time");
				}

				token.WaitHandle.WaitOne();
			}
			catch (Exception ex)
			{
				_log.Error(ex, "XMPP failed");
			}
			finally
			{
				_log.Debug("Closing");
				_client.OnDisconnect -= OnDisconnect;
				_client.OnAuthenticate -= OnAuthenticate;
				_client.OnInvalidCertificate -= OnClientInvalidCertificate;
				_client.OnAuthError -= OnClientAuthError;
				_client.OnMessage -= ClientOnOnMessage;
				_client.OnError -= ClientOnOnError;
				_client.Close();

				_authPending.Dispose();
			}
		}

		void ClientOnOnError(object sender, Exception exception)
		{
			_subject.OnError(exception);
		}
	}

	public sealed class XmppMessageWrapper : ICommunicatorMessage
	{
		public readonly Message Message;

		public XmppMessageWrapper(Message message)
		{
			Message = message;
		}

		public string Subject
		{
			get { return Message.Subject; }
		}

		public string Body
		{
			get { return Message.Body; }
		}

		public string Thread
		{
			get { return Message.Thread; }
		}

		public string Sender
		{
			get { return Message.From.ToString(); }
		}

		public CommunicationType Type
		{
			get { return EnumUtil<CommunicationType>.ConvertSafelyFrom(Message.Type); }
		}
	}
}