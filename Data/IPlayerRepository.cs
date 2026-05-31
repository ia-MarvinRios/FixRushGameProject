using FixRushGameAPI.DTOs;
using FixRushGameAPI.Models;

namespace FixRushGameAPI.Data
{
    public interface IPlayerRepository
    {
        Task<bool> ExisteCorreo(string correo);
        Task<bool> ExisteNickname(string nickname);
        Task<Player> Crear(Player player);
        Task<Player?> ObtenerPorCorreo(string correo);
        Task<Player?> ObtenerPorId(int id);
        Task<List<ItemDto>> ObtenerItems(int playerId);
        Task<bool> ActualizarDinero(int playerId, decimal dinero);
        Task ActualizarItem(int playerId, int itemId, bool tiene);
        Task<bool> ActualizarNivel(int playerId, int nivel, int experiencia);
        Task<bool> ActualizarEquip(int playerId, int? cuerpoId, int? gorroId);
    }
}
