using System;
using System.Collections.Generic;
using Autofac;
using Lokad.Cqrs.Transport;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Consume.Build
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

		internal void Apply(ConsumingProcess transport, IComponentContext context)
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