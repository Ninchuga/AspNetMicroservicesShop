using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Common
{
    public abstract class EntityBase
    {
        public Guid Id { get; set; }
    }
}
