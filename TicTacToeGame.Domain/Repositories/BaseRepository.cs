using Polly;
using System.Data;
using System.Data.SqlClient;
using Z.Dapper.Plus;

namespace TicTacToeGame.Domain.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        protected readonly Policy policy = Policy.Handle<SqlException>()
            .WaitAndRetry(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

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
            try
            {
                _db.Open();
                _db.BulkInsert(entity);
                _db.Close();
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
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
