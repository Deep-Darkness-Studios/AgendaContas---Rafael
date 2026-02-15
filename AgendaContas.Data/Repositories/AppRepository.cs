using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace AgendaContas.Data.Repositories;

public class AppRepository : IContaRepository, ILancamentoRepository
{
    private readonly string _connectionString;

    public AppRepository(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDatabase();
    }

    private async Task<SqliteConnection> OpenConnAsync()
    {
        var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        connection.Execute(@"CREATE TABLE IF NOT EXISTS Categorias (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nome TEXT NOT NULL)");
        
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Contas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                CategoriaId INTEGER,
                DiaVencimento INTEGER,
                ValorPadrao DECIMAL,
                Recorrente INTEGER,
                Ativa INTEGER,
                FormaPagamentoPadrao TEXT
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Lancamentos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ContaId INTEGER,
                Competencia TEXT,
                Vencimento DATETIME,
                Valor DECIMAL,
                Status TEXT,
                DataPagamento DATETIME,
                FormaPagamento TEXT,
                Observacao TEXT,
                AnexoPath TEXT,
                FOREIGN KEY(ContaId) REFERENCES Contas(Id)
            )");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Login TEXT NOT NULL UNIQUE,
                SenhaHash TEXT NOT NULL
            )");
        
        connection.Execute("INSERT OR IGNORE INTO Usuarios (Nome, Login, SenhaHash) VALUES ('Administrador', 'admin', 'admin123')");
        connection.Execute("CREATE UNIQUE INDEX IF NOT EXISTS UX_Lancamento_Conta_Competencia ON Lancamentos (ContaId, Competencia)");
        connection.Execute("UPDATE Lancamentos SET Status = 'Atrasado' WHERE Status = 'Pendente' AND Vencimento < date('now', 'localtime')");
    }

    public async Task<IEnumerable<Conta>> GetAllAsync()
    {
        using var conn = await OpenConnAsync();
        return await conn.QueryAsync<Conta>("SELECT * FROM Contas");
    }

    public async Task<int> AddAsync(Conta conta)
    {
        using var conn = await OpenConnAsync();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO Contas (Nome, CategoriaId, DiaVencimento, ValorPadrao, Recorrente, Ativa, FormaPagamentoPadrao) VALUES (@Nome, @CategoriaId, @DiaVencimento, @ValorPadrao, @Recorrente, @Ativa, @FormaPagamentoPadrao); SELECT last_insert_rowid();", conta);
    }

    public async Task UpdateAsync(Conta conta)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync(
            "UPDATE Contas SET Nome=@Nome, CategoriaId=@CategoriaId, DiaVencimento=@DiaVencimento, ValorPadrao=@ValorPadrao, Recorrente=@Recorrente, Ativa=@Ativa, FormaPagamentoPadrao=@FormaPagamentoPadrao WHERE Id=@Id", conta);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync("DELETE FROM Contas WHERE Id=@Id", new { Id = id });
    }

    public async Task<IEnumerable<Lancamento>> GetAllWithNamesAsync()
    {
        using var conn = await OpenConnAsync();
        return await conn.QueryAsync<Lancamento>(
            "SELECT L.*, C.Nome as NomeConta FROM Lancamentos L JOIN Contas C ON L.ContaId = C.Id ORDER BY L.Vencimento");
    }

    public async Task<IEnumerable<Lancamento>> GetByCompetenciaAsync(string competencia)
    {
        using var conn = await OpenConnAsync();
        return await conn.QueryAsync<Lancamento>(
            "SELECT L.*, C.Nome as NomeConta FROM Lancamentos L JOIN Contas C ON L.ContaId = C.Id WHERE L.Competencia = @Competencia", new { Competencia = competencia });
    }

    public async Task<int> AddAsync(Lancamento lancamento)
    {
        using var conn = await OpenConnAsync();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO Lancamentos (ContaId, Competencia, Vencimento, Valor, Status, FormaPagamento, Observacao, AnexoPath) VALUES (@ContaId, @Competencia, @Vencimento, @Valor, @Status, @FormaPagamento, @Observacao, @AnexoPath); SELECT last_insert_rowid();", lancamento);
    }

    public async Task UpdateStatusAsync(int id, string status, DateTime? dataPagamento = null)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync(
            "UPDATE Lancamentos SET Status=@Status, DataPagamento=@DataPagamento WHERE Id=@Id", new { Id = id, Status = status, DataPagamento = dataPagamento });
    }

    public async Task<IEnumerable<Lancamento>> GetAlertasAsync(int dias)
    {
        using var conn = await OpenConnAsync();
        return await conn.QueryAsync<Lancamento>(
            "SELECT L.*, C.Nome as NomeConta FROM Lancamentos L JOIN Contas C ON L.ContaId = C.Id WHERE L.Status = 'Pendente' AND L.Vencimento <= date('now', 'localtime', '+' || @Dias || ' days') AND L.Vencimento >= date('now', 'localtime')", new { Dias = dias });
    }

    public async Task<dynamic> GetDashboardSummaryAsync(string competencia)
    {
        using var conn = await OpenConnAsync();
        var result = await conn.QueryFirstOrDefaultAsync(@"
            SELECT 
                IFNULL(SUM(CASE WHEN Competencia = @Comp THEN Valor ELSE 0 END), 0) as TotalMes,
                IFNULL(SUM(CASE WHEN Competencia = @Comp AND Status = 'Pago' THEN Valor ELSE 0 END), 0) as TotalPago,
                IFNULL(SUM(CASE WHEN Competencia = @Comp AND Status = 'Pendente' THEN Valor ELSE 0 END), 0) as TotalPendente,
                IFNULL(SUM(CASE WHEN Status = 'Atrasado' THEN Valor ELSE 0 END), 0) as TotalAtrasado
            FROM Lancamentos", new { Comp = competencia });
            
        return result ?? new { TotalMes = 0m, TotalPago = 0m, TotalPendente = 0m, TotalAtrasado = 0m };
    }
}
