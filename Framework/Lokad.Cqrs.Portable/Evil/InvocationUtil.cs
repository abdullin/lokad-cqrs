#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Reflection;

namespace Lokad.Cqrs.Evil
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
        public static Exception Inner(TargetInvocationException e)
        {
            if (e == null) throw new ArgumentNullException("e");
            InternalPreserveStackTraceMethod.Invoke(e.InnerException, new object[0]);
            return e.InnerException;
        }
    }
}