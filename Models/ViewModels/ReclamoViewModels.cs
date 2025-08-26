using SupportWeb.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SupportWeb.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Propiedades básicas requeridas por el controller
        public int TotalReclamos { get; set; }
        public int ReclamosPendientes { get; set; }
        public int ReclamosResueltos { get; set; }
        public List<dynamic> Reclamos { get; set; } = new List<dynamic>();
        public int NotificacionesNoLeidas { get; set; }

        // Propiedades adicionales para funcionalidad avanzada
        public DashboardStatsDto Stats { get; set; } = new();
        public List<ReclamoDto> ReclamosRecientes { get; set; } = new();
        public List<RespuestaDto> NotificacionesNoVistas { get; set; } = new();
        public string UsuarioRol { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public UsuarioDto? UsuarioActual { get; set; }

        // Datos para gráficos
        public Dictionary<string, int> ReclamosPorEstado { get; set; } = new();
        public List<ChartDataPoint> TendenciaMensual { get; set; } = new();
        public List<ChartDataPoint> TiempoResolucion { get; set; } = new();
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string? Color { get; set; }
    }

    public class ReclamosIndexViewModel
    {
        public List<ReclamoDto> Reclamos { get; set; } = new();
        public string? FiltroEstado { get; set; }
        public string? FiltroUsuario { get; set; }
        public DateTime? FiltroFechaDesde { get; set; }
        public DateTime? FiltroFechaHasta { get; set; }
        public string? TerminoBusqueda { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalReclamos { get; set; }
        public string UsuarioRol { get; set; } = string.Empty;
    }


    public class CreateReclamoViewModel
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        [Display(Name = "Título del reclamo")]
        public string Titulo { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        [Display(Name = "Descripción detallada")]
        public string Descripcion { get; set; } = null!;

        [Required(ErrorMessage = "La prioridad es requerida")]
        [Display(Name = "Prioridad")]
        public string Prioridad { get; set; } = "Baja";

        [Display(Name = "Archivo adjunto (opcional)")]
        [DataType(DataType.Upload)]
        public IFormFile? Archivo { get; set; }

        public string? ErrorMessage { get; set; }
    }

    public class ReclamoEditViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        [Display(Name = "Título del reclamo")]
        public string Titulo { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
        [Display(Name = "Descripción detallada")]
        public string Descripcion { get; set; } = null!;

        [Display(Name = "Estado")]
        public string Estado { get; set; } = null!;

        [Display(Name = "Archivo adjunto (opcional)")]
        [DataType(DataType.Upload)]
        public IFormFile? Archivo { get; set; }

        public string? ArchivoActual { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class EditarPerfilViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "El teléfono no es válido")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
