# CHECKLIST_ENTREGA_2

## Escopo
- [x] Alterações somente em `src/TechChallenge.OS.Application`
- [x] Domain não alterado
- [x] Infrastructure não alterado
- [x] API não alterado
- [x] Testes não alterados

## Responsabilidades
- [x] Casos de uso de Billing removidos do OS Application
- [x] Casos de uso de Execution removidos do OS Application
- [x] Casos de uso de OS mantidos (`Criar/Listar/Obter/ConsultarStatus/Entregar/Deletar`)

## Contratos e DTOs
- [x] `IOrdemServicoApplicationService` ajustada
- [x] `INotificacaoService` ajustada (sem orçamento)
- [x] DTOs revisados com fronteiras de responsabilidade documentadas
- [x] Itens de serviços/peças preservados com decisão documentada

## Arquitetura
- [x] Sem CQRS
- [x] Sem mensageria/saga/HTTP clients
- [x] Application referencia somente Domain
- [x] Sem referência a Infrastructure/API

## Validação
- [x] Build da solução executado com sucesso
- [x] Busca textual confirma ausência de métodos de Billing/Execution no service/interface de OS

## Pendências
- [x] Uso de `IUsuarioContexto` documentado como pendência
- [x] Pendências de integração com Billing/Execution documentadas
- [x] Pendência arquitetural no Domain documentada (fora do escopo)
