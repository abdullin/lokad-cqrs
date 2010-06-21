#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.NHibernate;

// ReSharper disable CheckNamespace

namespace Lokad.Cqrs
// ReSharper restore CheckNamespace
{
	public static class ExtendSyntax
	{
		public static TSyntax WithNHibernate<TSyntax>(this TSyntax syntax, Action<NHibernateModule> config)
			where TSyntax : ISyntax<ContainerBuilder>
		{
			var module = new NHibernateModule();
			config(module);
			syntax.Target.RegisterModule(module);
			return syntax;
		}
	}
}