using System.ComponentModel.DataAnnotations;

namespace ztlme.Dtos;

public class AddUserLandingNoCheckDto
{
    [Required]
    [StringLength(11, MinimumLength = 11)]
    public string PersonalNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public required EmailAddressAttribute Email { get; set; }
}