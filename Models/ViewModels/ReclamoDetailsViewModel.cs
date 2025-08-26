using SupportWeb.Models.DTOs;
using System;
using System.Collections.Generic;

namespace SupportWeb.Models.ViewModels
{
    public class ReclamoDetailsViewModel
    {
        public ReclamoDto Reclamo { get; set; } = new();
        public List<RespuestaDto> Respuestas { get; set; } = new();
        public List<string> EstadosDisponibles { get; set; } = new();
        public bool PuedeEditarEstado { get; set; } = false;
        public bool PuedeResponder { get; set; } = false;
    }
}
