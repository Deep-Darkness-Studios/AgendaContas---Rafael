using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Models;

namespace AgendaContas.Domain.Services;

public class FinanceiroService
{
    private readonly IContaRepository _contaRepo;
    private readonly ILancamentoRepository _lancamentoRepo;

    public FinanceiroService(IContaRepository contaRepo, ILancamentoRepository lancamentoRepo)
    {
        _contaRepo = contaRepo;
        _lancamentoRepo = lancamentoRepo;
    }

    public async Task GerarLancamentosRecorrentesAsync()
    {
        var contas = await _contaRepo.GetAllAsync();
        var competenciaAtual = DateTime.Now.ToString("yyyy-MM");
        var lancamentosExistentes = await _lancamentoRepo.GetByCompetenciaAsync(competenciaAtual);
        
        foreach (var conta in contas.Where(c => c.Recorrente && c.Ativa))
        {
            if (!lancamentosExistentes.Any(l => l.ContaId == conta.Id))
            {
                var vencimento = new DateTime(DateTime.Now.Year, DateTime.Now.Month, Math.Min(conta.DiaVencimento, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)));
                
                await _lancamentoRepo.AddAsync(new Lancamento
                {
                    ContaId = conta.Id,
                    Competencia = competenciaAtual,
                    Vencimento = vencimento,
                    Valor = conta.ValorPadrao,
                    Status = vencimento < DateTime.Today ? "Atrasado" : "Pendente"
                });
            }
        }
    }
}
