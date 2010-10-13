#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Autofac;
using Lokad.Messaging;
using Lokad.Settings;

namespace Lokad.Cqrs.Consume.Build
{
	public sealed class ConfigureNotificationOnException
	{
		public string ConfigKeyForRecipient { get; set; }

		public ConfigureNotificationOnException()
		{
			ConfigKeyForRecipient = "NotifyOnException";
		}

		Func<UnpackedMessage, Exception, IMessageProfiler, string> _printMessage = PrintMessage;

		static string PrintMessage(UnpackedMessage unpackedMessage, Exception exception, IMessageProfiler arg3)
		{
			return string.Format(":-( {0} -> {1}", arg3.GetReadableMessageInfo(unpackedMessage), exception.Message);
		}

		public void FormatMessageWith(Func<UnpackedMessage, Exception, IMessageProfiler, string> formatter)
		{
			_printMessage = formatter;
		}


		internal void Apply(IMessageTransport transport, IComponentContext context)
		{
			var profiler = context.Resolve<IMessageProfiler>();
			var notifier = context.Resolve<ICommunicator>();
			var recipient = context.Resolve<ISettingsProvider>()
				.GetValue(ConfigKeyForRecipient)
				.ExposeException("Failed to load recipient address for '{0}'.", ConfigKeyForRecipient);

			Action<UnpackedMessage, Exception> action =
				(message, exception) => notifier.Notify(recipient, _printMessage(message, exception, profiler));
			transport.MessageHandlerFailed += action;

			context.WhenDisposed(() => { transport.MessageHandlerFailed -= action; });
		}
	}
}