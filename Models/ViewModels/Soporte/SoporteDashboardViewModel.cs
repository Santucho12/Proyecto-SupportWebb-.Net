using SupportWeb.Models.DTOs;

namespace SupportWeb.Models.ViewModels.Soporte
{
    public class SoporteDashboardViewModel
    {
        public UsuarioDto Usuario { get; set; } = new();
        public string UsuarioNombre { get; set; } = string.Empty;
        public int TotalReclamos { get; set; }
        public int ReclamosNuevos { get; set; }
        public int ReclamosEnProceso { get; set; }
        public int ReclamosPendientes { get; set; }
        public int ReclamosAsignados { get; set; }
        public int ReclamosResueltos { get; set; }
        public double TiempoPromedioResolucion { get; set; }
        public double PorcentajeSatisfaccion { get; set; }
        public Dictionary<string, int> ReclamosPrioridad { get; set; } = new();
        public Dictionary<string, int> ReclamosPorPrioridad { get; set; } = new();
        public Dictionary<string, int> ReclamosPorEstado { get; set; } = new();
        public List<TendenciaDataPoint> TendenciaSemanal { get; set; } = new();
        public List<ReclamoDto> ReclamosRecientes { get; set; } = new();
    }

    public class TendenciaDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
