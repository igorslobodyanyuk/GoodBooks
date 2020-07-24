using GoodBooks.Common.Pagination;
using Profile = AutoMapper.Profile;

namespace GoodBooks.Api.AutoMapperProfiles
{
    public class CommonProfile : Profile
    {
        public CommonProfile()
        {
            CreateMap(typeof(PaginationResult<>), typeof(PaginationResult<>));
        }
    }
}