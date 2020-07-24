using System.Collections.Generic;
using System.IO;
using CsvHelper.Configuration;

namespace GoodBooks.BusinessLogic.Csv
{
    public interface ICsvParserService
    {
        IEnumerable<TModel> Parse<TModel, TMap>(Stream fileStream) where TMap : ClassMap<TModel>;
    }
}