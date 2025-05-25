using Microsoft.EntityFrameworkCore;
using FileAnalysisService.Models;

namespace FileAnalysisService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts)
    {
    }

    public DbSet<AnalysisResult> Analyses { get; set; }
}