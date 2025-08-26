namespace SupportWeb.Models.DTOs
{
    public class UsuarioDto
    {
    public Guid Id { get; set; }
    public string Nombre { get; set; } = null!;
    [System.Text.Json.Serialization.JsonPropertyName("correoElectronico")]
    public string Email { get; set; } = null!;
    public string Rol { get; set; } = null!;
    }

    public class ReclamoDto
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string Prioridad { get; set; } = "Baja";
        public DateTime FechaCreacion { get; set; }
        public Guid UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
        public List<RespuestaDto>? Respuestas { get; set; }
    }

    public class RespuestaDto
    {
        public Guid Id { get; set; }
        public string Contenido { get; set; } = null!;
        public string Mensaje => Contenido; // Alias para compatibilidad
        public DateTime FechaRespuesta { get; set; }
        public DateTime FechaCreacion => FechaRespuesta; // Alias para compatibilidad
        public bool Visto { get; set; }
        public Guid ReclamoId { get; set; }
        public Guid UsuarioId { get; set; }
        public string? UsuarioNombre { get; set; }
    }

    public class CreateReclamoDto
    {
        public string Titulo { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string Prioridad { get; set; } = "Baja";
        public Guid UsuarioId { get; set; }
        public IFormFile? Archivo { get; set; }
    }

    public class CreateRespuestaDto
    {
        public string Contenido { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public Guid ReclamoId { get; set; }
        public Guid UsuarioId { get; set; }
    }

    public class DashboardStatsDto
    {
        public int TotalReclamos { get; set; }
        public int ReclamosNuevos { get; set; }
        public int ReclamosEnProceso { get; set; }
        public int ReclamosResueltos { get; set; }
        public int ReclamosPendientes { get; set; }
        public int ReclamosRespondidos { get; set; }
        public int ReclamosCerrados { get; set; }
        public double TiempoPromedioResolucion { get; set; }
        public List<ReclamoDto> ReclamosRecientes { get; set; } = new();
    }

    public class UpdateUsuarioDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class NotificacionDto
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }
        public string Tipo { get; set; } = "info";
    public Guid? ReclamoId { get; set; }
    public Guid UsuarioId { get; set; }
    // Última respuesta y fecha de la respuesta
    public DateTime? FechaUltimaRespuesta { get; set; }
    // Nueva propiedad para mostrar la última respuesta del reclamo
    public string? UltimaRespuesta { get; set; }
    }
}
