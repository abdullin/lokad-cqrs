using System;
using System.Collections.Generic;
using System.IO;
using Lokad.Cqrs.Core.Transport;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using ProtoBuf;
using System.Linq;

namespace Lokad.Cqrs.Feature.AzurePartition
{
	public sealed class StatelessAzureFutureList
	{
		readonly ISystemObserver _observer;
		readonly IMessageSerializer _serializer;
		readonly CloudBlobContainer _container;
		readonly CloudBlob _blob;


		public StatelessAzureFutureList(CloudStorageAccount account, string queueName, ISystemObserver observer, IMessageSerializer serializer)
		{
			_observer = observer;
			_serializer = serializer;
			_container = account.CreateCloudBlobClient().GetContainerReference(queueName);
			_blob = _container.GetBlobReference("__schedule.bin");
		}

		public void Initialize()
		{
			_container.CreateIfNotExist();
		}

		public void PutMessage(MessageEnvelope envelope)
		{
			if (envelope.DeliverOn == default(DateTimeOffset))
				throw new InvalidOperationException();

			

			ScheduleItem view;
			try
			{
				// use cached etags with stateful strategy, if needed
				
				var schedule = _blob.DownloadByteArray();
				using (var mem = new MemoryStream(schedule))
				{
					view = Serializer.Deserialize<ScheduleItem>(mem);
				}
			}
			catch (StorageClientException ex)
			{
				view = new ScheduleItem();
			}
			var id = "__scheduled-" + envelope.EnvelopeId + ".bin";

			if (view.References.ContainsKey(id))
			{
				// already added
				return;
			}

			_container.GetBlobReference(id).UploadByteArray(MessageUtil.SaveDataMessage(envelope, _serializer));
			
			view.References.Add(id, envelope.DeliverOn);
			using(var mem = new MemoryStream())
			{
				Serializer.Serialize(mem, view);
				_blob.UploadByteArray(mem.ToArray());
			}
		}

		public void TranferPendingMessages(Action<MessageEnvelope> atomicTranfer)
		{
			ScheduleItem view;
			try
			{
				// use cached etags with stateful strategy, if needed

				var schedule = _blob.DownloadByteArray();
				using (var mem = new MemoryStream(schedule))
				{
					view = Serializer.Deserialize<ScheduleItem>(mem);
				}

			}
			catch (StorageClientException ex)
			{
				return;
			}

			var now = DateTimeOffset.UtcNow;
			var pending = view.References.Where(r => r.Value <= now).ToArray();
			if (pending.Length == 0)
			{
				return;
			}

			// dispatch
			foreach (var pair in pending)
			{
				var item = _container.GetBlobReference(pair.Key).DownloadByteArray();
				var env = MessageUtil.ReadDataMessage(item, _serializer);
				atomicTranfer(env);
				view.References.Remove(pair.Key);
			}
			// commit
			using (var mem = new MemoryStream())
			{
				Serializer.Serialize(mem, view);
				_blob.UploadByteArray(mem.ToArray());
			}
			// cleanup
			foreach (var pair in pending)
			{
				_container.GetBlobReference(pair.Key).BeginDeleteIfExists(ar => { }, null);
			}
			
		}

		[ProtoContract]
		public sealed class ScheduleItem
		{
			[ProtoMember(1)]
			public IDictionary<string, DateTimeOffset> References = new Dictionary<string, DateTimeOffset>();

			public ScheduleItem()
			{
				References = new Dictionary<string, DateTimeOffset>();
			}
		}
	}
}