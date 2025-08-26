using SupportWeb.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SupportWeb.Models.ViewModels
{
    public class RespuestaViewModel
    {
        public Guid Id { get; set; }
        public Guid ReclamoId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public bool EsPrivada { get; set; }
    }

    public class CrearRespuestaViewModel
    {
        [Required(ErrorMessage = "El contenido de la respuesta es requerido")]
        [StringLength(2000, ErrorMessage = "La respuesta no puede exceder 2000 caracteres")]
        [Display(Name = "Respuesta")]
        public string Contenido { get; set; } = string.Empty;

        public Guid ReclamoId { get; set; }
        public bool EsPrivada { get; set; }
        public string? Mensaje { get; set; }
    }
}
