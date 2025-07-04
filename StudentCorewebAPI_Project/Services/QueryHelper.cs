//using Microsoft.EntityFrameworkCore;
//using StudentCorewebAPI_Project.Models;
//using System.Linq.Expressions;
//using System.Linq;

//public static class QueryHelper
//{
//    public static async Task<PagedResponse<T>> ApplyPagination<T>(
//        IQueryable<T> query,
//        PaginationParams paginationParams,
//        Expression<Func<T, bool>>? filter = null)
//    {
//        if (filter != null)
//        {
//            query = query.Where(filter);
//        }

//        if (!string.IsNullOrEmpty(paginationParams.SearchQuery))
//        {
//            string searchTerm = paginationParams.SearchQuery.ToLower();
//            query = query.Where(x => x.ToString().ToLower().Contains(searchTerm));
//        }

//        if (!string.IsNullOrEmpty(paginationParams.SortBy))
//        {
//            query = paginationParams.IsDescending
//                ? query.OrderByDescending(e => EF.Property<object>(e, paginationParams.SortBy))
//                : query.OrderBy(e => EF.Property<object>(e, paginationParams.SortBy));
//        }

//        int totalRecords = await query.CountAsync();
//        List<T> items = await query
//            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
//            .Take(paginationParams.PageSize)
//            .ToListAsync();

//        return new PagedResponse<T>(items, paginationParams.PageNumber, paginationParams.PageSize, totalRecords);
//    }
//}


using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public static class QueryHelper
{
    public static async Task<PagedResponse<T>> ApplyPagination<T>(
        IQueryable<T> query,
        PaginationParams paginationParams,
        Expression<Func<T, bool>>? filter = null)
    {
        // Apply any extra filter first (optional)
        if (filter != null)
        {
            query = query.Where(filter);
        }

        // Apply search (if provided)
        if (!string.IsNullOrWhiteSpace(paginationParams.SearchQuery))
        {
            query = ApplySearch(query, paginationParams.SearchQuery);
        }

        // Apply filters from Filters list (if any)
        if (paginationParams.Filters != null && paginationParams.Filters.Any())
        {
            query = ApplyFilters(query, paginationParams.Filters);
        }

        // Apply sorting if specified
        if (!string.IsNullOrEmpty(paginationParams.SortBy))
        {
            query = ApplySorting(query, paginationParams.SortBy, paginationParams.IsDescending);
        }
        else
        {
            // Default sorting by Id if available
            var prop = typeof(T).GetProperty("Id");
            if (prop != null)
            {
                query = query.OrderBy(e => EF.Property<object>(e, "Id"));
            }
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PagedResponse<T>(items, paginationParams.PageNumber, paginationParams.PageSize, totalRecords);
    }

    private static IQueryable<T> ApplySearch<T>(IQueryable<T> query, string searchTerm)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var stringProperties = typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(string)).ToList();

        if (!stringProperties.Any()) return query;

        Expression? orExpression = null;
        var searchTermLower = searchTerm.ToLower();
        var searchConstant = Expression.Constant(searchTermLower);

        foreach (var property in stringProperties)
        {
            var propertyAccess = Expression.Property(parameter, property);
            var toLowerCall = Expression.Call(propertyAccess, nameof(string.ToLower), Type.EmptyTypes);
            var containsCall = Expression.Call(toLowerCall, nameof(string.Contains), null, searchConstant);

            orExpression = orExpression == null
                ? containsCall
                : Expression.OrElse(orExpression, containsCall);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(orExpression!, parameter);
        return query.Where(lambda);
    }

    private static IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<FilterCriteria> filters)
    {
        if (filters == null || !filters.Any())
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedExpression = null;

        foreach (var filter in filters)
        {
            try
            {
                var propertyNames = filter.ColumnName.Split('.');
                Expression property = parameter;
                foreach (var propName in propertyNames)
                {
                    property = Expression.PropertyOrField(property, propName);
                }

                var filterExpression = BuildFilterExpression(property, filter);
                combinedExpression = combinedExpression == null
                    ? filterExpression
                    : Expression.AndAlso(combinedExpression, filterExpression);
            }
            catch
            {
                // Skip invalid filters silently or log
                continue;
            }
        }

        return combinedExpression == null
            ? query
            : query.Where(Expression.Lambda<Func<T, bool>>(combinedExpression, parameter));
    }

    private static Expression BuildFilterExpression(Expression property, FilterCriteria filter)
    {
        if (property.Type == typeof(string))
        {
            var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
            var lowerProperty = Expression.Call(property, toLowerMethod);
            var lowerValue = Expression.Constant(filter.Value.ToLower());

            return filter.Condition.ToLower() switch
            {
                "contains" => Expression.Call(lowerProperty, nameof(string.Contains), null, lowerValue),
                "notcontains" => Expression.Not(Expression.Call(lowerProperty, nameof(string.Contains), null, lowerValue)),
                "equals" => Expression.Equal(lowerProperty, lowerValue),
                "notequals" => Expression.NotEqual(lowerProperty, lowerValue),
                "startswith" => Expression.Call(lowerProperty, nameof(string.StartsWith), null, lowerValue),
                "endswith" => Expression.Call(lowerProperty, nameof(string.EndsWith), null, lowerValue),
                _ => throw new ArgumentException($"Unsupported condition for string: {filter.Condition}")
            };
        }

        // Handle nullable and other value types
        var underlyingType = Nullable.GetUnderlyingType(property.Type) ?? property.Type;
        var convertedValue = Convert.ChangeType(filter.Value, underlyingType);
        var constant = Expression.Constant(convertedValue, property.Type);

        return filter.Condition.ToLower() switch
        {
            "equals" => Expression.Equal(property, constant),
            "notequals" => Expression.NotEqual(property, constant),
            "greaterthan" => Expression.GreaterThan(property, constant),
            "lessthan" => Expression.LessThan(property, constant),
            "greaterthanorequal" => Expression.GreaterThanOrEqual(property, constant),
            "lessthanorequal" => Expression.LessThanOrEqual(property, constant),
            _ => throw new ArgumentException($"Unsupported condition: {filter.Condition}")
        };
    }

    private static IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortBy, bool isDescending)
    {
        return isDescending
            ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
            : query.OrderBy(e => EF.Property<object>(e, sortBy));
    }
}
