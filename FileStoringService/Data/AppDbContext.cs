using Microsoft.EntityFrameworkCore;
using FileStoringService.Models;

namespace FileStoringService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts)
    {
    }

    public DbSet<FileRecord> Files { get; set; }
}