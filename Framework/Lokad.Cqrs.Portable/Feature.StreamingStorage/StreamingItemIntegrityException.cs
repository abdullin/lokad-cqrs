#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// Happens, when data corruption was detected. Normally retrying the operation should solve the problem
    /// </summary>
    [Serializable]
    public class StreamingItemIntegrityException : StreamingBaseException
    {
        public StreamingItemIntegrityException()
        {
        }

        public StreamingItemIntegrityException(string message) : base(message)
        {
        }

        public StreamingItemIntegrityException(string message, Exception inner) : base(message, inner)
        {
        }

        protected StreamingItemIntegrityException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}