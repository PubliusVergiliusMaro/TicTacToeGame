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
        //public List<Room> GetAll() => _db.Query<Room>("Select", commandType: CommandType.StoredProcedure).AsList();
        //public Room? GetById(int id) => _db.Query<Room>("SelectById", new { Id = id }, commandType: CommandType.StoredProcedure).FirstOrDefault();// maybe check check if null
        public int AddEntity(Room entity)
        {
            try
            {
                int id = _db.Query<int>("InsertRoom", new
                {
                    ConnectionId = entity.ConnectionId,
                    IsOpen = entity.IsOpen,
                    UniqueId = entity.UniqueId,
                    CreatedAt = entity.CreatedAt,
                    UpdatedAt = entity.UpdatedAt,
                    IsDeleted = entity.IsDeleted
                }, commandType: CommandType.StoredProcedure).Single();
                return id;
            }
            catch (SqlException ex)
            {
                // Handle SQL-specific exceptions
                // Log or handle the exception as needed
                Console.WriteLine($"SqlException: {ex.Message}");
                return -1; // or throw a custom exception
            }
            catch (TimeoutException ex)
            {
                // Handle timeout exceptions
                Console.WriteLine($"TimeoutException: {ex.Message}");
                return -1; // or throw a custom exception
            }
            catch (ArgumentNullException ex)
            {
                // Handle invalid operation exceptions
                Console.WriteLine($"ArgumentNullException : {ex.Message}");
                return -1; // or throw a custom exception
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                Console.WriteLine($"Exception: {ex.Message}");
                return -1; // or throw a custom exception
            }

           
        }

        //public override void AddEntity(Room entity)
        //{
        //    base.AddEntity(entity);
        //}
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
