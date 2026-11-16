# SUMARIO_EXECUTIVO_ENTREGA_2

## Status
Entrega 2.1 implementada com build **OK**.

## Ajuste arquitetural aplicado
A Application do OS foi refatorada para manter apenas casos de uso do OS e remover casos de uso de Billing/Execution.

## Arquivos adaptados (Application)
- `Interfaces/Services/IOrdemServicoApplicationService.cs`
- `Services/OrdemServicos/OrdemServicoApplicationService.cs`
- `Interfaces/Services/INotificacaoService.cs`
- `DTOs/CreateOrdemServicoDto.cs` (documentação de decisão)
- `DTOs/OrdemServicoResponseDto.cs` (documentação de fronteira de responsabilidade)

## Métodos removidos e responsável futuro
### Billing Service
- `GerarOrcamentoAsync`
- `AprovarOrcamentoAsync`
- `ReprovarOrcamentoAsync`

### Execution Service
- `IniciarDiagnosticoAsync`
- `IniciarExecucaoAsync`
- `FinalizarExecucaoAsync`

## Métodos mantidos no OS
- `CriarAsync`
- `ListarTodosAsync`
- `ObterPorIdAsync`
- `ConsultarStatusAsync`
- `EntregarAsync`
- `DeletarAsync`

## DTOs ajustados/preservados
- Mantidos itens de serviços/peças em criação e resposta por dependência transitória e compatibilidade.
- Campos de orçamento e execução mantidos por compatibilidade contratual, marcados como responsabilidade alvo de Billing/Execution.

## Interfaces
- `INotificacaoService`: removidas notificações de orçamento; mantida notificação de entrega.
- `IUsuarioContexto`: pendente de uso, sem regras de autorização definidas.

## Dependências externas identificadas
- Cliente (validação)
- Veículo (propriedade)
- Notificação de entrega

Todas seguem como contrato (sem implementação nesta etapa).

## Resultado do build
- Build da solução: **successful**
- Application continua referenciando somente Domain: **sim**

## Pendências e riscos
- Domain ainda contém comportamentos de orçamento/execução (fora do escopo desta etapa).
- Numeração por `Count()+1` tem risco de concorrência.
- Regras de visibilidade com `IUsuarioContexto` pendentes.
