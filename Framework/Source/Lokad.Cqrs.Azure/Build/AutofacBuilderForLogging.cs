using Autofac;

namespace Lokad.Cqrs
{
	public sealed class AutofacBuilderForLogging : Syntax
	{
		readonly ContainerBuilder _builder;

		public AutofacBuilderForLogging(ContainerBuilder builder)
		{
			_builder = builder;
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
			_builder.RegisterInstance(provider);
		}
	}
}