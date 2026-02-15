using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Models;
using AgendaContas.Domain.Services;
using Xunit;

namespace AgendaContas.Tests;

public class FinanceiroServiceTests
{
    [Fact]
    public async Task GerarLancamentosRecorrentesAsync_NaoDuplicaCompetencia()
    {
        var competencia = DateTime.Now.ToString("yyyy-MM");
        var conta = new Conta
        {
            Id = 1,
            Nome = "Internet",
            DiaVencimento = 10,
            ValorPadrao = 100m,
            Recorrente = true,
            Ativa = true
        };

        var contaRepo = new FakeContaRepository(new[] { conta });
        var lancamentoRepo = new FakeLancamentoRepository(new[]
        {
            new Lancamento
            {
                Id = 1,
                ContaId = 1,
                Competencia = competencia,
                Vencimento = DateTime.Today,
                Valor = 100m,
                Status = "Pendente"
            }
        });

        var service = new FinanceiroService(contaRepo, lancamentoRepo);
        await service.GerarLancamentosRecorrentesAsync();

        var daCompetencia = await lancamentoRepo.GetByCompetenciaAsync(competencia);
        Assert.Single(daCompetencia);
    }

    [Fact]
    public async Task GerarLancamentosRecorrentesAsync_CriaQuandoNaoExiste()
    {
        var competencia = DateTime.Now.ToString("yyyy-MM");
        var conta = new Conta
        {
            Id = 2,
            Nome = "Energia",
            DiaVencimento = 15,
            ValorPadrao = 220m,
            Recorrente = true,
            Ativa = true
        };

        var contaRepo = new FakeContaRepository(new[] { conta });
        var lancamentoRepo = new FakeLancamentoRepository(Array.Empty<Lancamento>());
        var service = new FinanceiroService(contaRepo, lancamentoRepo);

        await service.GerarLancamentosRecorrentesAsync();

        var daCompetencia = (await lancamentoRepo.GetByCompetenciaAsync(competencia)).ToList();
        Assert.Single(daCompetencia);
        Assert.Equal(conta.Id, daCompetencia[0].ContaId);
    }

    private sealed class FakeContaRepository : IContaRepository
    {
        private readonly List<Conta> _contas;

        public FakeContaRepository(IEnumerable<Conta> contas)
        {
            _contas = contas.ToList();
        }

        public Task<IEnumerable<Conta>> GetAllAsync(bool apenasAtivas = true)
        {
            var result = apenasAtivas ? _contas.Where(x => x.Ativa) : _contas;
            return Task.FromResult(result);
        }

        public Task<Conta?> GetByIdAsync(int id)
        {
            return Task.FromResult(_contas.FirstOrDefault(x => x.Id == id));
        }

        public Task<int> AddAsync(Conta conta)
        {
            conta.Id = _contas.Count == 0 ? 1 : _contas.Max(c => c.Id) + 1;
            _contas.Add(conta);
            return Task.FromResult(conta.Id);
        }

        public Task UpdateAsync(Conta conta)
        {
            return Task.CompletedTask;
        }

        public Task SoftDeleteAsync(int id)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeLancamentoRepository : ILancamentoRepository
    {
        private readonly List<Lancamento> _lancamentos;

        public FakeLancamentoRepository(IEnumerable<Lancamento> lancamentos)
        {
            _lancamentos = lancamentos.ToList();
        }

        public Task<IEnumerable<Lancamento>> GetAllWithNamesAsync(LancamentoFiltro? filtro = null)
        {
            return Task.FromResult(_lancamentos.AsEnumerable());
        }

        public Task<Lancamento?> GetByIdAsync(int id)
        {
            return Task.FromResult(_lancamentos.FirstOrDefault(x => x.Id == id));
        }

        public Task<IEnumerable<Lancamento>> GetByCompetenciaAsync(string competencia)
        {
            return Task.FromResult(_lancamentos.Where(x => x.Competencia == competencia));
        }

        public Task<int> AddAsync(Lancamento lancamento)
        {
            lancamento.Id = _lancamentos.Count == 0 ? 1 : _lancamentos.Max(l => l.Id) + 1;
            _lancamentos.Add(lancamento);
            return Task.FromResult(lancamento.Id);
        }

        public Task UpdateAsync(Lancamento lancamento)
        {
            return Task.CompletedTask;
        }

        public Task UpdateStatusAsync(int id, string status, DateTime? dataPagamento = null)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Lancamento>> GetAlertasAsync(int dias)
        {
            return Task.FromResult(Enumerable.Empty<Lancamento>());
        }

        public Task<DashboardSummary> GetDashboardSummaryAsync(string competencia)
        {
            return Task.FromResult(new DashboardSummary());
        }
    }
}
