#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Consume
{


	public sealed class BlobExceptionLogger
	{
		readonly CloudBlobContainer _container;

		public BlobExceptionLogger(CloudStorageAccount account, string containerName)
		{
			_container = account.CreateCloudBlobClient().GetContainerReference(containerName);
			_container.CreateIfNotExist();
		}

		public event PrintMessageErrorDelegate OnRender = (message, exception, arg3) => { };

		public void Handle(UnpackedMessage message, Exception exception)
		{
			// get identity of the message or just unknown string
			var identity = message.Attributes
				.GetAttributeString(MessageAttributeTypeContract.Identity)
				.GetValue("unknown");

			// get creation time of message, falling back to current date
			var date = message.Attributes
				.GetAttributeDate(MessageAttributeTypeContract.CreatedUtc)
				.GetValue(DateTime.UtcNow);

			var builder = new StringBuilder();
			using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
			{
				writer.WriteLine(message.ContractType.Name);
				writer.WriteLine();

				MessagePrinter.PrintAttributes(message.Attributes, writer, "  ");

				RenderDelegates(message, writer, exception);

				writer.WriteLine();
				RenderException(exception, writer);
			}

			var fileName = date.ToString("yyyy-MM-dd-hh-mm-ss") + "-" + identity + ".txt";
			_container.GetBlobReference(fileName.ToLowerInvariant()).UploadText(builder.ToString());
		}

		void RenderDelegates(UnpackedMessage message, StringWriter writer, Exception exception)
		{
			foreach (PrintMessageErrorDelegate @delegate in OnRender.GetInvocationList())
			{
				writer.WriteLine();

				try
				{
					@delegate(message, exception, writer);
				}
				catch (Exception ex)
				{
					writer.WriteLine("Failure on render handler");
					writer.WriteLine(ex);
				}
			}
		}

		//static void RenderContent(UnpackedMessage message, StringBuilder builder)
		//{
		//    builder.AppendLine("Content").AppendLine("=======");
		//    builder.AppendLine(message.ContractType.Name);
		//    try
		//    {
		//        builder.AppendLine(JsonConvert.SerializeObject(message.Content, Formatting.Indented));
		//    }
		//    catch (Exception ex)
		//    {
		//        builder.AppendLine(ex.ToString());
		//    }
		//}

		static void RenderException(Exception ex, TextWriter writer)
		{
			writer.WriteLine("Exception");
			writer.WriteLine("=========");
			writer.WriteLine("Type: {0}", ex.GetType().FullName);
			writer.WriteLine("Message: {0}", ex.Message);
			writer.WriteLine("Source: {0}", ex.Source);
			writer.WriteLine("TargetSite: {0}", ex.TargetSite);
			foreach (DictionaryEntry entry in ex.Data)
			{
				writer.WriteLine("{0}: {1}", entry.Key, entry.Value);
			}
			writer.WriteLine("StackTrace: {0}", ex.StackTrace);
		}
	}

	public delegate void PrintMessageErrorDelegate(UnpackedMessage message, Exception ex, TextWriter writer);
}