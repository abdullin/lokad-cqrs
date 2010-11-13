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


			//var client =
			//    CloudStorageAccount.Parse(
			//        "DefaultEndpointsProtocol=http;AccountName=salescast;AccountKey=DnI8s1Rv7I7+hUWo/okGSuY3E5hR194MG5uqrARQsi2w2SOhHsLF2SjO5s5Up8GC2GyhSd8FAKh9VDH0kGPAFA==")
			//        .CreateCloudBlobClient();
			client.RetryPolicy = RetryPolicies.NoRetry();

			var root = new BlobStorageRoot(client, DebugLog.Provider);
			var cont = root.GetContainer("tests").Create();

			var storageItem = cont.GetItem("test");


			storageItem.WriteText("Cool");
			storageItem.ReadText();



			storageItem.Write(s =>
			{
				using (var writer = new StreamWriter(s))
				{
					writer.Write("Cool2");
				}
			}, options:StorageWriteOptions.CompressIfPossible);
			storageItem.ReadText();
		}
	}
}