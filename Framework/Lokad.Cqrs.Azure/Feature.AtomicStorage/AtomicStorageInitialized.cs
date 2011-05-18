#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AtomicStorageInitialized : ISystemEvent
    {
        public string[] CreatedFolders;

        public AtomicStorageInitialized(string[] createdFolders)
        {
            CreatedFolders = createdFolders;
        }

        public override string ToString()
        {
            return string.Format("Azure atomic storage created: {0}", string.Join(", ", CreatedFolders));
        }
    }
}