using System.ComponentModel.DataAnnotations;

namespace FixRushGameAPI.DTOs
{
    public class UpdateItemRequest
    {
        [Required]
        public bool Tiene { get; set; }
    }
}
