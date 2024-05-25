namespace ztlme.Models;

public class Claim
{
    public int Id { get; set; }
    public double Amount { get; set; }
    public DateTime ClaimDate { get; set; }
    public string ClaimFilesUploadUrl { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}