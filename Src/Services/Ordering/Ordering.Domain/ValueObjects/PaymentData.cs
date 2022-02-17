using Ordering.Domain.Common;
using Ordering.Domain.Exceptions;
using System.Collections.Generic;

namespace Ordering.Domain.ValueObjects
{
    public class PaymentData : ValueObject
    {
        // Empty constructor in this case is required by EF Core
        // because has a complex type as a parameter in the default constructor
        private PaymentData() { }

        private PaymentData(string cardName, string cardNumber, bool orderPaid, CVV cvv)
        {
            CardName = cardName;
            CardNumber = cardNumber;
            OrderPaid = orderPaid;
            CVV = cvv;
        }

        public static PaymentData From(string cardName, string cardNumber, bool orderPaid, CVV cvv)
        {
            if (string.IsNullOrWhiteSpace(cardName))
                throw new PaymentDataException("Card name is mandatory");
            if (string.IsNullOrWhiteSpace(cardNumber))
                throw new PaymentDataException("Card number is mandatory");

            return new PaymentData(cardName, cardNumber, orderPaid, cvv);
        }

        public string CardName { get; private set; }
        public string CardNumber { get; private set; }
        public bool OrderPaid { get; private set; }
        public CVV CVV { get; private set; }

        public PaymentData OrderIsPaid() => From(CardName, CardNumber, true, CVV);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return CardName;
            yield return CardNumber;
            yield return OrderPaid;
            yield return CVV;
        }
    }
}
