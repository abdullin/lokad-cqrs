#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    public enum StreamingConditionType
    {
        None,
        /// <summary>
        /// Only perform the action if the client supplied entity matches the same entity on the server. 
        /// This is mainly for methods like PUT to only update a resource if it has not been modified since 
        /// the user last updated it.
        /// </summary>
        IfMatch,
        IfNoneMatch
    }
}