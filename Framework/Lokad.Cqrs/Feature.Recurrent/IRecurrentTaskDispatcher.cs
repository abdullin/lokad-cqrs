using System;

namespace Lokad.Cqrs.Feature.Recurrent
{
	public interface IRecurrentTaskDispatcher
	{
		TimeSpan Execute(RecurrentTaskInfo info);
	}
}