using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Scenarios.SimpleES.Contracts
{
    public sealed class LoginIndex
    {
        readonly HashSet<string> _set = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public void AddOrThrow(string login)
        {
            if (!_set.Add(login))
                throw new InvalidOperationException("Index already existed");
        }
        public void Release(string login)
        {
            _set.Remove(login);
        }
    }
}