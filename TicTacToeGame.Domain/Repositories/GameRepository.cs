﻿using Dapper;
using System.Data;
using System.Data.SqlClient;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace TicTacToeGame.Domain.Repositories
{
    public class GameRepository : BaseRepository<Game>
    {
        private readonly IDbConnection _db;
        private string _connectionString;
        public GameRepository(string connstring) : base(connstring)
        {
            _connectionString = connstring;
            _db = new SqlConnection(connstring);
        }
        public List<Game> GetAll() => _db.Query<Game>("Select", commandType: CommandType.StoredProcedure).AsList();
        public Game? GetById(int id)
        {

            return policy.Execute(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    return connection.Query<Game>("GetGameById", new { GameId = id }, commandType: CommandType.StoredProcedure).FirstOrDefault();
                }
            });
        }
        public List<Game> GetEmptyGames()
        {
            return policy.Execute(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    return connection.Query<Game>("SelectEmptyGames", commandType: CommandType.StoredProcedure).AsList();
                }
            });
        }
        public List<Game> GetOldNotClosedGamesByPlayerId(string playerId)
        {
            return policy.Execute(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    return connection.Query<Game>("SelectOldNotClosedGamesByPlayerId", new { PlayerId = playerId }, commandType: CommandType.StoredProcedure).AsList();
                }
            });
        }
        public void UpdateGameResult(int gameId, GameState gameResult)
        {
            policy.Execute(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Query("UpdateGameResult", new { Id = gameId, GameResult = gameResult }, commandType: CommandType.StoredProcedure);
                }
            });
        }
        public Game? GetByUserId(string userId)
        {
            try
            {
                return policy.Execute(() =>
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        Game game = connection.Query<Game>("SelectGameByPlayersId", new { Id = userId }, commandType: CommandType.StoredProcedure).FirstOrDefault();

                        if (game != null)
                        {
                            return game;
                        }
                        else
                        {
                            // handle error or maybe return error message to the user using SignalR
                            return null;
                        }
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
            catch (ArgumentNullException ex)
            {
                // Handle invalid operation exceptions
                Console.WriteLine($"ArgumentNullException : {ex.Message}");
                return null; // or throw a custom exception
            }
            catch (InvalidOperationException ex)
            {
                // Handle invalid operation exceptions
                Console.WriteLine($"InvalidOperationException: {ex.Message}");
                return null; // or throw a custom exception
            }
        }
        public List<Game> GetGameHistoryInSession(GameState gameResult, string hostId, string guestId, int? roomId)
        {

            try
            {
                return policy.Execute(() =>
                {
                    using (var connection = new SqlConnection(_connectionString))
                    {
                        return connection.Query<Game>("GetGamesByResultAndPlayers", new { GameResult = gameResult, HostId = hostId, GuestId = guestId, RoomId = roomId },
                        commandType: CommandType.StoredProcedure).ToList();
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
            catch (ArgumentNullException ex)
            {
                // Handle invalid operation exceptions
                Console.WriteLine($"ArgumentNullException : {ex.Message}");
                return null; // or throw a custom exception
            }
            catch (InvalidOperationException ex)
            {
                // Handle invalid operation exceptions
                Console.WriteLine($"InvalidOperationException: {ex.Message}");
                return null; // or throw a custom exception
            }
        }
        public override void AddEntity(Game entity)
        {
            base.AddEntity(entity);
        }
        public override void UpdateEntity(Game entity)
        {
            base.UpdateEntity(entity);
        }
        public override void DeleteEntity(Game entity)
        {
            base.DeleteEntity(entity);
        }

        public List<Game>? GetGamesByGamesHistoryId(int id)
        {
            try
            {
                if (id == null)
                {
                    throw new ArgumentNullException(nameof(id), "id cannot be null.");
                }

                return _db.Query<Game>("SelectGamesByGamesHistoryId", new
                {
                    GamesHistoryId = id
                }, commandType: CommandType.StoredProcedure).AsList();
            }
            catch (SqlException ex)
            {
                // Handle SQL-specific exceptions
                // Log or handle the exception as needed
                Console.WriteLine($"SqlException: {ex.Message}");
                return null; // or throw a custom exception
            }
            catch (ArgumentNullException ex)
            {
                // Handle invalid operation exceptions
                Console.WriteLine($"ArgumentNullException : {ex.Message}");
                return null; // or throw a custom exception
            }
            catch (InvalidOperationException ex)
            {
                // Handle invalid operation exceptions
                Console.WriteLine($"InvalidOperationException: {ex.Message}");
                return null; // or throw a custom exception
            }
        }

    }
}
