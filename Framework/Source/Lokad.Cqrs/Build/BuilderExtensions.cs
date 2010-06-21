using System;
using Autofac;
using Autofac.Core;
using Microsoft.WindowsAzure;

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


		/// <summary>
		/// Uses default Development storage account for Windows Azure
		/// </summary>
		/// <typeparam name="TSyntax">The type of the syntax.</typeparam>
		/// <param name="builder">The builder to extend.</param>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		/// <remarks>This option is enabled by default</remarks>
		public static TSyntax CloudStorageAccountIsDev<TSyntax>(this TSyntax builder)
			where TSyntax : ISyntax<ContainerBuilder>
		{
			builder.Target.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			return builder;
		}

		/// <summary>
		/// Uses development storage account defined in the configuration setting.
		/// </summary>
		/// <typeparam name="TSyntax">The type of the syntax.</typeparam>
		/// <param name="builder">The builder to extend.</param>
		/// <param name="name">The name of the configuration value to look up.</param>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		public static TSyntax CloudStorageAccountIsFromConfig<TSyntax>(this TSyntax builder, string name)
			where TSyntax : ISyntax<ContainerBuilder>
		{
			builder.Target.Register(c =>
			{
				var value = c.Resolve<IProfileSettings>()
					.GetString(name)
					.ExposeException("Failed to load account from '{0}'", name);
				return CloudStorageAccount.Parse(value);
			}).SingleInstance();
			return builder;
		}
	}
}