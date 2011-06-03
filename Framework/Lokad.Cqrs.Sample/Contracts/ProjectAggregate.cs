using System.Collections.Generic;

namespace Lokad.Cqrs.Sample.Contracts
{
    public sealed class ProjectAggregate
    {
        IList<TaskEntity> Tasks = new List<TaskEntity>();

        public void AddTask(string name)
        {
            Tasks.Add(new TaskEntity()
                {
                    Name = name
                });
        }
    }
}