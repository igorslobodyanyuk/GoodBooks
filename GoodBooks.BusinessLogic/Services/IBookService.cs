using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GoodBooks.BusinessLogic.Models;
using GoodBooks.Common.Pagination;

namespace GoodBooks.BusinessLogic.Services
{
    public interface IBookService
    {
        Task<PaginationResult<BookModelExtended>> FindBooks(PaginationParameters paginationParameters,
            string searchQuery = null);
        Task<BookModelExtended> GetBook(int id);
        
        Task<BookModelExtended> CreateBook(BookModel bookModel);
        Task<BookModelExtended> UpdateBook(int id, BookModel bookModel);
        Task<BookModelExtended> DeleteBook(int id);
        
        Task BulkAdd(Stream booksStream);
        Task ResetLibrary();
    }
}