#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.NHibernate;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Extends Lokad.CQRS Builder syntax
	/// </summary>
	public static class ExtendSyntax
	{
		/// <summary>
		/// Configures NHibernate ORM to be used in the Container
		/// </summary>
		/// <typeparam name="TSyntax">The type of the syntax to extend.</typeparam>
		/// <param name="syntax">The syntax.</param>
		/// <param name="config">Configuration for the NHibernate.</param>
		/// <returns>Same configurer instance for inlining multiple config statements.</returns>
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