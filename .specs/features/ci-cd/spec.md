# Criar pipeline CI/CD do OS Service

Como pessoa responsável pela entrega do OS Service, quero validar cada mudança na `feat/developtment` e gerar um pacote associado ao commit para implantar a versão aprovada com configuração e migrations reproduzíveis.

## Critérios de aceite

- Pushes e pull requests direcionados à `feat/developtment` validam o build Release e executam as suítes unitária e de integração HTTP/JWT e PostgreSQL. A integração usa PostgreSQL 16 efêmero.
- Cada suíte disponibiliza TRX, cobertura Cobertura XML e resumo. Suítes vazias, com testes ignorados, com falhas ou sem cobertura válida bloqueiam o pacote de entrega.
- Após a aprovação das suítes, um push na `feat/developtment` publica a API em Release. A mesma execução aplica as migrations em PostgreSQL efêmero, confirma `200 Healthy` em `/health` e verifica o consumidor na fila `os-service.events` de um RabbitMQ efêmero.
- O artefato `os-service-feat-developtment-<SHA>` contém a API publicada, `migrations.sql` idempotente dentro do pacote e o checksum SHA256 ao lado do pacote. O artefato fica disponível por 30 dias. Pull requests publicam os relatórios das suítes.
- O GitHub Actions entrega o pacote para implantação manual, como no Billing. As etapas abaixo cobrem a instalação e o retorno à versão anterior.

## Implantação do pacote em Linux

### Preparar o ambiente

- Disponibilize o runtime **ASP.NET Core 10**, o cliente `psql`, PostgreSQL 16 e acesso ao RabbitMQ. Confira o runtime com `dotnet --list-runtimes` e identifique a versão anterior da API antes da troca.
- Provisione o banco do OS e configure no destino uma conexão para aplicar migrations e outra com as permissões de operação da API. Armazene senhas e chaves no gerenciador de segredos do ambiente.
- Use os valores de emissor, audiência e chave correspondentes ao emissor dos tokens do ambiente. No RabbitMQ, configure o vhost compartilhado pelo OS, Billing e Execution e conceda à conta do OS as permissões de declarar, publicar e consumir na topologia da Saga.

| Configuração da API | Variáveis de ambiente |
| --- | --- |
| Runtime | `ASPNETCORE_ENVIRONMENT=Production`, `ASPNETCORE_URLS` com o endereço e a porta do serviço |
| PostgreSQL | `ConnectionStrings__OSDb` com host, porta, banco, usuário e senha do OS |
| JWT | `Jwt__Issuer`, `Jwt__Audience`, `Jwt__SigningKey` |
| RabbitMQ | `RABBITMQ_ENABLED=true`, `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_USER`, `RABBITMQ_PASSWORD`, `RABBITMQ_VHOST` e `RABBITMQ_QUEUE=os-service.events` |

### Instalar e verificar

1. Em **Actions → OS Service - CI/CD (feat/developtment) → execução bem-sucedida → Artifacts**, baixe `os-service-feat-developtment-<SHA>` e extraia o ZIP. Confira o SHA com o commit que pretende implantar.
2. No diretório extraído, valide a integridade e descompacte a entrega em um diretório novo de release.

   ```bash
   sha256sum --check os-service-feat-developtment.tar.gz.sha256 &&
     mkdir os-service-release &&
     tar -xzf os-service-feat-developtment.tar.gz -C os-service-release
   ```

3. Revise `os-service-release/migrations.sql`, confirme o backup do banco e aplique as migrations com a identidade de implantação. O exemplo usa um serviço `os-migrations` previamente configurado no cliente PostgreSQL para o banco de destino. O script consulta o histórico do EF Core e aplica as migrations pendentes. [Documentação do EF Core sobre migrations idempotentes](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying#idempotent-sql-scripts).

   ```bash
   psql --dbname='service=os-migrations' --set=ON_ERROR_STOP=1 \
     --file=os-service-release/migrations.sql
   ```

4. Após a aplicação do SQL com sucesso, configure as variáveis no gerenciador do processo e execute `dotnet TechChallenge.OS.Api.dll` dentro da nova release. Para verificar no próprio host, o exemplo usa a API configurada em `http://127.0.0.1:8080`.

   ```bash
   curl --fail --silent --show-error http://127.0.0.1:8080/health
   ```

5. Confirme `200 Healthy`, uma requisição autenticada a `GET /api/v1/ordens-servico` e um consumidor ativo na fila `os-service.events` no vhost configurado. Use os logs da API e do RabbitMQ para investigar falhas antes de liberar tráfego.

### Voltar à versão anterior

Para falhas de aplicação, confira se a versão anterior suporta o schema atual. Se for compatível, aponte o gerenciador do processo para a release anterior, reinicie e repita as verificações de saúde, autenticação e consumo. Se a migration impedir a versão anterior de iniciar, trate a reversão do banco com a pessoa responsável pelo PostgreSQL a partir do backup ou de um script de reversão revisado, considerando as gravações realizadas após a implantação.

## Verificação local

Workflow validado com `actionlint` 1.7.12 e build Release aprovado. Passaram 63 testes unitários e 13 de integração, todos com TRX e cobertura e sem testes ignorados. O cenário de PostgreSQL indisponível confirmou HTTP 503 em `/health`. No smoke test, a reaplicação do SQL foi bem-sucedida, a API publicada respondeu `Healthy` e o RabbitMQ registrou um consumidor ativo. O checksum e o conteúdo do pacote foram conferidos após o empacotamento.
