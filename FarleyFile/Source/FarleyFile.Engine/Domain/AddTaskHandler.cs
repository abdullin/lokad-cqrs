using Lokad.Cqrs.Feature.AtomicStorage;

namespace FarleyFile.Engine.Domain
{
    public sealed class AddTaskHandler : Farley.Handle<AddTask>
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