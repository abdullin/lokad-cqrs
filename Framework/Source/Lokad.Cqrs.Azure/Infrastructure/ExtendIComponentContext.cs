#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;

namespace Lokad.Cqrs
{
	public static class ExtendIComponentContext
	{
		public static void WhenDisposed(this IComponentContext context, Action disposal)
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance(new DisposableAction(disposal));
			builder.Update(context.ComponentRegistry);
		}

		/// <summary>
		/// Class that allows action to be executed, when it is disposed
		/// </summary>
		[Serializable]
		sealed class DisposableAction : IDisposable
		{
			readonly Action _action;

			/// <summary>
			/// Initializes a new instance of the <see cref="DisposableAction"/> class.
			/// </summary>
			/// <param name="action">The action.</param>
			public DisposableAction(Action action)
			{
				_action = action;
			}

			/// <summary>
			/// Executes the action
			/// </summary>
			public void Dispose()
			{
				_action();
			}
		}

	}
}