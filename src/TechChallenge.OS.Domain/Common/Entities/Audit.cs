using System;
using System.Diagnostics.CodeAnalysis;

namespace TechChallenge.OS.Domain.Common.Entities
{
    [ExcludeFromCodeCoverage]
    public class Audit : BaseDomain
    {
        public bool Ativo { get; set; }
        public Guid CriadoPor { get; set; }
        public DateTime DataCriacao { get; set; }
        public Guid? AlteradoPor { get; set; }
        public DateTime? DataAlteracao { get; set; }
    }
}
