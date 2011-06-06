using FarleyFile.Engine.Domain.Persist;
using Lokad.Cqrs;
using Lokad.Cqrs.Feature.AtomicStorage;

namespace FarleyFile.Engine.Domain
{
    public sealed class AddContactHandler : Farley.Handle<AddContact>
    {
        readonly IAtomicSingletonWriter<UserAggregate> _project;
        readonly IMessageSender _sender;
        public AddContactHandler(IAtomicSingletonWriter<UserAggregate> project, IMessageSender sender)
        {
            _project = project;
            _sender = sender;
        }

        public void Consume(AddContact message)
        {
            _project.UpdateEnforcingNew(p => p.AddTask(message.Name));
            _sender.SendOne(new ContactAdded(message.Name));

        }
    }
}