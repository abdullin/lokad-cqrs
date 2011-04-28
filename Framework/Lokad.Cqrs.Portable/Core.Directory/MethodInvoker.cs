using System;
using System.Diagnostics;
using System.Reflection;
using Lokad.Cqrs.Core.Evil;

namespace Lokad.Cqrs.Core.Directory
{
    public sealed class MethodInvoker : IMethodInvoker
    {

        readonly Func<MessageEnvelope, MessageItem, object> _contextProvider;
        readonly MethodInvokerHint _hint;


        [DebuggerNonUserCode]
        public void InvokeConsume(object messageHandler, MessageItem item, MessageEnvelope envelope)
        {
            var handlerType = messageHandler.GetType();
            var content = item.Content;
            var messageType = item.MappedType;

            if (_hint.HasContext)
            {
                var contextType = _hint.MessageContextType.Value;
                var consume = handlerType.GetMethod(_hint.MethodName, new[] { messageType, contextType });
                if (null == consume)
                {
                    throw new InvalidOperationException(
                        string.Format("Unable to find consuming method {0}.{1}({2}, {3}).",
                            handlerType.Name,
                            _hint.MethodName,
                            messageType.Name,
                            contextType.Name));
                }
                var converted = _contextProvider(envelope,item);
                try
                {
                    consume.Invoke(messageHandler, new[] { content, converted });
                }
                catch (TargetInvocationException e)
                {

                    throw InvocationUtil.Inner(e);
                }
            }
            else
            {
                var consume = handlerType.GetMethod(_hint.MethodName, new[] { messageType });

                if (null == consume)
                {
                    throw new InvalidOperationException(string.Format("Unable to find consuming method {0}.{1}({2}).",
                        handlerType.Name,
                        _hint.MethodName,
                        messageType.Name));
                }

                try
                {
                    consume.Invoke(messageHandler, new[] { content });
                }
                catch (TargetInvocationException e)
                {
                    throw InvocationUtil.Inner(e);
                }

            }
        }

        public MethodInvoker(Func<MessageEnvelope, MessageItem, object> contextProvider, MethodInvokerHint hint)
        {
            _contextProvider = contextProvider;
            _hint = hint;
        }
    }
}