#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;
using Autofac.Core;
using jabber;
using Lokad.Messaging;
using Lokad.Settings;

namespace Lokad.Cqrs.Xmpp
{
	public sealed class GoogleTalkNotificationsModule : IModule
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public Maybe<string> Resource { get; set; }
		public Maybe<string> Identity { get; set; }
		public Maybe<string> Password { get; set; }
		public Maybe<string> NetworkHost { get; set; }

		public XmppOptions Options { get; set; }
		public int Priority { get; set; }


		Maybe<XmppCertificateValidationCallback> _callback = Maybe<XmppCertificateValidationCallback>.Empty;

		public GoogleTalkNotificationsModule()
		{
			Resource = Maybe<string>.Empty;
			Identity = Maybe<string>.Empty;
			Password = Maybe<string>.Empty;
			NetworkHost = Maybe<string>.Empty;

			Options = XmppOptions.AutoIQErrors | XmppOptions.AutoLogin | XmppOptions.AutoPresence | XmppOptions.AutoRoster;
		}

		Maybe<string> LoadString(IComponentContext registry, string name)
		{
			return registry
				.Resolve<ISettingsProvider>()
				.GetValue(name);
		}

		public GoogleTalkNotificationsModule OnCertificateError(XmppCertificateValidationCallback callback)
		{
			_callback = callback;
			return this;
		}

		void IModule.Configure(IComponentRegistry reg)
		{
			_builder.Register(ctx =>
				{
					var settings = new XmppSettings
						{
							Options = Options,
							Priority = Priority
						};


					Identity
						.GetValue(LoadString(ctx, "XmppIdentity"))
						.Convert(s => new JID(s))
						.Apply(jid =>
							{
								settings.UserName = jid.User;
								settings.Resource = Resource.GetValue(jid.Resource);
								settings.Server = jid.Server;
							})
						.ExposeException("Failed to load 'Identity' property or 'XmppIdentity'");

					NetworkHost
						.GetValue(LoadString(ctx, "XmppNetworkHost"))
						.Apply(s => settings.NetworkHost = s);

					return settings;
				})
				.As<IXmppSettings>()
				.SingleInstance();


			var builder = _builder.RegisterType<XmppNotifier>().As<IRealtimeNotifier, IStartable>();

			_callback.Apply(c => builder.OnActivated(args => args.Instance.OnInvalidCertificate += c));
			_builder.Update(reg);
		}
	}
}