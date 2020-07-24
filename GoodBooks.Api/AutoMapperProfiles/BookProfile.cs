using AutoMapper;
using GoodBooks.BusinessLogic.Models;
using GoodBooks.Common.Extensions;
using GoodBooks.Data.Model.Models;

namespace GoodBooks.Api.AutoMapperProfiles
{
    public class BookProfile : Profile
    {
        public BookProfile()
        {
            AllowNullCollections = true;

            // Return empty collections as nulls in order to omit them in returned book models.
            CreateMap<Book, BookModelExtended>()
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews.IsEmpty() ? null : src.Reviews));
            CreateMap<BookModelExtended, Book>();
            CreateMap<BookModel, Book>();
        }
    }
}
