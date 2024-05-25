namespace ztlme.Models;

public class UserContributionSummary
{
    public int Id { get; set; }
    public double TotalContribution { get; set; }
    public double ContributionCurrentYear { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}