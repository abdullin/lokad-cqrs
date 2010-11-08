#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;
using Lokad.Cqrs.Storage;
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
			const string uri = "http://ipv4.fiddler:10000/devstoreaccount1";
			var credentials = CloudStorageAccount.DevelopmentStorageAccount.Credentials;
			var client = new CloudBlobClient(uri, credentials);
			client.RetryPolicy = RetryPolicies.NoRetry();

			var root = new BlobStorageRoot(client, DebugLog.Provider);
			var cont = root.GetContainer("hash-test").Create();

			var storageItem = cont.GetItem("test");


			storageItem.ReadInto((props, designationStream) => designationStream.PumpTo(new MemoryStream(), 20));


			var b = new byte[0];

			storageItem.Write(stream => { });
		}
	}
}