namespace AgendaContas.UI.Forms;

partial class InfoForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblSubTitle;
    private Panel pnlActions;
    private Button btnCriacao;
    private Button btnSobreNos;
    private Button btnManifesto;
    private Button btnSistema;
    private Label lblSectionTitle;
    private RichTextBox rtbContent;
    private Panel pnlBottom;
    private Button btnCopiar;
    private Button btnAtualizarBcb;
    private Button btnFechar;

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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InfoForm));
        pnlHeader = new Panel();
        lblSubTitle = new Label();
        lblTitle = new Label();
        pnlActions = new Panel();
        btnSistema = new Button();
        btnManifesto = new Button();
        btnSobreNos = new Button();
        btnCriacao = new Button();
        lblSectionTitle = new Label();
        rtbContent = new RichTextBox();
        pnlBottom = new Panel();
        btnFechar = new Button();
        btnAtualizarBcb = new Button();
        btnCopiar = new Button();
        pnlHeader.SuspendLayout();
        pnlActions.SuspendLayout();
        pnlBottom.SuspendLayout();
        SuspendLayout();
        // 
        // pnlHeader
        // 
        pnlHeader.BackColor = Color.FromArgb(20, 28, 38);
        pnlHeader.Controls.Add(lblSubTitle);
        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Location = new Point(0, 0);
        pnlHeader.Name = "pnlHeader";
        pnlHeader.Size = new Size(820, 86);
        pnlHeader.TabIndex = 0;
        // 
        // lblSubTitle
        // 
        lblSubTitle.AutoSize = true;
        lblSubTitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        lblSubTitle.ForeColor = Color.Gainsboro;
        lblSubTitle.Location = new Point(22, 52);
        lblSubTitle.Name = "lblSubTitle";
        lblSubTitle.Size = new Size(369, 15);
        lblSubTitle.TabIndex = 1;
        lblSubTitle.Text = "Painel institucional, manifesto e dados técnicos da aplicação.";
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI Semibold", 15.75F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(20, 18);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(248, 30);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Deep Darkness Studios";
        // 
        // pnlActions
        // 
        pnlActions.BackColor = Color.FromArgb(34, 42, 53);
        pnlActions.Controls.Add(btnSistema);
        pnlActions.Controls.Add(btnManifesto);
        pnlActions.Controls.Add(btnSobreNos);
        pnlActions.Controls.Add(btnCriacao);
        pnlActions.Dock = DockStyle.Top;
        pnlActions.Location = new Point(0, 86);
        pnlActions.Name = "pnlActions";
        pnlActions.Size = new Size(820, 52);
        pnlActions.TabIndex = 1;
        // 
        // btnSistema
        // 
        btnSistema.FlatStyle = FlatStyle.Flat;
        btnSistema.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSistema.ForeColor = Color.WhiteSmoke;
        btnSistema.Location = new Point(552, 10);
        btnSistema.Name = "btnSistema";
        btnSistema.Size = new Size(120, 30);
        btnSistema.TabIndex = 3;
        btnSistema.Text = "Sistema";
        btnSistema.UseVisualStyleBackColor = true;
        btnSistema.Click += btnSistema_Click;
        // 
        // btnManifesto
        // 
        btnManifesto.FlatStyle = FlatStyle.Flat;
        btnManifesto.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnManifesto.ForeColor = Color.WhiteSmoke;
        btnManifesto.Location = new Point(426, 10);
        btnManifesto.Name = "btnManifesto";
        btnManifesto.Size = new Size(120, 30);
        btnManifesto.TabIndex = 2;
        btnManifesto.Text = "Manifesto";
        btnManifesto.UseVisualStyleBackColor = true;
        btnManifesto.Click += btnManifesto_Click;
        // 
        // btnSobreNos
        // 
        btnSobreNos.FlatStyle = FlatStyle.Flat;
        btnSobreNos.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnSobreNos.ForeColor = Color.WhiteSmoke;
        btnSobreNos.Location = new Point(300, 10);
        btnSobreNos.Name = "btnSobreNos";
        btnSobreNos.Size = new Size(120, 30);
        btnSobreNos.TabIndex = 1;
        btnSobreNos.Text = "Sobre";
        btnSobreNos.UseVisualStyleBackColor = true;
        btnSobreNos.Click += btnSobreNos_Click;
        // 
        // btnCriacao
        // 
        btnCriacao.FlatStyle = FlatStyle.Flat;
        btnCriacao.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnCriacao.ForeColor = Color.WhiteSmoke;
        btnCriacao.Location = new Point(174, 10);
        btnCriacao.Name = "btnCriacao";
        btnCriacao.Size = new Size(120, 30);
        btnCriacao.TabIndex = 0;
        btnCriacao.Text = "Equipe";
        btnCriacao.UseVisualStyleBackColor = true;
        btnCriacao.Click += btnCriacao_Click;
        // 
        // lblSectionTitle
        // 
        lblSectionTitle.AutoSize = true;
        lblSectionTitle.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold);
        lblSectionTitle.Location = new Point(24, 148);
        lblSectionTitle.Name = "lblSectionTitle";
        lblSectionTitle.Size = new Size(127, 20);
        lblSectionTitle.TabIndex = 2;
        lblSectionTitle.Text = "Sobre o Projeto";
        // 
        // rtbContent
        // 
        rtbContent.BackColor = Color.FromArgb(248, 249, 251);
        rtbContent.BorderStyle = BorderStyle.FixedSingle;
        rtbContent.Font = new Font("Segoe UI", 10F);
        rtbContent.Location = new Point(24, 174);
        rtbContent.Name = "rtbContent";
        rtbContent.ReadOnly = true;
        rtbContent.Size = new Size(772, 318);
        rtbContent.TabIndex = 3;
        rtbContent.Text = "";
        // 
        // pnlBottom
        // 
        pnlBottom.Controls.Add(btnAtualizarBcb);
        pnlBottom.Controls.Add(btnFechar);
        pnlBottom.Controls.Add(btnCopiar);
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Location = new Point(0, 505);
        pnlBottom.Name = "pnlBottom";
        pnlBottom.Size = new Size(820, 56);
        pnlBottom.TabIndex = 4;
        // 
        // btnAtualizarBcb
        // 
        btnAtualizarBcb.Location = new Point(150, 15);
        btnAtualizarBcb.Name = "btnAtualizarBcb";
        btnAtualizarBcb.Size = new Size(120, 28);
        btnAtualizarBcb.TabIndex = 2;
        btnAtualizarBcb.Text = "Atualizar BCB";
        btnAtualizarBcb.UseVisualStyleBackColor = true;
        btnAtualizarBcb.Click += btnAtualizarBcb_Click;
        // 
        // btnFechar
        // 
        btnFechar.Location = new Point(721, 15);
        btnFechar.Name = "btnFechar";
        btnFechar.Size = new Size(75, 28);
        btnFechar.TabIndex = 1;
        btnFechar.Text = "Fechar";
        btnFechar.UseVisualStyleBackColor = true;
        btnFechar.Click += btnFechar_Click;
        // 
        // btnCopiar
        // 
        btnCopiar.Location = new Point(24, 15);
        btnCopiar.Name = "btnCopiar";
        btnCopiar.Size = new Size(120, 28);
        btnCopiar.TabIndex = 0;
        btnCopiar.Text = "Copiar conteúdo";
        btnCopiar.UseVisualStyleBackColor = true;
        btnCopiar.Click += btnCopiar_Click;
        // 
        // InfoForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(820, 561);
        Controls.Add(pnlBottom);
        Controls.Add(rtbContent);
        Controls.Add(lblSectionTitle);
        Controls.Add(pnlActions);
        Controls.Add(pnlHeader);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(836, 600);
        Name = "InfoForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Informações - AgendaContas";
        Load += InfoForm_Load;
        pnlHeader.ResumeLayout(false);
        pnlHeader.PerformLayout();
        pnlActions.ResumeLayout(false);
        pnlBottom.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
