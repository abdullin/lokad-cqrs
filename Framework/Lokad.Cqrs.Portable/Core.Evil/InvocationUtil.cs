#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Reflection;

namespace Lokad.Cqrs.Core.Evil
{
    /// <summary>
    /// Helper class for generating exceptions
    /// </summary>
    public static class InvocationUtil
    {
        static readonly MethodInfo InternalPreserveStackTraceMethod;

        static InvocationUtil()
        {
            InternalPreserveStackTraceMethod = typeof (Exception).GetMethod("InternalPreserveStackTrace",
                BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Returns inner exception, while preserving the stack trace
        /// </summary>
        /// <param name="e">The target invocation exception to unwrap.</param>
        /// <returns>inner exception</returns>
        public static Exception Inner(Exception e)
        {
            if (e == null) throw new ArgumentNullException("e");
            InternalPreserveStackTraceMethod.Invoke(e.InnerException, new object[0]);
            return e.InnerException;
        }


        [DebuggerNonUserCode]
        public static void InvokeConsume(object messageHandler, object messageInstance, string methodName)
        {
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");
            if (messageInstance == null) throw new ArgumentNullException("messageInstance");
            if (methodName == null) throw new ArgumentNullException("methodName");

            try
            {
                var handlerType = messageHandler.GetType();
                var messageType = messageInstance.GetType();
                var consume = handlerType.GetMethod(methodName, new[] {messageType});

                if (null == consume)
                {
                    var text = string.Format("Unable to find consuming method {0}.{1}({2}).",
                        handlerType.Name,
                        methodName,
                        messageType.Name);
                    throw new InvalidOperationException(text);
                }

                consume.Invoke(messageHandler, new[] {messageInstance});
            }
            catch (TargetInvocationException e)
            {
                throw Inner(e);
            }
        }
    }
}