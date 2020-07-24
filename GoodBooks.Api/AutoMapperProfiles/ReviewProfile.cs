using AutoMapper;
using GoodBooks.BusinessLogic.Models;
using GoodBooks.Data.Model.Models;

namespace GoodBooks.Api.AutoMapperProfiles
{
    public class ReviewProfile : Profile
    {
        public ReviewProfile()
        {
            CreateMap<Review, ReviewModel>().ReverseMap();
        }
    }
}
