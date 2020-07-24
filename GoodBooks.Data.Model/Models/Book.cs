using System.Collections.Generic;

namespace GoodBooks.Data.Model.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}