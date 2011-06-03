using Lokad.Cqrs.Feature.AtomicStorage;
using Lokad.Cqrs.Sample.Contracts;

namespace Lokad.Cqrs.Sample.Domain
{
    public sealed class AddTaskHandler : Define.Handle<AddTask>
    {
        readonly IAtomicSingletonWriter<ProjectAggregate> _project;

        public AddTaskHandler(IAtomicSingletonWriter<ProjectAggregate> project)
        {
            _project = project;
        }

        public void Consume(AddTask message)
        {
            _project.UpdateEnforcingNew(p => p.AddTask(message.Name));
        }
    }
}