namespace FixRushGameAPI.DTOs
{
    public class RegisterResponse
    {
        public int PlayerId { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public string Mensaje { get; set; } = "Registro exitoso";
    }
}
