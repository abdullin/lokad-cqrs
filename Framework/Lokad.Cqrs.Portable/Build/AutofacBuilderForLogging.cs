using Autofac;
using Lokad.Cqrs.Feature.Logging;

namespace Lokad.Cqrs.Build
{
	public sealed class AutofacBuilderForLogging : AutofacBuilderBase
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