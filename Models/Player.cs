namespace FixRushGameAPI.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateOnly FechaNacimiento { get; set; }
        public decimal Dinero { get; set; } = 0;
        public int Nivel { get; set; } = 1;
        public int Experiencia { get; set; } = 0;
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public int? CosmeticoCuerpoId { get; set; }
        public int? CosmeticoGorroId { get; set; }
    }
}
