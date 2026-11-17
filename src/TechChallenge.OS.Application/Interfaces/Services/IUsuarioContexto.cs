namespace TechChallenge.OS.Application.Interfaces.Services
{
    /// <summary>
    /// Interface para obter informações do usuário logado (porta hexagonal).
    /// Representa a extração de contexto de autenticação/autorização.
    /// 
    /// Implementação: Será preenchida quando integrado com ASP.NET Core Identity/JWT.
    /// Responsabilidade: Fornecer ID do usuário logado para filtros e auditoria.
    /// </summary>
    public interface IUsuarioContexto
    {
        /// <summary>
        /// Retorna o identificador único do usuário logado.
        /// </summary>
        Guid? UniqueCodeUsuario { get; }

        /// <summary>
        /// Retorna se um usuário está autenticado.
        /// </summary>
        bool EstaAutenticado { get; }
    }
}
