using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TourBuddy.Helpers;
using TourBuddy.Models;
using SQLite;

namespace TourBuddy.Services.Database
{
   public  class SQLiteService : ISQLiteService
    {
        private SQLiteAsyncConnection _database;
        private readonly DatabaseSettings _settings;

        public SQLiteService(DatabaseSettings settings)
        {
            _settings = settings;
        }

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_database == null)
            {
                await InitializeDatabaseAsync();
            }
            return _database;
        }

        public async Task InitializeDatabaseAsync()
        {
            if (_database != null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, _settings.DatabaseFilename);
            _database = new SQLiteAsyncConnection(databasePath);

            // Create tables
            await _database.CreateTableAsync<User>();
        }

        public async Task<int> InsertAsync<T>(T item) where T : new()
        {
            await InitializeDatabaseAsync();
            return await _database.InsertAsync(item);
        }

        public async Task<int> UpdateAsync<T>(T item) where T : new()
        {
            await InitializeDatabaseAsync();
            return await _database.UpdateAsync(item);
        }

        public async Task<int> DeleteAsync<T>(T item) where T : new()
        {
            await InitializeDatabaseAsync();
            return await _database.DeleteAsync(item);
        }

        public async Task<List<T>> GetAllAsync<T>() where T : new()
        {
            await InitializeDatabaseAsync();
            return await _database.Table<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync<T>(int id) where T : new()
        {
            await InitializeDatabaseAsync();
            return await _database.FindAsync<T>(id);
        }

        public async Task<T> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            await InitializeDatabaseAsync();
            return await _database.Table<T>().Where(predicate).FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetManyAsync<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            await InitializeDatabaseAsync();
            return await _database.Table<T>().Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            await InitializeDatabaseAsync();
            return await _database.Table<T>().Where(predicate).CountAsync() > 0;
        }
    }
}