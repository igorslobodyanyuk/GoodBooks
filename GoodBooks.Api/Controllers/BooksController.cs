using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GoodBooks.BusinessLogic.Models;
using GoodBooks.BusinessLogic.Services;
using GoodBooks.Common.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoodBooks.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService bookService;

        public BooksController(IBookService bookService)
        {
            this.bookService = bookService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationParameters>> Get(int pageNumber = 1, int pageSize = 10, string searchQuery = null)
        {
            var books = await bookService.FindBooks(new PaginationParameters(pageNumber, pageSize), searchQuery);

            return Ok(books);
        }
        
        [HttpGet("get-by-id")]
        public async Task<ActionResult<BookModelExtended>> GetById(int id)
        {
            var book = await bookService.GetBook(id);

            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<BookModelExtended>> Post(BookModel bookModel)
        {
            var book = await bookService.CreateBook(bookModel);

            return CreatedAtAction(nameof(Get), new { id = book.BookId }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, BookModel bookModel)
        {
            await bookService.UpdateBook(id, bookModel);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<BookModelExtended>> Delete(int id)
        {
            return await bookService.DeleteBook(id);
        }

        [HttpPost]
        [Route("bulk-add")]
        public async Task<IActionResult> BulkAdd(IFormFile booksCsv)
        {
            await using var fileContentStream = new MemoryStream();
            await booksCsv.CopyToAsync(fileContentStream);

            await bookService.BulkAdd(fileContentStream);

            return Ok();
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetLibrary()
        {
            await bookService.ResetLibrary();

            return Ok();
        }
    }
}
