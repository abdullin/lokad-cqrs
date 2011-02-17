using Autofac;

namespace Lokad.Cqrs
{
	public sealed class AutofacBuilderForLogging : Syntax
	{
		public AutofacBuilderForLogging(ContainerBuilder builder) : base(builder)
		{
	
		}

		public void LogToNull()
		{
			RegisterLogProvider(NullLog.Provider);
		}

		public void LogToTrace()
		{
			RegisterLogProvider(TraceLog.Provider);
		}

		public void RegisterLogProvider(ILogProvider provider)
		{
			Builder.RegisterInstance(provider);
		}
	}
}