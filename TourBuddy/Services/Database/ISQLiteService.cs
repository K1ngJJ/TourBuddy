using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TourBuddy.Models;

namespace TourBuddy.Services.Database
{
   public interface ISQLiteService
    {
        Task InitializeDatabaseAsync();
        Task<SQLiteAsyncConnection> GetConnectionAsync();
        Task<int> InsertAsync<T>(T item) where T : new();
        Task<int> UpdateAsync<T>(T item) where T : new();
        Task<int> DeleteAsync<T>(T item) where T : new();
        Task<List<T>> GetAllAsync<T>() where T : new();
        Task<T> GetByIdAsync<T>(int id) where T : new();
        Task<T> GetAsync<T>(Expression<Func<T, bool>> predicate) where T : new();
        Task<List<T>> GetManyAsync<T>(Expression<Func<T, bool>> predicate) where T : new();
        Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate) where T : new();
    }
}

