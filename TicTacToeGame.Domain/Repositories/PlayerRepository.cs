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

        public List<Player> GetAllNotActive()
        {
            return policy.Execute(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    return connection.Query<Player>("SelectNotActiveUsers", commandType: CommandType.StoredProcedure).AsList();
                }
            });
        }
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
        public Player? GetByUserName(string userName)
        {
            return policy.Execute(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    return connection.Query<Player>("SelectUserByUserName", new { UserName = userName }, commandType: CommandType.StoredProcedure).ToList().LastOrDefault();
                }
            });
        }
        public void UpdateCurrentPlayerGameContextId(string ContextId, Player currentPlayer)
        {
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
        public bool CheckIfTwoPlayersArePlaying(string hostId, string guestId)
        {
            return policy.Execute(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    List<Player> players = connection.Query<Player>("GetHostAndGuestPlayer", new { HostId = hostId, GuestId = guestId }, commandType: CommandType.StoredProcedure).ToList();

                    bool areBothPlaying = players.All(r => r.IsPlaying == true);
                    return areBothPlaying;
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
