using Microsoft.EntityFrameworkCore;
using WhatsAppCampaignManager.DTOs;
using System.Linq.Expressions;

namespace WhatsAppCampaignManager.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PaginatedResponse<T>> ToPaginatedResponseAsync<T>(
            this IQueryable<T> query,
            PaginationRequest request)
        {
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResponse<T>
            {
                Data = items,
                Pagination = new PaginationMetadata
                {
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasPrevious = request.Page > 1,
                    HasNext = request.Page < totalPages,
                    Search = request.Search,
                    SortBy = request.SortBy,
                    SortDirection = request.SortDirection
                }
            };
        }

        public static IQueryable<T> ApplySearch<T>(
            this IQueryable<T> query,
            string? searchTerm,
            params Expression<Func<T, string>>[] searchProperties)
        {
            if (string.IsNullOrWhiteSpace(searchTerm) || !searchProperties.Any())
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combinedExpression = null;

            foreach (var property in searchProperties)
            {
                var propertyAccess = Expression.Invoke(property, parameter);
                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                var propertyToLower = Expression.Call(propertyAccess, toLowerMethod!);
                var searchToLower = Expression.Constant(searchTerm.ToLower());
                var containsExpression = Expression.Call(propertyToLower, containsMethod!, searchToLower);

                combinedExpression = combinedExpression == null
                    ? containsExpression
                    : Expression.OrElse(combinedExpression, containsExpression);
            }

            if (combinedExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        public static IQueryable<T> ApplySort<T>(
            this IQueryable<T> query,
            string? sortBy,
            string? sortDirection)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = typeof(T).GetProperty(sortBy);

            if (property == null)
                return query;

            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            var methodName = sortDirection?.ToLower() == "asc" ? "OrderBy" : "OrderByDescending";
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), property.PropertyType);

            return (IQueryable<T>)method.Invoke(null, new object[] { query, lambda })!;
        }
    }
}
