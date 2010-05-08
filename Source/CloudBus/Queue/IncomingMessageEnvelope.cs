using System.Collections.Specialized;
using Lokad;
using Microsoft.WindowsAzure.StorageClient;

namespace Bus2.Queue
{
	public sealed class IncomingMessageEnvelope
	{
		readonly object _message;
		readonly string _blobReference;
		readonly string _typeReference;

		public readonly NameValueCollection Headers;
		public readonly bool ContainsMessageObject;
		public readonly string TransportMessageId;
		public readonly string Topic;
		public readonly string Sender;

		public object MessageObject
		{
			get
			{
				Enforce.That(ContainsMessageObject, "Don't ask for message without checking first");
				return _message;
			}
		}

		public string DataReference
		{
			get
			{
				Enforce.That(!ContainsMessageObject, "Don't ask for reference, when there is object");
				return _blobReference;
			}
		}

		public string TypeReference
		{
			get
			{
				Enforce.That(!ContainsMessageObject, "Don't ask for reference, when there is object");
				return _typeReference;
			}
		}

		public override string ToString()
		{
			if (null == _message)
				return "[Blob]";
			return "[" + _message.GetType().Name + "]";
		}


		public IncomingMessageEnvelope(MessageMessage message, CloudQueueMessage cloudMessage)
		{
			_message = message.Message;
			TransportMessageId = cloudMessage.Id;


			Headers = new NameValueCollection();
			foreach (var headerInfo in message.Headers)
			{
				Headers.Add(headerInfo.Key, headerInfo.Value);
			}

			Headers["azure-receipt"] = cloudMessage.PopReceipt;

			Topic = Headers["topic"];
			Sender = Headers["sender"];


			ContainsMessageObject = _message != null;

			if (!ContainsMessageObject)
			{
				_blobReference = Headers["content-blob-ref"];
				_typeReference = Headers["content-blob-type"];
			}
		}
	}
}