using AgendaContas.Domain.Models;
using AgendaContas.Domain.Services;
using AgendaContas.UI.Services;
using AgendaContas.Data.Repositories;

namespace AgendaContas.UI.Forms;

public partial class MainForm : Form
{
    private readonly AppRepository _repo;
    private readonly FinanceiroService _service;

    public MainForm()
    {
        _repo = new AppRepository("Data Source=agenda.db");
        _service = new FinanceiroService(_repo, _repo);

        InitializeComponent();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await StartupAsync();
    }

    private async Task StartupAsync()
    {
        try
        {
            await _service.GerarLancamentosRecorrentesAsync();
            await LoadDashboardAsync();
            VerificarAlertas();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao iniciar: " + ex.Message);
        }
    }

    private async Task LoadDashboardAsync()
    {
        var comp = DateTime.Now.ToString("yyyy-MM");
        var summary = await _repo.GetDashboardSummaryAsync(comp);

        lblTotalMes.Text = $"Total do Mês\n{((decimal)summary.TotalMes):C2}";
        lblPago.Text = $"Total Pago\n{((decimal)summary.TotalPago):C2}";
        lblPendente.Text = $"Pendente\n{((decimal)summary.TotalPendente):C2}";
        lblAtrasado.Text = $"Atrasado\n{((decimal)summary.TotalAtrasado):C2}";

        var lancamentos = await _repo.GetAllWithNamesAsync();
        dgvLancamentos.DataSource = lancamentos.ToList();
    }

    private async void btnPagar_Click(object sender, EventArgs e)
    {
        if (dgvLancamentos.SelectedRows.Count > 0)
        {
            var lancamento = (Lancamento)dgvLancamentos.SelectedRows[0].DataBoundItem;
            if (lancamento.Status == "Pago") return;

            await _repo.UpdateStatusAsync(lancamento.Id, "Pago", DateTime.Now);
            await LoadDashboardAsync();
            ToastService.ShowToast("Pagamento Realizado", $"A conta {lancamento.NomeConta} foi marcada como paga.");
        }
    }

    private async void btnNovaConta_Click(object sender, EventArgs e)
    {
        var id = await _repo.AddAsync(new Conta
        {
            Nome = "Conta de Exemplo",
            DiaVencimento = DateTime.Now.Day,
            ValorPadrao = 100.00m,
            Recorrente = true
        });
        await _service.GerarLancamentosRecorrentesAsync();
        await LoadDashboardAsync();
        MessageBox.Show("Conta de exemplo criada!");
    }

    private void btnExportar_Click(object sender, EventArgs e)
    {
        var lancamentos = (List<Lancamento>)dgvLancamentos.DataSource;
        if (lancamentos == null || !lancamentos.Any()) return;

        using (var sfd = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = "Relatorio_Contas.csv" })
        {
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                UtilService.ExportarParaCSV(lancamentos, sfd.FileName);
                MessageBox.Show("Relatório exportado com sucesso!");
            }
        }
    }

    private void btnAnexo_Click(object sender, EventArgs e)
    {
        if (dgvLancamentos.SelectedRows.Count > 0)
        {
            var lancamento = (Lancamento)dgvLancamentos.SelectedRows[0].DataBoundItem;
            using (var ofd = new OpenFileDialog { Filter = "Arquivos de Imagem/PDF|*.jpg;*.png;*.pdf" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show($"Arquivo {Path.GetFileName(ofd.FileName)} anexado à conta {lancamento.NomeConta}!");
                }
            }
        }
    }

    private void btnInformacoes_Click(object sender, EventArgs e)
    {
        using (var info = new InfoForm())
        {
            info.ShowDialog();
        }
    }

    private async void VerificarAlertas()
    {
        var alertas = await _repo.GetAlertasAsync(3);
        if (alertas.Any())
        {
            ToastService.ShowToast("Contas a Vencer", $"Você tem {alertas.Count()} contas vencendo nos próximos 3 dias.");
        }
    }

    private void dgvLancamentos_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {

    }
}
