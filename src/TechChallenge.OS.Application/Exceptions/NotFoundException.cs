using System.Diagnostics.CodeAnalysis;

namespace TechChallenge.OS.Application.Exceptions
{
    /// <summary>
    /// Exceção lançada quando um recurso não é encontrado.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string name, object key)
            : base($"{name} ({key}) não foi encontrado.")
        {
        }
    }
}
