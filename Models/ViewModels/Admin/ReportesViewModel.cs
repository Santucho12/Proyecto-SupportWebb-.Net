using SupportWeb.Models.DTOs;
using System.Collections.Generic;



namespace SupportWeb.Models.ViewModels.Admin
{
	public class ReportesViewModel
	{
		public int TotalReclamos { get; set; }
		public int TotalUsuarios { get; set; }
		public Dictionary<string, int> ReclamosPorEstado { get; set; } = new();
		public Dictionary<string, int> ReclamosPorPrioridad { get; set; } = new();
		public Dictionary<string, int> UsuariosPorRol { get; set; } = new();
		public Dictionary<string, int> ReclamosPorMes { get; set; } = new();
		public double TiempoPromedioResolucion { get; set; }
		public string UsuarioConMasReclamos { get; set; } = string.Empty;
		public string PrioridadMasFrecuente { get; set; } = string.Empty;
		public List<UsuarioDto> Usuarios { get; set; } = new();
		public List<string> TiposReclamo { get; set; } = new();
		public List<string> Areas { get; set; } = new();
		public Dictionary<string, int> ReclamosPorUsuario { get; set; } = new();
		public Dictionary<string, int> ReclamosPorArea { get; set; } = new();
		public Dictionary<string, int> ReclamosPorTipo { get; set; } = new();
		public Dictionary<string, int> RankingUsuarios { get; set; } = new();
		public Dictionary<string, int> RankingSoporte { get; set; } = new();
		public double PorcentajeResueltosEnTiempo { get; set; }
		// ...otras propiedades...

		// Para evolución histórica (lista de objetos con Fecha y Cantidad)
		public List<EvolucionHistoricaItem> EvolucionHistorica { get; set; } = new();

		// Para mapa de calor (día/cantidad)
		public Dictionary<string, int> MapaCalor { get; set; } = new();

		// Para satisfacción (soporte/porcentaje)
		public Dictionary<string, double> Satisfaccion { get; set; } = new();
	}

	public class EvolucionHistoricaItem
	{
		public string Fecha { get; set; }
		public int Cantidad { get; set; }
	}
}



