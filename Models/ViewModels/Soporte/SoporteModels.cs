using SupportWeb.Models.DTOs;

namespace SupportWeb.Models.ViewModels.Soporte
{
    public class GestionReclamosViewModel
    {
        public string UsuarioNombre { get; set; } = string.Empty;
        public int TotalReclamos { get; set; }
    public List<ReclamoDto> ReclamosNuevos { get; set; } = new();
    public List<ReclamoDto> ReclamosEnProceso { get; set; } = new();
    public List<ReclamoDto> ReclamosRespondidos { get; set; } = new();
    public List<ReclamoDto> ReclamosCerrados { get; set; } = new();
    public int ReclamosResueltos { get; set; } // Incluye los reclamos respondidos
        public int ReclamosHoy { get; set; }
        public int ReclamosSemana { get; set; }
        public int ReclamosMes { get; set; }
        public int ReclamosPrioridadAlta { get; set; }
        public int ReclamosPrioridadMedia { get; set; }
        public int ReclamosPrioridadBaja { get; set; }
        public List<ReclamoDto> Reclamos { get; set; } = new();
        public List<ReclamoDto> TodosLosReclamos { get; set; } = new();
        public List<ReclamoDto> ReclamosUrgentes { get; set; } = new();
        public List<ReclamoDto> ReclamosRecientes { get; set; } = new();
        public List<UsuarioDto> Usuarios { get; set; } = new();
        public List<UsuarioDto> UsuariosConReclamos { get; set; } = new();
        public double TiempoPromedioResolucion { get; set; }
        public double PromedioTiempoResolucion { get; set; }
        public Dictionary<string, int> ReclamosPorEstado { get; set; } = new();
        public Dictionary<string, int> ReclamosPorPrioridad { get; set; } = new();
        public string? FiltroEstado { get; set; }
        public string? FiltroPrioridad { get; set; }
        public string? TerminoBusqueda { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }

    public class CasosUrgentesViewModel
    {
        public string UsuarioNombre { get; set; } = string.Empty;
        public List<ReclamoDto> CasosUrgentes { get; set; } = new();
        public int TotalUrgentes { get; set; }
        public int UrgentesNuevos { get; set; }
        public int UrgentesEnProceso { get; set; }
        public double TiempoPromedioAtencion { get; set; }
        public List<ReclamoDto> CasosEscalados { get; set; } = new();
        public Dictionary<string, int> UrgentesCategoria { get; set; } = new();
        public List<ReclamoDto> CasosCriticos { get; set; } = new();
    }

    public class SoportePerfilViewModel
    {
        public UsuarioDto Usuario { get; set; } = new();
        public string StatusActual { get; set; } = string.Empty;
        public int TotalReviews { get; set; }
        public DateTime FechaUltimaActividad { get; set; }
        public int CasosAtendidos { get; set; }
        public int CasosResueltos { get; set; }
        public int CasosPendientes { get; set; }
        public double PromedioResolucion { get; set; }
        public List<ReclamoDto> UltimosReclamos { get; set; } = new();
        public List<EstadisticaMensual> EstadisticasMensuales { get; set; } = new();
    }

    public class SoporteConfiguracionViewModel
    {
        public UsuarioDto Usuario { get; set; } = new();
        public bool NotificacionesPush { get; set; }
        public bool NotificacionCasosUrgentes { get; set; }
        public bool SonidoNotificaciones { get; set; }
        public bool AutoAsignacion { get; set; }
        public int ActualizacionAutomatica { get; set; }
        public bool MostrarEstadisticas { get; set; }
        public bool TemaOscuro { get; set; }
        public string IdiomaPreferido { get; set; } = "es";
        public string ZonaHoraria { get; set; } = "America/Lima";
        public bool RequiereCambioPassword { get; set; }
        public bool SesionUnica { get; set; }
        public int TiempoSesionMinutos { get; set; } = 60;
    }

    public class EstadisticaMensual
    {
        public string Mes { get; set; } = string.Empty;
        public int CasosAtendidos { get; set; }
        public int CasosResueltos { get; set; }
        public double TiempoPromedio { get; set; }
    }

    public class DetalleReclamoViewModel
    {
        public ReclamoDto Reclamo { get; set; } = null!;
        public List<RespuestaDto> Respuestas { get; set; } = new();
        public string UsuarioNombre { get; set; } = string.Empty;
        public bool PuedeEditarEstado { get; set; }
        public bool PuedeResponder { get; set; }
    }

    public class NuevaRespuestaViewModel
    {
        public Guid ReclamoId { get; set; }
        public ReclamoDto? ReclamoSeleccionado { get; set; }
        public ReclamoDto Reclamo { get; set; } = null!;
        public List<ReclamoDto> ReclamosDisponibles { get; set; } = new();
        public List<ReclamoDto> ReclamosPendientes { get; set; } = new();
        public string UsuarioNombre { get; set; } = string.Empty;
        public string ContenidoRespuesta { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public List<RespuestaDto> RespuestasAnteriores { get; set; } = new();
        public bool EsRespuestaPrivada { get; set; }
        public string TipoRespuesta { get; set; } = "Respuesta";
    public int TotalPendientes { get; set; }
    public int PendientesHoy { get; set; }
    public int PendientesUrgentes { get; set; }
    public int ReclamosResueltos { get; set; } // Nuevo: cantidad de reclamos respondidos/resueltos
    }
}
