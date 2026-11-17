# TechChallenge.OS

## Mensageria (RabbitMQ) - Saga coreografada

Exchange `topic` durável: `techchallenge.events`. Todos os eventos (JSON camelCase) têm `eventId` (Guid), `ordemServicoUniqueCode` (Guid) e `occurredAt` (UTC); `motivo` é opcional.

| Direção | Routing key | Evento |
|---|---|---|
| Publica | `os.criada` | OrdemServicoCriada (após persistir a OS) |
| Consome | `billing.aprovado` | OrcamentoAprovado |
| Consome | `billing.reprovado` | OrcamentoReprovado |
| Consome | `execution.finalizada` | ExecucaoFinalizada |
| Consome | `execution.falhou` | ExecucaoFalhou |

### Configuração (variáveis de ambiente)
`RABBITMQ_HOST` (localhost), `RABBITMQ_PORT` (5672), `RABBITMQ_USER`/`RABBITMQ_PASSWORD` (guest), `RABBITMQ_VHOST` (/), `RABBITMQ_QUEUE` (os-service.events), `RABBITMQ_ENABLED` (false desativa publisher/consumer).

### Comportamento
- Fila exclusiva do OS (`os-service.events`, durável) e fila de erro `os-service.events.error` (via dead-letter exchange `techchallenge.events.os.error`).
- ACK somente após processar e gravar no banco. Falhas transitórias: 3 tentativas locais; depois, ou em mensagens inválidas (JSON/campos inválidos, OS inexistente, transição inválida), `nack` sem requeue -> fila de erro (sem loop de reentrega).
- Idempotência: tabela `processed_event` (PK = `eventId`), gravada na mesma transação da mudança de estado.
- Publicação: ocorre após o commit da OS; se o broker estiver indisponível, a falha é registrada em log e a criação não é desfeita (sem outbox - limitação conhecida).
- O OS não acessa bancos de outros serviços.

### Decisões sobre a máquina de estados
- Domain/Application inalterados. Não há eventos para "orçamento gerado"/"execução iniciada"; por isso `billing.aprovado` aplica, com os métodos existentes, `Recebida/EmDiagnostico -> AguardandoAprovacao -> (aprovado) -> EmExecucao`. `billing.reprovado` leva a OS a `AguardandoAprovacao` e a mantém (não há estado de cancelamento).
- `execution.finalizada`: `EmExecucao -> Finalizada`.
- `execution.falhou` (falha ao criar a execução): grava registro em `ordem_servico_compensacao` (OS afetada, motivo, eventId, status `Pendente`). Enquanto existir compensação, qualquer outro evento para a OS é rejeitado (fila de erro), impedindo progressão indevida.

### Execução local
```
docker compose up --build
```
API em http://localhost:8080, RabbitMQ management em http://localhost:15672 (guest/guest). As migrations (`AddMessagingTables`) devem ser aplicadas ao banco (`dotnet ef database update -p src/TechChallenge.OS.Infrastructure -s src/TechChallenge.OS.Api`).

Não validado contra um broker real nesta entrega; testes unitários cobrem validação de mensagens e o decorator de publicação.
