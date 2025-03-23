using IdentityService.Dtos;
using IdentityService.Entities;

namespace IdentityService.Extensions
{
    public static class UserExtension
    {
        public static UserDto AsDto(this ApplicationUser user)
        {
            return new UserDto(
                user.Id,
                user.UserName,
                user.Email,
                user.Gil,
                user.CreatedOn);
        }
    }
}
