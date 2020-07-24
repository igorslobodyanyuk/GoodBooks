using System.Collections.Generic;

namespace GoodBooks.BusinessLogic.Models
{
    public class BookModelExtended : BookModel
    {
        public int BookId { get; set; }
        public IEnumerable<ReviewModel> Reviews { get; set; }

        public BookModelExtended()
        {
        }

        public BookModelExtended(string title, string authors, int bookId, IEnumerable<ReviewModel> reviews = null)
            : base(title, authors)
        {
            BookId = bookId;
            Reviews = reviews;
        }
    }
}
