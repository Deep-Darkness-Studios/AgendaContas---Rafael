using System.Globalization;
using System.Net.Http;
using System.Text;
using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Models;
using AgendaContas.Domain.Services;
using Dapper;
using Microsoft.Data.Sqlite;

namespace AgendaContas.Data.Repositories;

public class AppRepository : IContaRepository, ILancamentoRepository, ICategoriaRepository, IUsuarioRepository, IAuditoriaRepository, IParticipanteStrRepository
{
    private const string ParticipantesStrCsvUrl = "https://www.bcb.gov.br/content/estabilidadefinanceira/str1/ParticipantesSTR.csv";
    private const string IntegracaoParticipantesStrKey = "BCB_PARTICIPANTES_STR";

    private static readonly HttpClient HttpClient = new();
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

        connection.Execute("PRAGMA foreign_keys = ON;");

        CreateBaseTables(connection);
        ApplySchemaMigrations(connection);
        EnsureIndexes(connection);
        SeedAdminUser(connection);
        NormalizeLegacyRows(connection);
    }

    private static void CreateBaseTables(SqliteConnection connection)
    {
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Categorias (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Ativa INTEGER NOT NULL DEFAULT 1
            );");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Contas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                CategoriaId INTEGER,
                BancoIspb TEXT,
                BancoCodigo TEXT,
                DiaVencimento INTEGER,
                ValorPadrao REAL,
                Recorrente INTEGER NOT NULL DEFAULT 1,
                Ativa INTEGER NOT NULL DEFAULT 1,
                FormaPagamentoPadrao TEXT,
                FOREIGN KEY(CategoriaId) REFERENCES Categorias(Id)
            );");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Lancamentos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ContaId INTEGER NOT NULL,
                Competencia TEXT NOT NULL,
                Vencimento DATETIME NOT NULL,
                Valor REAL NOT NULL,
                Status TEXT NOT NULL,
                DataPagamento DATETIME,
                FormaPagamento TEXT,
                Observacao TEXT,
                AnexoPath TEXT,
                FOREIGN KEY(ContaId) REFERENCES Contas(Id)
            );");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS CompetenciasFechadas (
                Competencia TEXT PRIMARY KEY,
                FechadaEm DATETIME NOT NULL,
                Observacao TEXT
            );");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS ParticipantesSTR (
                Ispb TEXT PRIMARY KEY,
                NomeReduzido TEXT NOT NULL,
                NumeroCodigo TEXT NOT NULL,
                ParticipaDaCompe INTEGER NOT NULL,
                AcessoPrincipal TEXT NOT NULL,
                NomeExtenso TEXT NOT NULL,
                InicioOperacao TEXT NULL,
                AtualizadoEmUtc TEXT NOT NULL
            );");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS IntegracoesStatus (
                Chave TEXT PRIMARY KEY,
                UltimaAtualizacaoUtc TEXT NOT NULL
            );");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Login TEXT NOT NULL UNIQUE,
                SenhaHash TEXT NOT NULL,
                SenhaSalt TEXT NOT NULL DEFAULT '',
                Iteracoes INTEGER NOT NULL DEFAULT 0,
                Perfil TEXT NOT NULL DEFAULT 'Operador'
            );");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Auditoria (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DataHoraUtc TEXT NOT NULL,
                UsuarioId INTEGER NULL,
                UsuarioLogin TEXT NOT NULL,
                Acao TEXT NOT NULL,
                Entidade TEXT NOT NULL,
                EntidadeId INTEGER NULL,
                Detalhes TEXT NULL,
                FOREIGN KEY(UsuarioId) REFERENCES Usuarios(Id)
            );");
    }

    private static void ApplySchemaMigrations(SqliteConnection connection)
    {
        EnsureColumn(connection, "Categorias", "Ativa", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(connection, "Contas", "Ativa", "INTEGER NOT NULL DEFAULT 1");
        EnsureColumn(connection, "Contas", "FormaPagamentoPadrao", "TEXT");
        EnsureColumn(connection, "Contas", "BancoIspb", "TEXT");
        EnsureColumn(connection, "Contas", "BancoCodigo", "TEXT");
        EnsureColumn(connection, "Usuarios", "SenhaSalt", "TEXT NOT NULL DEFAULT ''");
        EnsureColumn(connection, "Usuarios", "Iteracoes", "INTEGER NOT NULL DEFAULT 0");
        EnsureColumn(connection, "Usuarios", "Perfil", "TEXT NOT NULL DEFAULT 'Operador'");
    }

    private static void EnsureIndexes(SqliteConnection connection)
    {
        connection.Execute("CREATE UNIQUE INDEX IF NOT EXISTS UX_Lancamento_Conta_Competencia ON Lancamentos (ContaId, Competencia);");
        connection.Execute("CREATE UNIQUE INDEX IF NOT EXISTS UX_Categoria_Nome_Ativa ON Categorias (Nome, Ativa);");
        connection.Execute("CREATE INDEX IF NOT EXISTS IX_Contas_CategoriaId ON Contas (CategoriaId);");
        connection.Execute("CREATE INDEX IF NOT EXISTS IX_Contas_BancoIspb ON Contas (BancoIspb);");
        connection.Execute("CREATE INDEX IF NOT EXISTS IX_Lancamentos_Competencia ON Lancamentos (Competencia);");
        connection.Execute("CREATE INDEX IF NOT EXISTS IX_ParticipantesSTR_NumeroCodigo ON ParticipantesSTR (NumeroCodigo);");
        connection.Execute("CREATE INDEX IF NOT EXISTS IX_ParticipantesSTR_ParticipaDaCompe ON ParticipantesSTR (ParticipaDaCompe);");
        connection.Execute("CREATE INDEX IF NOT EXISTS IX_Auditoria_DataHoraUtc ON Auditoria (DataHoraUtc DESC);");
        connection.Execute("CREATE INDEX IF NOT EXISTS IX_Auditoria_UsuarioId ON Auditoria (UsuarioId);");
    }

    private static void NormalizeLegacyRows(SqliteConnection connection)
    {
        connection.Execute("UPDATE Categorias SET Ativa = 1 WHERE Ativa IS NULL;");
        connection.Execute("UPDATE Contas SET Ativa = 1 WHERE Ativa IS NULL;");
        connection.Execute("UPDATE Lancamentos SET Status = 'Atrasado' WHERE Status = 'Pendente' AND Vencimento < date('now', 'localtime');");
        connection.Execute("UPDATE Usuarios SET Perfil = 'Operador' WHERE Login <> 'admin';");
        connection.Execute("UPDATE Usuarios SET Perfil = 'Admin' WHERE Login = 'admin';");
    }

    private static void SeedAdminUser(SqliteConnection connection)
    {
        var admin = connection.QueryFirstOrDefault<Usuario>(
            "SELECT Id, Nome, Login, SenhaHash, SenhaSalt, Iteracoes, Perfil FROM Usuarios WHERE Login = 'admin';");

        if (admin == null)
        {
            var (hash, salt, iterations) = PasswordHasher.HashPassword("admin123");
            connection.Execute(
                "INSERT INTO Usuarios (Nome, Login, SenhaHash, SenhaSalt, Iteracoes, Perfil) VALUES (@Nome, @Login, @SenhaHash, @SenhaSalt, @Iteracoes, @Perfil);",
                new Usuario
                {
                    Nome = "Administrador",
                    Login = "admin",
                    SenhaHash = hash,
                    SenhaSalt = salt,
                    Iteracoes = iterations,
                    Perfil = PerfisUsuario.Admin
                });
            return;
        }

        var hashInvalido = string.IsNullOrWhiteSpace(admin.SenhaSalt) ||
                           admin.Iteracoes <= 0 ||
                           string.Equals(admin.SenhaHash, "admin123", StringComparison.Ordinal);

        if (hashInvalido)
        {
            var (hash, salt, iterations) = PasswordHasher.HashPassword("admin123");
            connection.Execute(
                "UPDATE Usuarios SET SenhaHash = @SenhaHash, SenhaSalt = @SenhaSalt, Iteracoes = @Iteracoes WHERE Id = @Id;",
                new
                {
                    admin.Id,
                    SenhaHash = hash,
                    SenhaSalt = salt,
                    Iteracoes = iterations
                });
        }

        connection.Execute(
            "UPDATE Usuarios SET Perfil = @Perfil WHERE Id = @Id;",
            new { admin.Id, Perfil = PerfisUsuario.Admin });
    }

    private static void EnsureColumn(SqliteConnection connection, string tableName, string columnName, string sqlDefinition)
    {
        var count = connection.ExecuteScalar<int>(
            $"SELECT COUNT(*) FROM pragma_table_info('{tableName}') WHERE name = '{columnName}';");

        if (count == 0)
        {
            connection.Execute($"ALTER TABLE {tableName} ADD COLUMN {columnName} {sqlDefinition};");
        }
    }

    private static bool IsCompetenciaFechada(SqliteConnection connection, string competencia)
    {
        return connection.ExecuteScalar<int>(
                   "SELECT COUNT(*) FROM CompetenciasFechadas WHERE Competencia = @Competencia;",
                   new { Competencia = competencia }) > 0;
    }

    private static void ThrowIfCompetenciaFechada(SqliteConnection connection, string competencia, string operation)
    {
        if (IsCompetenciaFechada(connection, competencia))
        {
            throw new InvalidOperationException(
                $"A competência {competencia} está fechada e não permite {operation}.");
        }
    }

    private static string GetCompetenciaByLancamentoId(SqliteConnection connection, int lancamentoId)
    {
        var competencia = connection.ExecuteScalar<string?>(
            "SELECT Competencia FROM Lancamentos WHERE Id = @Id;",
            new { Id = lancamentoId });

        if (string.IsNullOrWhiteSpace(competencia))
        {
            throw new InvalidOperationException("Lançamento não encontrado.");
        }

        return competencia;
    }

    private string GetDatabaseFilePath()
    {
        var builder = new SqliteConnectionStringBuilder(_connectionString);
        return Path.GetFullPath(builder.DataSource);
    }

    private static string NormalizePerfil(string? perfil)
    {
        if (string.Equals(perfil, PerfisUsuario.Admin, StringComparison.OrdinalIgnoreCase))
        {
            return PerfisUsuario.Admin;
        }

        return PerfisUsuario.Operador;
    }

    public Task<int> SincronizarParticipantesStrAsync()
    {
        return SincronizarParticipantesStrAsync(null);
    }

    public async Task<int> SincronizarParticipantesStrAsync(string? csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            using var response = await HttpClient.GetAsync(ParticipantesStrCsvUrl);
            response.EnsureSuccessStatusCode();
            csvContent = await response.Content.ReadAsStringAsync();
        }

        var participantes = ParseParticipantesStrCsv(csvContent).ToList();
        if (participantes.Count == 0)
        {
            return 0;
        }

        var nowUtc = DateTime.UtcNow;

        using var conn = await OpenConnAsync();
        using var tx = await conn.BeginTransactionAsync();

        const string upsertSql = @"
            INSERT INTO ParticipantesSTR
                (Ispb, NomeReduzido, NumeroCodigo, ParticipaDaCompe, AcessoPrincipal, NomeExtenso, InicioOperacao, AtualizadoEmUtc)
            VALUES
                (@Ispb, @NomeReduzido, @NumeroCodigo, @ParticipaDaCompe, @AcessoPrincipal, @NomeExtenso, @InicioOperacao, @AtualizadoEmUtc)
            ON CONFLICT(Ispb) DO UPDATE SET
                NomeReduzido = excluded.NomeReduzido,
                NumeroCodigo = excluded.NumeroCodigo,
                ParticipaDaCompe = excluded.ParticipaDaCompe,
                AcessoPrincipal = excluded.AcessoPrincipal,
                NomeExtenso = excluded.NomeExtenso,
                InicioOperacao = excluded.InicioOperacao,
                AtualizadoEmUtc = excluded.AtualizadoEmUtc;";

        foreach (var participante in participantes)
        {
            await conn.ExecuteAsync(
                upsertSql,
                new
                {
                    participante.Ispb,
                    participante.NomeReduzido,
                    participante.NumeroCodigo,
                    ParticipaDaCompe = participante.ParticipaDaCompe ? 1 : 0,
                    participante.AcessoPrincipal,
                    participante.NomeExtenso,
                    InicioOperacao = participante.InicioOperacao?.ToString("yyyy-MM-dd"),
                    AtualizadoEmUtc = nowUtc.ToString("O")
                },
                tx);
        }

        await conn.ExecuteAsync(
            @"INSERT INTO IntegracoesStatus (Chave, UltimaAtualizacaoUtc)
              VALUES (@Chave, @UltimaAtualizacaoUtc)
              ON CONFLICT(Chave) DO UPDATE SET
                 UltimaAtualizacaoUtc = excluded.UltimaAtualizacaoUtc;",
            new
            {
                Chave = IntegracaoParticipantesStrKey,
                UltimaAtualizacaoUtc = nowUtc.ToString("O")
            },
            tx);

        await tx.CommitAsync();
        return participantes.Count;
    }

    public async Task<IEnumerable<ParticipanteStr>> GetParticipantesStrAsync(bool somenteCompe = false)
    {
        using var conn = await OpenConnAsync();
        var sql = new StringBuilder(@"
            SELECT Ispb,
                   NomeReduzido,
                   NumeroCodigo,
                   ParticipaDaCompe,
                   AcessoPrincipal,
                   NomeExtenso,
                   InicioOperacao,
                   AtualizadoEmUtc
            FROM ParticipantesSTR");

        if (somenteCompe)
        {
            sql.Append(" WHERE ParticipaDaCompe = 1");
        }

        sql.Append(" ORDER BY NomeReduzido;");
        return await conn.QueryAsync<ParticipanteStr>(sql.ToString());
    }

    public async Task<int> GetParticipantesStrCountAsync()
    {
        using var conn = await OpenConnAsync();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ParticipantesSTR;");
    }

    public async Task<DateTime?> GetUltimaSincronizacaoParticipantesStrAsync()
    {
        using var conn = await OpenConnAsync();
        var value = await conn.ExecuteScalarAsync<string?>(
            "SELECT UltimaAtualizacaoUtc FROM IntegracoesStatus WHERE Chave = @Chave;",
            new { Chave = IntegracaoParticipantesStrKey });

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTime.TryParse(value, null, DateTimeStyles.RoundtripKind, out var date)
            ? date
            : null;
    }

    private static IEnumerable<ParticipanteStr> ParseParticipantesStrCsv(string csvContent)
    {
        var lines = csvContent
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (lines.Count <= 1)
        {
            yield break;
        }

        for (var i = 1; i < lines.Count; i++)
        {
            var fields = ParseCsvLine(lines[i]);
            if (fields.Count < 7)
            {
                continue;
            }

            var inicioOperacao = ParseDate(fields[6]);
            var numeroCodigo = fields[2].Trim();
            if (string.Equals(numeroCodigo, "n/a", StringComparison.OrdinalIgnoreCase))
            {
                numeroCodigo = string.Empty;
            }

            yield return new ParticipanteStr
            {
                Ispb = fields[0].Trim(),
                NomeReduzido = fields[1].Trim(),
                NumeroCodigo = numeroCodigo,
                ParticipaDaCompe = string.Equals(fields[3].Trim(), "Sim", StringComparison.OrdinalIgnoreCase),
                AcessoPrincipal = fields[4].Trim(),
                NomeExtenso = fields[5].Trim(),
                InicioOperacao = inicioOperacao
            };
        }
    }

    private static DateTime? ParseDate(string value)
    {
        if (DateTime.TryParseExact(
                value.Trim(),
                "dd/MM/yyyy",
                CultureInfo.GetCultureInfo("pt-BR"),
                DateTimeStyles.None,
                out var parsed))
        {
            return parsed.Date;
        }

        return null;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }

                continue;
            }

            if (c == ',' && !insideQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        fields.Add(current.ToString());
        return fields;
    }

    public async Task<IEnumerable<Categoria>> GetCategoriasAsync(bool apenasAtivas = true)
    {
        using var conn = await OpenConnAsync();
        var sql = new StringBuilder("SELECT Id, Nome, Ativa FROM Categorias");
        if (apenasAtivas)
        {
            sql.Append(" WHERE Ativa = 1");
        }

        sql.Append(" ORDER BY Nome;");
        return await conn.QueryAsync<Categoria>(sql.ToString());
    }

    Task<IEnumerable<Categoria>> ICategoriaRepository.GetAllAsync(bool apenasAtivas)
    {
        return GetCategoriasAsync(apenasAtivas);
    }

    public async Task<int> AddAsync(Categoria categoria)
    {
        using var conn = await OpenConnAsync();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Categorias (Nome, Ativa)
              VALUES (@Nome, @Ativa);
              SELECT last_insert_rowid();",
            categoria);
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync(
            "UPDATE Categorias SET Nome = @Nome, Ativa = @Ativa WHERE Id = @Id;",
            categoria);
    }

    public async Task SoftDeleteCategoriaAsync(int id)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync("UPDATE Categorias SET Ativa = 0 WHERE Id = @Id;", new { Id = id });
    }

    Task ICategoriaRepository.SoftDeleteAsync(int id)
    {
        return SoftDeleteCategoriaAsync(id);
    }

    public async Task<IEnumerable<Conta>> GetContasAsync(bool apenasAtivas = true)
    {
        using var conn = await OpenConnAsync();
        var sql = new StringBuilder(@"
            SELECT C.Id,
                   C.Nome,
                   C.CategoriaId,
                   C.BancoIspb,
                   C.BancoCodigo,
                   C.DiaVencimento,
                   C.ValorPadrao,
                   C.Recorrente,
                   C.Ativa,
                   C.FormaPagamentoPadrao,
                   P.NomeReduzido AS BancoNome
            FROM Contas C
            LEFT JOIN ParticipantesSTR P ON P.Ispb = C.BancoIspb");
        if (apenasAtivas)
        {
            sql.Append(" WHERE C.Ativa = 1");
        }

        sql.Append(" ORDER BY C.Nome;");
        return await conn.QueryAsync<Conta>(sql.ToString());
    }

    public async Task<IEnumerable<Conta>> GetContasComCategoriaAsync(bool apenasAtivas = true)
    {
        using var conn = await OpenConnAsync();
        var sql = new StringBuilder(@"
            SELECT C.Id,
                   C.Nome,
                   C.CategoriaId,
                   C.BancoIspb,
                   C.BancoCodigo,
                   C.DiaVencimento,
                   C.ValorPadrao,
                   C.Recorrente,
                   C.Ativa,
                   C.FormaPagamentoPadrao,
                   CAT.Nome AS CategoriaNome,
                   P.NomeReduzido AS BancoNome
            FROM Contas C
            LEFT JOIN Categorias CAT ON CAT.Id = C.CategoriaId
            LEFT JOIN ParticipantesSTR P ON P.Ispb = C.BancoIspb");

        if (apenasAtivas)
        {
            sql.Append(" WHERE C.Ativa = 1");
        }

        sql.Append(" ORDER BY C.Nome;");
        return await conn.QueryAsync<Conta>(sql.ToString());
    }

    Task<IEnumerable<Conta>> IContaRepository.GetAllAsync(bool apenasAtivas)
    {
        return GetContasAsync(apenasAtivas);
    }

    public async Task<Conta?> GetContaByIdAsync(int id)
    {
        using var conn = await OpenConnAsync();
        return await conn.QueryFirstOrDefaultAsync<Conta>(@"
            SELECT C.Id,
                   C.Nome,
                   C.CategoriaId,
                   C.BancoIspb,
                   C.BancoCodigo,
                   C.DiaVencimento,
                   C.ValorPadrao,
                   C.Recorrente,
                   C.Ativa,
                   C.FormaPagamentoPadrao,
                   P.NomeReduzido AS BancoNome
            FROM Contas C
            LEFT JOIN ParticipantesSTR P ON P.Ispb = C.BancoIspb
            WHERE C.Id = @Id;",
            new { Id = id });
    }

    Task<Conta?> IContaRepository.GetByIdAsync(int id)
    {
        return GetContaByIdAsync(id);
    }

    public async Task<int> AddAsync(Conta conta)
    {
        using var conn = await OpenConnAsync();
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Contas (Nome, CategoriaId, BancoIspb, BancoCodigo, DiaVencimento, ValorPadrao, Recorrente, Ativa, FormaPagamentoPadrao)
              VALUES (@Nome, @CategoriaId, @BancoIspb, @BancoCodigo, @DiaVencimento, @ValorPadrao, @Recorrente, @Ativa, @FormaPagamentoPadrao);
              SELECT last_insert_rowid();",
            conta);
    }

    public async Task UpdateAsync(Conta conta)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync(
            @"UPDATE Contas
              SET Nome = @Nome,
                  CategoriaId = @CategoriaId,
                  BancoIspb = @BancoIspb,
                  BancoCodigo = @BancoCodigo,
                  DiaVencimento = @DiaVencimento,
                  ValorPadrao = @ValorPadrao,
                  Recorrente = @Recorrente,
                  Ativa = @Ativa,
                  FormaPagamentoPadrao = @FormaPagamentoPadrao
              WHERE Id = @Id;",
            conta);
    }

    public async Task SoftDeleteContaAsync(int id)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync("UPDATE Contas SET Ativa = 0 WHERE Id = @Id;", new { Id = id });
    }

    Task IContaRepository.SoftDeleteAsync(int id)
    {
        return SoftDeleteContaAsync(id);
    }

    public async Task<IEnumerable<Lancamento>> GetAllWithNamesAsync(LancamentoFiltro? filtro = null)
    {
        using var conn = await OpenConnAsync();
        var sql = new StringBuilder(@"
            SELECT L.Id,
                   L.ContaId,
                   L.Competencia,
                   L.Vencimento,
                   L.Valor,
                   L.Status,
                   L.DataPagamento,
                   L.FormaPagamento,
                   L.Observacao,
                   L.AnexoPath,
                   C.Nome AS NomeConta,
                   C.CategoriaId AS CategoriaId,
                   CAT.Nome AS CategoriaNome
            FROM Lancamentos L
            JOIN Contas C ON C.Id = L.ContaId
            LEFT JOIN Categorias CAT ON CAT.Id = C.CategoriaId
            WHERE 1 = 1");

        var parameters = new DynamicParameters();
        filtro ??= new LancamentoFiltro();

        if (!string.IsNullOrWhiteSpace(filtro.Competencia))
        {
            sql.Append(" AND L.Competencia = @Competencia");
            parameters.Add("Competencia", filtro.Competencia);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Status) &&
            !string.Equals(filtro.Status, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            sql.Append(" AND L.Status = @Status");
            parameters.Add("Status", filtro.Status);
        }

        if (filtro.CategoriaId.HasValue)
        {
            sql.Append(" AND C.CategoriaId = @CategoriaId");
            parameters.Add("CategoriaId", filtro.CategoriaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.TextoBusca))
        {
            sql.Append(" AND C.Nome LIKE @TextoBusca");
            parameters.Add("TextoBusca", $"%{filtro.TextoBusca.Trim()}%");
        }

        if (!filtro.IncluirContasInativas)
        {
            sql.Append(" AND C.Ativa = 1");
        }

        sql.Append(" ORDER BY L.Vencimento;");
        return await conn.QueryAsync<Lancamento>(sql.ToString(), parameters);
    }

    public async Task<Lancamento?> GetLancamentoByIdAsync(int id)
    {
        using var conn = await OpenConnAsync();
        return await conn.QueryFirstOrDefaultAsync<Lancamento>(@"
            SELECT L.Id,
                   L.ContaId,
                   L.Competencia,
                   L.Vencimento,
                   L.Valor,
                   L.Status,
                   L.DataPagamento,
                   L.FormaPagamento,
                   L.Observacao,
                   L.AnexoPath,
                   C.Nome AS NomeConta,
                   C.CategoriaId AS CategoriaId,
                   CAT.Nome AS CategoriaNome
            FROM Lancamentos L
            JOIN Contas C ON C.Id = L.ContaId
            LEFT JOIN Categorias CAT ON CAT.Id = C.CategoriaId
            WHERE L.Id = @Id;",
            new { Id = id });
    }

    Task<Lancamento?> ILancamentoRepository.GetByIdAsync(int id)
    {
        return GetLancamentoByIdAsync(id);
    }

    public async Task<IEnumerable<Lancamento>> GetByCompetenciaAsync(string competencia)
    {
        return await GetAllWithNamesAsync(new LancamentoFiltro { Competencia = competencia, IncluirContasInativas = true });
    }

    public async Task<int> AddAsync(Lancamento lancamento)
    {
        using var conn = await OpenConnAsync();
        ThrowIfCompetenciaFechada(conn, lancamento.Competencia, "inclusão de lançamentos");

        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Lancamentos (ContaId, Competencia, Vencimento, Valor, Status, DataPagamento, FormaPagamento, Observacao, AnexoPath)
              VALUES (@ContaId, @Competencia, @Vencimento, @Valor, @Status, @DataPagamento, @FormaPagamento, @Observacao, @AnexoPath);
              SELECT last_insert_rowid();",
            lancamento);
    }

    public async Task UpdateAsync(Lancamento lancamento)
    {
        using var conn = await OpenConnAsync();
        var competenciaAtual = GetCompetenciaByLancamentoId(conn, lancamento.Id);
        ThrowIfCompetenciaFechada(conn, competenciaAtual, "edição de lançamentos");
        ThrowIfCompetenciaFechada(conn, lancamento.Competencia, "edição de lançamentos");

        await conn.ExecuteAsync(
            @"UPDATE Lancamentos
              SET ContaId = @ContaId,
                  Competencia = @Competencia,
                  Vencimento = @Vencimento,
                  Valor = @Valor,
                  Status = @Status,
                  DataPagamento = @DataPagamento,
                  FormaPagamento = @FormaPagamento,
                  Observacao = @Observacao,
                  AnexoPath = @AnexoPath
              WHERE Id = @Id;",
            lancamento);
    }

    public async Task UpdateStatusAsync(int id, string status, DateTime? dataPagamento = null)
    {
        using var conn = await OpenConnAsync();
        var competencia = GetCompetenciaByLancamentoId(conn, id);
        ThrowIfCompetenciaFechada(conn, competencia, "alteração de status");

        await conn.ExecuteAsync(
            @"UPDATE Lancamentos
              SET Status = @Status,
                  DataPagamento = @DataPagamento
              WHERE Id = @Id;",
            new { Id = id, Status = status, DataPagamento = dataPagamento });
    }

    public async Task DeleteLancamentoAsync(int id)
    {
        using var conn = await OpenConnAsync();
        var competencia = GetCompetenciaByLancamentoId(conn, id);
        ThrowIfCompetenciaFechada(conn, competencia, "exclusão de lançamentos");

        await conn.ExecuteAsync("DELETE FROM Lancamentos WHERE Id = @Id;", new { Id = id });
    }

    public async Task UpdateAnexoPathAsync(int lancamentoId, string? anexoPath)
    {
        using var conn = await OpenConnAsync();
        var competencia = GetCompetenciaByLancamentoId(conn, lancamentoId);
        ThrowIfCompetenciaFechada(conn, competencia, "alteração de anexo");

        await conn.ExecuteAsync(
            "UPDATE Lancamentos SET AnexoPath = @AnexoPath WHERE Id = @Id;",
            new { Id = lancamentoId, AnexoPath = anexoPath });
    }

    public async Task<bool> IsCompetenciaFechadaAsync(string competencia)
    {
        using var conn = await OpenConnAsync();
        return IsCompetenciaFechada(conn, competencia);
    }

    public async Task<IEnumerable<string>> GetCompetenciasFechadasAsync()
    {
        using var conn = await OpenConnAsync();
        return await conn.QueryAsync<string>(
            "SELECT Competencia FROM CompetenciasFechadas ORDER BY Competencia DESC;");
    }

    public async Task FecharCompetenciaAsync(string competencia, string? observacao = null)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync(
            @"INSERT OR IGNORE INTO CompetenciasFechadas (Competencia, FechadaEm, Observacao)
              VALUES (@Competencia, @FechadaEm, @Observacao);",
            new
            {
                Competencia = competencia,
                FechadaEm = DateTime.Now,
                Observacao = observacao
            });
    }

    public async Task ReabrirCompetenciaAsync(string competencia)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync(
            "DELETE FROM CompetenciasFechadas WHERE Competencia = @Competencia;",
            new { Competencia = competencia });
    }

    public async Task BackupDatabaseAsync(string destinationFilePath)
    {
        var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
        if (!string.IsNullOrWhiteSpace(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        await using var source = new SqliteConnection(_connectionString);
        await source.OpenAsync();

        var destinationConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = destinationFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        await using var destination = new SqliteConnection(destinationConnectionString);
        await destination.OpenAsync();
        source.BackupDatabase(destination);
    }

    public async Task RestoreDatabaseAsync(string sourceFilePath)
    {
        if (!File.Exists(sourceFilePath))
        {
            throw new FileNotFoundException("Arquivo de backup não encontrado.", sourceFilePath);
        }

        var targetDatabasePath = GetDatabaseFilePath();
        if (string.Equals(
                Path.GetFullPath(sourceFilePath),
                targetDatabasePath,
                StringComparison.OrdinalIgnoreCase))
        {
            InitializeDatabase();
            return;
        }

        await using var source = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = sourceFilePath,
            Mode = SqliteOpenMode.ReadOnly
        }.ToString());
        await source.OpenAsync();

        await using var destination = new SqliteConnection(_connectionString);
        await destination.OpenAsync();
        source.BackupDatabase(destination);

        InitializeDatabase();
    }

    public async Task<IEnumerable<Lancamento>> GetAlertasAsync(int dias)
    {
        using var conn = await OpenConnAsync();
        return await conn.QueryAsync<Lancamento>(@"
            SELECT L.Id,
                   L.ContaId,
                   L.Competencia,
                   L.Vencimento,
                   L.Valor,
                   L.Status,
                   L.DataPagamento,
                   L.FormaPagamento,
                   L.Observacao,
                   L.AnexoPath,
                   C.Nome AS NomeConta,
                   C.CategoriaId AS CategoriaId,
                   CAT.Nome AS CategoriaNome
            FROM Lancamentos L
            JOIN Contas C ON C.Id = L.ContaId
            LEFT JOIN Categorias CAT ON CAT.Id = C.CategoriaId
            WHERE L.Status = 'Pendente'
              AND L.Vencimento <= date('now', 'localtime', '+' || @Dias || ' days')
              AND L.Vencimento >= date('now', 'localtime')
              AND C.Ativa = 1
            ORDER BY L.Vencimento;",
            new { Dias = dias });
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync(string competencia)
    {
        using var conn = await OpenConnAsync();
        var result = await conn.QueryFirstOrDefaultAsync<DashboardSummary>(@"
            SELECT
                IFNULL(SUM(CASE WHEN L.Competencia = @Comp THEN L.Valor ELSE 0 END), 0) AS TotalMes,
                IFNULL(SUM(CASE WHEN L.Competencia = @Comp AND L.Status = 'Pago' THEN L.Valor ELSE 0 END), 0) AS TotalPago,
                IFNULL(SUM(CASE WHEN L.Competencia = @Comp AND L.Status = 'Pendente' THEN L.Valor ELSE 0 END), 0) AS TotalPendente,
                IFNULL(SUM(CASE WHEN L.Competencia = @Comp AND L.Status = 'Atrasado' THEN L.Valor ELSE 0 END), 0) AS TotalAtrasado
            FROM Lancamentos L
            JOIN Contas C ON C.Id = L.ContaId
            WHERE C.Ativa = 1;",
            new { Comp = competencia });

        return result ?? new DashboardSummary();
    }

    public async Task<Usuario?> GetByLoginAsync(string login)
    {
        using var conn = await OpenConnAsync();
        var usuario = await conn.QueryFirstOrDefaultAsync<Usuario>(
            "SELECT Id, Nome, Login, SenhaHash, SenhaSalt, Iteracoes, Perfil FROM Usuarios WHERE Login = @Login;",
            new { Login = login });

        if (usuario == null)
        {
            return null;
        }

        usuario.Perfil = NormalizePerfil(usuario.Perfil);
        return usuario;
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        using var conn = await OpenConnAsync();
        var usuarios = (await conn.QueryAsync<Usuario>(
            "SELECT Id, Nome, Login, SenhaHash, SenhaSalt, Iteracoes, Perfil FROM Usuarios ORDER BY Nome, Login;")).ToList();

        foreach (var usuario in usuarios)
        {
            usuario.Perfil = NormalizePerfil(usuario.Perfil);
        }

        return usuarios;
    }

    public async Task<int> AddAsync(Usuario usuario)
    {
        using var conn = await OpenConnAsync();
        var perfil = NormalizePerfil(usuario.Perfil);
        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Usuarios (Nome, Login, SenhaHash, SenhaSalt, Iteracoes, Perfil)
              VALUES (@Nome, @Login, @SenhaHash, @SenhaSalt, @Iteracoes, @Perfil);
              SELECT last_insert_rowid();",
            new
            {
                usuario.Nome,
                usuario.Login,
                usuario.SenhaHash,
                usuario.SenhaSalt,
                usuario.Iteracoes,
                Perfil = perfil
            });
    }

    public async Task UpdatePasswordAsync(int id, string senhaHash, string senhaSalt, int iteracoes)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync(
            @"UPDATE Usuarios
              SET SenhaHash = @SenhaHash,
                  SenhaSalt = @SenhaSalt,
                  Iteracoes = @Iteracoes
              WHERE Id = @Id;",
            new { Id = id, SenhaHash = senhaHash, SenhaSalt = senhaSalt, Iteracoes = iteracoes });
    }

    public async Task UpdatePerfilAsync(int id, string perfil)
    {
        using var conn = await OpenConnAsync();
        await conn.ExecuteAsync(
            "UPDATE Usuarios SET Perfil = @Perfil WHERE Id = @Id;",
            new { Id = id, Perfil = NormalizePerfil(perfil) });
    }

    public async Task<int> RegistrarAuditoriaAsync(AuditoriaLog log)
    {
        using var conn = await OpenConnAsync();
        var usuarioLogin = string.IsNullOrWhiteSpace(log.UsuarioLogin)
            ? "sistema"
            : log.UsuarioLogin.Trim();
        var acao = string.IsNullOrWhiteSpace(log.Acao) ? "SEM_ACAO" : log.Acao.Trim().ToUpperInvariant();
        var entidade = string.IsNullOrWhiteSpace(log.Entidade) ? "GERAL" : log.Entidade.Trim();

        return await conn.ExecuteScalarAsync<int>(
            @"INSERT INTO Auditoria (DataHoraUtc, UsuarioId, UsuarioLogin, Acao, Entidade, EntidadeId, Detalhes)
              VALUES (@DataHoraUtc, @UsuarioId, @UsuarioLogin, @Acao, @Entidade, @EntidadeId, @Detalhes);
              SELECT last_insert_rowid();",
            new
            {
                DataHoraUtc = log.DataHoraUtc == default ? DateTime.UtcNow : log.DataHoraUtc,
                log.UsuarioId,
                UsuarioLogin = usuarioLogin,
                Acao = acao,
                Entidade = entidade,
                log.EntidadeId,
                log.Detalhes
            });
    }

    public async Task<int> RegistrarAuditoriaAsync(
        string acao,
        string entidade,
        int? entidadeId = null,
        Usuario? usuario = null,
        string? detalhes = null)
    {
        return await RegistrarAuditoriaAsync(new AuditoriaLog
        {
            DataHoraUtc = DateTime.UtcNow,
            UsuarioId = usuario?.Id,
            UsuarioLogin = usuario?.Login ?? "sistema",
            Acao = acao,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Detalhes = detalhes
        });
    }

    public async Task<IEnumerable<AuditoriaLog>> GetAuditoriaAsync(int limite = 200)
    {
        using var conn = await OpenConnAsync();
        var take = limite <= 0 ? 200 : Math.Min(limite, 1000);

        return await conn.QueryAsync<AuditoriaLog>(
            @"SELECT Id,
                     DataHoraUtc,
                     UsuarioId,
                     UsuarioLogin,
                     Acao,
                     Entidade,
                     EntidadeId,
                     Detalhes
              FROM Auditoria
              ORDER BY DataHoraUtc DESC
              LIMIT @Take;",
            new { Take = take });
    }
}
