using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Core.Serialization
{
    public sealed class SerializationContractRegistry
    {
        readonly List<Type> _types = new List<Type>();
        bool _readonly;
        readonly object _lock = new object();
        public Type[] GetAndMakeReadOnly()
        {
            lock(_lock)
            {
                _readonly = true;
            }
            
            return _types.ToArray();
        }

        public void AddRange(IEnumerable<Type> types)
        {
            lock(_lock)
            {
                if (_readonly)
                    throw new InvalidOperationException("registry has already been read from. Make sure all regs are done before reading.");
            }
            _types.AddRange(types);
            
        }
    }
}