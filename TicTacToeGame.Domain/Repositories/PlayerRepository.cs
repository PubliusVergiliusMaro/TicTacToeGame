using Dapper;
using System.Data;
using System.Data.SqlClient;
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

        public Player? GetById(string id) {
            return policy.Execute(() =>
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    return connection.Query<Player>("SelectUserById", new { Id = id }, commandType: CommandType.StoredProcedure).ToList().LastOrDefault();

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
