using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Core.Directory
{
    public sealed class MessageActivationMap
    {
        public readonly ICollection<MessageActivationInfo> Infos;

        public MessageActivationMap(ICollection<MessageActivationInfo> infos)
        {
            Infos = infos;
        }

        public ICollection<Type> QueryAllConsumingTypes()
        {
            return Infos
                .SelectMany(m => m.AllConsumers)
                .Distinct()
                .ToArray();
        }
        public ICollection<Type> QueryAllMessageTypes()
        {
            return Infos
                .Select(m => m.MessageType)
                .ToArray();
        }
    }
}