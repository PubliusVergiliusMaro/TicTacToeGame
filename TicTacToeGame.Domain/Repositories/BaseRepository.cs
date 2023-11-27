using System.Data;
using System.Data.SqlClient;
using Z.Dapper.Plus;

namespace TicTacToeGame.Domain.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        private readonly IDbConnection _db;
        public BaseRepository(string connstring)
        {
            _db = new SqlConnection(connstring);
        }
        
        public virtual void AddEntities(ICollection<T> entities)
        {
            _db.Open();
            _db.BulkInsert(entities);
            _db.Close();
        }
        public virtual void AddEntity(T entity)
        {
            _db.Open();
            _db.BulkInsert(entity);
            _db.Close();
        }
        public virtual void UpdateEntity(T entity)
        {
            _db.Open();
            _db.BulkUpdate(entity);
            _db.Close();
        }
        public virtual void DeleteEntity(T entity)
        {
            _db.Open();
            _db.BulkDelete(entity);
            _db.Close();
        }
        public virtual void DeleteEntities(ICollection<T> entities)
        {
            _db.Open();
            _db.BulkDelete(entities);
            _db.Close();
        }
    }
}
