using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace GoodBooks.BusinessLogic.Csv
{
    public class CsvParserService : ICsvParserService
    {
        public IEnumerable<TModel> Parse<TModel, TMap>(Stream fileStream) where TMap : ClassMap<TModel>
        {
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Configuration.HasHeaderRecord = true;
            csv.Configuration.Delimiter = ",";
            csv.Configuration.RegisterClassMap<TMap>();

            fileStream.Position = 0;
            var records = csv.GetRecords<TModel>().ToList();

            return records;
        }
    }
}