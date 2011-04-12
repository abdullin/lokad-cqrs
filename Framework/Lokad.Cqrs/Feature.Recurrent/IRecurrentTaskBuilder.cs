using System.Collections.Generic;

namespace Lokad.Cqrs.Feature.Recurrent
{
	public interface IRecurrentTaskBuilder
	{
		IEnumerable<RecurrentTaskInfo> BuildTasks();
	}
}