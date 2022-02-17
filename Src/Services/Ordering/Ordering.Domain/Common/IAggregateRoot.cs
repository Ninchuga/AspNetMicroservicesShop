using System;
using System.Collections.Generic;
using System.Text;

namespace Ordering.Domain.Common
{
    /// <summary>
    /// This is a marker interface mainly for the repositories
    /// Only aggregate root has repository, other entities are reached through that aggregate root
    /// </summary>
    public interface IAggregateRoot
    {
    }
}
