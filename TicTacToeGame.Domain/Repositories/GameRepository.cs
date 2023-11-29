using Dapper;
using System.Data;
using System.Data.SqlClient;
using TicTacToeGame.Domain.Enums;
using TicTacToeGame.Domain.Models;

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
        public Game? GetById(int id) => _db.Query<Game>("SelectById", new { Id = id }, commandType: CommandType.StoredProcedure).FirstOrDefault();
        public Game? GetByUsersId(string userId)
        {
            return policy.Execute(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    Game game = connection.Query<Game>("SelectGameByPlayersId", new { Id = userId }, commandType: CommandType.StoredProcedure).LastOrDefault();

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

        public List<Game>? GetGamesByGamesHistoryId(int id)// Implement
        {
            try
            {

                return _db.Query<Game>("SelectGamesByGamesHistoryId", new
                {
                    GamesHistoryId = id
                }, commandType: CommandType.StoredProcedure).AsList();
            }
            catch (Exception ex)
            {
                // handle error
                throw new Exception(ex.Message);
            }
        }
    }
}
