using TechChallenge.OS.Application.Interfaces.Services;

namespace TechChallenge.OS.Infrastructure.Services;

/// <summary>
/// Stub de implementação para INotificacaoService.
/// Será substituída pela implementação real com adaptador de email quando disponível.
/// 
/// Comportamento atual:
/// - Simula sucesso no envio de notificações
/// - Não envia emails reais
/// </summary>
public class NotificacaoServiceStub : INotificacaoService
{
    public Task<bool> EnviarConfirmacaoEntregaAsync(string clienteEmail, int numeroOS)
    {
        // TODO: Integração com serviço de Notificação
        // Atualmente simula sucesso sem enviar email real
        // Quando implementado, integrar com SMTP ou serviço de email

        return Task.FromResult(true);
    }
}
