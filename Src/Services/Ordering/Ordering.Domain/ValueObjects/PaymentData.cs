using Ordering.Domain.Common;
using System.Collections.Generic;

namespace Ordering.Domain.ValueObjects
{
    public class PaymentData : ValueObject
    {
        public PaymentData() { }

        public PaymentData(string cardName, string cardNumber, bool orderPaid, int cVV)
        {
            CardName = cardName;
            CardNumber = cardNumber;
            OrderPaid = orderPaid;
            CVV = cVV;
        }

        public string CardName { get; private set; }
        public string CardNumber { get; private set; }
        public bool OrderPaid { get; private set; }
        public int CVV { get; private set; }

        public void SetOrderToPaid() => OrderPaid = true;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return CardName;
            yield return CardNumber;
            yield return OrderPaid;
            yield return CVV;
        }
    }
}
