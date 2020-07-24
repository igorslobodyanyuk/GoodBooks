using System.ComponentModel.DataAnnotations;

namespace GoodBooks.BusinessLogic.Models
{
    public class ReviewModel
    {
        public int BookId { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public ReviewModel()
        {
        }

        public ReviewModel(int bookId, string text, string email)
        {
            BookId = bookId;
            Text = text;
            Email = email;
        }
    }
}
