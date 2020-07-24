using System.Globalization;
using CsvHelper.Configuration;
using GoodBooks.BusinessLogic.Models;

namespace GoodBooks.BusinessLogic.Csv
{
    public sealed class BookModelMap : ClassMap<BookModel>
    {
        public BookModelMap()
        {
            AutoMap(CultureInfo.InvariantCulture);
            Map(m => m.Title).Name("title");
            Map(m => m.Authors).Name("authors");
        }
    }
}