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
            EventCreationDate = DateTime.UtcNow;
        }

        public IntegrationBaseEvent(Guid id, DateTime creationDate)
        {
            Id = id;
            EventCreationDate = creationDate;
        }

        public Guid Id { get; private set; }
        public DateTime EventCreationDate { get; private set; }
        public SecurityContext SecurityContext { get; set; } = new SecurityContext();
        public Guid CorrelationId { get; set; }
    }
}
