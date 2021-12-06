using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Messages.Events.Order
{
    public class OrderFailedToBeBilled : OrderBaseEvent
    {
        public string Reason { get; set; }
    }
}
