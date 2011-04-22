namespace Lokad.Cqrs.Build.Engine.Events
{
	public sealed class HostStarted : ISystemEvent
	{
		public readonly string[] EngineProcesses;

		public HostStarted(string[] engineProcesses)
		{
			EngineProcesses = engineProcesses;
		}

		public override string ToString()
		{
			return string.Format("Host started: {0}", string.Join(",", EngineProcesses));
		}
	}
}