#region (c) 2010-2011 Lokad. New BSD License

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Sample_03.Worker
{
    public class AccountEntity
    {
        public virtual Guid Id { get; private set; }
        public virtual IList<BalanceEntity> Balance { get; private set; }

        public AccountEntity()
        {
            Balance = new List<BalanceEntity>();
        }
    }

    public class BalanceEntity
    {
        public virtual AccountEntity Account { get; set; }
        public virtual long Id { get; private set; }
        public virtual string Name { get; set; }
        public virtual decimal Change { get; set; }
        public virtual decimal Total { get; set; }
    }
}