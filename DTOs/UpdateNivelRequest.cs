using System.ComponentModel.DataAnnotations;

namespace FixRushGameAPI.DTOs
{
    public class UpdateNivelRequest
    {
        [Required]
        [Range(1, 9999, ErrorMessage = "El nivel debe ser mayor a 0")]
        public int Nivel { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La experiencia no puede ser negativa")]
        public int Experiencia { get; set; }
        
    }
}
