using System;
using System.Diagnostics.CodeAnalysis;

namespace TechChallenge.OS.Domain.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }
}
