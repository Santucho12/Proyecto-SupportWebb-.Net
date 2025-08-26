using SupportWeb.Models.DTOs;

namespace SupportWeb.Models.ViewModels.Admin
{
    public class GestionUsuariosViewModel
    {
        public List<UsuarioDto> Usuarios { get; set; } = new();
        public int TotalUsuarios { get; set; }
        public Dictionary<string, int> UsuariosPorRol { get; set; } = new();
    }

    public class AdminGestionReclamosViewModel
    {
    public List<ReclamoDto> Reclamos { get; set; } = new();
    public string Busqueda { get; set; } = string.Empty;
        public int TotalReclamos { get; set; }
        public int ReclamosNuevos { get; set; }
        public int ReclamosEnProceso { get; set; }
        public int ReclamosResueltos { get; set; }
        public Dictionary<Guid, string> UsuariosDict { get; set; } = new();
        public Dictionary<string, int> ReclamosPorMes { get; set; } = new();
        public double PromedioTiempoResolucion { get; set; }
    }

}
