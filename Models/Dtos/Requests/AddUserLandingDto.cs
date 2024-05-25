using System.ComponentModel.DataAnnotations;

namespace ztlme.Dtos;

public class AddUserLandingDto
{
    [Required]
    [StringLength(11, MinimumLength = 11)]
    public string PersonalNumber { get; set; } = string.Empty;
}