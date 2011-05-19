#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Core.Envelope;

namespace Lokad.Cqrs
{
    /// <summary>
    /// Generic message publishing interface that is provided by the infrastructure, should user configure it for publishing
    /// </summary>
    public interface IMessageSender
    {
        /// <summary>
        /// Sends the specified messages to the designated recipient.
        /// </summary>
        /// <param name="content">The message to send.</param>
        void SendOne(object content);
        void SendOne(object content, Action<EnvelopeBuilder> configure);


        
        void SendBatch(object[] content);
        void SendBatch(object[] content, Action<EnvelopeBuilder> builder);
    }
}