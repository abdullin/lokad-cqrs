#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Xmpp;
using Lokad.Messaging;

namespace Lokad.Cqrs
{
	public static class ExtendContainerBuilderSyntax
	{
		/// <summary>
		/// Uses notifications Notifies the via google talk.
		/// </summary>
		/// <typeparam name="TModule">The type of the module.</typeparam>
		/// <param name="module">The module.</param>
		/// <param name="configure">The configure.</param>
		/// <returns></returns>
		public static TModule CommunicateWithXmpp<TModule>(this TModule module,
			Action<XmppCommunicationModule> configure)
			where TModule : ISyntax<ContainerBuilder, IRealtimeNotifier>
		{
			var m = new XmppCommunicationModule();
			configure(m);
			module.Target.RegisterModule(m);
			return module;
		}
	}
}