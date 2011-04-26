#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Threading;

namespace Lokad.Cqrs.Core.Inbox
{
    public interface IPartitionInbox
    {
        void Init();
        void AckMessage(MessageContext message);
        bool TakeMessage(CancellationToken token, out MessageContext context);
        void TryNotifyNack(MessageContext context);
    }
}