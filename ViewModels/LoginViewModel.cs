using System.ComponentModel.DataAnnotations;

namespace SistemaReservas.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo electronico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo electronico valido.")]
        [Display(Name = "Correo electronico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contrasena")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordar sesion en este equipo")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
