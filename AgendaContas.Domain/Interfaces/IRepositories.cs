using AgendaContas.Domain.Models;

namespace AgendaContas.Domain.Interfaces;

public interface ICategoriaRepository
{
    Task<IEnumerable<Categoria>> GetAllAsync(bool apenasAtivas = true);
    Task<int> AddAsync(Categoria categoria);
    Task UpdateAsync(Categoria categoria);
    Task SoftDeleteAsync(int id);
}

public interface IContaRepository
{
    Task<IEnumerable<Conta>> GetAllAsync(bool apenasAtivas = true);
    Task<Conta?> GetByIdAsync(int id);
    Task<int> AddAsync(Conta conta);
    Task UpdateAsync(Conta conta);
    Task SoftDeleteAsync(int id);
}

public interface ILancamentoRepository
{
    Task<IEnumerable<Lancamento>> GetAllWithNamesAsync(LancamentoFiltro? filtro = null);
    Task<Lancamento?> GetByIdAsync(int id);
    Task<IEnumerable<Lancamento>> GetByCompetenciaAsync(string competencia);
    Task<int> AddAsync(Lancamento lancamento);
    Task UpdateAsync(Lancamento lancamento);
    Task UpdateStatusAsync(int id, string status, DateTime? dataPagamento = null);
    Task<IEnumerable<Lancamento>> GetAlertasAsync(int dias);
    Task<DashboardSummary> GetDashboardSummaryAsync(string competencia);
}

public interface IUsuarioRepository
{
    Task<Usuario?> GetByLoginAsync(string login);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<int> AddAsync(Usuario usuario);
    Task UpdatePasswordAsync(int id, string senhaHash, string senhaSalt, int iteracoes);
    Task UpdatePerfilAsync(int id, string perfil);
}

public interface IAuditoriaRepository
{
    Task<int> RegistrarAuditoriaAsync(AuditoriaLog log);
    Task<IEnumerable<AuditoriaLog>> GetAuditoriaAsync(int limite = 200);
}

public interface IParticipanteStrRepository
{
    Task<int> SincronizarParticipantesStrAsync();
    Task<IEnumerable<ParticipanteStr>> GetParticipantesStrAsync(bool somenteCompe = false);
    Task<int> GetParticipantesStrCountAsync();
    Task<DateTime?> GetUltimaSincronizacaoParticipantesStrAsync();
}
