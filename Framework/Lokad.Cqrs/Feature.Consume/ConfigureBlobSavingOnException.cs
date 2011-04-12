using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Feature.Consume
{
	public sealed class ConfigureBlobSavingOnException
	{
		public string ContainerName { get; set; }

		readonly IList<PrintMessageErrorDelegate> _printers = new List<PrintMessageErrorDelegate>();

		public void WithTextAppender(PrintMessageErrorDelegate @delegate)
		{
			_printers.Add(@delegate);
		}

		public ConfigureBlobSavingOnException()
		{
			ContainerName = "errors";
		}

		internal void Apply(SingleThreadConsumingProcess transport, IComponentContext context)
		{
			var account = context.Resolve<CloudStorageAccount>();
			var logger = new BlobExceptionLogger(account, ContainerName);

			foreach (var @delegate in _printers)
			{
				logger.OnRender += @delegate;
			}

			//Action<UnpackedMessage, Exception> action = logger.Handle;
			throw new NotImplementedException();
			//transport.MessageHandlerFailed += action;

			//context.WhenDisposed(() =>
			//    {
			//        transport.MessageHandlerFailed -= action;
			//    });
		}
	}
}