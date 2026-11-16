# ENTREGA_2_APPLICATION_LAYER

## Objetivo
Extrair e adaptar a camada Application de Ordem de Serviço para o microsserviço `TechChallenge.OS`, mantendo foco no contexto OS e sem dependências de Infrastructure/API.

## Escopo atendido
- Alterações apenas em `src/TechChallenge.OS.Application`.
- Sem alterações em Domain, Infrastructure, API e testes.
- Sem CQRS.
- Casos de uso implementados como Services.
- Application referenciando apenas Domain.

## Ajuste arquitetural da Entrega 2.1
Com base na revisão arquitetural, a Application do OS foi refatorada para remover casos de uso pertencentes a Billing e Execution.

### Métodos removidos do OS Application
#### Responsável futuro: Billing Service
- `GerarOrcamentoAsync`
- `AprovarOrcamentoAsync`
- `ReprovarOrcamentoAsync`

#### Responsável futuro: Execution Service
- `IniciarDiagnosticoAsync`
- `IniciarExecucaoAsync`
- `FinalizarExecucaoAsync`

### Métodos mantidos no OS Application
- `CriarAsync`
- `ListarTodosAsync`
- `ObterPorIdAsync`
- `ConsultarStatusAsync`
- `EntregarAsync` (limitado ao ciclo de vida geral da OS)
- `DeletarAsync`

## Estrutura atual
### DTOs
- `CreateOrdemServicoDto`
- `CreateOrdemServicoItemServicoDto`
- `CreateOrdemServicoItemPecaInsumoDto`
- `OrcamentoResponseDto` (compatibilidade contratual; alvo futuro Billing)
- `OrdemServicoItemServicoResponseDto` (compatibilidade contratual; alvo futuro Execution/Billing)
- `OrdemServicoItemPecaInsumoResponseDto` (compatibilidade contratual; alvo futuro Execution/Billing)
- `OrdemServicoResponseDto`

### Interfaces (ports)
- `IOrdemServicoApplicationService`
- `IOrdemServicoRepository`
- `IClienteService`
- `IVeiculoService`
- `INotificacaoService` (ajustada: apenas notificação de entrega)
- `IUsuarioContexto`

### Service / Casos de uso
- `OrdemServicoApplicationService`
  - `CriarAsync`
  - `ListarTodosAsync`
  - `ObterPorIdAsync`
  - `ConsultarStatusAsync`
  - `EntregarAsync`
  - `DeletarAsync`

### Mapper
- `OrdemServicoMapper`
  - `ToResponseDto`
  - `ToResponseDtoList`

### Validação
- `OrdemServicoValidator`
  - validações de entrada para create e itens

### Exceções
- `ApplicationException`
- `NotFoundException`

## Decisão sobre itens de serviços e peças
- **Não removidos nesta etapa**.
- Permanecem no DTO de criação e no DTO de resposta por dependência transitória do fluxo atual e para evitar quebra contratual.
- Foram marcados semanticamente como dados com responsabilidade alvo em Billing/Execution para migração futura.

## IUsuarioContexto
- Mantido como contrato.
- Ainda não aplicado em regras de visibilidade/autorização por ausência de definição formal dessas regras.
- Pendência documentada para próxima etapa.

## Regras preservadas
- Fluxo de ciclo de vida da OS continua sendo controlado pelo Domain.
- Validações de cliente/veículo na camada Application.
- Geração de número sequencial com base em `CountAsync()+1` (padrão original).
- Notificação tratada como não-crítica (falha não quebra fluxo principal).

## Limites de responsabilidade (OS x Billing x Execution)
### Mantido em OS (Application desta entrega)
- Criação da OS
- Consulta/listagem/status
- Registro de entrega no ciclo de vida da OS
- Exclusão lógica

### Removido do OS e delegado para serviços futuros
- **Billing**: geração/aprovação/reprovação de orçamento, pagamentos
- **Execution**: diagnóstico, início/finalização da execução, fila e acompanhamento operacional

### Não implementado nesta etapa
- Integrações HTTP/mensageria/saga

## Dependências externas identificadas (somente contratos)
- Validação de cliente
- Validação de propriedade do veículo
- Notificação de entrega por email

Todas deixadas como interfaces, sem implementação fictícia.

## Verificações finais
- Build do projeto Application: **sucesso**
- Build da solução: **sucesso**
- Application referencia apenas Domain: **sim**
- Sem referência a Infrastructure/API: **sim**
- Sem casos de uso de Billing/Execution em `IOrdemServicoApplicationService` e `OrdemServicoApplicationService`: **sim**

## Riscos e pendências
1. `CountAsync()+1` para número sequencial pode gerar concorrência em alta escala (avaliar estratégia futura).
2. Integração de catálogos externos (serviço/peça) ainda não implementada (somente contrato).
3. `IUsuarioContexto` pendente de uso após definição das regras de visibilidade.
4. Domain ainda contém comportamentos de orçamento/execução; refatoração fora do escopo desta entrega.
5. Ausência de testes unitários nesta entrega (fora do escopo solicitado).
