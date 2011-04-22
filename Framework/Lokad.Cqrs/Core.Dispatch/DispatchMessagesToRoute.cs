using System;
using System.Collections.Generic;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Transport;
using System.Linq;

namespace Lokad.Cqrs.Core.Dispatch
{
	///<summary>
	/// Simple dispatcher that forwards messages according to the rules.
	/// We don't care about duplication management, since recipients will do that.
	///</summary>
	public sealed class DispatchMessagesToRoute : ISingleThreadMessageDispatcher
	{
		readonly IEnumerable<IQueueWriterFactory> _queueFactory;
		Func<MessageEnvelope, string> _routerRule;

		public DispatchMessagesToRoute(IEnumerable<IQueueWriterFactory> queueFactory)
		{
			// static implementation for now
			_queueFactory = queueFactory.ToArray();
		}


		public void DispatchMessage(MessageEnvelope message)
		{
			var route = _routerRule(message);

			foreach (var factory in _queueFactory)
			{
				IQueueWriter writer;
				if (factory.TryGetWriteQueue(route, out writer))
				{
					writer.PutMessage(message);
				}
			}
		}

		public void SpecifyRouter(Func<MessageEnvelope, string> router)
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