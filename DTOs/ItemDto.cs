namespace FixRushGameAPI.DTOs
{
    public class ItemDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Grupo { get; set; }
        public bool Tiene { get; set; }
    }
}
