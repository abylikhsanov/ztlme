using Microsoft.EntityFrameworkCore;
using ztlme.Models;

namespace ztlme.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        Console.WriteLine($"The connection string is: {options.ToString()}");

    }

    public DbSet<User> Users { get; set; }
}