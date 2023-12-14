using Polly;
using System.Data;
using System.Data.SqlClient;
using Z.Dapper.Plus;

namespace TicTacToeGame.Domain.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        // check for polly configuration
        protected readonly Policy policy = Policy.Handle<SqlException>(e => e.Number == -2) // Only retry on SQL timeout exceptions
        .Or<TimeoutException>() // Also retry on general timeout exceptions
        .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            // Only retry if the operation is not user-initiated
            if (context.ContainsKey("UserInitiated") && (bool)context["UserInitiated"])
            {
                throw new Exception("Operation failed", exception);
            }
        });
        private readonly IDbConnection _db;
        private string _connectionString;
        public BaseRepository(string connstring)
        {
            _connectionString = connstring;
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
            policy.Execute(() =>
            {
                // check if here needs this using
                using (var db = new SqlConnection(_connectionString))
                {
                    db.Open();
                    db.BulkInsert(entity);
                    db.Close();
                }
            });
        }
        public virtual void UpdateEntity(T entity)
        {
            policy.Execute(() =>
            {
                // check if here needs this using
                using (var db = new SqlConnection(_connectionString))
                {
                    db.Open();
                    db.BulkUpdate(entity);
                    db.Close();
                }
            });
        }
        public virtual void UpdateEntitys(ICollection<T> entities)
        {
            _db.Open();
            _db.BulkUpdate(entities);
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
