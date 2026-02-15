using System.ComponentModel;
using System.Diagnostics;
using AgendaContas.Data.Repositories;
using AgendaContas.Domain.Models;
using AgendaContas.Domain.Services;
using AgendaContas.UI.Services;

namespace AgendaContas.UI.Forms;

public partial class MainForm : Form
{
    private AppRepository? _repo;
    private FinanceiroService? _service;
    private Usuario? _usuarioLogado;
    private bool _runtimeReady;

    private readonly ComboBox _cmbCompetencia = new();
    private readonly ComboBox _cmbStatus = new();
    private readonly ComboBox _cmbCategoria = new();
    private readonly TextBox _txtBusca = new();
    private readonly Button _btnAplicarFiltro = new();
    private readonly Button _btnLimparFiltro = new();
    private readonly Button _btnEditarLancamento = new();
    private readonly Button _btnExcluirLancamento = new();
    private readonly Button _btnSobreApp = new();
    private readonly Button _btnAnexarComprovante = new();
    private readonly Button _btnAbrirComprovante = new();
    private readonly Button _btnFecharCompetencia = new();
    private readonly Button _btnReabrirCompetencia = new();
    private readonly Button _btnBackup = new();
    private readonly Button _btnRestore = new();
    private readonly Label _lblCompetenciaStatus = new();
    private readonly Label _lblUsuario = new();

    private bool IsAdmin =>
        _usuarioLogado == null || _usuarioLogado.IsAdmin;

    public MainForm()
    {
        InitializeComponent();
        ConfigureRuntimeDependencies(null, null);
    }

    public MainForm(AppRepository repository)
        : this(repository, null)
    {
    }

    public MainForm(AppRepository repository, Usuario? usuarioLogado)
    {
        InitializeComponent();
        ConfigureRuntimeDependencies(repository, usuarioLogado);
    }

    private void ConfigureRuntimeDependencies(AppRepository? repository, Usuario? usuarioLogado)
    {
        _usuarioLogado = usuarioLogado;

        if (repository != null)
        {
            _repo = repository;
            _service = new FinanceiroService(repository, repository);
            _runtimeReady = true;
            InitializeRuntimeControls();
            return;
        }

        if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
        {
            return;
        }

        _repo = new AppRepository(AppPaths.GetConnectionString());
        _service = new FinanceiroService(_repo, _repo);
        _usuarioLogado = new Usuario
        {
            Nome = "Administrador",
            Login = "admin",
            Perfil = PerfisUsuario.Admin
        };
        _runtimeReady = true;
        InitializeRuntimeControls();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (!_runtimeReady || _repo == null || _service == null)
        {
            return;
        }

        try
        {
            await LoadFiltrosAsync();
            await RegistrarAuditoriaSafeAsync("LOGIN", "USUARIO", _usuarioLogado?.Id, "Login realizado no sistema.");

            try
            {
                await _service.GerarLancamentosRecorrentesAsync();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Competência fechada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            await LoadDashboardAsync();
            await VerificarAlertasAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao iniciar: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InitializeRuntimeControls()
    {
        var usuarioLabel = _usuarioLogado?.Nome ?? "Administrador";
        Text = $"DDS - Cofre Real - Dashboard ({usuarioLabel})";

        ConfigureBottomPanelLayout();
        ConfigureFilterControls();
        ConfigureGridInteractions();
        ApplyRolePermissions();
    }

    private void ConfigureBottomPanelLayout()
    {
        pnlButtons.Height = 160;

        btnPagar.Text = "Marcar Pago";
        btnNovaConta.Text = "Contas";
        btnExportar.Text = "Exportar";
        btnAnexo.Text = "Novo Lanç.";
        btnInformacoes.Text = "Categorias";

        btnPagar.SetBounds(12, 112, 92, 30);
        btnNovaConta.SetBounds(108, 112, 68, 30);
        btnExportar.SetBounds(180, 112, 84, 30);
        btnAnexo.SetBounds(268, 112, 88, 30);
        btnInformacoes.SetBounds(360, 112, 86, 30);

        _btnSobreApp.Text = "Sobre App";
        _btnSobreApp.SetBounds(450, 112, 80, 30);
        _btnSobreApp.Click += (_, _) =>
        {
            using var info = new InfoForm(_repo);
            info.ShowDialog(this);
        };

        _btnEditarLancamento.Text = "Editar Lanç.";
        _btnEditarLancamento.SetBounds(534, 112, 78, 30);
        _btnEditarLancamento.Click += async (_, _) => await EditarLancamentoSelecionadoAsync();

        _btnExcluirLancamento.Text = "Excluir Lanç.";
        _btnExcluirLancamento.SetBounds(616, 112, 78, 30);
        _btnExcluirLancamento.Click += async (_, _) => await ExcluirLancamentoSelecionadoAsync();

        _btnAnexarComprovante.Text = "Anexar";
        _btnAnexarComprovante.SetBounds(698, 112, 92, 30);
        _btnAnexarComprovante.Click += async (_, _) => await AnexarComprovanteSelecionadoAsync();

        _btnAbrirComprovante.Text = "Abrir Anexo";
        _btnAbrirComprovante.SetBounds(794, 112, 92, 30);
        _btnAbrirComprovante.Click += (_, _) => AbrirComprovanteSelecionado();

        _lblCompetenciaStatus.AutoSize = true;
        _lblCompetenciaStatus.SetBounds(12, 46, 300, 15);
        _lblCompetenciaStatus.Text = "Competência: -";

        var login = _usuarioLogado?.Login ?? "admin";
        var perfil = _usuarioLogado?.Perfil ?? PerfisUsuario.Admin;
        _lblUsuario.AutoSize = true;
        _lblUsuario.SetBounds(12, 66, 400, 15);
        _lblUsuario.Text = $"Usuário: {login} | Perfil: {perfil}";

        _btnFecharCompetencia.Text = "Fechar Mês";
        _btnFecharCompetencia.SetBounds(320, 40, 95, 26);
        _btnFecharCompetencia.Click += async (_, _) => await FecharCompetenciaAtualAsync();

        _btnReabrirCompetencia.Text = "Reabrir Mês";
        _btnReabrirCompetencia.SetBounds(420, 40, 95, 26);
        _btnReabrirCompetencia.Click += async (_, _) => await ReabrirCompetenciaAtualAsync();

        _btnBackup.Text = "Backup";
        _btnBackup.SetBounds(760, 40, 70, 26);
        _btnBackup.Click += async (_, _) => await BackupDatabaseAsync();

        _btnRestore.Text = "Restaurar";
        _btnRestore.SetBounds(834, 40, 76, 26);
        _btnRestore.Click += async (_, _) => await RestoreDatabaseAsync();

        pnlButtons.Controls.Add(_btnEditarLancamento);
        pnlButtons.Controls.Add(_btnExcluirLancamento);
        pnlButtons.Controls.Add(_btnSobreApp);
        pnlButtons.Controls.Add(_btnAnexarComprovante);
        pnlButtons.Controls.Add(_btnAbrirComprovante);
        pnlButtons.Controls.Add(_lblCompetenciaStatus);
        pnlButtons.Controls.Add(_lblUsuario);
        pnlButtons.Controls.Add(_btnFecharCompetencia);
        pnlButtons.Controls.Add(_btnReabrirCompetencia);
        pnlButtons.Controls.Add(_btnBackup);
        pnlButtons.Controls.Add(_btnRestore);
    }

    private void ConfigureFilterControls()
    {
        var lblCompetencia = new Label { Text = "Competência:", AutoSize = true, Left = 10, Top = 14 };
        _cmbCompetencia.Left = 84;
        _cmbCompetencia.Top = 10;
        _cmbCompetencia.Width = 110;
        _cmbCompetencia.DropDownStyle = ComboBoxStyle.DropDown;
        _cmbCompetencia.SelectedIndexChanged += async (_, _) =>
        {
            if (_repo != null)
            {
                await UpdateCompetenciaStatusAsync(GetCompetenciaSelecionada());
            }
        };

        var lblStatus = new Label { Text = "Status:", AutoSize = true, Left = 203, Top = 14 };
        _cmbStatus.Left = 248;
        _cmbStatus.Top = 10;
        _cmbStatus.Width = 96;
        _cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;

        var lblCategoria = new Label { Text = "Categoria:", AutoSize = true, Left = 352, Top = 14 };
        _cmbCategoria.Left = 414;
        _cmbCategoria.Top = 10;
        _cmbCategoria.Width = 140;
        _cmbCategoria.DropDownStyle = ComboBoxStyle.DropDownList;

        var lblBusca = new Label { Text = "Busca:", AutoSize = true, Left = 561, Top = 14 };
        _txtBusca.Left = 607;
        _txtBusca.Top = 10;
        _txtBusca.Width = 120;
        _txtBusca.PlaceholderText = "Nome da conta";
        _txtBusca.KeyDown += async (_, args) =>
        {
            if (args.KeyCode == Keys.Enter)
            {
                args.Handled = true;
                args.SuppressKeyPress = true;
                await LoadDashboardAsync();
            }
        };

        _btnAplicarFiltro.Text = "Aplicar";
        _btnAplicarFiltro.Left = 735;
        _btnAplicarFiltro.Top = 9;
        _btnAplicarFiltro.Width = 70;
        _btnAplicarFiltro.Click += async (_, _) => await LoadDashboardAsync();

        _btnLimparFiltro.Text = "Limpar";
        _btnLimparFiltro.Left = 810;
        _btnLimparFiltro.Top = 9;
        _btnLimparFiltro.Width = 70;
        _btnLimparFiltro.Click += async (_, _) =>
        {
            _cmbStatus.SelectedIndex = 0;
            _cmbCategoria.SelectedIndex = 0;
            _txtBusca.Clear();
            _cmbCompetencia.Text = DateTime.Now.ToString("yyyy-MM");
            await LoadDashboardAsync();
        };

        pnlButtons.Controls.Add(lblCompetencia);
        pnlButtons.Controls.Add(_cmbCompetencia);
        pnlButtons.Controls.Add(lblStatus);
        pnlButtons.Controls.Add(_cmbStatus);
        pnlButtons.Controls.Add(lblCategoria);
        pnlButtons.Controls.Add(_cmbCategoria);
        pnlButtons.Controls.Add(lblBusca);
        pnlButtons.Controls.Add(_txtBusca);
        pnlButtons.Controls.Add(_btnAplicarFiltro);
        pnlButtons.Controls.Add(_btnLimparFiltro);
    }

    private void ConfigureGridInteractions()
    {
        dgvLancamentos.CellDoubleClick += async (_, _) => await EditarLancamentoSelecionadoAsync();

        var menu = new ContextMenuStrip();
        menu.Items.Add("Editar lançamento", null, async (_, _) => await EditarLancamentoSelecionadoAsync());
        menu.Items.Add("Excluir lançamento", null, async (_, _) => await ExcluirLancamentoSelecionadoAsync());
        menu.Items.Add("Anexar comprovante", null, async (_, _) => await AnexarComprovanteSelecionadoAsync());
        menu.Items.Add("Abrir comprovante", null, (_, _) => AbrirComprovanteSelecionado());
        menu.Items.Add("Remover comprovante", null, async (_, _) => await RemoverComprovanteSelecionadoAsync());
        dgvLancamentos.ContextMenuStrip = menu;
    }

    private void ApplyRolePermissions()
    {
        if (IsAdmin)
        {
            return;
        }

        btnNovaConta.Enabled = false;
        btnInformacoes.Enabled = false;
        _btnExcluirLancamento.Enabled = false;
        _btnFecharCompetencia.Enabled = false;
        _btnReabrirCompetencia.Enabled = false;
        _btnBackup.Enabled = false;
        _btnRestore.Enabled = false;
    }

    private bool EnsureAdminAction(string acao)
    {
        if (IsAdmin)
        {
            return true;
        }

        MessageBox.Show(
            $"Seu perfil não permite {acao}.",
            "Permissão",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        return false;
    }

    private async Task RegistrarAuditoriaSafeAsync(string acao, string entidade, int? entidadeId = null, string? detalhes = null)
    {
        if (_repo == null)
        {
            return;
        }

        try
        {
            await _repo.RegistrarAuditoriaAsync(
                acao,
                entidade,
                entidadeId,
                _usuarioLogado,
                detalhes);
        }
        catch
        {
            // Auditoria não deve interromper o fluxo da operação principal.
        }
    }

    private async Task LoadFiltrosAsync()
    {
        if (_repo == null)
        {
            return;
        }

        var competencias = Enumerable.Range(-6, 13)
            .Select(offset => DateTime.Now.AddMonths(offset).ToString("yyyy-MM"))
            .ToList();

        _cmbCompetencia.DataSource = competencias;
        _cmbCompetencia.Text = DateTime.Now.ToString("yyyy-MM");

        _cmbStatus.Items.Clear();
        _cmbStatus.Items.AddRange(new object[] { "Todos", "Pendente", "Pago", "Atrasado" });
        _cmbStatus.SelectedIndex = 0;

        var categorias = (await _repo.GetCategoriasAsync(apenasAtivas: true)).ToList();
        var itens = new List<ComboOption> { new() { Id = null, Nome = "Todas" } };
        itens.AddRange(categorias.Select(c => new ComboOption { Id = c.Id, Nome = c.Nome }));

        _cmbCategoria.DataSource = itens;
        _cmbCategoria.DisplayMember = nameof(ComboOption.Nome);
        _cmbCategoria.ValueMember = nameof(ComboOption.Id);
    }

    private async Task LoadDashboardAsync()
    {
        if (_repo == null)
        {
            return;
        }

        var competencia = GetCompetenciaSelecionada();
        await UpdateCompetenciaStatusAsync(competencia);

        var summary = await _repo.GetDashboardSummaryAsync(competencia);

        lblTotalMes.Text = $"Total do Mês\n{summary.TotalMes:C2}";
        lblPago.Text = $"Total Pago\n{summary.TotalPago:C2}";
        lblPendente.Text = $"Pendente\n{summary.TotalPendente:C2}";
        lblAtrasado.Text = $"Atrasado\n{summary.TotalAtrasado:C2}";

        var filtro = new LancamentoFiltro
        {
            Competencia = competencia,
            Status = _cmbStatus.SelectedItem?.ToString(),
            CategoriaId = (_cmbCategoria.SelectedItem as ComboOption)?.Id,
            TextoBusca = _txtBusca.Text,
            IncluirContasInativas = false
        };

        var lancamentos = (await _repo.GetAllWithNamesAsync(filtro)).ToList();
        dgvLancamentos.DataSource = lancamentos;
        ConfigureGridColumns();
    }

    private string GetCompetenciaSelecionada()
    {
        var input = _cmbCompetencia.Text.Trim();
        if (DateTime.TryParseExact(
                input + "-01",
                "yyyy-MM-dd",
                null,
                System.Globalization.DateTimeStyles.None,
                out var date))
        {
            return date.ToString("yyyy-MM");
        }

        return DateTime.Now.ToString("yyyy-MM");
    }

    private void ConfigureGridColumns()
    {
        if (dgvLancamentos.Columns.Count == 0)
        {
            return;
        }

        HideColumn(nameof(Lancamento.Id));
        HideColumn(nameof(Lancamento.ContaId));
        HideColumn(nameof(Lancamento.CategoriaId));
        HideColumn(nameof(Lancamento.AnexoPath));

        RenameColumn(nameof(Lancamento.NomeConta), "Conta");
        RenameColumn(nameof(Lancamento.CategoriaNome), "Categoria");
        RenameColumn(nameof(Lancamento.Competencia), "Competência");
        RenameColumn(nameof(Lancamento.Vencimento), "Vencimento");
        RenameColumn(nameof(Lancamento.Valor), "Valor");
        RenameColumn(nameof(Lancamento.Status), "Status");
        RenameColumn(nameof(Lancamento.DataPagamento), "Data Pagamento");
        RenameColumn(nameof(Lancamento.FormaPagamento), "Forma Pagamento");
        RenameColumn(nameof(Lancamento.Observacao), "Observação");
    }

    private void HideColumn(string columnName)
    {
        if (dgvLancamentos.Columns[columnName] != null)
        {
            dgvLancamentos.Columns[columnName].Visible = false;
        }
    }

    private void RenameColumn(string columnName, string header)
    {
        if (dgvLancamentos.Columns[columnName] != null)
        {
            dgvLancamentos.Columns[columnName].HeaderText = header;
        }
    }

    private Lancamento? LancamentoSelecionado()
    {
        if (dgvLancamentos.SelectedRows.Count == 0)
        {
            return null;
        }

        return dgvLancamentos.SelectedRows[0].DataBoundItem as Lancamento;
    }

    private async Task<bool> EnsureCompetenciaAbertaAsync(string competencia, string acao)
    {
        if (_repo == null)
        {
            return false;
        }

        if (!await _repo.IsCompetenciaFechadaAsync(competencia))
        {
            return true;
        }

        MessageBox.Show(
            $"A competência {competencia} está fechada e não permite {acao}.",
            "Competência fechada",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        return false;
    }

    private async Task EditarLancamentoSelecionadoAsync()
    {
        if (_repo == null)
        {
            return;
        }

        var selecionado = LancamentoSelecionado();
        if (selecionado == null)
        {
            return;
        }

        if (!await EnsureCompetenciaAbertaAsync(selecionado.Competencia, "edição"))
        {
            return;
        }

        var lancamento = await _repo.GetLancamentoByIdAsync(selecionado.Id);
        if (lancamento == null)
        {
            return;
        }

        using var form = new LancamentoForm(_repo, lancamento);
        if (form.ShowDialog() != DialogResult.OK || form.LancamentoResult == null)
        {
            return;
        }

        try
        {
            await _repo.UpdateAsync(form.LancamentoResult);
            await LoadDashboardAsync();
            await RegistrarAuditoriaSafeAsync(
                "EDITAR",
                "LANCAMENTO",
                form.LancamentoResult.Id,
                $"ContaId={form.LancamentoResult.ContaId}; Competencia={form.LancamentoResult.Competencia}; Valor={form.LancamentoResult.Valor:F2}");
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Competência fechada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async Task ExcluirLancamentoSelecionadoAsync()
    {
        if (_repo == null)
        {
            return;
        }

        if (!EnsureAdminAction("excluir lançamentos"))
        {
            return;
        }

        var lancamento = LancamentoSelecionado();
        if (lancamento == null)
        {
            return;
        }

        if (!await EnsureCompetenciaAbertaAsync(lancamento.Competencia, "exclusão"))
        {
            return;
        }

        if (MessageBox.Show(
                $"Excluir o lançamento da conta '{lancamento.NomeConta}'?",
                "Confirmar exclusão",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _repo.DeleteLancamentoAsync(lancamento.Id);
            await LoadDashboardAsync();
            await RegistrarAuditoriaSafeAsync(
                "EXCLUIR",
                "LANCAMENTO",
                lancamento.Id,
                $"Conta={lancamento.NomeConta}; Competencia={lancamento.Competencia}; Valor={lancamento.Valor:F2}");
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Competência fechada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async Task AnexarComprovanteSelecionadoAsync()
    {
        if (_repo == null)
        {
            return;
        }

        var lancamento = LancamentoSelecionado();
        if (lancamento == null)
        {
            return;
        }

        if (!await EnsureCompetenciaAbertaAsync(lancamento.Competencia, "alteração de anexo"))
        {
            return;
        }

        using var ofd = new OpenFileDialog
        {
            Filter = "Arquivos|*.pdf;*.jpg;*.jpeg;*.png;*.bmp;*.webp",
            Title = "Selecionar comprovante"
        };

        if (ofd.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var targetDirectory = AppPaths.GetAttachmentsDirectory();
        var extension = Path.GetExtension(ofd.FileName);
        var targetFileName = $"{lancamento.Id}_{DateTime.Now:yyyyMMddHHmmssfff}{extension}";
        var targetPath = Path.Combine(targetDirectory, targetFileName);

        try
        {
            File.Copy(ofd.FileName, targetPath, overwrite: false);
            await _repo.UpdateAnexoPathAsync(lancamento.Id, targetPath);
            await LoadDashboardAsync();
            await RegistrarAuditoriaSafeAsync(
                "ANEXAR",
                "LANCAMENTO",
                lancamento.Id,
                $"Arquivo={Path.GetFileName(targetPath)}");
            MessageBox.Show("Comprovante anexado com sucesso.", "Anexo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Competência fechada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Falha ao anexar comprovante: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AbrirComprovanteSelecionado()
    {
        var lancamento = LancamentoSelecionado();
        if (lancamento == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(lancamento.AnexoPath) || !File.Exists(lancamento.AnexoPath))
        {
            MessageBox.Show("Este lançamento não possui anexo válido.", "Anexo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var info = new ProcessStartInfo
            {
                FileName = lancamento.AnexoPath,
                UseShellExecute = true
            };
            Process.Start(info);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível abrir o anexo: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RemoverComprovanteSelecionadoAsync()
    {
        if (_repo == null)
        {
            return;
        }

        var lancamento = LancamentoSelecionado();
        if (lancamento == null || string.IsNullOrWhiteSpace(lancamento.AnexoPath))
        {
            return;
        }

        if (!await EnsureCompetenciaAbertaAsync(lancamento.Competencia, "remoção de anexo"))
        {
            return;
        }

        if (MessageBox.Show(
                "Remover o comprovante deste lançamento?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            var path = lancamento.AnexoPath;
            await _repo.UpdateAnexoPathAsync(lancamento.Id, null);

            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                var attachmentsDir = Path.GetFullPath(AppPaths.GetAttachmentsDirectory());
                var fullPath = Path.GetFullPath(path);
                if (fullPath.StartsWith(attachmentsDir, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(fullPath);
                }
            }

            await LoadDashboardAsync();
            await RegistrarAuditoriaSafeAsync("REMOVER_ANEXO", "LANCAMENTO", lancamento.Id);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Competência fechada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Falha ao remover anexo: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task BackupDatabaseAsync()
    {
        if (_repo == null)
        {
            return;
        }

        if (!EnsureAdminAction("executar backup"))
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "SQLite Database|*.db",
            FileName = $"AgendaContas_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _repo.BackupDatabaseAsync(dialog.FileName);
            await RegistrarAuditoriaSafeAsync("BACKUP", "BANCO", null, dialog.FileName);
            MessageBox.Show("Backup criado com sucesso.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Falha ao criar backup: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RestoreDatabaseAsync()
    {
        if (_repo == null)
        {
            return;
        }

        if (!EnsureAdminAction("restaurar backup"))
        {
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Filter = "SQLite Database|*.db",
            Title = "Selecionar backup para restauração"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        if (MessageBox.Show(
                "Restaurar backup substituirá o estado atual do banco. Deseja continuar?",
                "Confirmar restauração",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            await _repo.RestoreDatabaseAsync(dialog.FileName);
            await LoadFiltrosAsync();
            await LoadDashboardAsync();
            await RegistrarAuditoriaSafeAsync("RESTORE", "BANCO", null, dialog.FileName);
            MessageBox.Show("Backup restaurado com sucesso.", "Restauração", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Falha ao restaurar backup: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task FecharCompetenciaAtualAsync()
    {
        if (_repo == null)
        {
            return;
        }

        if (!EnsureAdminAction("fechar competência"))
        {
            return;
        }

        var competencia = GetCompetenciaSelecionada();
        if (MessageBox.Show(
                $"Fechar a competência {competencia}? Isso bloqueará edições e pagamentos deste mês.",
                "Fechar competência",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        await _repo.FecharCompetenciaAsync(competencia);
        await UpdateCompetenciaStatusAsync(competencia);
        await LoadDashboardAsync();
        await RegistrarAuditoriaSafeAsync("FECHAR_COMPETENCIA", "COMPETENCIA", null, competencia);
    }

    private async Task ReabrirCompetenciaAtualAsync()
    {
        if (_repo == null)
        {
            return;
        }

        if (!EnsureAdminAction("reabrir competência"))
        {
            return;
        }

        var competencia = GetCompetenciaSelecionada();
        if (MessageBox.Show(
                $"Reabrir a competência {competencia}? Ela voltará a permitir alterações.",
                "Reabrir competência",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        await _repo.ReabrirCompetenciaAsync(competencia);
        await UpdateCompetenciaStatusAsync(competencia);
        await LoadDashboardAsync();
        await RegistrarAuditoriaSafeAsync("REABRIR_COMPETENCIA", "COMPETENCIA", null, competencia);
    }

    private async Task UpdateCompetenciaStatusAsync(string competencia)
    {
        if (_repo == null)
        {
            return;
        }

        var fechada = await _repo.IsCompetenciaFechadaAsync(competencia);
        _lblCompetenciaStatus.Text = fechada
            ? $"Competência {competencia}: FECHADA"
            : $"Competência {competencia}: ABERTA";
        _lblCompetenciaStatus.ForeColor = fechada ? Color.DarkRed : Color.DarkGreen;
    }

    private async void btnPagar_Click(object sender, EventArgs e)
    {
        if (_repo == null)
        {
            return;
        }

        var lancamento = LancamentoSelecionado();
        if (lancamento == null || string.Equals(lancamento.Status, "Pago", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!await EnsureCompetenciaAbertaAsync(lancamento.Competencia, "pagamento"))
        {
            return;
        }

        try
        {
            await _repo.UpdateStatusAsync(lancamento.Id, "Pago", DateTime.Now);
            await LoadDashboardAsync();
            await RegistrarAuditoriaSafeAsync(
                "PAGAR",
                "LANCAMENTO",
                lancamento.Id,
                $"Conta={lancamento.NomeConta}; Competencia={lancamento.Competencia}; Valor={lancamento.Valor:F2}");
            ToastService.ShowToast("Pagamento Realizado", $"A conta {lancamento.NomeConta} foi marcada como paga.");
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Competência fechada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void btnNovaConta_Click(object sender, EventArgs e)
    {
        if (_repo == null || _service == null)
        {
            return;
        }

        if (!EnsureAdminAction("gerenciar contas"))
        {
            return;
        }

        using var form = new ContaManagementForm(_repo, _usuarioLogado);
        var result = form.ShowDialog();
        if (result != DialogResult.OK)
        {
            return;
        }

        try
        {
            await _service.GerarLancamentosRecorrentesAsync();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Competência fechada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        await LoadDashboardAsync();
        await RegistrarAuditoriaSafeAsync("GERENCIAR", "CONTA");
    }

    private async void btnExportar_Click(object sender, EventArgs e)
    {
        if (dgvLancamentos.DataSource is not List<Lancamento> lancamentos || lancamentos.Count == 0)
        {
            return;
        }

        using var sfd = new SaveFileDialog
        {
            Filter = "CSV Files|*.csv|PDF Files|*.pdf",
            FileName = "Relatorio_Contas"
        };

        if (sfd.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var extension = Path.GetExtension(sfd.FileName);
        if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            var competencia = GetCompetenciaSelecionada();
            var summary = _repo == null
                ? new DashboardSummary()
                : await _repo.GetDashboardSummaryAsync(competencia);

            PdfReportService.ExportarLancamentos(lancamentos, summary, competencia, sfd.FileName);
            await RegistrarAuditoriaSafeAsync("EXPORTAR_PDF", "RELATORIO", null, sfd.FileName);
            MessageBox.Show("Relatório PDF exportado com sucesso!", "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        UtilService.ExportarParaCSV(lancamentos, sfd.FileName);
        await RegistrarAuditoriaSafeAsync("EXPORTAR_CSV", "RELATORIO", null, sfd.FileName);
        MessageBox.Show("Relatório CSV exportado com sucesso!", "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void btnAnexo_Click(object sender, EventArgs e)
    {
        if (_repo == null)
        {
            return;
        }

        var competencia = GetCompetenciaSelecionada();
        if (!await EnsureCompetenciaAbertaAsync(competencia, "inclusão de lançamento"))
        {
            return;
        }

        using var form = new LancamentoForm(_repo);
        if (form.ShowDialog() != DialogResult.OK || form.LancamentoResult == null)
        {
            return;
        }

        try
        {
            var novoId = await _repo.AddAsync(form.LancamentoResult);
            await LoadDashboardAsync();
            await RegistrarAuditoriaSafeAsync(
                "CRIAR",
                "LANCAMENTO",
                novoId,
                $"ContaId={form.LancamentoResult.ContaId}; Competencia={form.LancamentoResult.Competencia}; Valor={form.LancamentoResult.Valor:F2}");
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Competência fechada", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void btnInformacoes_Click(object sender, EventArgs e)
    {
        if (_repo == null)
        {
            return;
        }

        if (!EnsureAdminAction("gerenciar categorias"))
        {
            return;
        }

        using var form = new CategoriaManagementForm(_repo, _usuarioLogado);
        var result = form.ShowDialog();
        if (result != DialogResult.OK)
        {
            return;
        }

        await LoadFiltrosAsync();
        await LoadDashboardAsync();
        await RegistrarAuditoriaSafeAsync("GERENCIAR", "CATEGORIA");
    }

    private async Task VerificarAlertasAsync()
    {
        if (_repo == null)
        {
            return;
        }

        var alertas = (await _repo.GetAlertasAsync(3)).ToList();
        if (alertas.Count > 0)
        {
            ToastService.ShowToast("Contas a Vencer", $"Você tem {alertas.Count} contas vencendo nos próximos 3 dias.");
        }
    }

    private void dgvLancamentos_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
    }

    private class ComboOption
    {
        public int? Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}
