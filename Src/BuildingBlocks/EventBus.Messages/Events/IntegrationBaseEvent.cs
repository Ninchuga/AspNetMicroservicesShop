using Destructurama.Attributed;
using MassTransit;
using System;

namespace EventBus.Messages.Events
{
    public class SecurityContext
    {
        public string AccessToken { get; set; }
    }

    public class IntegrationBaseEvent : CorrelatedBy<Guid>
    {
        public IntegrationBaseEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        public IntegrationBaseEvent(Guid id, DateTime creationDate)
        {
            Id = id;
            CreationDate = creationDate;
        }

        public Guid Id { get; private set; }
        public DateTime CreationDate { get; private set; }
        [NotLogged]
        public SecurityContext SecurityContext { get; set; } = new SecurityContext();
        public Guid CorrelationId { get; set; }
    }
}
