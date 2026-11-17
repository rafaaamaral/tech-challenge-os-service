using TechChallenge.OS.Domain.Common.Entities;
using TechChallenge.OS.Domain.Common.Enums;
using TechChallenge.OS.Domain.Exceptions;

namespace TechChallenge.OS.Domain.Aggregates.OrdemServicos
{
    /// <summary>
    /// Aggregate Root que representa uma Ordem de Serviço.
    /// Responsável pela identidade da OS, solicitação inicial e estado geral.
    /// </summary>
    public class OrdemServico : Audit
    {
        public OrdemServico() { }

        /// <summary>
        /// Identificador do cliente proprietário da OS (referência externa).
        /// </summary>
        public required int ClienteId { get; set; }

        /// <summary>
        /// Identificador do veículo da OS (referência externa).
        /// </summary>
        public required int VeiculoId { get; set; }

        /// <summary>
        /// Número sequencial da OS.
        /// </summary>
        public required int Numero { get; set; }

        /// <summary>
        /// Status geral da OS.
        /// </summary>
        public StatusOrdemServico Status { get; set; } = StatusOrdemServico.Recebida;

        /// <summary>
        /// Itens de serviço informados na solicitação inicial.
        /// Snapshot sem regras econômicas.
        /// </summary>
        public ICollection<OrdemServicoItemServico> ItensServicos { get; set; } = new List<OrdemServicoItemServico>();

        /// <summary>
        /// Itens de peça/insumo informados na solicitação inicial.
        /// Snapshot sem regras econômicas.
        /// </summary>
        public ICollection<OrdemServicoItemPecaInsumo> ItensPecasInsumos { get; set; } = new List<OrdemServicoItemPecaInsumo>();

        /// <summary>
        /// Cria uma nova Ordem de Serviço com status inicial Recebida.
        /// </summary>
        public static OrdemServico Criar(int clienteId, int veiculoId, int numero)
        {
            var ordemServico = new OrdemServico
            {
                UniqueCode = Guid.NewGuid(),
                ClienteId = clienteId,
                VeiculoId = veiculoId,
                Numero = numero,
                Status = StatusOrdemServico.Recebida
            };

            ordemServico.Validar();
            return ordemServico;
        }

        /// <summary>
        /// Adiciona item de serviço como snapshot da solicitação inicial.
        /// </summary>
        public void AdicionarServico(int servicoId, string descricao)
        {
            if (Status != StatusOrdemServico.Recebida)
                throw new InvalidOperationException("Só é permitido adicionar itens na abertura da OS.");

            ItensServicos.Add(OrdemServicoItemServico.Criar(servicoId, descricao));
        }

        /// <summary>
        /// Adiciona item de peça/insumo como snapshot da solicitação inicial.
        /// </summary>
        public void AdicionarPecaInsumo(int pecaInsumoId, string descricao, int quantidade)
        {
            if (Status != StatusOrdemServico.Recebida)
                throw new InvalidOperationException("Só é permitido adicionar itens na abertura da OS.");

            ItensPecasInsumos.Add(OrdemServicoItemPecaInsumo.Criar(pecaInsumoId, descricao, quantidade));
        }

        /// <summary>
        /// Evento externo de Execution: diagnóstico iniciado.
        /// Transição: Recebida -> EmDiagnostico.
        /// </summary>
        public void RegistrarDiagnosticoIniciado()
        {
            if (Status != StatusOrdemServico.Recebida)
                throw new InvalidOperationException("A OS precisa estar recebida para registrar diagnóstico iniciado.");

            Status = StatusOrdemServico.EmDiagnostico;
        }

        /// <summary>
        /// Evento externo de Billing: orçamento gerado e enviado para aprovação.
        /// Transição: Recebida|EmDiagnostico -> AguardandoAprovacao.
        /// </summary>
        public void RegistrarAguardandoAprovacaoOrcamento()
        {
            if (Status != StatusOrdemServico.Recebida && Status != StatusOrdemServico.EmDiagnostico)
                throw new InvalidOperationException("A OS precisa estar recebida ou em diagnóstico para aguardar aprovação de orçamento.");

            Status = StatusOrdemServico.AguardandoAprovacao;
        }

        /// <summary>
        /// Evento externo de Billing: orçamento aprovado.
        /// Mantém o status geral em AguardandoAprovacao até o início da execução.
        /// </summary>
        public void RegistrarOrcamentoAprovado()
        {
            if (Status != StatusOrdemServico.AguardandoAprovacao)
                throw new InvalidOperationException("A OS precisa estar aguardando aprovação para registrar orçamento aprovado.");
        }

        /// <summary>
        /// Evento externo de Billing: orçamento reprovado.
        /// Mantém o status geral em AguardandoAprovacao.
        /// </summary>
        public void RegistrarOrcamentoReprovado()
        {
            if (Status != StatusOrdemServico.AguardandoAprovacao)
                throw new InvalidOperationException("A OS precisa estar aguardando aprovação para registrar orçamento reprovado.");
        }

        /// <summary>
        /// Evento externo de Execution: execução iniciada.
        /// Transição: AguardandoAprovacao -> EmExecucao.
        /// </summary>
        public void RegistrarExecucaoIniciada()
        {
            if (Status != StatusOrdemServico.AguardandoAprovacao)
                throw new InvalidOperationException("A OS precisa estar aguardando aprovação para registrar execução iniciada.");

            Status = StatusOrdemServico.EmExecucao;
        }

        /// <summary>
        /// Evento externo de Execution: execução finalizada.
        /// Transição: EmExecucao -> Finalizada.
        /// </summary>
        public void RegistrarExecucaoFinalizada()
        {
            if (Status != StatusOrdemServico.EmExecucao)
                throw new InvalidOperationException("A OS precisa estar em execução para registrar execução finalizada.");

            Status = StatusOrdemServico.Finalizada;
        }

        /// <summary>
        /// Transição: Finalizada → Entregue
        /// 
        /// Entrega o veículo ao cliente.
        /// A OS atinge seu estado final e imutável.
        /// </summary>
        /// <exception cref="InvalidOperationException">Lançado se o status não for "Finalizada"</exception>
        public void Entregar()
        {
            if (Status != StatusOrdemServico.Finalizada)
                throw new InvalidOperationException("A OS precisa estar finalizada para entregar o veículo.");

            Status = StatusOrdemServico.Entregue;
        }

        /// <summary>
        /// Desativa a OS de qualquer estado.
        /// Define Ativo = false, marcando a OS como logicamente deletada.
        /// 
        /// Não é uma transição de status, mas uma marcação de exclusão lógica.
        /// </summary>
        public void Delete()
        {
            Ativo = false;
        }


        /// <summary>
        /// Valida as regras de negócio no momento de criação da OS.
        /// </summary>
        /// <exception cref="DomainException">Lançado se alguma validação falhar</exception>
        private void Validar()
        {
            if (ClienteId <= 0)
                throw new DomainException("O cliente da OS é obrigatório.");
            if (VeiculoId <= 0)
                throw new DomainException("O veículo da OS é obrigatório.");
            if (Numero <= 0)
                throw new DomainException("O número da OS é obrigatório.");
        }
    }
}
