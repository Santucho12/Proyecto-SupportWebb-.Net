using SupportWeb.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SupportWeb.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class NotificacionesViewModel
    {
        public UsuarioDto? Usuario { get; set; }
        public List<NotificacionDto> Notificaciones { get; set; } = new();
    }

    public class ConfiguracionViewModel
    {
        public UsuarioDto? Usuario { get; set; }
        public bool NotificacionesEmail { get; set; }
        public bool NotificacionesPush { get; set; }
        public bool TemaOscuro { get; set; }
        public string IdiomaPreferido { get; set; } = "es";
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required(ErrorMessage = "El tipo de usuario es requerido")]
        [Display(Name = "Tipo de usuario")]
        public string Rol { get; set; } = "Usuario";

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}
