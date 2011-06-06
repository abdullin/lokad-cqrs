using System.Collections.Generic;

namespace FarleyFile.Engine.Domain.Persist
{
    public sealed class UserAggregate
    {
        IList<ContactEntity> Tasks = new List<ContactEntity>();

        public void AddTask(string name)
        {
            Tasks.Add(new ContactEntity()
                {
                    Name = name
                });
        }
    }
}