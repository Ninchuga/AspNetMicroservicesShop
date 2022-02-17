using Ordering.Domain.Common;
using Ordering.Domain.Exceptions;
using System.Collections.Generic;

namespace Ordering.Domain.ValueObjects
{
    public class CVV : ValueObject
    {
        public int Value { get; private set; }

        private CVV(int value)
        {
            Value = value;
        }

        public static CVV From(int value)
        {
            if (value == 0)
                throw new PaymentDataException("CVV can't be zero (0)");
            if (value.ToString().Length > 3 || value.ToString().Length < 3)
                throw new PaymentDataException("CVV must contain 3 numbers");

            return new CVV(value);
        }

        public static implicit operator int(CVV cVV) => cVV.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
