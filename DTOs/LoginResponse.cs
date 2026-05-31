namespace FixRushGameAPI.DTOs
{
    public class LoginResponse
    {
        public int PlayerId { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Dinero { get; set; }
        public int Nivel { get; set; }
        public int Experiencia { get; set; }
        public int? CosmeticoCuerpoId { get; set; }
        public int? CosmeticoGorroId { get; set; }
        public List<ItemDto> Items { get; set; } = new();
    }
}
