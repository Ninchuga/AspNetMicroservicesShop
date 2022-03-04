using Shopping.UnitTests.Ordering.Builders;
using System.Collections.Generic;

namespace Shopping.UnitTests.Utility
{
    public class TestOrderDataGenerator
    {
        public static IEnumerable<object[]> GetOrderItems()
        {
            yield return new object[]
            {
                OrderItemBuilder.BuildWithProductName("iPhone X"),
                OrderItemBuilder.BuildWithProductName("Samsung Galaxy s10"),
                OrderItemBuilder.BuildWithProductName("Xiaomi Redmi Note 10")
            };
        }
    }
}
