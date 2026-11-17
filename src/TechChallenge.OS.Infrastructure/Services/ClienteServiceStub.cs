using TechChallenge.OS.Application.Interfaces.Services;

namespace TechChallenge.OS.Infrastructure.Services;

/// <summary>
/// Stub de implementação para IClienteService.
/// Será substituída pela implementação real quando o serviço de Cliente estiver integrado.
/// 
/// Comportamento atual:
/// - Sempre retorna que o cliente existe e está ativo
/// - Retorna informações fictícias de cliente
/// </summary>
public class ClienteServiceStub : IClienteService
{
    public Task<bool> ClienteExisteEstaAtivoAsync(int clienteId)
    {
        // TODO: Integração com serviço de Cliente
        // Atualmente aceita qualquer cliente ID
        return Task.FromResult(true);
    }

    public async Task<(int Id, string Nome, string Email)?> ObterInfoClienteAsync(int clienteId)
    {
        // TODO: Integração com serviço de Cliente
        // Retorna informações fixas para stub
        await Task.Delay(0);
        return (Id: clienteId, Nome: $"Cliente {clienteId}", Email: $"cliente{clienteId}@example.com");
    }
}
