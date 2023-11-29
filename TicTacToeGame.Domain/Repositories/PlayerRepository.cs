using Dapper;
using System.Data;
using System.Data.SqlClient;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Domain.Repositories
{
    public class PlayerRepository : BaseRepository<Player>
    {
        private readonly IDbConnection _db;
        public PlayerRepository(string connstring) : base(connstring)
        {
            _db = new SqlConnection(connstring);
        }
        public List<Player> GetAll() => _db.Query<Player>("SelectPlayers", commandType: CommandType.StoredProcedure).AsList();
        
        public Player? GetById(string id) => _db.Query<Player>("SelectUserById", new { Id = id }, commandType: CommandType.StoredProcedure).LastOrDefault();
        
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
