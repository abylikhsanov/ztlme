using System.Buffers.Text;

namespace ztlme.Models;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public string? SignedBlob { get; set; } = string.Empty;
}