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
			RegisterLogProvider(new NullLog());
		}

		public void LogToTrace()
		{
			RegisterLogProvider(new TraceLog());
		}

		public void RegisterLogProvider(ILog provider)
		{
			Builder.RegisterInstance(provider);
		}
	}
}