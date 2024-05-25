namespace ztlme.Models;

public class UserClaimSummary
{
    public int Id { get; set; }
    public double UserClaimTotal { get; set; }
    public double ClaimCurrentYear { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}