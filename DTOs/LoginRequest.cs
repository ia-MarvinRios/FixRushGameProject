using System.ComponentModel.DataAnnotations;

namespace FixRushGameAPI.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo invalido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es requerida")]
        public string Password { get; set; } = string.Empty;
    }
}
