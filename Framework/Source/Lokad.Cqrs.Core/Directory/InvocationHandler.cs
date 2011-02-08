using System;
using System.Diagnostics;
using System.Reflection;

namespace Lokad.Cqrs.Directory
{
	public sealed class InvocationHandler
	{
		
		readonly Func<MessageAttributesContract, object> _contextProvider;
		readonly InvocationHint _hint;
		
		
		[DebuggerNonUserCode]
		public void Invoke([NotNull]object messageHandler, [NotNull]object messageInstance, MessageAttributesContract attributes)
		{
			if (messageHandler == null) throw new ArgumentNullException("messageHandler");
			if (messageInstance == null) throw new ArgumentNullException("messageInstance");

			var handlerType = messageHandler.GetType();
			var messageType = messageInstance.GetType();

			if (_hint.HasContext)
			{
				var contextType = _hint.MessageContextType.Value;
				var consume = handlerType.GetMethod(_hint.MethodName, new[] { messageType, contextType });
				if (null == consume)
					throw Errors.InvalidOperation("Unable to find consuming method {0}.{1}({2}, {3}).",
						handlerType.Name,
						_hint.MethodName,
						messageType.Name,
						contextType.Name);
				var converted = _contextProvider(attributes);
				try
				{
					consume.Invoke(messageHandler, new[] {messageInstance, converted});
				}
				catch (TargetInvocationException e)
				{
					throw Errors.Inner(e);
				}
			}
			else
			{
				var consume = handlerType.GetMethod(_hint.MethodName, new[] { messageType });

				if (null == consume)
					throw Errors.InvalidOperation("Unable to find consuming method {0}.{1}({2}).",
						handlerType.Name,
						_hint.MethodName,
						messageType.Name);

				try
				{
					consume.Invoke(messageHandler, new[] {messageInstance});
				}
				catch(TargetInvocationException e)
				{
					throw Errors.Inner(e);
				}

			}
		}

		public InvocationHandler(InvocationHint hint, Func<MessageAttributesContract, object> contextProvider)
		{
			_contextProvider = contextProvider;
			_hint = hint;
		}
	}
}