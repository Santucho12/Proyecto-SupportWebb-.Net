using SupportWeb.Models.DTOs;

namespace SupportWeb.Models.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public UsuarioDto Usuario { get; set; } = new();
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int TotalReclamos { get; set; }
        public int ReclamosNuevos { get; set; }
        public int ReclamosEnProceso { get; set; }
        public int ReclamosResueltos { get; set; }
        public int ReclamosCerrados { get; set; }
        public List<ReclamoDto> ReclamosRecientes { get; set; } = new();
        public List<UsuarioDto> UsuariosRecientes { get; set; } = new();
        public Dictionary<string, int> EstadisticasPorRol { get; set; } = new();
        public Dictionary<string, int> ReclamosPorPrioridad { get; set; } = new();
        public Dictionary<string, int> ReclamosPorEstado { get; set; } = new();
        public List<EstadisticaDiaria> EstadisticasPorDia { get; set; } = new();
    }
    
    public class EstadisticaDiaria
    {
        public string Fecha { get; set; } = string.Empty;
        public int Reclamos { get; set; }
        public int Usuarios { get; set; }
    }
}
