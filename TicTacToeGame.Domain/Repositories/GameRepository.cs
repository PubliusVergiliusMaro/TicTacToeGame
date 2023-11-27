using Dapper;
using System.Data;
using System.Data.SqlClient;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Domain.Repositories
{
    public class GameRepository : BaseRepository<Game>
    {
        private readonly IDbConnection _db;
        public GameRepository(string connstring) : base(connstring)
        {
            _db = new SqlConnection(connstring);
        }
        public List<Game> GetAll() => _db.Query<Game>("Select", commandType: CommandType.StoredProcedure).AsList();
        public Game? GetById(int id) => _db.Query<Game>("SelectById", new { Id = id }, commandType: CommandType.StoredProcedure).FirstOrDefault();
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
    }
}
