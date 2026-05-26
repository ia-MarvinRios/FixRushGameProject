namespace FixRushGameAPI.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? GroupId { get; set; }
        public bool Activo { get; set; } = true;
        public int Version { get; set; } = 1;
    }
}
