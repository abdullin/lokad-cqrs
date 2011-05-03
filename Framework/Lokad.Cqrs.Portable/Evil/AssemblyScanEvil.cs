using System;
using System.Reflection;

namespace Lokad.Cqrs.Evil
{
    public static class AssemblyScanEvil
    {
        public static bool IsUserAssembly(Assembly a)
        {
            if (String.IsNullOrEmpty(a.FullName))
                return false;

            var prefixes = new[]
                {
                    "System", "Microsoft", "nunit", "JetBrains", "Autofac", "mscorlib", "ProtoBuf"
                };

            foreach (var prefix in prefixes)
            {
                if (a.FullName.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            return true;
        }
    }
}