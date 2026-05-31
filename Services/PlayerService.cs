using FixRushGameAPI.Data;

namespace FixRushGameAPI.Services
{
    public interface IPlayerService
    {
        Task<(bool success, string message, decimal? dinero)> ActualizarDinero(int playerId, decimal dinero);
        Task<(bool success, string message)> ActualizarItem(int playerId, int itemId, bool tiene);
        Task<(bool success, string message)> ActualizarNivel(int playerId, int nivel, int experiencia);
        
        Task<(bool success, string message)> ActualizarEquip(int playerId, int? cuerpoId, int? gorroId);
    }

    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _repo;

        public PlayerService(IPlayerRepository repo) => _repo = repo;

        public async Task<(bool success, string message, decimal? dinero)> ActualizarDinero(int playerId, decimal dinero)
        {
            var existe = await _repo.ObtenerPorId(playerId);
            if (existe is null)
                return (false, "Jugador no encontrado.", null);

            await _repo.ActualizarDinero(playerId, dinero);
            return (true, "Dinero actualizado.", dinero);
        }

        public async Task<(bool success, string message)> ActualizarItem(int playerId, int itemId, bool tiene)
        {
            var existe = await _repo.ObtenerPorId(playerId);
            if (existe is null)
                return (false, "Jugador no encontrado.");

            await _repo.ActualizarItem(playerId, itemId, tiene);
            return (true, "Item actualizado.");

        }

        public async Task<(bool success, string message)> ActualizarNivel(int playerId, int nivel, int experiencia)
        {
            var existe = await _repo.ObtenerPorId(playerId);
            if (existe is null)
                return (false, "Jugador no encontrado.");

            await _repo.ActualizarNivel(playerId, nivel, experiencia);
            return (true, "Nivel actualizado.");
        }

        public async Task<(bool success, string message)> ActualizarEquip(int playerId, int? cuerpoId, int? gorroId)
        {
            if (!cuerpoId.HasValue && !gorroId.HasValue)
                return (false, "Debes enviar al menos un cosmetico para equipar.");

            var existe = await _repo.ObtenerPorId(playerId);
            if (existe is null)
                return (false, "Jugador no encontrado.");

            await _repo.ActualizarEquip(playerId, cuerpoId, gorroId);
            return (true, "Cosmeticos equipados correctamente.");
        }
    }
}
