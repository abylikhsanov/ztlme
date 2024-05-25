using System.ComponentModel.DataAnnotations;

namespace ztlme.Models;

public class User
{
    public int Id { get; set; }
    [Required]
    [StringLength(11), MinLength(11)]
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public EmailAddressAttribute? Email { get; set; }
    public bool CreditScoreOk { get; set; } = false;
    public string? SignedBlob { get; set; } = string.Empty;
    public ICollection<Contribution> Contributions { get; } = new List<Contribution>();
    public ICollection<Claim> Claims { get; } = new List<Claim>();
    public UserContributionSummary? UserContributionSummary { get; set; }
    public UserClaimSummary? UserClaimSummary { get; set; }
}