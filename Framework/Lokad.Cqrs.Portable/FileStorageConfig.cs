#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;

namespace Lokad.Cqrs
{
    public sealed class FileStorageConfig
    {
        public DirectoryInfo Folder { get; private set; }
        public string AccountName { get; private set; }

        public FileStorageConfig(DirectoryInfo folder, string accountName)
        {
            Folder = folder;
            AccountName = accountName;
        }

        public void Wipe()
        {
            if (Folder.Exists)
                Folder.Delete(true);
        }
    }
}