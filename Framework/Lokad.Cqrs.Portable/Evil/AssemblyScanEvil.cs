#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Reflection;

namespace Lokad.Cqrs.Evil
{
    /// <summary>
    /// One of these evil utility classes to filter out quickly some common non-user assemblies (this speeds assembly scans)
    /// </summary>
    public static class AssemblyScanEvil
    {
        /// <summary>
        /// Determines whether the specified assembly is user assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        ///   <c>true</c> if specified assembly is probably a user assembly; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsProbablyUserAssembly(Assembly assembly)
        {
            if (String.IsNullOrEmpty(assembly.FullName))
                return false;

            if (assembly.IsDynamic)
                return false;

            var prefixes = new[]
                {
                    "System", "Microsoft", "nunit", "JetBrains", "Autofac", "mscorlib", "ProtoBuf"
                };

            foreach (var prefix in prefixes)
            {
                if (assembly.FullName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            return true;
        }
    }
}