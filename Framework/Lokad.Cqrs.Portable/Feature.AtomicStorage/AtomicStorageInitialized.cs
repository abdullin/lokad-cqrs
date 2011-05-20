#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AtomicStorageInitialized : ISystemEvent
    {
        public readonly string[] CreatedFolders;
        public readonly Type Storage;

        public AtomicStorageInitialized(string[] createdFolders, Type storage)
        {
            CreatedFolders = createdFolders;
            Storage = storage;
        }

        public override string ToString()
        {
            return string.Format("{1} created: {0}", string.Join(", ", CreatedFolders), Storage.Name);
        }
    }
}