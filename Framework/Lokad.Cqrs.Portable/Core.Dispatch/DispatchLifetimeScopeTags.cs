#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Autofac;

namespace Lokad.Cqrs.Core.Dispatch
{
    /// <summary>
    /// Tags used to differentiate lifetime scopes for handling message envelopes.
    /// </summary>
    public static class DispatchLifetimeScopeTags
    {
        /// <summary>
        /// Used to mark <see cref="ILifetimeScope"/> created for processing message envelopes.
        /// </summary>
        public const string MessageEnvelopeScopeTag = "Envelope(UoW)";

        /// <summary>
        /// Used to mark nested <see cref="ILifetimeScope"/> created for processing individual message items within an envelope.
        /// </summary>
        public const string MessageItemScopeTag = "Item(Handler)";
    }
}