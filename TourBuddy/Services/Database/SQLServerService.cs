using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourBuddy.Models;


namespace TourBuddy.Services.Database
{
   public  class SQLServerService : ISQLServerService
    {
        private readonly DatabaseSettings _settings;
        private string _connectionString;

        public SQLServerService(DatabaseSettings settings)
        {
            _settings = settings;
            _connectionString = _settings.ServerConnectionString;
        }

        public Task InitializeConnectionAsync()
        {
            // Verify connection
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                return Task.CompletedTask;
            }
        }

        public async Task<int> InsertUserAsync(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
                    INSERT INTO Users (Username, Email, PasswordHash, CreatedAt) 
                    VALUES (@Username, @Email, @PasswordHash, @CreatedAt);
                    SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
                    UPDATE Users 
                    SET Username = @Username, 
                        Email = @Email, 
                        PasswordHash = @PasswordHash,
                        ResetToken = @ResetToken,
                        ResetTokenExpiry = @ResetTokenExpiry
                    WHERE Id = @Id";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@ResetToken", (object)user.ResetToken ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ResetTokenExpiry", (object)user.ResetTokenExpiry ?? DBNull.Value);

                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> DeleteUserAsync(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = "DELETE FROM Users WHERE Id = @Id";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", userId);
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = "SELECT * FROM Users WHERE Id = @Id";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapUserFromReader(reader);
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = "SELECT * FROM Users WHERE Email = @Email";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapUserFromReader(reader);
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = "SELECT * FROM Users WHERE Username = @Username";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapUserFromReader(reader);
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = new List<User>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = "SELECT * FROM Users";

                using (var command = new SqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(MapUserFromReader(reader));
                        }
                    }
                }
            }

            return users;
        }

        public async Task<List<User>> GetUsersModifiedSinceAsync(DateTime lastSyncTime)
        {
            List<User> users = new List<User>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = @"
                    SELECT * FROM Users 
                    WHERE CreatedAt > @LastSyncTime 
                    OR LastModifiedAt > @LastSyncTime";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@LastSyncTime", lastSyncTime);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add(MapUserFromReader(reader));
                        }
                    }
                }
            }

            return users;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0;
                }
            }
        }

        private User MapUserFromReader(SqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                ResetToken = reader.IsDBNull(reader.GetOrdinal("ResetToken")) ? null : reader.GetString(reader.GetOrdinal("ResetToken")),
                ResetTokenExpiry = reader.IsDBNull(reader.GetOrdinal("ResetTokenExpiry")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ResetTokenExpiry")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                LastSyncedAt = DateTime.UtcNow,
                IsSynced = true
            };
        }
    }
}
