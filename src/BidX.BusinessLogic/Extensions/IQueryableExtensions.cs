namespace BidX.BusinessLogic.Extensions;

public static class IQueryableExtensions
    {
        /// <summary>
        /// Extension method to paginate an IQueryable.
        /// </summary>
        public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
    }