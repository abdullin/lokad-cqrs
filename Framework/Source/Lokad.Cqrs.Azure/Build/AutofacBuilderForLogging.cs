using Autofac;
using Lokad.Cqrs.Logging;

namespace Lokad.Cqrs
{
	public sealed class AutofacBuilderForLogging : Syntax
	{
		public AutofacBuilderForLogging(ContainerBuilder builder) : base(builder)
		{
	
		}

		public void LogToNull()
		{
			RegisterLogProvider(new NullSystemObserver());
		}

		public void LogToTrace()
		{
			RegisterLogProvider(new TraceSystemObserver());
		}

		public void RegisterLogProvider(ISystemObserver provider)
		{
			Builder.RegisterInstance(provider);
		}
	}
}