﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Messages.Events.Order
{
    public class OrderDispatched : OrderBaseEvent
    {
        public DateTime DispatchTime { get; set; }
    }
}
