using Dapper;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Domain.Repositories
{
    public class PlayerRepository : BaseRepository<Player>
    {
        private readonly IDbConnection _db;
        private string _connectionString;

        public PlayerRepository(string connstring) : base(connstring)
        {
            _connectionString = connstring;
            _db = new SqlConnection(connstring);
        }
        public List<Player> GetAll() => _db.Query<Player>("SelectPlayers", commandType: CommandType.StoredProcedure).AsList();

        public Player? GetById(string id)
        {
            try
            {
                return policy.Execute(() =>
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        return connection.Query<Player>("SelectUserById", new { Id = id }, commandType: CommandType.StoredProcedure).ToList().LastOrDefault();
                    }
                });
            }
            catch (SqlException ex)
            {
                // Handle SQL-specific exceptions
                // Log or handle the exception as needed
                Console.WriteLine($"SqlException: {ex.Message}");
                return null; // or throw a custom exception
            }
            catch (TimeoutException ex)
            {
                // Handle timeout exceptions
                Console.WriteLine($"TimeoutException: {ex.Message}");
                return null; // or throw a custom exception
            }
            catch (InvalidOperationException ex)
            {
                // Handle invalid operation exceptions
                Console.WriteLine($"InvalidOperationException: {ex.Message}");
                return null; // or throw a custom exception
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                return null; // or throw a custom exception
            }
        }

        public Player GetCurrentPlayer(Player host, Player guest, ClaimsPrincipal claimsPrincipal)
        {
            string? userId = claimsPrincipal.Claims.FirstOrDefault()?.Value;

            if (userId == null)
            {
                throw new InvalidOperationException("User is not authenticated");
            }

            if (host.Id == userId)
            {
                return host;
            }
            else if (guest.Id == userId)
            {
                return guest;
            }
            else throw new Exception("User is not recognized");
        }

        public void UpdatePlayerGameContextId(string ContextId, Player host, Player guest, ClaimsPrincipal claimsPrincipal)
        {
            Player currentPlayer = GetCurrentPlayer(host, guest, claimsPrincipal);

            currentPlayer.GameConnectionId = ContextId;
            UpdateEntity(currentPlayer);
        }
        public void UpdatePlayerStatus(string playerId, bool isPlaying)
        {
            policy.Execute(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    return connection.Query("UpdatePlayerStatus", new { Id = playerId, IsPlaying = isPlaying }, commandType: CommandType.StoredProcedure);
                }
            });
        }
        public override void AddEntity(Player entity)
        {
            base.AddEntity(entity);
        }
        public override void UpdateEntity(Player entity)
        {
            base.UpdateEntity(entity);
        }
        public override void DeleteEntity(Player entity)
        {
            base.DeleteEntity(entity);
        }
    }
}
