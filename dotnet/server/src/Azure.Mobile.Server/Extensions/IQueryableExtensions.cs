using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Azure.Mobile.Server.Extensions
{
    internal static class IQueryableExtensions
    {
        /// <summary>
        /// If Soft-Delete is enabled, and the query does not include __includeDeleted, then add
        /// a clause to the query to exclude deleted records.  If soft-delete is not enabled, or
        /// the query includes __includeDeleted, then all records are returned.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="query">An <see cref="IQueryable{T}"/> that is to be adjusted</param>
        /// <param name="tableOptions">The table options for the table controller</param>
        /// <param name="request">The <see cref="HttpRequest"/> for the current request</param>
        /// <returns></returns>
        public static IQueryable<T> ApplyDeletedFilter<T>(this IQueryable<T> query, TableControllerOptions<T> tableOptions, HttpRequest request) where T : class, ITableData
        {
            const string IncludeDeletedParameter = "__includedeleted";
            var includeDeleted = request.Query.Any(t => t.Key.ToLowerInvariant() == IncludeDeletedParameter);

            if (tableOptions.SoftDeleteEnabled && !includeDeleted)
            {
                return query.Where(entity => !entity.Deleted);
            }
            return query;
        }
    }
}
