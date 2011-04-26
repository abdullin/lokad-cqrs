#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    [Serializable]
    public class StorageBaseException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public StorageBaseException()
        {
        }

        public StorageBaseException(string message) : base(message)
        {
        }

        public StorageBaseException(string message, Exception inner) : base(message, inner)
        {
        }

        protected StorageBaseException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}