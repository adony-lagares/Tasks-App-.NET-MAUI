using SQLite;

namespace Tasks.Service
{
    public class DatabaseService<T> where T : new()
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<T>().Wait();
        }

        public Task<int> IncludeAsync(T item)
        {
            return _database.InsertAsync(item);
        }

        public Task<int> DeleteAsync(T item)
        {
            return _database.DeleteAsync(item);
        }

        public Task<int> ChangeAsync(T item)
        {
            return _database.UpdateAsync(item);
        }

        public Task<List<T>> AllAsync()
        {
            return _database.Table<T>().ToListAsync();
        }

        public AsyncTableQuery<T> Query()
        {
            return _database.Table<T>();
        }

        public Task<int> QuantityAsync(T item)
        {
            return _database.Table<T>().CountAsync();
        }
    }
}