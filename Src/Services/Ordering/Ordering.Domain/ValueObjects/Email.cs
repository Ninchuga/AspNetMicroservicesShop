using Ordering.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ordering.Domain.ValueObjects
{
    public class Email : ValueObject
    {
        public string Value { get; private set; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email From(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email can't be empty");
            if (email.Length > 100)
                throw new ArgumentException("E-mail is too long");
            if (!Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
                throw new ArgumentException("E-mail is invalid");

            return new Email(email);
        }

        public static implicit operator string(Email email) => email.Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
