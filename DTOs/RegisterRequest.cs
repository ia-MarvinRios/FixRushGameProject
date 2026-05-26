using System.ComponentModel.DataAnnotations;

namespace FixRushGameAPI.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nickname es requerido")]
        [MaxLength(50)]
        public string Nickname { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo invalido")]
        [MaxLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es requerida")]
        [MinLength(6, ErrorMessage = "Minimo 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateOnly FechaNacimiento { get; set; }
    }
}
