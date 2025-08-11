using Microsoft.EntityFrameworkCore;
using SlovniFotbal.Models;

namespace SlovniFotbal;

public class WordContext : DbContext
{
    public DbSet<WordModel> Words { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=words.db");
    }
    
}