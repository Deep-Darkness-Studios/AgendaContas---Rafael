using System;
using System.Drawing;
using System.Windows.Forms;

namespace AgendaContas.UI.Forms
{
    partial class InfoForm
    {
        private System.ComponentModel.IContainer components = null;
        private Button btnCriacao;
        private Button btnSobreNos;
        private Button btnManifesto;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InfoForm));
            btnCriacao = new Button();
            btnSobreNos = new Button();
            btnManifesto = new Button();
            SuspendLayout();
            // 
            // btnCriacao
            // 
            btnCriacao.BackColor = Color.FromArgb(45, 45, 45);
            btnCriacao.FlatStyle = FlatStyle.Flat;
            btnCriacao.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCriacao.ForeColor = Color.Cyan;
            btnCriacao.Location = new Point(34, 292);
            btnCriacao.Name = "btnCriacao";
            btnCriacao.Size = new Size(141, 30);
            btnCriacao.TabIndex = 0;
            btnCriacao.Text = "Criação";
            btnCriacao.UseVisualStyleBackColor = false;
            btnCriacao.Click += btnCriacao_Click;
            // 
            // btnSobreNos
            // 
            btnSobreNos.BackColor = Color.FromArgb(45, 45, 45);
            btnSobreNos.FlatStyle = FlatStyle.Flat;
            btnSobreNos.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSobreNos.ForeColor = Color.Cyan;
            btnSobreNos.Location = new Point(181, 292);
            btnSobreNos.Name = "btnSobreNos";
            btnSobreNos.Size = new Size(151, 30);
            btnSobreNos.TabIndex = 1;
            btnSobreNos.Text = "SOBRE NÓS";
            btnSobreNos.UseVisualStyleBackColor = false;
            btnSobreNos.Click += btnSobreNos_Click;
            // 
            // btnManifesto
            // 
            btnManifesto.BackColor = Color.FromArgb(45, 45, 45);
            btnManifesto.FlatStyle = FlatStyle.Flat;
            btnManifesto.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnManifesto.ForeColor = Color.Cyan;
            btnManifesto.Location = new Point(338, 292);
            btnManifesto.Name = "btnManifesto";
            btnManifesto.Size = new Size(180, 30);
            btnManifesto.TabIndex = 2;
            btnManifesto.Text = "Manifesto DDS";
            btnManifesto.UseVisualStyleBackColor = false;
            btnManifesto.Click += btnManifesto_Click;
            // 
            // InfoForm
            // 
            AutoSize = true;
            BackColor = Color.FromArgb(30, 30, 30);
            BackgroundImage = Properties.Resources.fb_page_cover_dds;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(600, 371);
            Controls.Add(btnCriacao);
            Controls.Add(btnSobreNos);
            Controls.Add(btnManifesto);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "InfoForm";
            Text = "Informações - Deep Darkness Studios™";
            Load += InfoForm_Load;
            ResumeLayout(false);
        }
    }
}
