using Microsoft.EntityFrameworkCore;
using ztlme.Models;

namespace ztlme.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<Contribution> Contributions { get; set; }
    public DbSet<Pool> Pools { get; set; }
    public DbSet<UserClaimSummary> UserClaimSummaries { get; set; }
    public DbSet<UserContributionSummary> UserContributionSummaries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}