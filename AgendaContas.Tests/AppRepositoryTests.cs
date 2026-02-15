using AgendaContas.Data.Repositories;
using AgendaContas.Domain.Models;
using Microsoft.Data.Sqlite;
using Xunit;

namespace AgendaContas.Tests;

public class AppRepositoryTests
{
    [Fact]
    public async Task SincronizarParticipantesStrAsync_ImportaCsvDoBCBNoBancoLocal()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            var repo = new AppRepository(ToConnectionString(dbPath));
            const string csv = """
ISPB,Nome_Reduzido,Número_Código,Participa_da_Compe,Acesso_Principal,Nome_Extenso,Início_da_Operação
00000000,BCO DO BRASIL S.A.,001,Sim,RSFN,Banco do Brasil S.A.,22/04/2002
00122327,SANTINVEST S.A. - CFI,539,Não,RSFN,"SANTINVEST S.A. - CREDITO, FINANCIAMENTO E INVESTIMENTOS",17/04/2023
""";

            var imported = await repo.SincronizarParticipantesStrAsync(csv);

            Assert.Equal(2, imported);
            Assert.Equal(2, await repo.GetParticipantesStrCountAsync());

            var somenteCompe = (await repo.GetParticipantesStrAsync(somenteCompe: true)).ToList();
            Assert.Single(somenteCompe);
            Assert.Equal("00000000", somenteCompe[0].Ispb);

            var ultima = await repo.GetUltimaSincronizacaoParticipantesStrAsync();
            Assert.True(ultima.HasValue);
        }
        finally
        {
            TryDelete(dbPath);
        }
    }

    [Fact]
    public async Task AddConta_ComBanco_PersisteDadosBancarios()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            var repo = new AppRepository(ToConnectionString(dbPath));
            const string csv = """
ISPB,Nome_Reduzido,Número_Código,Participa_da_Compe,Acesso_Principal,Nome_Extenso,Início_da_Operação
00000000,BCO DO BRASIL S.A.,001,Sim,RSFN,Banco do Brasil S.A.,22/04/2002
""";
            await repo.SincronizarParticipantesStrAsync(csv);

            var categoriaId = await repo.AddAsync(new Categoria { Nome = "Teste", Ativa = true });
            var contaId = await repo.AddAsync(new Conta
            {
                Nome = "Conta Com Banco",
                CategoriaId = categoriaId,
                BancoIspb = "00000000",
                BancoCodigo = "001",
                DiaVencimento = 5,
                ValorPadrao = 120m,
                Recorrente = true,
                Ativa = true,
                FormaPagamentoPadrao = "Pix"
            });

            var conta = await repo.GetContaByIdAsync(contaId);
            Assert.NotNull(conta);
            Assert.Equal("00000000", conta!.BancoIspb);
            Assert.Equal("001", conta.BancoCodigo);
            Assert.Equal("BCO DO BRASIL S.A.", conta.BancoNome);
        }
        finally
        {
            TryDelete(dbPath);
        }
    }

    [Fact]
    public async Task CompetenciaFechada_BloqueiaAlteracoesDeLancamento()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            var repo = new AppRepository(ToConnectionString(dbPath));
            var competencia = DateTime.Now.ToString("yyyy-MM");

            var categoriaId = await repo.AddAsync(new Categoria { Nome = "Fixa", Ativa = true });
            var contaAId = await repo.AddAsync(new Conta
            {
                Nome = "Conta A",
                CategoriaId = categoriaId,
                DiaVencimento = 8,
                ValorPadrao = 100m,
                Recorrente = true,
                Ativa = true
            });

            var contaBId = await repo.AddAsync(new Conta
            {
                Nome = "Conta B",
                CategoriaId = categoriaId,
                DiaVencimento = 10,
                ValorPadrao = 50m,
                Recorrente = true,
                Ativa = true
            });

            var lancamentoId = await repo.AddAsync(new Lancamento
            {
                ContaId = contaAId,
                Competencia = competencia,
                Vencimento = DateTime.Today,
                Valor = 100m,
                Status = "Pendente"
            });

            await repo.FecharCompetenciaAsync(competencia);
            Assert.True(await repo.IsCompetenciaFechadaAsync(competencia));

            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateStatusAsync(lancamentoId, "Pago", DateTime.Today));
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.AddAsync(new Lancamento
            {
                ContaId = contaBId,
                Competencia = competencia,
                Vencimento = DateTime.Today,
                Valor = 50m,
                Status = "Pendente"
            }));
        }
        finally
        {
            TryDelete(dbPath);
        }
    }

    [Fact]
    public async Task BackupERestore_RestauraEstadoAnteriorDoBanco()
    {
        var dbPath = CreateTempDbPath();
        var backupPath = CreateTempDbPath();

        try
        {
            var repo = new AppRepository(ToConnectionString(dbPath));
            await repo.AddAsync(new Categoria { Nome = "AntesBackup", Ativa = true });
            await repo.BackupDatabaseAsync(backupPath);

            await repo.AddAsync(new Categoria { Nome = "DepoisBackup", Ativa = true });
            var categoriasComMudanca = (await repo.GetCategoriasAsync(apenasAtivas: true)).ToList();
            Assert.Contains(categoriasComMudanca, c => c.Nome == "DepoisBackup");

            await repo.RestoreDatabaseAsync(backupPath);
            var categoriasRestauradas = (await repo.GetCategoriasAsync(apenasAtivas: true)).ToList();

            Assert.Contains(categoriasRestauradas, c => c.Nome == "AntesBackup");
            Assert.DoesNotContain(categoriasRestauradas, c => c.Nome == "DepoisBackup");
        }
        finally
        {
            TryDelete(dbPath);
            TryDelete(backupPath);
        }
    }

    [Fact]
    public async Task GetAllWithNamesAsync_AplicaFiltrosPorCompetenciaStatusCategoriaEBusca()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            var repo = new AppRepository(ToConnectionString(dbPath));
            var competencia = DateTime.Now.ToString("yyyy-MM");

            var categoriaCasaId = await repo.AddAsync(new Categoria { Nome = "Casa", Ativa = true });
            var categoriaLazerId = await repo.AddAsync(new Categoria { Nome = "Lazer", Ativa = true });

            var contaInternetId = await repo.AddAsync(new Conta
            {
                Nome = "Internet Fibra",
                CategoriaId = categoriaCasaId,
                DiaVencimento = 10,
                ValorPadrao = 129.90m,
                Recorrente = true,
                Ativa = true,
                FormaPagamentoPadrao = "Pix"
            });

            var contaCinemaId = await repo.AddAsync(new Conta
            {
                Nome = "Cinema",
                CategoriaId = categoriaLazerId,
                DiaVencimento = 18,
                ValorPadrao = 49.90m,
                Recorrente = true,
                Ativa = true,
                FormaPagamentoPadrao = "Cartão"
            });

            await repo.AddAsync(new Lancamento
            {
                ContaId = contaInternetId,
                Competencia = competencia,
                Vencimento = DateTime.Today,
                Valor = 129.90m,
                Status = "Pendente"
            });

            await repo.AddAsync(new Lancamento
            {
                ContaId = contaCinemaId,
                Competencia = competencia,
                Vencimento = DateTime.Today,
                Valor = 49.90m,
                Status = "Pago",
                DataPagamento = DateTime.Today
            });

            var filtrado = (await repo.GetAllWithNamesAsync(new LancamentoFiltro
            {
                Competencia = competencia,
                Status = "Pendente",
                CategoriaId = categoriaCasaId,
                TextoBusca = "Inter"
            })).ToList();

            Assert.Single(filtrado);
            Assert.Equal("Internet Fibra", filtrado[0].NomeConta);
            Assert.Equal("Casa", filtrado[0].CategoriaNome);
        }
        finally
        {
            TryDelete(dbPath);
        }
    }

    [Fact]
    public async Task InitializeDatabase_MigraSchemaLegadoESenhaAdmin()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            CreateLegacySchema(dbPath);
            var repo = new AppRepository(ToConnectionString(dbPath));

            using var conn = new SqliteConnection(ToConnectionString(dbPath));
            conn.Open();

            Assert.True(ColumnExists(conn, "Categorias", "Ativa"));
            Assert.True(ColumnExists(conn, "Contas", "Ativa"));
            Assert.True(ColumnExists(conn, "Contas", "BancoIspb"));
            Assert.True(ColumnExists(conn, "Contas", "BancoCodigo"));
            Assert.True(ColumnExists(conn, "Usuarios", "SenhaSalt"));
            Assert.True(ColumnExists(conn, "Usuarios", "Iteracoes"));
            Assert.True(ColumnExists(conn, "Usuarios", "Perfil"));
            Assert.True(ColumnExists(conn, "Auditoria", "DataHoraUtc"));
            Assert.True(ColumnExists(conn, "Auditoria", "Acao"));

            var admin = await repo.GetByLoginAsync("admin");
            Assert.NotNull(admin);
            Assert.NotEqual("admin123", admin!.SenhaHash);
            Assert.False(string.IsNullOrWhiteSpace(admin.SenhaSalt));
            Assert.True(admin.Iteracoes > 0);
            Assert.Equal("Admin", admin.Perfil);

            var operador = await repo.GetByLoginAsync("operador");
            Assert.NotNull(operador);
            Assert.Equal("Operador", operador!.Perfil);
        }
        finally
        {
            TryDelete(dbPath);
        }
    }

    [Fact]
    public async Task RegistrarAuditoriaAsync_PersisteRegistro()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            var repo = new AppRepository(ToConnectionString(dbPath));
            var admin = await repo.GetByLoginAsync("admin");

            var id = await repo.RegistrarAuditoriaAsync(
                "CRIAR",
                "LANCAMENTO",
                entidadeId: 25,
                usuario: admin,
                detalhes: "Teste de auditoria");

            Assert.True(id > 0);

            var logs = (await repo.GetAuditoriaAsync(10)).ToList();
            Assert.NotEmpty(logs);

            var log = logs.First(x => x.Id == id);
            Assert.Equal("CRIAR", log.Acao);
            Assert.Equal("LANCAMENTO", log.Entidade);
            Assert.Equal(25, log.EntidadeId);
            Assert.Equal("admin", log.UsuarioLogin);
        }
        finally
        {
            TryDelete(dbPath);
        }
    }

    [Fact]
    public async Task UpdatePerfilAsync_AtualizaPerfilDoUsuario()
    {
        var dbPath = CreateTempDbPath();
        try
        {
            var repo = new AppRepository(ToConnectionString(dbPath));
            var (hash, salt, iteracoes) = AgendaContas.Domain.Services.PasswordHasher.HashPassword("123456");

            var usuarioId = await repo.AddAsync(new Usuario
            {
                Nome = "Usuário Teste",
                Login = "teste.perfil",
                SenhaHash = hash,
                SenhaSalt = salt,
                Iteracoes = iteracoes,
                Perfil = "Operador"
            });

            await repo.UpdatePerfilAsync(usuarioId, "Admin");
            var usuario = await repo.GetByLoginAsync("teste.perfil");

            Assert.NotNull(usuario);
            Assert.Equal("Admin", usuario!.Perfil);
        }
        finally
        {
            TryDelete(dbPath);
        }
    }

    private static string CreateTempDbPath()
    {
        return Path.Combine(Path.GetTempPath(), $"agenda-contas-tests-{Guid.NewGuid():N}.db");
    }

    private static string ToConnectionString(string dbPath)
    {
        return $"Data Source={dbPath}";
    }

    private static void CreateLegacySchema(string dbPath)
    {
        using var conn = new SqliteConnection(ToConnectionString(dbPath));
        conn.Open();

        Execute(conn, "CREATE TABLE Categorias (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nome TEXT NOT NULL);");
        Execute(conn, @"CREATE TABLE Contas (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome TEXT NOT NULL,
            CategoriaId INTEGER,
            DiaVencimento INTEGER,
            ValorPadrao REAL,
            Recorrente INTEGER
        );");
        Execute(conn, @"CREATE TABLE Lancamentos (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ContaId INTEGER,
            Competencia TEXT,
            Vencimento DATETIME,
            Valor REAL,
            Status TEXT
        );");
        Execute(conn, @"CREATE TABLE Usuarios (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Nome TEXT NOT NULL,
            Login TEXT NOT NULL UNIQUE,
            SenhaHash TEXT NOT NULL
        );");
        Execute(conn, "INSERT INTO Usuarios (Nome, Login, SenhaHash) VALUES ('Administrador', 'admin', 'admin123');");
        Execute(conn, "INSERT INTO Usuarios (Nome, Login, SenhaHash) VALUES ('Operador', 'operador', 'op123');");
    }

    private static bool ColumnExists(SqliteConnection conn, string table, string column)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name = '{column}';";
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private static void Execute(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static void TryDelete(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // Sqlite pode manter lock no arquivo por alguns instantes em ambiente de teste.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
