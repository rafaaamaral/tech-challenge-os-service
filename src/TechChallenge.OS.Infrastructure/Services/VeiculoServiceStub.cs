using TechChallenge.OS.Application.Interfaces.Services;

namespace TechChallenge.OS.Infrastructure.Services;

/// <summary>
/// Stub de implementação para IVeiculoService.
/// Será substituída pela implementação real quando o serviço de Veículo estiver integrado.
/// 
/// Comportamento atual:
/// - Sempre retorna que o veículo existe e pertence ao cliente
/// - Retorna informações fictícias de veículo
/// </summary>
public class VeiculoServiceStub : IVeiculoService
{
    public Task<bool> VeiculoPertenceAoClienteAsync(int veiculoId, int clienteId)
    {
        // TODO: Integração com serviço de Veículo
        // Atualmente aceita qualquer combinação de veículo/cliente
        return Task.FromResult(true);
    }

    public async Task<(int Id, string Modelo, string Placa)?> ObterInfoVeiculoAsync(int veiculoId)
    {
        // TODO: Integração com serviço de Veículo
        // Retorna informações fixas para stub
        await Task.Delay(0);
        return (Id: veiculoId, Modelo: $"Modelo Veículo {veiculoId}", Placa: $"ABC{veiculoId:D4}");
    }
}
