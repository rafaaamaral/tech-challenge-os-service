# TechChallenge.OS

## CI/CD em `feat/developtment`

A [esteira do OS Service](.github/workflows/os-ci-cd.yml) executa restore, build Release e testes unitários e de integração em pushes e pull requests para `feat/developtment`. Os relatórios TRX e Cobertura XML ficam em **Actions → execução → Artifacts**. O build mantém o aviso de documentação XML `CS1591` como aviso e trata os demais avisos como erros.

Após a aprovação dos testes em um push na branch, a esteira publica a API, aplica as migrations em PostgreSQL efêmero e verifica `/health` e o consumidor no RabbitMQ. A entrega é o pacote `os-service-feat-developtment.tar.gz` com migrations idempotentes e checksum SHA256.

A [estória e o procedimento de implantação](.specs/features/ci-cd/spec.md) descrevem o download, a configuração do ambiente, a instalação, a verificação e o rollback. Quando o workflow estiver presente na branch padrão do repositório, também será possível iniciá-lo manualmente pela aba Actions selecionando `feat/developtment`. [Execução manual no GitHub Actions](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow).

## SonarQube

O job **SonarQube** analisa o código C# com o SonarScanner for .NET, importa a cobertura OpenCover dos testes da esteira e aguarda o Quality Gate. Ele roda em pushes para `feat/developtment` e em pull requests do próprio repositório para essa branch.

1. Crie ou importe o projeto no SonarQube. No Cloud, selecione análise via GitHub Actions e desative a análise automática se ela estiver ativa.
2. Em **Settings → Secrets and variables → Actions** no GitHub, cadastre o secret `SONAR_TOKEN` com permissão **Execute Analysis** para o projeto.
3. Em **Variables**, cadastre `SONAR_ORGANIZATION` para SonarQube Cloud, ou `SONAR_HOST_URL` para um SonarQube Server acessível pelo runner. Para uma organização Cloud na região dos EUA, defina também `SONAR_REGION=us`.
4. Cadastre a variável `SONAR_PROJECT_KEY` por último para ativar o job. Consulte o resultado do Quality Gate e o link da análise no log do job.

Para analisar branches de desenvolvimento e seus pull requests, use um plano Cloud com esse suporte (OSS, Team ou Enterprise) ou SonarQube Server Developer Edition ou superior. Em um projeto Cloud novo, faça a análise inicial da branch principal antes de analisar a branch de desenvolvimento. [Planos e suporte a branches](https://docs.sonarsource.com/sonarqube-cloud/administering-sonarcloud/managing-subscription/subscription-plans) · [Primeira análise no Cloud](https://docs.sonarsource.com/sonarqube-cloud/getting-started/first-analysis)

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
