﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Messages.Events.Order
{
    public class OrderDelivered : OrderBaseEvent
    {
        public DateTime DeliveryTime { get; set; }
    }
}
