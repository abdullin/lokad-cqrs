#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;

namespace Lokad.Cqrs
{
	public static class BuilderExtensions
	{
		/// <summary>
		/// Applies configuration action to the builder
		/// </summary>
		/// <typeparam name="TSyntax">The type of the syntax.</typeparam>
		/// <param name="builder">The builder to extend.</param>
		/// <param name="configuration">The configuration.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public static TSyntax WithAction<TSyntax>(this TSyntax builder, Action<ContainerBuilder> configuration)
			where TSyntax : ISyntax<ContainerBuilder>
		{
			configuration(builder.Target);
			return builder;
		}

		/// <summary>
		/// Applies module configuration to the builder
		/// </summary>
		/// <typeparam name="TSyntax">The type of the syntax.</typeparam>
		/// <typeparam name="TModule">The type of the module.</typeparam>
		/// <param name="builder">The builder to extend.</param>
		/// <param name="config">The configuration.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public static TSyntax WithModule<TSyntax, TModule>(this TSyntax builder, Action<TModule> config)
			where TModule : IModule, new()
			where TSyntax : ISyntax<ContainerBuilder>
		{
			var module = new TModule();
			config(module);
			builder.Target.RegisterModule(module);
			return builder;
		}

		/// <summary>
		/// Applies module to the builder
		/// </summary>
		/// <typeparam name="TSyntax">The type of the syntax.</typeparam>
		/// <param name="builder">The builder to extend.</param>
		/// <param name="module">The module to apply.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public static TSyntax WithModule<TSyntax>(this TSyntax builder, IModule module)
			where TSyntax : ISyntax<ContainerBuilder>
		{
			builder.Target.RegisterModule(module);
			return builder;
		}
	}
}