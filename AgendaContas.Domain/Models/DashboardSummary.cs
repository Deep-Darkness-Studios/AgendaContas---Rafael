namespace AgendaContas.Domain.Models;

public class DashboardSummary
{
    public decimal TotalMes { get; set; }
    public decimal TotalPago { get; set; }
    public decimal TotalPendente { get; set; }
    public decimal TotalAtrasado { get; set; }
}
