using System.ComponentModel.DataAnnotations.Schema;

namespace ztlme.Models;

public class Contribution
{
    public int Id { get; set; }
    public double Amount { get; set; }
    public DateTime ContributionDate { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}