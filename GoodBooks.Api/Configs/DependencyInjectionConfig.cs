using GoodBooks.BusinessLogic.Csv;
using GoodBooks.BusinessLogic.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GoodBooks.Api.Configs
{
    public class DependencyInjectionConfig
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<ICsvParserService, CsvParserService>();
        }
    }
}