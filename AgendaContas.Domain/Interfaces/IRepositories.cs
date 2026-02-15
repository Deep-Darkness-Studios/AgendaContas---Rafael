using AgendaContas.Domain.Models;

namespace AgendaContas.Domain.Interfaces;

public interface IContaRepository
{
    Task<IEnumerable<Conta>> GetAllAsync();
    Task<int> AddAsync(Conta conta);
    Task UpdateAsync(Conta conta);
    Task DeleteAsync(int id);
}

public interface ILancamentoRepository
{
    Task<IEnumerable<Lancamento>> GetAllWithNamesAsync();
    Task<IEnumerable<Lancamento>> GetByCompetenciaAsync(string competencia);
    Task<int> AddAsync(Lancamento lancamento);
    Task UpdateStatusAsync(int id, string status, DateTime? dataPagamento = null);
    Task<IEnumerable<Lancamento>> GetAlertasAsync(int dias);
    Task<dynamic> GetDashboardSummaryAsync(string competencia);
}
