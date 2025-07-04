
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.Data;
using StudentCorewebAPI_Project.Repository_Interface;
using System.Linq.Expressions;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<PagedResponse<T>> GetAllAsync(PaginationParams paginationParams, Expression<Func<T, bool>>? filter = null)
    {
        var query = _dbSet.AsQueryable();
        return await QueryHelper.ApplyPagination(query, paginationParams, filter);
    }
}