using System;
using System.Diagnostics.CodeAnalysis;

namespace TechChallenge.OS.Domain.Common.Entities
{
    [ExcludeFromCodeCoverage]
    public class BaseDomain
    {
        public int Id { get; set; }
        public Guid UniqueCode { get; set; } = Guid.NewGuid();
    }
}
