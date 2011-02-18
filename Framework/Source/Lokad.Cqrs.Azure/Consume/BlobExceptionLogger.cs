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

	public delegate void PrintMessageErrorDelegate(MessageEnvelope message, Exception ex, TextWriter writer);
	public sealed class BlobExceptionLogger
	{
		readonly CloudBlobContainer _container;

		public BlobExceptionLogger(CloudStorageAccount account, string containerName)
		{
			_container = account.CreateCloudBlobClient().GetContainerReference(containerName);
			_container.CreateIfNotExist();
		}

		public event PrintMessageErrorDelegate OnRender = (message, exception, arg3) => { };

		public void Handle(MessageEnvelope message, Exception exception)
		{
			// get identity of the message or just unknown string
			var identity = message.EnvelopeId;

			// get creation time of message, falling back to current date
			var date = message.GetAttributeValue<DateTime>(EnvelopeAttribute.CreatedUtc).GetValue(DateTime.UtcNow);

			var builder = new StringBuilder();
			using (var writer = new StringWriter(builder, CultureInfo.InvariantCulture))
			{
				writer.WriteLine(message.ContractType.Name);
				writer.WriteLine();

				MessagePrinter.PrintAttributes(message.GetAllAttributes(), writer, "  ");

				RenderDelegates(message, writer, exception);

				writer.WriteLine();
				RenderException(exception, writer);
			}
			// we might have multiple errors for this message

			var landingTime = Math.Round((DateTime.UtcNow - date).TotalSeconds,0);

			// error for Lokad.CQRS message would be in format:
			// 2010-02-08-13-58-7b19df43-344c-4488-91ae-960d8a843318-0000127.txt
			// Sending time     Message identity                     error time as seconds since the creation


			var fileName = string.Format("{0:yyyy-MM-dd-HH-mm}-{1}-{2:0000000}.txt", date, identity, landingTime);
			var blob = _container.GetBlobReference(fileName.ToLowerInvariant());
			blob.UploadText(builder.ToString());
		}

		void RenderDelegates(MessageEnvelope message, StringWriter writer, Exception exception)
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

		static void RenderException(Exception ex, TextWriter writer)
		{
			writer.WriteLine("Exception Detail");
			writer.WriteLine("================");
			while (true)
			{
				RenderExceptionBody(writer, ex);

				if (null == ex.InnerException)
					return;

				writer.WriteLine();
				writer.WriteLine("Inner Exception");
				writer.WriteLine("---------------");
				ex = ex.InnerException;
			}
		}

		static void RenderExceptionBody(TextWriter writer, Exception ex)
		{
			writer.WriteLine("Type:        {0}", ex.GetType().FullName);
			writer.WriteLine("Message:     {0}", ex.Message);
			writer.WriteLine("Source:      {0}", ex.Source);
			writer.WriteLine("Recorded:    {0}", DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss-ffff"));
			writer.WriteLine("TargetSite:  {0}", ex.TargetSite);
			foreach (DictionaryEntry entry in ex.Data)
			{
				writer.WriteLine("{0}: {1}", entry.Key, entry.Value);
			}
			writer.WriteLine("StackTrace:");
			writer.WriteLine(ex.StackTrace);
		}
	}

	
}