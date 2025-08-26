using SupportWeb.Models.DTOs;

namespace SupportWeb.Models.ViewModels.Usuario
{
    public class UsuarioDashboardViewModel
    {
        public UsuarioDto Usuario { get; set; } = new();
        public int TotalReclamos { get; set; }
        public int ReclamosNuevos { get; set; }
        public int ReclamosEnProceso { get; set; }
        public int ReclamosResueltos { get; set; }
        public int ReclamosCerrados { get; set; }
        public List<ReclamoDto> ReclamosRecientes { get; set; } = new();
        public List<NotificacionDto> NotificacionesRecientes { get; set; } = new();
    public int CantidadNotificaciones { get; set; }
    }
}
