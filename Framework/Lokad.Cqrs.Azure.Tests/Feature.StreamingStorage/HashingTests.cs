#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Lokad.Cqrs.Evil;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    [TestFixture]
    public sealed class HashingTests
    {
        // ReSharper disable InconsistentNaming

        [Test, Explicit]
        public void Test()
        {
            var client = Play_all_for_BlobStreaming.GetCustom();

            client.RetryPolicy = RetryPolicies.NoRetry();

            var root = new BlobStorageRoot(client);
            var cont = root.GetContainer("tests").Create();

            var storageItem = cont.GetItem("test");


            storageItem.Write(w => w.WriteByte(1), options : StorageWriteOptions.CompressIfPossible);
            storageItem.ReadInto((props, stream) => StreamUtil.BlockCopy(stream, new MemoryStream(), 10));

            var format = storageItem.GetInfo();

            string ctx;
            if (!format.Value.Properties.TryGetValue("ContentMD5", out ctx))
            {
                ctx = "None";
            }

            Console.WriteLine("MD5: {0}", ctx);

            //storageItem.ReadText();
        }

        static CloudBlobClient GetFiddler()
        {
            const string uri = "http://ipv4.fiddler:10000/devstoreaccount1";
            var credentials = CloudStorageAccount.DevelopmentStorageAccount.Credentials;
            return new CloudBlobClient(uri, credentials);
        }
    }
}