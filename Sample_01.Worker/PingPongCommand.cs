using System.Runtime.Serialization;
using CloudBus;

namespace Sample_01.Worker
{
	[DataContract]
	public sealed class PingPongCommand : IBusMessage
	{
		[DataMember]
		public int Ball { get; set; }
		[DataMember]
		public string Game { get; set; }
	}
}