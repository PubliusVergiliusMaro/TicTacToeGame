using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToeGame.Domain.Models;

namespace TicTacToeGame.Domain.Repositories
{
    public class RoomRepository : BaseRepository<Room>
    {
        private readonly IDbConnection _db;
        public RoomRepository(string connstring) : base(connstring)
        {
            _db = new SqlConnection(connstring);
        }
        public List<Room> GetAll() => _db.Query<Room>("Select", commandType: CommandType.StoredProcedure).AsList();
        public Room? GetById(int id) => _db.Query<Room>("SelectById", new { Id = id }, commandType: CommandType.StoredProcedure).FirstOrDefault();
        public override void AddEntity(Room entity)
        {
            base.AddEntity(entity);
        }
        public override void UpdateEntity(Room entity)
        {
            base.UpdateEntity(entity);
        }
        public override void DeleteEntity(Room entity)
        {
            base.DeleteEntity(entity);
        }
    }
}
