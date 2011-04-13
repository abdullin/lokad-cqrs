using System;
using System.Diagnostics;
using System.Reflection;
using System.Transactions;
using Autofac;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Feature.Recurrent
{

	/// <summary>
	/// Scheduled task executor that uses transactions.
	/// </summary>
	public sealed class RecurrentTaskDispatcherWithTransactions : IRecurrentTaskDispatcher
	{
		readonly ILifetimeScope _scope;
		const IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;

		public RecurrentTaskDispatcherWithTransactions(ILifetimeScope scope)
		{
			_scope = scope;
		}

		static TransactionOptions GetTransactionOptions()
		{
			return new TransactionOptions
			{
				IsolationLevel = Transaction.Current == null ? _isolationLevel : Transaction.Current.IsolationLevel,
				Timeout = Debugger.IsAttached ? 45.Minutes() : 0.Minutes(),
			};
		}

		public TimeSpan Execute(RecurrentTaskInfo info)
		{
			using (var scope = _scope.BeginLifetimeScope())
			{
				var instance = scope.Resolve(info.Task);
				try
				{
					TimeSpan timeSpan;
					var transactionOptions = GetTransactionOptions();
					using (var tx = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
					{
						timeSpan = (TimeSpan) info.Invoker.Invoke(instance, new object[0]);
						tx.Complete();
					}
					return timeSpan;
				}
				catch (TargetInvocationException e)
				{
					throw Errors.Inner(e);
				}
			}
		}
	}
}