using AgendaContas.Domain.Interfaces;
using AgendaContas.Domain.Models;

namespace AgendaContas.UI.Forms;

public class LancamentoForm : Form
{
    private readonly IAppRepository _repo;
    private readonly Lancamento? _lancamentoAtual;

    private readonly ComboBox _cmbConta = new();
    private readonly TextBox _txtCompetencia = new();
    private readonly DateTimePicker _dtpVencimento = new();
    private readonly NumericUpDown _numValor = new();
    private readonly ComboBox _cmbStatus = new();
    private readonly DateTimePicker _dtpPagamento = new();
    private readonly ComboBox _cmbFormaPagamento = new();
    private readonly TextBox _txtObservacao = new();
    private readonly Button _btnSalvar = new();
    private readonly Button _btnCancelar = new();

    private List<Conta> _contas = new();

    public Lancamento? LancamentoResult { get; private set; }

    public LancamentoForm(IAppRepository repo, Lancamento? lancamento = null)
    {
        _repo = repo;
        _lancamentoAtual = lancamento;
        BuildLayout();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadContasAsync();
        LoadValues();
    }

    private void BuildLayout()
    {
        Text = _lancamentoAtual == null ? "Novo Lançamento" : "Editar Lançamento";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(480, 390);

        var lblConta = new Label { Text = "Conta", Left = 20, Top = 20, Width = 100 };
        _cmbConta.Left = 20;
        _cmbConta.Top = 40;
        _cmbConta.Width = 440;
        _cmbConta.DropDownStyle = ComboBoxStyle.DropDownList;

        var lblCompetencia = new Label { Text = "Competência (yyyy-MM)", Left = 20, Top = 72, Width = 180 };
        _txtCompetencia.Left = 20;
        _txtCompetencia.Top = 92;
        _txtCompetencia.Width = 130;

        var lblVencimento = new Label { Text = "Vencimento", Left = 160, Top = 72, Width = 100 };
        _dtpVencimento.Left = 160;
        _dtpVencimento.Top = 92;
        _dtpVencimento.Width = 150;
        _dtpVencimento.Format = DateTimePickerFormat.Short;

        var lblValor = new Label { Text = "Valor", Left = 320, Top = 72, Width = 80 };
        _numValor.Left = 320;
        _numValor.Top = 92;
        _numValor.Width = 140;
        _numValor.DecimalPlaces = 2;
        _numValor.Maximum = 1_000_000;
        _numValor.Value = 100;

        var lblStatus = new Label { Text = "Status", Left = 20, Top = 124, Width = 100 };
        _cmbStatus.Left = 20;
        _cmbStatus.Top = 144;
        _cmbStatus.Width = 130;
        _cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbStatus.Items.AddRange(new object[] { "Pendente", "Pago", "Atrasado" });
        _cmbStatus.SelectedIndex = 0;

        var lblPagamento = new Label { Text = "Data Pagamento", Left = 160, Top = 124, Width = 120 };
        _dtpPagamento.Left = 160;
        _dtpPagamento.Top = 144;
        _dtpPagamento.Width = 150;
        _dtpPagamento.Format = DateTimePickerFormat.Short;
        _dtpPagamento.ShowCheckBox = true;

        var lblForma = new Label { Text = "Forma Pagamento", Left = 320, Top = 124, Width = 120 };
        _cmbFormaPagamento.Left = 320;
        _cmbFormaPagamento.Top = 144;
        _cmbFormaPagamento.Width = 140;
        _cmbFormaPagamento.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbFormaPagamento.Items.AddRange(new object[] { "Pix", "Boleto", "Cartão", "Débito", "Dinheiro" });
        _cmbFormaPagamento.SelectedIndex = 0;

        var lblObs = new Label { Text = "Observação", Left = 20, Top = 176, Width = 120 };
        _txtObservacao.Left = 20;
        _txtObservacao.Top = 196;
        _txtObservacao.Width = 440;
        _txtObservacao.Height = 120;
        _txtObservacao.Multiline = true;
        _txtObservacao.ScrollBars = ScrollBars.Vertical;

        _btnSalvar.Text = "Salvar";
        _btnSalvar.Left = 304;
        _btnSalvar.Top = 336;
        _btnSalvar.Width = 75;
        _btnSalvar.Click += btnSalvar_Click;

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Left = 385;
        _btnCancelar.Top = 336;
        _btnCancelar.Width = 75;
        _btnCancelar.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        AcceptButton = _btnSalvar;
        CancelButton = _btnCancelar;

        Controls.Add(lblConta);
        Controls.Add(_cmbConta);
        Controls.Add(lblCompetencia);
        Controls.Add(_txtCompetencia);
        Controls.Add(lblVencimento);
        Controls.Add(_dtpVencimento);
        Controls.Add(lblValor);
        Controls.Add(_numValor);
        Controls.Add(lblStatus);
        Controls.Add(_cmbStatus);
        Controls.Add(lblPagamento);
        Controls.Add(_dtpPagamento);
        Controls.Add(lblForma);
        Controls.Add(_cmbFormaPagamento);
        Controls.Add(lblObs);
        Controls.Add(_txtObservacao);
        Controls.Add(_btnSalvar);
        Controls.Add(_btnCancelar);
    }

    private async Task LoadContasAsync()
    {
        _contas = (await _repo.GetContasAsync(apenasAtivas: true)).ToList();
        _cmbConta.DataSource = _contas;
        _cmbConta.DisplayMember = nameof(Conta.Nome);
        _cmbConta.ValueMember = nameof(Conta.Id);
    }

    private void LoadValues()
    {
        _txtCompetencia.Text = DateTime.Now.ToString("yyyy-MM");
        _dtpVencimento.Value = DateTime.Today;

        if (_lancamentoAtual == null)
        {
            return;
        }

        _cmbConta.SelectedValue = _lancamentoAtual.ContaId;
        _txtCompetencia.Text = _lancamentoAtual.Competencia;
        _dtpVencimento.Value = _lancamentoAtual.Vencimento;
        _numValor.Value = _lancamentoAtual.Valor;
        _cmbStatus.SelectedItem = _lancamentoAtual.Status;
        _txtObservacao.Text = _lancamentoAtual.Observacao ?? string.Empty;

        if (_lancamentoAtual.DataPagamento.HasValue)
        {
            _dtpPagamento.Checked = true;
            _dtpPagamento.Value = _lancamentoAtual.DataPagamento.Value;
        }
        else
        {
            _dtpPagamento.Checked = false;
        }

        if (!string.IsNullOrWhiteSpace(_lancamentoAtual.FormaPagamento))
        {
            _cmbFormaPagamento.SelectedItem = _lancamentoAtual.FormaPagamento;
        }
    }

    private void btnSalvar_Click(object? sender, EventArgs e)
    {
        if (_cmbConta.SelectedValue is not int contaId)
        {
            MessageBox.Show("Selecione uma conta.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!TryParseCompetencia(_txtCompetencia.Text, out var competencia))
        {
            MessageBox.Show("Competência inválida. Use o formato yyyy-MM.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        LancamentoResult = new Lancamento
        {
            Id = _lancamentoAtual?.Id ?? 0,
            ContaId = contaId,
            Competencia = competencia,
            Vencimento = _dtpVencimento.Value.Date,
            Valor = _numValor.Value,
            Status = _cmbStatus.SelectedItem?.ToString() ?? "Pendente",
            DataPagamento = _dtpPagamento.Checked ? _dtpPagamento.Value.Date : null,
            FormaPagamento = _cmbFormaPagamento.SelectedItem?.ToString(),
            Observacao = _txtObservacao.Text.Trim()
        };

        DialogResult = DialogResult.OK;
        Close();
    }

    private static bool TryParseCompetencia(string input, out string competencia)
    {
        competencia = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (!DateTime.TryParseExact(
                input.Trim() + "-01",
                "yyyy-MM-dd",
                null,
                System.Globalization.DateTimeStyles.None,
                out var date))
        {
            return false;
        }

        competencia = date.ToString("yyyy-MM");
        return true;
    }
}
