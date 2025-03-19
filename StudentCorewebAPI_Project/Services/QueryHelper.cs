using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.Models;
using System.Linq.Expressions;
using System.Linq;

public static class QueryHelper
{
    public static async Task<PagedResponse<T>> ApplyPagination<T>(
        IQueryable<T> query,
        PaginationParams paginationParams,
        Expression<Func<T, bool>>? filter = null)
    {
        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (!string.IsNullOrEmpty(paginationParams.SearchQuery))
        {
            string searchTerm = paginationParams.SearchQuery.ToLower();
            query = query.Where(x => x.ToString().ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(paginationParams.SortBy))
        {
            query = paginationParams.IsDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, paginationParams.SortBy))
                : query.OrderBy(e => EF.Property<object>(e, paginationParams.SortBy));
        }

        int totalRecords = await query.CountAsync();
        List<T> items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResponse<T>(items, paginationParams.PageNumber, paginationParams.PageSize, totalRecords);
    }
}
