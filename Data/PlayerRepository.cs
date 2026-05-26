using FixRushGameAPI.DTOs;
using FixRushGameAPI.Models;
using Microsoft.Data.SqlClient;

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
    }

    public class PlayerRepository : IPlayerRepository
    {
        private readonly string _connectionString;

        public PlayerRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string no configurada.");
            Console.WriteLine($"[DEBUG] ConnectionString: {_connectionString}");
        }

        private SqlConnection NuevaConexion() => new(_connectionString);
       

        public async Task<bool> ExisteCorreo(string correo)
        {
            await using var conn = NuevaConexion();
            await conn.OpenAsync();
            
            await using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Players WHERE correo = @correo", conn);
            cmd.Parameters.AddWithValue("@correo", correo);
            return (int)(await cmd.ExecuteScalarAsync())! > 0;
        }

        public async Task<bool> ExisteNickname(string nickname)
        {
            await using var conn = NuevaConexion();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Players WHERE nickname = @nickname", conn);
            cmd.Parameters.AddWithValue("@nickname", nickname);
            return (int)(await cmd.ExecuteScalarAsync())! > 0;
        }

        public async Task<Player> Crear(Player player)
        {
            const string sql = @"
            INSERT INTO Players
                (nombre, nickname, correo, password_hash, fecha_nacimiento,
                 dinero, nivel, experiencia, creado_en, actualizado_en)
            OUTPUT INSERTED.id, INSERTED.creado_en
            VALUES
                (@nombre, @nickname, @correo, @passwordHash, @fechaNacimiento,
                 0, 1, 0, GETUTCDATE(), GETUTCDATE())";

            await using var conn = NuevaConexion();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nombre", player.Nombre);
            cmd.Parameters.AddWithValue("@nickname", player.Nickname);
            cmd.Parameters.AddWithValue("@correo", player.Correo);
            cmd.Parameters.AddWithValue("@passwordHash", player.PasswordHash);
            cmd.Parameters.AddWithValue("@fechaNacimiento", player.FechaNacimiento.ToDateTime(TimeOnly.MinValue));

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                player.Id = reader.GetInt32(0);
                player.CreadoEn = reader.GetDateTime(1);
            }
            return player;
        }

        public async Task<Player?> ObtenerPorCorreo(string correo)
        {
            const string sql = @"
            SELECT id, nombre, nickname, correo, password_hash,
                   fecha_nacimiento, dinero, nivel, experiencia,
                   creado_en, actualizado_en
            FROM Players WHERE correo = @correo";

            await using var conn = NuevaConexion();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@correo", correo);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            return MapPlayer(reader);
        }

        public async Task<Player?> ObtenerPorId(int id)
        {
            const string sql = @"
            SELECT id, nombre, nickname, correo, password_hash,
                   fecha_nacimiento, dinero, nivel, experiencia,
                   creado_en, actualizado_en
            FROM Players WHERE id = @id";

            await using var conn = NuevaConexion();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            return MapPlayer(reader);
        }

        public async Task<List<ItemDto>> ObtenerItems(int playerId)
        {
            const string sql = @"
            SELECT
                i.id,
                i.nombre,
                i.tipo,
                ig.nombre             AS grupo,
                COALESCE(pi.tiene, 0) AS tiene
            FROM Items i
            LEFT JOIN PlayerItems pi
                ON pi.item_id = i.id AND pi.player_id = @playerId
            LEFT JOIN ItemGroups ig
                ON ig.id = i.group_id
            WHERE i.activo = 1
            ORDER BY i.tipo, i.id";

            await using var conn = NuevaConexion();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@playerId", playerId);

            var items = new List<ItemDto>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new ItemDto
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Tipo = reader.GetString(2),
                    Grupo = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Tiene = reader.GetBoolean(4)
                });
            }
            return items;
        }

        public async Task<bool> ActualizarDinero(int playerId, decimal dinero)
        {
            const string sql = @"
            UPDATE Players
            SET dinero = @dinero, actualizado_en = GETUTCDATE()
            WHERE id = @playerId";

            await using var conn = NuevaConexion();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dinero", dinero);
            cmd.Parameters.AddWithValue("@playerId", playerId);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task ActualizarItem(int playerId, int itemId, bool tiene)
        {
            // MERGE: inserta la fila si no existe, actualiza si ya existe
            const string sql = @"
            MERGE PlayerItems AS target
            USING (SELECT @playerId AS player_id, @itemId AS item_id) AS source
                ON target.player_id = source.player_id
               AND target.item_id   = source.item_id
            WHEN MATCHED THEN
                UPDATE SET tiene = @tiene, actualizado_en = GETUTCDATE()
            WHEN NOT MATCHED THEN
                INSERT (player_id, item_id, tiene, actualizado_en)
                VALUES (@playerId, @itemId, @tiene, GETUTCDATE());";

            await using var conn = NuevaConexion();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@playerId", playerId);
            cmd.Parameters.AddWithValue("@itemId", itemId);
            cmd.Parameters.AddWithValue("@tiene", tiene);
            await cmd.ExecuteNonQueryAsync();
        }

        // ── Mapper privado ───────────────────────────────────────────
        private static Player MapPlayer(SqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Nombre = r.GetString(1),
            Nickname = r.GetString(2),
            Correo = r.GetString(3),
            PasswordHash = r.GetString(4),
            FechaNacimiento = DateOnly.FromDateTime(r.GetDateTime(5)),
            Dinero = r.GetDecimal(6),
            Nivel = r.GetInt32(7),
            Experiencia = r.GetInt32(8),
            CreadoEn = r.GetDateTime(9),
            ActualizadoEn = r.GetDateTime(10)
        };
    }
}
