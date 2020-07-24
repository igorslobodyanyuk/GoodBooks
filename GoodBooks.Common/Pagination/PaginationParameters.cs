using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GoodBooks.Common.Pagination
{
    public class PaginationParameters
    {
        public int Page { get; }
        public int Size { get; }
        public int From => (Page - 1) * Size;

        public PaginationParameters(int page = 1, int size = 10)
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page), $"Page number is expected to be > 0.");

            if (size < 1)
                throw new ArgumentOutOfRangeException(nameof(page), $"Page size is expected to be > 0.");

            Page = page;
            Size = size;
        }

        public async Task<PaginationResult<T>> ApplyPaging<T>(IQueryable<T> queryable)
        {
            var result = await queryable.Skip((Page - 1) * Size).Take(Size).ToListAsync();
            var count = await queryable.CountAsync();

            return new PaginationResult<T>(result, count);
        }
    }
}