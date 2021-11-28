using System;

namespace Shopping.OrderSagaOrchestrator.Models
{
    public class Event
    {
        public Event(Guid id, string name, Guid userId, string data, DateTime createdAt)
        {
            Id = id;
            Name = name;
            UserId = userId;
            Data = data;
            CreatedAt = createdAt;
            UserId = userId;
            //Payload = payload;
        }

        public Guid Id { get; }
        public string Name { get; }
        public Guid UserId { get; }
        public string Data { get; }
        public DateTime CreatedAt { get; }
        public byte[] Payload { get; }
    }
}
