﻿#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.DirectoryDispatch.Default
{
    /// <summary>
    /// <para>Default CQRS interface for interface-base domain setup. By default Lokad.CQRS 
    /// scans user assemblies for message handlers inheriting from this interface. You don't need to inherit from interface,
    /// use <see cref="IConsume{TMessage}"/> instead.</para>
    /// <para>If you don't want to reference Lokad.CQRS assemblies in your domain, 
    /// you can declare your own consumer interface and point to it in the configuration,
    /// as shown in the samples.</para>
    /// </summary>
    /// <remarks>Look in the samples for more details on the usage</remarks>
    public interface IConsumeMessage
    {
    }
}