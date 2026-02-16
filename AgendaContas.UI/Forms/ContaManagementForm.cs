using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Models;

namespace AgendaContas.UI.Forms;

public class ContaManagementForm : Form
{
    private readonly IAppRepository _repo;
    private readonly Usuario? _usuarioLogado;
    private readonly IContaRepository _contaRepository;
    private readonly DataGridView _grid = new();
    private readonly Button _btnNova = new();
    private readonly Button _btnEditar = new();
    private readonly Button _btnDesativar = new();
    private readonly Button _btnFechar = new();

    public ContaManagementForm(IAppRepository repo, Usuario? usuarioLogado = null)
    {
        _repo = repo;
        _usuarioLogado = usuarioLogado;
        _contaRepository = repo;
        BuildLayout();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await RefreshGridAsync();
    }

    private void BuildLayout()
    {
        Text = "Contas";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 420);

        _grid.Dock = DockStyle.Fill;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.ReadOnly = true;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        var panelButtons = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 52
        };

        _btnNova.Text = "Nova";
        _btnNova.Left = 12;
        _btnNova.Top = 12;
        _btnNova.Width = 80;
        _btnNova.Click += async (_, _) => await NovaAsync();

        _btnEditar.Text = "Editar";
        _btnEditar.Left = 98;
        _btnEditar.Top = 12;
        _btnEditar.Width = 80;
        _btnEditar.Click += async (_, _) => await EditarAsync();

        _btnDesativar.Text = "Desativar";
        _btnDesativar.Left = 184;
        _btnDesativar.Top = 12;
        _btnDesativar.Width = 90;
        _btnDesativar.Click += async (_, _) => await DesativarAsync();

        _btnFechar.Text = "Fechar";
        _btnFechar.Left = 808;
        _btnFechar.Top = 12;
        _btnFechar.Width = 80;
        _btnFechar.Click += (_, _) => Close();

        panelButtons.Controls.Add(_btnNova);
        panelButtons.Controls.Add(_btnEditar);
        panelButtons.Controls.Add(_btnDesativar);
        panelButtons.Controls.Add(_btnFechar);

        Controls.Add(_grid);
        Controls.Add(panelButtons);
    }

    private async Task RefreshGridAsync()
    {
        var contas = (await _repo.GetContasComCategoriaAsync(apenasAtivas: false)).ToList();
        _grid.DataSource = contas;

        if (_grid.Columns["Id"] != null)
        {
            _grid.Columns["Id"].Visible = false;
        }

        if (_grid.Columns["CategoriaId"] != null)
        {
            _grid.Columns["CategoriaId"].Visible = false;
        }
    }

    private Conta? ContaSelecionada()
    {
        if (_grid.SelectedRows.Count == 0)
        {
            return null;
        }

        return _grid.SelectedRows[0].DataBoundItem as Conta;
    }

    private async Task NovaAsync()
    {
        using var form = new ContaForm(_repo);
        if (form.ShowDialog() != DialogResult.OK || form.ContaResult == null)
        {
            return;
        }

        var contaId = await _contaRepository.AddAsync(form.ContaResult);
        await RegistrarAuditoriaSafeAsync(
            "CRIAR",
            "CONTA",
            contaId,
            $"Nome={form.ContaResult.Nome}; CategoriaId={form.ContaResult.CategoriaId}; Valor={form.ContaResult.ValorPadrao:F2}");
        await RefreshGridAsync();
        DialogResult = DialogResult.OK;
    }

    private async Task EditarAsync()
    {
        var conta = ContaSelecionada();
        if (conta == null)
        {
            return;
        }

        using var form = new ContaForm(_repo, conta);
        if (form.ShowDialog() != DialogResult.OK || form.ContaResult == null)
        {
            return;
        }

        await _contaRepository.UpdateAsync(form.ContaResult);
        await RegistrarAuditoriaSafeAsync(
            "EDITAR",
            "CONTA",
            form.ContaResult.Id,
            $"Nome={form.ContaResult.Nome}; Ativa={form.ContaResult.Ativa}");
        await RefreshGridAsync();
        DialogResult = DialogResult.OK;
    }

    private async Task DesativarAsync()
    {
        var conta = ContaSelecionada();
        if (conta == null)
        {
            return;
        }

        if (MessageBox.Show(
                $"Desativar conta '{conta.Nome}'?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        await _repo.SoftDeleteContaAsync(conta.Id);
        await RegistrarAuditoriaSafeAsync("DESATIVAR", "CONTA", conta.Id, $"Nome={conta.Nome}");
        await RefreshGridAsync();
        DialogResult = DialogResult.OK;
    }

    private async Task RegistrarAuditoriaSafeAsync(string acao, string entidade, int? entidadeId = null, string? detalhes = null)
    {
        try
        {
            await _repo.RegistrarAuditoriaAsync(acao, entidade, entidadeId, _usuarioLogado, detalhes);
        }
        catch
        {
            // Não interromper operação principal por falha de auditoria.
        }
    }
}
