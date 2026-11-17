using System.Diagnostics.CodeAnalysis;

namespace TechChallenge.OS.Application.Exceptions
{
    /// <summary>
    /// Exceção base para erros de aplicação.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ApplicationException : Exception
    {
        public ApplicationException(string message) : base(message)
        {
        }

        public ApplicationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
