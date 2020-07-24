namespace GoodBooks.BusinessLogic.Models
{
    public class BookModel
    {
        public string Title { get; set; }
        public string Authors { get; set; }

        public BookModel()
        {
        }
        public BookModel(string title, string authors)
        {
            Title = title;
            Authors = authors;
        }
    }
}