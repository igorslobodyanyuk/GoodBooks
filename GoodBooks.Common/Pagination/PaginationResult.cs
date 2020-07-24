using System.Collections.Generic;

namespace GoodBooks.Common.Pagination
{
    public class PaginationResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public long TotalCount { get; set; }

        public PaginationResult()
        {
            Items = new List<T>();
        }

        public PaginationResult(IEnumerable<T> items, long totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }
    }
}