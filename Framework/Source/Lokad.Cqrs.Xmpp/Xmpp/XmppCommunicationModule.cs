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
	public sealed class XmppCommunicationModule : IModule
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public Maybe<string> Resource { get; set; }
		public string ConfigKeyForIdentity { get; set; }
		public string ConfigKeyForPassword { get; set; }
		public string ConfigKeyForNetworkHost { get; set; }

		public XmppOptions Options { get; set; }
		public int Priority { get; set; }


		Maybe<XmppCertificateValidationCallback> _callback = Maybe<XmppCertificateValidationCallback>.Empty;

		public XmppCommunicationModule()
		{
			Resource = Maybe<string>.Empty;
			ConfigKeyForIdentity = "XmppIdentity";
			ConfigKeyForNetworkHost = "XmppNetworkHost";
			ConfigKeyForPassword = "XmppPassword";

			Options = XmppOptions.AutoIQErrors | XmppOptions.AutoLogin | XmppOptions.AutoPresence | XmppOptions.AutoRoster;
		}

		static Maybe<string> LoadOptionalString(ISettingsProvider settings, string name)
		{
			return settings.GetValue(name);
		}

		static string LoadRequiredString(ISettingsProvider setting, string name)
		{
			return LoadOptionalString(setting, name)
				.ExposeException("Failed to load key '{0}' from the settings", name);
		}

		public XmppCommunicationModule OnCertificateError(XmppCertificateValidationCallback callback)
		{
			_callback = callback;
			return this;
		}

		void IModule.Configure(IComponentRegistry reg)
		{
			_builder.Register(ctx =>
				{
					var provider = ctx.Resolve<ISettingsProvider>();

					var identity = LoadRequiredString(provider, ConfigKeyForIdentity);
					var jid = new JID(identity);

					var settings = new XmppSettings
						{
							Options = Options,
							Priority = Priority,
							UserName = jid.User,
							Resource = Resource.GetValue(jid.Resource),
							Server = jid.Server,
							Password =  LoadRequiredString(provider, ConfigKeyForPassword),
						};

					LoadOptionalString(provider, ConfigKeyForNetworkHost)
						.Apply(s => settings.NetworkHost = s);

					return settings;

				})
				.As<IXmppSettings>()
				.SingleInstance();


			var builder = _builder.RegisterType<XmppCommunicator>()
				.As<IRealtimeNotifier>()
				.As<IStartable>()
				.SingleInstance();

			_callback.Apply(c => builder.OnActivated(args => args.Instance.OnInvalidCertificate += c));
			_builder.Update(reg);
		}
	}
}