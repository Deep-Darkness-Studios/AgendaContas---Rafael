using System.Reflection;
using System.Runtime.InteropServices;
using AgendaContas.Data.Repositories;
using AgendaContas.UI.Services;

namespace AgendaContas.UI.Forms;

public partial class InfoForm : Form
{
    private readonly AppRepository? _repo;
    private readonly Label _lblSplash = new();
    private readonly ComboBox _cmbSplashMode = new();
    private readonly NumericUpDown _numSplashSegundos = new();
    private readonly Label _lblSplashSegLabel = new();
    private readonly Button _btnCargaInicial = new();
    private readonly Button _btnSalvarSplash = new();

    private const string EquipeContent =
        "Desenvolvido por Erika Lellis e Davi Torrezim.\n\n" +
        "Objetivo do projeto:\n" +
        "- Controlar contas, lançamentos e competências mensais.\n" +
        "- Entregar um fluxo simples de uso no desktop com foco em produtividade.\n" +
        "- Manter histórico financeiro com operações seguras (fechamento mensal, backup e restauração).";

    private const string SobreNosContent =
        "Deep Darkness Studios Systems\n\n" +
        "A proposta do estúdio une criatividade e engenharia para construir soluções " +
        "com personalidade visual e funcionalidade prática. Neste projeto, a prioridade " +
        "é transformar controle financeiro em uma rotina clara, rápida e confiável.\n\n" +
        "Foco técnico aplicado no AgendaContas:\n" +
        "- Persistência local em SQLite\n" +
        "- Camadas separadas (Domain/Data/UI)\n" +
        "- Evolução incremental com testes automatizados";

    private const string ManifestoContent =
        "MANIFESTO DDS\n" +
        "Declaração da Revolução Digital\n\n" +
        "Princípios:\n" +
        "1. Tecnologia deve servir pessoas e ampliar autonomia.\n" +
        "2. Código com qualidade é compromisso de longo prazo.\n" +
        "3. Evolução contínua supera soluções temporárias.\n" +
        "4. Colaboração humano-IA deve gerar resultados concretos.\n\n" +
        "Compromisso:\n" +
        "- Criar software útil, consistente e sustentável,\n" +
        "- preservando identidade criativa sem abrir mão da engenharia.";

    public InfoForm(AppRepository? repo = null)
    {
        _repo = repo;
        InitializeComponent();
        BuildSplashSettingsControls();
        LoadSplashSettingsToUi();
    }

    private void InfoForm_Load(object sender, EventArgs e)
    {
        ShowSection("Sobre o Projeto", SobreNosContent);
    }

    private void btnCriacao_Click(object sender, EventArgs e)
    {
        ShowSection("Equipe e Criação", EquipeContent);
    }

    private void btnSobreNos_Click(object sender, EventArgs e)
    {
        ShowSection("Sobre o Projeto", SobreNosContent);
    }

    private void btnManifesto_Click(object sender, EventArgs e)
    {
        ShowSection("Manifesto DDS", ManifestoContent);
    }

    private async void btnSistema_Click(object sender, EventArgs e)
    {
        ShowSection("Informações de Sistema", await BuildSystemContentAsync());
    }

    private async void btnAtualizarBcb_Click(object sender, EventArgs e)
    {
        if (_repo == null)
        {
            MessageBox.Show("Repositório indisponível para sincronização.", "BCB", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnAtualizarBcb.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var count = await _repo.SincronizarParticipantesStrAsync();
            ShowSection("Informações de Sistema", await BuildSystemContentAsync());
            MessageBox.Show(
                $"Participantes STR sincronizados com sucesso.\nRegistros processados: {count}.",
                "BCB",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Falha na sincronização do BCB: " + ex.Message, "BCB", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            btnAtualizarBcb.Enabled = true;
        }
    }

    private void btnCopiar_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(rtbContent.Text))
        {
            return;
        }

        Clipboard.SetText($"{lblSectionTitle.Text}\n\n{rtbContent.Text}");
        MessageBox.Show("Conteúdo copiado para a área de transferência.", "Informações", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnFechar_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void BuildSplashSettingsControls()
    {
        _lblSplash.AutoSize = true;
        _lblSplash.Left = 280;
        _lblSplash.Top = 20;
        _lblSplash.Text = "Splash:";

        _cmbSplashMode.Left = 332;
        _cmbSplashMode.Top = 16;
        _cmbSplashMode.Width = 110;
        _cmbSplashMode.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbSplashMode.Items.AddRange(new object[]
        {
            SplashModes.Rapido,
            SplashModes.Padrao,
            SplashModes.Apresentacao
        });

        _numSplashSegundos.Left = 448;
        _numSplashSegundos.Top = 16;
        _numSplashSegundos.Width = 58;
        _numSplashSegundos.Minimum = 3;
        _numSplashSegundos.Maximum = 120;
        _numSplashSegundos.Value = 30;

        _lblSplashSegLabel.AutoSize = true;
        _lblSplashSegLabel.Left = 509;
        _lblSplashSegLabel.Top = 20;
        _lblSplashSegLabel.Text = "seg";

        _btnSalvarSplash.Left = 542;
        _btnSalvarSplash.Top = 15;
        _btnSalvarSplash.Width = 120;
        _btnSalvarSplash.Height = 28;
        _btnSalvarSplash.Text = "Salvar Splash";
        _btnSalvarSplash.Click += btnSalvarSplash_Click;

        _btnCargaInicial.Left = 668;
        _btnCargaInicial.Top = 15;
        _btnCargaInicial.Width = 128;
        _btnCargaInicial.Height = 28;
        _btnCargaInicial.Text = "Fechar Etapa";
        _btnCargaInicial.Click += btnCargaInicial_Click;

        pnlBottom.Controls.Add(_lblSplash);
        pnlBottom.Controls.Add(_cmbSplashMode);
        pnlBottom.Controls.Add(_numSplashSegundos);
        pnlBottom.Controls.Add(_lblSplashSegLabel);
        pnlBottom.Controls.Add(_btnSalvarSplash);
        pnlBottom.Controls.Add(_btnCargaInicial);
    }

    private void LoadSplashSettingsToUi()
    {
        var settings = StartupSettingsService.Load();
        settings = StartupSettingsService.Normalize(settings);

        var mode = settings.SplashMode;
        if (!_cmbSplashMode.Items.Contains(mode))
        {
            mode = SplashModes.Apresentacao;
        }

        _cmbSplashMode.SelectedItem = mode;
        _numSplashSegundos.Value = settings.SplashDurationSeconds ?? 30;
    }

    private void btnSalvarSplash_Click(object? sender, EventArgs e)
    {
        var mode = _cmbSplashMode.SelectedItem?.ToString() ?? SplashModes.Apresentacao;
        var settings = new StartupSettings
        {
            SplashMode = mode,
            SplashDurationSeconds = (int)_numSplashSegundos.Value
        };

        StartupSettingsService.Save(settings);
        MessageBox.Show(
            "Configuração do splash salva com sucesso.\nEla será aplicada na próxima abertura do app.",
            "Splash",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private async void btnCargaInicial_Click(object? sender, EventArgs e)
    {
        if (_repo == null)
        {
            MessageBox.Show("Repositório indisponível para executar carga inicial.", "Fechar Etapa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _btnCargaInicial.Enabled = false;
        Cursor = Cursors.WaitCursor;

        try
        {
            var report = await StageClosureService.ExecutarCargaInicialAsync(_repo);
            ShowSection("Fechamento da Etapa", report.ToDisplayText());
            MessageBox.Show(
                "Carga inicial finalizada com sucesso.\nRevise o relatório e prossiga com o preenchimento dos dados reais.",
                "Fechar Etapa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Falha ao executar carga inicial: " + ex.Message, "Fechar Etapa", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
            _btnCargaInicial.Enabled = true;
        }
    }

    private void ShowSection(string title, string content)
    {
        lblSectionTitle.Text = title;
        rtbContent.Text = content;
        rtbContent.SelectionStart = 0;
        rtbContent.SelectionLength = 0;
        rtbContent.ScrollToCaret();
    }

    private async Task<string> BuildSystemContentAsync()
    {
        var assembly = Assembly.GetExecutingAssembly().GetName();
        var version = assembly.Version?.ToString() ?? "N/D";
        var dbPath = AppPaths.GetDatabasePath();
        var anexosPath = AppPaths.GetAttachmentsDirectory();
        var startupSettings = StartupSettingsService.Normalize(StartupSettingsService.Load());
        var startupSettingsPath = StartupSettingsService.GetSettingsPath();
        var participantesInfo = "Participantes STR: sincronização ainda não realizada.";

        if (_repo != null)
        {
            try
            {
                var total = await _repo.GetParticipantesStrCountAsync();
                var ultimaSync = await _repo.GetUltimaSincronizacaoParticipantesStrAsync();
                var ultimaLabel = ultimaSync.HasValue
                    ? ultimaSync.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss")
                    : "N/D";

                participantesInfo =
                    $"Participantes STR (BCB): {total}\n" +
                    $"Última sincronização: {ultimaLabel}\n" +
                    $"Fonte: https://www.bcb.gov.br/content/estabilidadefinanceira/str1/ParticipantesSTR.csv";
            }
            catch (Exception ex)
            {
                participantesInfo = "Participantes STR: erro ao consultar base local.\n" + ex.Message;
            }
        }

        return
            $"Aplicação: {assembly.Name}\n" +
            $"Versão: {version}\n" +
            $"Runtime: {RuntimeInformation.FrameworkDescription}\n" +
            $"SO: {RuntimeInformation.OSDescription}\n" +
            $"Arquitetura: {RuntimeInformation.ProcessArchitecture}\n\n" +
            $"Banco de dados:\n{dbPath}\n\n" +
            $"Pasta de anexos:\n{anexosPath}\n\n" +
            $"Splash startup:\n" +
            $"- Modo: {startupSettings.SplashMode}\n" +
            $"- Duração mínima: {startupSettings.SplashDurationSeconds ?? 30}s\n" +
            $"- Configuração: {startupSettingsPath}\n\n" +
            $"{participantesInfo}\n\n" +
            $"Data/Hora local: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
    }
}
