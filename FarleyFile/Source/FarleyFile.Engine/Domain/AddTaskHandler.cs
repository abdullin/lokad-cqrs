using Farley.Engine.Design;
using FarleyFile;
using Lokad.Cqrs.Feature.AtomicStorage;

namespace Farley.Engine.Domain
{
    public sealed class AddTaskHandler : Handle<AddTask>
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