#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.Collections;
using System.Text;
using Lokad.Cqrs;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Newtonsoft.Json;

namespace Sample_04.Worker
{
	public sealed class CustomExceptionLogger
	{
		
		readonly CloudBlobContainer _container;

		public CustomExceptionLogger(CloudStorageAccount account)
		{
			_container = account.CreateCloudBlobClient().GetContainerReference("errors");
			_container.CreateIfNotExist();
		}

		public void Handle(UnpackedMessage message, Exception exception)
		{
			var builder = new StringBuilder();
			var identity = message.Attributes.GetAttributeString(MessageAttributeType.Identity).GetValue("unknown");
			var dateTime = DateTime.UtcNow;
			builder.AppendFormat("{0} - {1}", dateTime, message.ContractType.Name).AppendLine();

			builder.AppendLine(MessagePrinter.PrintAttributes(message.Attributes));

			
			builder.AppendLine();
			RenderException(exception, builder);

			builder.AppendLine();
			RenderContent(message, builder);

			var fileName = dateTime.ToString("yyyy-MM-dd-hh-mm-ss") + "-" + identity + ".txt";
			_container.GetBlobReference(fileName.ToLowerInvariant()).UploadText(builder.ToString());
		}

		static void RenderContent(UnpackedMessage message, StringBuilder builder)
		{
			builder.AppendLine("Content").AppendLine("=======");
			builder.AppendLine(message.ContractType.Name);
			try
			{
				builder.AppendLine(JsonConvert.SerializeObject(message.Content, Formatting.Indented));
			}
			catch (Exception ex)
			{
				builder.AppendLine(ex.ToString());
			}
		}

		static void RenderException(Exception ex, StringBuilder writer)
		{
			writer.AppendLine("Exception").AppendLine("=========");
			writer.AppendLine(string.Format("Type: {0}", ex.GetType().FullName));
			writer.AppendLine(string.Format("Message: {0}", ex.Message));
			writer.AppendLine(string.Format("Source: {0}", ex.Source));
			writer.AppendLine(string.Format("TargetSite: {0}", ex.TargetSite));
			foreach (DictionaryEntry entry in ex.Data)
			{
				writer.AppendLine(string.Format("{0}: {1}", entry.Key, entry.Value));
			}
			writer.AppendLine(string.Format("StackTrace: {0}", ex.StackTrace));
		}
	}
}