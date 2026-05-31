using FixRushGameAPI.Data;
using FixRushGameAPI.DTOs;
using FixRushGameAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace FixRushGameAPI.Services
{
    public interface IAuthService
    {
        Task<(bool success, string message, RegisterResponse? data)> Registrar(RegisterRequest request);
        Task<(bool success, string message, LoginResponse? data)> Login(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly IPlayerRepository _repo;

        public AuthService(IPlayerRepository repo) => _repo = repo;

        public async Task<(bool success, string message, RegisterResponse? data)> Registrar(RegisterRequest req)
        {
            if (await _repo.ExisteCorreo(req.Correo))
                return (false, "El correo ya esta registrado.", null);

            if (await _repo.ExisteNickname(req.Nickname))
                return (false, "El nickname ya esta en uso.", null);

            var player = new Player
            {
                Nombre = req.Nombre.Trim(),
                Nickname = req.Nickname.Trim(),
                Correo = req.Correo.ToLower().Trim(),
                PasswordHash = HashPassword(req.Password),
                FechaNacimiento = req.FechaNacimiento
            };

            var creado = await _repo.Crear(player);

            return (true, "Registro exitoso.", new RegisterResponse
            {
                PlayerId = creado.Id,
                Nickname = creado.Nickname,
                Mensaje = "Registro exitoso"
            });
        }

        public async Task<(bool success, string message, LoginResponse? data)> Login(LoginRequest req)
        {
            var player = await _repo.ObtenerPorCorreo(req.Correo.ToLower().Trim());

            if (player is null || !VerificarPassword(req.Password, player.PasswordHash))
                return (false, "Correo o contrasena incorrectos.", null);

            var items = await _repo.ObtenerItems(player.Id);

            return (true, "Login exitoso.", new LoginResponse
            {
                PlayerId = player.Id,
                Nickname = player.Nickname,
                Nombre = player.Nombre,
                Dinero = player.Dinero,
                Nivel = player.Nivel,
                Experiencia = player.Experiencia,
                CosmeticoCuerpoId = player.CosmeticoCuerpoId,  
                CosmeticoGorroId = player.CosmeticoGorroId,  
                Items = items
                
            });
        }

        // ── Hash helpers ─────────────────────────────────────────────
        private static string HashPassword(string password)
        {
            var salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(salt + password)));
            return $"{salt}:{hash}";
        }

        private static bool VerificarPassword(string password, string stored)
        {
            var parts = stored.Split(':');
            if (parts.Length != 2) return false;
            var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(parts[0] + password)));
            return hash == parts[1];
        }
    }
}
