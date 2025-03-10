using AutoMapper;
using Luxa.Models;
using Luxa.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace Luxa.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile() 
        {
            CreateMap<CreateUserVM, UserModel>();
            CreateMap<UserModel, EditUserVM>()
           .ForMember(editUser => editUser.Roles, opt => opt.MapFrom((src, dest, destMember, context) =>
                context.Items.TryGetValue("Roles", out object? value) ? value : new List<string>()));
        }
    }
}
