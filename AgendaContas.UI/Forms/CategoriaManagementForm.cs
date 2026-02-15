using AgendaContas.Data.Repositories;
using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Models;

namespace AgendaContas.UI.Forms;

public class CategoriaManagementForm : Form
{
    private readonly AppRepository _repo;
    private readonly Usuario? _usuarioLogado;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly DataGridView _grid = new();
    private readonly Button _btnNovo = new();
    private readonly Button _btnEditar = new();
    private readonly Button _btnDesativar = new();
    private readonly Button _btnFechar = new();

    public CategoriaManagementForm(AppRepository repo, Usuario? usuarioLogado = null)
    {
        _repo = repo;
        _usuarioLogado = usuarioLogado;
        _categoriaRepository = repo;
        BuildLayout();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await RefreshGridAsync();
    }

    private void BuildLayout()
    {
        Text = "Categorias";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(560, 360);

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

        _btnNovo.Text = "Nova";
        _btnNovo.Left = 12;
        _btnNovo.Top = 12;
        _btnNovo.Width = 80;
        _btnNovo.Click += async (_, _) => await NovoAsync();

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
        _btnFechar.Left = 466;
        _btnFechar.Top = 12;
        _btnFechar.Width = 80;
        _btnFechar.Click += (_, _) => Close();

        panelButtons.Controls.Add(_btnNovo);
        panelButtons.Controls.Add(_btnEditar);
        panelButtons.Controls.Add(_btnDesativar);
        panelButtons.Controls.Add(_btnFechar);

        Controls.Add(_grid);
        Controls.Add(panelButtons);
    }

    private async Task RefreshGridAsync()
    {
        var categorias = (await _categoriaRepository.GetAllAsync(apenasAtivas: false)).ToList();
        _grid.DataSource = categorias;
        if (_grid.Columns["Id"] != null)
        {
            _grid.Columns["Id"].Visible = false;
        }
    }

    private Categoria? CategoriaSelecionada()
    {
        if (_grid.SelectedRows.Count == 0)
        {
            return null;
        }

        return _grid.SelectedRows[0].DataBoundItem as Categoria;
    }

    private async Task NovoAsync()
    {
        using var form = new CategoriaForm();
        if (form.ShowDialog() != DialogResult.OK || form.CategoriaResult == null)
        {
            return;
        }

        var categoriaId = await _categoriaRepository.AddAsync(form.CategoriaResult);
        await RegistrarAuditoriaSafeAsync("CRIAR", "CATEGORIA", categoriaId, $"Nome={form.CategoriaResult.Nome}");
        await RefreshGridAsync();
        DialogResult = DialogResult.OK;
    }

    private async Task EditarAsync()
    {
        var categoria = CategoriaSelecionada();
        if (categoria == null)
        {
            return;
        }

        using var form = new CategoriaForm(categoria);
        if (form.ShowDialog() != DialogResult.OK || form.CategoriaResult == null)
        {
            return;
        }

        await _categoriaRepository.UpdateAsync(form.CategoriaResult);
        await RegistrarAuditoriaSafeAsync(
            "EDITAR",
            "CATEGORIA",
            form.CategoriaResult.Id,
            $"Nome={form.CategoriaResult.Nome}; Ativa={form.CategoriaResult.Ativa}");
        await RefreshGridAsync();
        DialogResult = DialogResult.OK;
    }

    private async Task DesativarAsync()
    {
        var categoria = CategoriaSelecionada();
        if (categoria == null)
        {
            return;
        }

        if (MessageBox.Show(
                $"Desativar categoria '{categoria.Nome}'?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        await _repo.SoftDeleteCategoriaAsync(categoria.Id);
        await RegistrarAuditoriaSafeAsync("DESATIVAR", "CATEGORIA", categoria.Id, $"Nome={categoria.Nome}");
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
