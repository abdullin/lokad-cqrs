using System;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Feature.Send;

namespace Lokad.Cqrs.Core.Dispatch
{
	///<summary>
	/// Simple dispatcher that forwards messages according to the rules.
	/// We don't care about duplication management, since recipients will do that.
	///</summary>
	public sealed class DispatchMessagesToRoute : ISingleThreadMessageDispatcher
	{
		readonly IWriteQueueFactory _queueFactory;
		Func<MessageEnvelope, string> _routerRule;

		public DispatchMessagesToRoute(IWriteQueueFactory queueFactory)
		{
			_queueFactory = queueFactory;
		}


		public void DispatchMessage(MessageEnvelope message)
		{
			var route = _routerRule(message);
			var queue = _queueFactory.GetWriteQueue(route);
			queue.SendMessage(message);
		}

		public void SpecifyRouter(Func<MessageEnvelope, string>  router)
		{
			_routerRule = router;
		}

		public void Init()
		{
			if (null == _routerRule)
			{
				throw new InvalidOperationException("Message router must be configured!");
			}
		}
	}
}