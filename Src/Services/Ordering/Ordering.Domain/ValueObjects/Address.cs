using Ordering.Domain.Common;
using System;
using System.Collections.Generic;

namespace Ordering.Domain.ValueObjects
{
    public class Address : ValueObject
    {
        // Empty constructor in this case is required by EF Core
        // because has a complex type as a parameter in the default constructor
        private Address() { }

        public Address(string firstName, string lastName, Email email, string street, string country, string city)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Email = email;
            Street = street ?? throw new ArgumentNullException(nameof(street));
            Country = country ?? throw new ArgumentNullException(nameof(country));
            City = city ?? throw new ArgumentNullException(nameof(city));
        }

        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Email Email { get; private set; }
        public string Street { get; private set; }
        public string Country { get; private set; }
        public string City { get; private set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            // Using a yield return statement to return each element one at a time
            yield return FirstName;
            yield return LastName;
            yield return Email;
            yield return Street;
            yield return Country;
            yield return City;
        }
    }
}
