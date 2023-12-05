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

        public async Task<GamesHistory?> GetGamesHistoryByPlayerId(string playerId)
        {
            try
            {
                if (playerId == null)
                {
                    // Handle the null case
                    return null;
                }

                return await _db.QuerySingleOrDefaultAsync<GamesHistory>("SelectGamesHistoryByPlayerId", new
                {
                    PlayerId = playerId
                }, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                // Handle SQL-related exceptions
                Console.WriteLine($"SQL Exception: {ex.Message}");
                return null;
            }
            catch (ArgumentNullException ex)
            {
                // Handle ArgumentNullException
                Console.WriteLine($"Argument Null Exception: {ex.Message}");
                return null;
            }
            catch (InvalidOperationException ex)
            {
                // Handle InvalidOperationException
                Console.WriteLine($"Invalid Operation Exception: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

    }
}
