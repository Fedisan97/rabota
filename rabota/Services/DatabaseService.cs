using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using rabota.Models;
using rabota.ViewModels;

namespace rabota.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    private NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);


    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        const string sql = @"
            SELECT user_id, username, password_hash, role, full_name
            FROM users 
            WHERE username = @username";

        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("username", username);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                UserId = reader.GetInt32(0),
                Username = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Role = reader.GetString(3),
                FullName = reader.GetString(4)
            };
        }
        return null;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await GetUserByUsernameAsync(username);
        if (user == null) return null;
        return user.PasswordHash == password ? user : null;
    }

    public async Task<bool> RegisterUserAsync(string username, string password, string role, string fullName)
    {
        var existing = await GetUserByUsernameAsync(username);
        if (existing != null) return false;

        using var connection = CreateConnection();
        await connection.OpenAsync();
        const string sql = @"
            INSERT INTO users (username, password_hash, role, full_name)
            VALUES (@username, @password, @role, @fullName)";

        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("password", password);
        cmd.Parameters.AddWithValue("role", role);
        cmd.Parameters.AddWithValue("fullName", fullName);

        int affected = await cmd.ExecuteNonQueryAsync();
        return affected > 0;
    }

    public async Task<List<SessionViewModel>> GetSessionsForPosterAsync(
        string? genreFilter = null,
        DateTime? dateFilter = null,
        string? searchTitle = null,
        string? sortBy = null)
    {
        var result = new List<SessionViewModel>();
        using var connection = CreateConnection();
        await connection.OpenAsync();

        var sql = @"
            SELECT 
                s.session_id,
                f.title,
                f.genre,
                f.duration,
                s.start_time,
                h.name,
                COALESCE(p.price_value, 0) as price
            FROM sessions s
            JOIN films f ON s.film_id = f.film_id
            JOIN halls h ON s.hall_id = h.hall_id
            LEFT JOIN prices p ON s.session_id = p.session_id
            WHERE 1=1";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(genreFilter))
        {
            conditions.Add("f.genre = @genre");
            parameters["@genre"] = genreFilter;
        }

        if (dateFilter.HasValue)
        {
            conditions.Add("s.start_time >= @dateStart AND s.start_time < @dateEnd");
            parameters["@dateStart"] = dateFilter.Value.Date;
            parameters["@dateEnd"] = dateFilter.Value.Date.AddDays(1);
        }

        if (!string.IsNullOrEmpty(searchTitle))
        {
            conditions.Add("f.title ILIKE @search");
            parameters["@search"] = $"%{searchTitle}%";
        }

        if (conditions.Count > 0)
            sql += " AND " + string.Join(" AND ", conditions);

        if (sortBy == "price")
            sql += " ORDER BY price";
        else
            sql += " ORDER BY s.start_time";

        using var cmd = new NpgsqlCommand(sql, connection);
        foreach (var p in parameters)
            cmd.Parameters.AddWithValue(p.Key, p.Value);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new SessionViewModel
            {
                SessionId = reader.GetInt32(0),
                FilmTitle = reader.GetString(1),
                Genre = reader.IsDBNull(2) ? "Не указан" : reader.GetString(2),
                Duration = reader.GetInt32(3),
                StartTime = reader.GetDateTime(4),
                HallName = reader.GetString(5),
                Price = reader.GetDecimal(6)
            });
        }
        return result;
    }

    public async Task<List<string>> GetGenresAsync()
    {
        var genres = new List<string>();
        using var connection = CreateConnection();
        await connection.OpenAsync();
        const string sql = "SELECT DISTINCT genre FROM films WHERE genre IS NOT NULL AND genre != ''";
        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            genres.Add(reader.GetString(0));
        return genres;
    }

    public async Task<(int hallId, string hallName, int capacity, DateTime startTime, string filmTitle)> GetSessionInfoAsync(int sessionId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        const string sql = @"
            SELECT s.hall_id, h.name, h.capacity, s.start_time, f.title
            FROM sessions s
            JOIN halls h ON s.hall_id = h.hall_id
            JOIN films f ON s.film_id = f.film_id
            WHERE s.session_id = @sessionId";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("sessionId", sessionId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetDateTime(3),
                reader.GetString(4)
            );
        }
        throw new Exception("Сеанс не найден");
    }

    public async Task<List<(int row, int number)>> GetOccupiedSeatsAsync(int sessionId)
    {
        var occupied = new List<(int, int)>();
        using var connection = CreateConnection();
        await connection.OpenAsync();
        const string sql = @"
            SELECT seat_row, seat_number
            FROM tickets
            WHERE session_id = @sessionId AND is_sold = true";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("sessionId", sessionId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            occupied.Add((reader.GetInt32(0), reader.GetInt32(1)));
        }
        return occupied;
    }

    public async Task<List<Viewer>> FindViewersAsync(string search)
    {
        var result = new List<Viewer>();
        using var connection = CreateConnection();
        await connection.OpenAsync();
        const string sql = @"
            SELECT viewer_id, full_name, phone
            FROM viewers
            WHERE full_name ILIKE @search OR phone ILIKE @search
            LIMIT 20";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("search", $"%{search}%");
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Viewer
            {
                ViewerId = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Phone = reader.IsDBNull(2) ? null : reader.GetString(2)
            });
        }
        return result;
    }

    public async Task<int> CreateViewerAsync(string fullName, string? phone)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        const string sql = @"
            INSERT INTO viewers (full_name, phone)
            VALUES (@fullName, @phone)
            RETURNING viewer_id";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("fullName", fullName);
        cmd.Parameters.AddWithValue("phone", phone ?? (object)DBNull.Value);
        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<(bool success, string error)> SellTicketAsync(int sessionId, int viewerId, int row, int number, decimal price, int userId)
    {
        using var connection = CreateConnection();
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            const string checkSessionSql = "SELECT start_time FROM sessions WHERE session_id = @sessionId";
            using var cmdSession = new NpgsqlCommand(checkSessionSql, connection, transaction);
            cmdSession.Parameters.AddWithValue("sessionId", sessionId);
            var startTime = (DateTime?)await cmdSession.ExecuteScalarAsync();
            if (startTime == null)
                return (false, "Сеанс не найден");
            if (startTime < DateTime.Now)
                return (false, "Сеанс уже прошёл");

            const string checkSeatSql = @"
                SELECT ticket_id FROM tickets 
                WHERE session_id = @sessionId AND seat_row = @row AND seat_number = @number AND is_sold = true";
            using var cmdSeat = new NpgsqlCommand(checkSeatSql, connection, transaction);
            cmdSeat.Parameters.AddWithValue("sessionId", sessionId);
            cmdSeat.Parameters.AddWithValue("row", row);
            cmdSeat.Parameters.AddWithValue("number", number);
            var existing = await cmdSeat.ExecuteScalarAsync();
            if (existing != null)
                return (false, "Место уже занято");

            const string checkViewerSql = @"
                SELECT t.ticket_id 
                FROM tickets t
                JOIN sessions s ON t.session_id = s.session_id
                WHERE t.viewer_id = @viewerId 
                  AND s.film_id = (SELECT film_id FROM sessions WHERE session_id = @sessionId)
                LIMIT 1";
            using var cmdViewer = new NpgsqlCommand(checkViewerSql, connection, transaction);
            cmdViewer.Parameters.AddWithValue("viewerId", viewerId);
            cmdViewer.Parameters.AddWithValue("sessionId", sessionId);
            var viewerTicket = await cmdViewer.ExecuteScalarAsync();
            if (viewerTicket != null)
                return (false, "Зритель уже покупал билет на этот фильм");

            const string insertTicketSql = @"
                INSERT INTO tickets (session_id, viewer_id, seat_row, seat_number, price, is_sold, qr_code)
                VALUES (@sessionId, @viewerId, @row, @number, @price, true, @qrCode)
                RETURNING ticket_id";
            using var cmdTicket = new NpgsqlCommand(insertTicketSql, connection, transaction);
            cmdTicket.Parameters.AddWithValue("sessionId", sessionId);
            cmdTicket.Parameters.AddWithValue("viewerId", viewerId);
            cmdTicket.Parameters.AddWithValue("row", row);
            cmdTicket.Parameters.AddWithValue("number", number);
            cmdTicket.Parameters.AddWithValue("price", price);
            var qrCode = Guid.NewGuid().ToString();
            cmdTicket.Parameters.AddWithValue("qrCode", qrCode);
            var ticketId = (int)await cmdTicket.ExecuteScalarAsync();

            const string insertSaleSql = @"
                INSERT INTO sales (ticket_id, user_id, sale_time, total_amount)
                VALUES (@ticketId, @userId, now(), @price)";
            using var cmdSale = new NpgsqlCommand(insertSaleSql, connection, transaction);
            cmdSale.Parameters.AddWithValue("ticketId", ticketId);
            cmdSale.Parameters.AddWithValue("userId", userId);
            cmdSale.Parameters.AddWithValue("price", price);
            await cmdSale.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            return (true, "");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Ошибка: {ex.Message}");
        }
    }
}