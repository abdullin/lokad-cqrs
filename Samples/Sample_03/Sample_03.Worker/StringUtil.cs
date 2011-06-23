#region (c) 2010-2011 Lokad. New BSD License

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Sample_03.Worker
{
    static class StringUtil
    {
        public static string ToReadable(this Guid guid)
        {
            return guid.ToString().Substring(0, 6);
        }
    }
}