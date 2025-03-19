using StudentCorewebAPI_Project.Models;
using System.Linq.Expressions;

namespace StudentCorewebAPI_Project.Repository_Interface
{
    public interface IGenericRepository<T> where T : class
    {
        Task<PagedResponse<T>> GetAllAsync(PaginationParams paginationParams, Expression<Func<T, bool>>? filter = null);
    }

}
