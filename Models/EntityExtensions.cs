using ztlme.Dtos;

namespace ztlme.Models;

public static class EntityExtensions
{
    public static GetUserDto ToGetUserDto(this User user)
    {
        return new GetUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }

    public static AddUserLandingResDto ToAddUserLandingResDto(this User user)
    {
        return new AddUserLandingResDto
        {
            CanBeSignedUp = user.CreditScoreOk
        };
    }

    public static User FromAddUserLandingNoCheck(this AddUserLandingNoCheckDto landingNoCheckDto)
    {
        return new User
        {
            UserName = landingNoCheckDto.PersonalNumber,
            FirstName = landingNoCheckDto.FirstName,
            LastName = landingNoCheckDto.LastName,
            Email = landingNoCheckDto.Email
        };
    }
}