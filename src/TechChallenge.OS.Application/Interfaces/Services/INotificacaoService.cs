namespace TechChallenge.OS.Application.Interfaces.Services
{
    /// <summary>
    /// Interface para serviço de notificação (porta hexagonal).
    /// Representa a integração com o serviço de notificações por email.
    ///
    /// Implementação: Será preenchida com adaptador de email.
    /// Responsabilidade no OS: Notificações do ciclo de vida da OS que permanecem neste serviço.
    /// </summary>
    public interface INotificacaoService
    {
        /// <summary>
        /// Envia email de confirmação da entrega do veículo.
        /// </summary>
        Task<bool> EnviarConfirmacaoEntregaAsync(string clienteEmail, int numeroOS);
    }
}
