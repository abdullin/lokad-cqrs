#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public interface IAtomicStorageSerializer
    {
        void Serialize<TView>(TView view, Stream stream);
        TView Deserialize<TView>(Stream stream);
    }
}