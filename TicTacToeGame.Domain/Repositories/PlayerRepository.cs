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
            //try cath
            return policy.Execute(() =>
            {
                // check if here needs this using
                using (var connection = new SqlConnection(_connectionString))
                {
                    return connection.Query<Player>("SelectUserById", new { Id = id }, commandType: CommandType.StoredProcedure).ToList().LastOrDefault();
                }
            });
        }
        public void UpdatePlayerGameContextId(string ContextId, Player host, Player guest, ClaimsPrincipal claimsPrincipal)
        {
            string? userId = claimsPrincipal.Claims.FirstOrDefault().Value.ToString();

            if (userId == null)
            {
                throw new Exception("User is not authenticated");
            }

            if (host.Id == userId)
            {
                host.GameConnectionId = ContextId;
                UpdateEntity(host);
            }
            else if (guest.Id == userId)
            {
                guest.GameConnectionId = ContextId;
                UpdateEntity(guest);
            }
            else
            {
                return;
            }
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
