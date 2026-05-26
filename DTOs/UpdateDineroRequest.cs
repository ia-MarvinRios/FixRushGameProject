using System.ComponentModel.DataAnnotations;

namespace FixRushGameAPI.DTOs
{
    public class UpdateDineroRequest
    {
        [Required]
        [Range(0, 999999999, ErrorMessage = "El dinero no puede ser negativo")]
        public decimal Dinero { get; set; }
    }
}
