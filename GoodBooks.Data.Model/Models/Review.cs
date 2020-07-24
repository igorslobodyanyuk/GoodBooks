using System.Collections.Generic;

namespace GoodBooks.Data.Model.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int BookId { get; set; }
        public string Text { get; set; }
        public string Email { get; set; }

        public Book Book { get; set; }
    }
}