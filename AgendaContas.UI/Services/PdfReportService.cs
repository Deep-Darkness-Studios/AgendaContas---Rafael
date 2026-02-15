using AgendaContas.Domain.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace AgendaContas.UI.Services;

public static class PdfReportService
{
    public static void ExportarLancamentos(
        IReadOnlyCollection<Lancamento> lancamentos,
        DashboardSummary summary,
        string competencia,
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Caminho do arquivo PDF inválido.", nameof(filePath));
        }

        using var document = new PdfDocument();
        document.Info.Title = $"AgendaContas - Relatório {competencia}";
        document.Info.Author = "AgendaContas";

        var titleFont = new XFont("Arial", 14, XFontStyle.Bold);
        var normalFont = new XFont("Arial", 9, XFontStyle.Regular);
        var boldFont = new XFont("Arial", 9, XFontStyle.Bold);

        const double margin = 30;
        const double rowHeight = 18;
        const double tableTopPadding = 8;

        PdfPage page = null!;
        XGraphics gfx = null!;
        double y = 0;

        void StartPage()
        {
            page = document.AddPage();
            gfx = XGraphics.FromPdfPage(page);
            y = margin;
        }

        void DrawTableHeader()
        {
            var x = margin;
            gfx.DrawRectangle(XBrushes.LightGray, x, y, page.Width - (margin * 2), rowHeight);
            gfx.DrawString("Venc.", boldFont, XBrushes.Black, new XRect(x + 2, y + 3, 56, rowHeight), XStringFormats.TopLeft);
            x += 60;
            gfx.DrawString("Conta", boldFont, XBrushes.Black, new XRect(x + 2, y + 3, 116, rowHeight), XStringFormats.TopLeft);
            x += 120;
            gfx.DrawString("Categoria", boldFont, XBrushes.Black, new XRect(x + 2, y + 3, 81, rowHeight), XStringFormats.TopLeft);
            x += 85;
            gfx.DrawString("Valor", boldFont, XBrushes.Black, new XRect(x + 2, y + 3, 61, rowHeight), XStringFormats.TopLeft);
            x += 65;
            gfx.DrawString("Status", boldFont, XBrushes.Black, new XRect(x + 2, y + 3, 51, rowHeight), XStringFormats.TopLeft);
            x += 55;
            gfx.DrawString("Pagamento", boldFont, XBrushes.Black, new XRect(x + 2, y + 3, 61, rowHeight), XStringFormats.TopLeft);
            x += 65;
            gfx.DrawString("Observação", boldFont, XBrushes.Black, new XRect(x + 2, y + 3, 81, rowHeight), XStringFormats.TopLeft);
            y += rowHeight;
        }

        void EnsureRowSpace()
        {
            var limit = page.Height - margin - rowHeight;
            if (y <= limit)
            {
                return;
            }

            StartPage();
            DrawTableHeader();
        }

        StartPage();
        gfx.DrawString("AgendaContas - Relatório Financeiro", titleFont, XBrushes.Black, new XRect(margin, y, page.Width - (margin * 2), 20), XStringFormats.TopLeft);
        y += 22;
        gfx.DrawString($"Competência: {competencia}", boldFont, XBrushes.Black, new XRect(margin, y, 220, 16), XStringFormats.TopLeft);
        y += 18;
        gfx.DrawString($"Total do Mês: {summary.TotalMes:C2}", normalFont, XBrushes.Black, new XRect(margin, y, 220, 16), XStringFormats.TopLeft);
        gfx.DrawString($"Total Pago: {summary.TotalPago:C2}", normalFont, XBrushes.Black, new XRect(margin + 220, y, 220, 16), XStringFormats.TopLeft);
        y += 16;
        gfx.DrawString($"Pendente: {summary.TotalPendente:C2}", normalFont, XBrushes.Black, new XRect(margin, y, 220, 16), XStringFormats.TopLeft);
        gfx.DrawString($"Atrasado: {summary.TotalAtrasado:C2}", normalFont, XBrushes.Black, new XRect(margin + 220, y, 220, 16), XStringFormats.TopLeft);
        y += 16;
        gfx.DrawString($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}", normalFont, XBrushes.Black, new XRect(margin, y, 220, 16), XStringFormats.TopLeft);
        y += tableTopPadding;

        DrawTableHeader();

        foreach (var lancamento in lancamentos.OrderBy(x => x.Vencimento).ThenBy(x => x.NomeConta))
        {
            EnsureRowSpace();

            var x = margin;
            gfx.DrawString(lancamento.Vencimento.ToString("dd/MM/yyyy"), normalFont, XBrushes.Black, new XRect(x + 2, y + 3, 56, rowHeight), XStringFormats.TopLeft);
            x += 60;
            gfx.DrawString(TrimForCell(lancamento.NomeConta, 24), normalFont, XBrushes.Black, new XRect(x + 2, y + 3, 116, rowHeight), XStringFormats.TopLeft);
            x += 120;
            gfx.DrawString(TrimForCell(lancamento.CategoriaNome, 16), normalFont, XBrushes.Black, new XRect(x + 2, y + 3, 81, rowHeight), XStringFormats.TopLeft);
            x += 85;
            gfx.DrawString(lancamento.Valor.ToString("C2"), normalFont, XBrushes.Black, new XRect(x + 2, y + 3, 61, rowHeight), XStringFormats.TopLeft);
            x += 65;
            gfx.DrawString(TrimForCell(lancamento.Status, 10), normalFont, XBrushes.Black, new XRect(x + 2, y + 3, 51, rowHeight), XStringFormats.TopLeft);
            x += 55;
            gfx.DrawString(lancamento.DataPagamento?.ToString("dd/MM/yyyy") ?? "-", normalFont, XBrushes.Black, new XRect(x + 2, y + 3, 61, rowHeight), XStringFormats.TopLeft);
            x += 65;
            gfx.DrawString(TrimForCell(lancamento.Observacao, 18), normalFont, XBrushes.Black, new XRect(x + 2, y + 3, 81, rowHeight), XStringFormats.TopLeft);

            gfx.DrawLine(XPens.LightGray, margin, y + rowHeight, page.Width - margin, y + rowHeight);
            y += rowHeight;
        }

        document.Save(filePath);
    }

    private static string TrimForCell(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        var input = value.Trim();
        return input.Length <= maxLength ? input : input[..(maxLength - 3)] + "...";
    }
}
