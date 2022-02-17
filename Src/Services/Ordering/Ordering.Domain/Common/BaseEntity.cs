using System;

namespace Ordering.Domain.Common
{
    public abstract class BaseEntity<T>
    {
        public T Id { get; }

        protected BaseEntity(T id)
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is BaseEntity<T>))
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            BaseEntity<T> item = (BaseEntity<T>)obj;

            return Id.Equals(item.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(BaseEntity<T> left, BaseEntity<T> right)
        {
            if (Object.Equals(left, null))
                return (Object.Equals(right, null)) ? true : false;
            else
                return left.Equals(right);
        }

        public static bool operator !=(BaseEntity<T> left, BaseEntity<T> right)
        {
            return !(left == right);
        }
    }
}
