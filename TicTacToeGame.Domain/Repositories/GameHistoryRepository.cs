using Dapper;
using System.Data;
using System.Data.SqlClient;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Domain.Repositories
{
    public class GamesHistoryRepository : BaseRepository<GamesHistory>
    {
        private readonly IDbConnection _db;
        public GamesHistoryRepository(string connstring) : base(connstring)
        {
            _db = new SqlConnection(connstring);
        }
        public List<GamesHistory> GetAll() => _db.Query<GamesHistory>("Select", commandType: CommandType.StoredProcedure).AsList();
        public GamesHistory? GetById(int id) => _db.QuerySingleOrDefault<GamesHistory>("SelectById", new { Id = id }, commandType: CommandType.StoredProcedure);
        public override void AddEntity(GamesHistory entity)
        {
            base.AddEntity(entity);
        }
        public override void UpdateEntity(GamesHistory entity)
        {
            base.UpdateEntity(entity);
        }
        public override void DeleteEntity(GamesHistory entity)
        {
            base.DeleteEntity(entity);
        }

        public async Task<GamesHistory?> GetGamesHistoryByPlayerId(string playerId)// implement stored procedure
        {
            // handle null
            return await _db.QuerySingleOrDefaultAsync<GamesHistory>("SelectGamesHistoryByPlayerId", new
            {
                PlayerId = playerId
            }, commandType: CommandType.StoredProcedure);
        }
    }
}
