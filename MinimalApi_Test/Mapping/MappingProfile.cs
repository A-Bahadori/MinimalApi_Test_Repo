using AutoMapper;
using MinimalApi_Test.DTOs.User;
using MinimalApi_Test.Entities.User;

namespace MinimalApi_Test.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) =>
                        srcMember != null &&
                        (srcMember is not string || !string.IsNullOrWhiteSpace(srcMember.ToString()))));
            CreateMap<PatchUserDto, User>().ReverseMap();
        }
    }

}
