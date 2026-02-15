namespace AgendaContas.UI.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.FlowLayoutPanel pnlDash;
    private System.Windows.Forms.Label lblTotalMes;
    private System.Windows.Forms.Label lblPago;
    private System.Windows.Forms.Label lblPendente;
    private System.Windows.Forms.Label lblAtrasado;
    private System.Windows.Forms.DataGridView dgvLancamentos;
    private System.Windows.Forms.Panel pnlButtons;
    private System.Windows.Forms.Button btnPagar;
    private System.Windows.Forms.Button btnNovaConta;
    private System.Windows.Forms.Button btnExportar;
    private System.Windows.Forms.Button btnAnexo;
    private System.Windows.Forms.Button btnInformacoes;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        pnlDash = new FlowLayoutPanel();
        lblTotalMes = new Label();
        lblPago = new Label();
        lblPendente = new Label();
        lblAtrasado = new Label();
        dgvLancamentos = new DataGridView();
        pnlButtons = new Panel();
        btnInformacoes = new Button();
        btnAnexo = new Button();
        btnExportar = new Button();
        btnNovaConta = new Button();
        btnPagar = new Button();
        pnlDash.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvLancamentos).BeginInit();
        pnlButtons.SuspendLayout();
        SuspendLayout();
        // 
        // pnlDash
        // 
        pnlDash.BackColor = Color.WhiteSmoke;
        pnlDash.Controls.Add(lblTotalMes);
        pnlDash.Controls.Add(lblPago);
        pnlDash.Controls.Add(lblPendente);
        pnlDash.Controls.Add(lblAtrasado);
        pnlDash.Dock = DockStyle.Top;
        pnlDash.Location = new Point(0, 0);
        pnlDash.Name = "pnlDash";
        pnlDash.Padding = new Padding(10);
        pnlDash.Size = new Size(934, 100);
        pnlDash.TabIndex = 0;
        // 
        // lblTotalMes
        // 
        lblTotalMes.BackColor = Color.LightBlue;
        lblTotalMes.BorderStyle = BorderStyle.FixedSingle;
        lblTotalMes.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblTotalMes.Location = new Point(15, 15);
        lblTotalMes.Margin = new Padding(5);
        lblTotalMes.Name = "lblTotalMes";
        lblTotalMes.Size = new Size(210, 75);
        lblTotalMes.TabIndex = 0;
        lblTotalMes.Text = "Total do Mês\r\nR$ 0,00";
        lblTotalMes.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblPago
        // 
        lblPago.BackColor = Color.LightGreen;
        lblPago.BorderStyle = BorderStyle.FixedSingle;
        lblPago.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblPago.Location = new Point(235, 15);
        lblPago.Margin = new Padding(5);
        lblPago.Name = "lblPago";
        lblPago.Size = new Size(210, 75);
        lblPago.TabIndex = 1;
        lblPago.Text = "Total Pago\r\nR$ 0,00";
        lblPago.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblPendente
        // 
        lblPendente.BackColor = Color.LightYellow;
        lblPendente.BorderStyle = BorderStyle.FixedSingle;
        lblPendente.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblPendente.Location = new Point(455, 15);
        lblPendente.Margin = new Padding(5);
        lblPendente.Name = "lblPendente";
        lblPendente.Size = new Size(210, 75);
        lblPendente.TabIndex = 2;
        lblPendente.Text = "Pendente\r\nR$ 0,00";
        lblPendente.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lblAtrasado
        // 
        lblAtrasado.BackColor = Color.LightCoral;
        lblAtrasado.BorderStyle = BorderStyle.FixedSingle;
        lblAtrasado.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblAtrasado.Location = new Point(675, 15);
        lblAtrasado.Margin = new Padding(5);
        lblAtrasado.Name = "lblAtrasado";
        lblAtrasado.Size = new Size(210, 75);
        lblAtrasado.TabIndex = 3;
        lblAtrasado.Text = "Atrasado\r\nR$ 0,00";
        lblAtrasado.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // dgvLancamentos
        // 
        dgvLancamentos.AllowUserToAddRows = false;
        dgvLancamentos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvLancamentos.BackgroundColor = Color.White;
        dgvLancamentos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvLancamentos.Dock = DockStyle.Fill;
        dgvLancamentos.Location = new Point(0, 100);
        dgvLancamentos.Name = "dgvLancamentos";
        dgvLancamentos.ReadOnly = true;
        dgvLancamentos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvLancamentos.Size = new Size(934, 451);
        dgvLancamentos.TabIndex = 1;
        dgvLancamentos.CellContentClick += dgvLancamentos_CellContentClick;
        // 
        // pnlButtons
        // 
        pnlButtons.BackColor = Color.WhiteSmoke;
        pnlButtons.Controls.Add(btnInformacoes);
        pnlButtons.Controls.Add(btnAnexo);
        pnlButtons.Controls.Add(btnExportar);
        pnlButtons.Controls.Add(btnNovaConta);
        pnlButtons.Controls.Add(btnPagar);
        pnlButtons.Dock = DockStyle.Bottom;
        pnlButtons.Location = new Point(0, 551);
        pnlButtons.Name = "pnlButtons";
        pnlButtons.Size = new Size(934, 60);
        pnlButtons.TabIndex = 2;
        // 
        // btnInformacoes
        // 
        btnInformacoes.FlatStyle = FlatStyle.Flat;
        btnInformacoes.Location = new Point(606, 15);
        btnInformacoes.Name = "btnInformacoes";
        btnInformacoes.Size = new Size(120, 30);
        btnInformacoes.TabIndex = 4;
        btnInformacoes.Text = "Informações";
        btnInformacoes.UseVisualStyleBackColor = true;
        btnInformacoes.Click += btnInformacoes_Click;
        // 
        // btnAnexo
        // 
        btnAnexo.FlatStyle = FlatStyle.Flat;
        btnAnexo.Location = new Point(450, 15);
        btnAnexo.Name = "btnAnexo";
        btnAnexo.Size = new Size(150, 30);
        btnAnexo.TabIndex = 3;
        btnAnexo.Text = "Anexar Comprovante";
        btnAnexo.UseVisualStyleBackColor = true;
        btnAnexo.Click += btnAnexo_Click;
        // 
        // btnExportar
        // 
        btnExportar.FlatStyle = FlatStyle.Flat;
        btnExportar.Location = new Point(324, 15);
        btnExportar.Name = "btnExportar";
        btnExportar.Size = new Size(120, 30);
        btnExportar.TabIndex = 2;
        btnExportar.Text = "Exportar CSV";
        btnExportar.UseVisualStyleBackColor = true;
        btnExportar.Click += btnExportar_Click;
        // 
        // btnNovaConta
        // 
        btnNovaConta.FlatStyle = FlatStyle.Flat;
        btnNovaConta.Location = new Point(168, 15);
        btnNovaConta.Name = "btnNovaConta";
        btnNovaConta.Size = new Size(150, 30);
        btnNovaConta.TabIndex = 1;
        btnNovaConta.Text = "Nova Conta (Exemplo)";
        btnNovaConta.UseVisualStyleBackColor = true;
        btnNovaConta.Click += btnNovaConta_Click;
        // 
        // btnPagar
        // 
        btnPagar.FlatStyle = FlatStyle.Flat;
        btnPagar.Location = new Point(12, 15);
        btnPagar.Name = "btnPagar";
        btnPagar.Size = new Size(150, 30);
        btnPagar.TabIndex = 0;
        btnPagar.Text = "Marcar como Pago";
        btnPagar.UseVisualStyleBackColor = true;
        btnPagar.Click += btnPagar_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(934, 611);
        Controls.Add(dgvLancamentos);
        Controls.Add(pnlButtons);
        Controls.Add(pnlDash);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Agenda Financeira Pro - Dashboard";
        pnlDash.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvLancamentos).EndInit();
        pnlButtons.ResumeLayout(false);
        ResumeLayout(false);

    }
}
