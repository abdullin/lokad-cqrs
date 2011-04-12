#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Lokad.Cqrs.Feature.SimpleStorage;
using Lokad.Cqrs.Feature.SimpleStorage.Blob;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;

namespace Lokad.Cqrs.Tests.Storage
{
	[TestFixture]
	public sealed class HashingTests
	{
		// ReSharper disable InconsistentNaming

		[Test, Explicit]
		public void Test()
		{
			var client = BlobStorage.GetCustom();
			
			client.RetryPolicy = RetryPolicies.NoRetry();

			var root = new BlobStorageRoot(client);
			var cont = root.GetContainer("tests").Create();

			var storageItem = cont.GetItem("test");


			storageItem.Write(w => w.WriteByte(1), options:StorageWriteOptions.CompressIfPossible);
			storageItem.ReadInto((props, stream) => StreamUtil.BlockCopy(stream, new MemoryStream(), 10));

			var format = storageItem.GetInfo();
			Console.WriteLine("MD5: {0}", format.Value.Properties.GetValue("ContentMD5").GetValue("None"));

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