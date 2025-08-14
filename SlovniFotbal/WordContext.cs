using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SlovniFotbal.Models;

namespace SlovniFotbal;

public class WordContext : DbContext
{
    public DbSet<WordModel> Words { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=words.db");
    }

    public static async Task InitializeDatabaseAsync()
    {
        await using var context = new WordContext();
        await context.Database.EnsureCreatedAsync();

        // check if there are any words in the database
        if (await context.Words.AnyAsync())
            return;

        // check if the word list exists
        if (!File.Exists("word-list.txt"))
            throw new FileNotFoundException("Missing word list file.");

        // create buffer for loading the words
        const int batchSize = 1000;
        var buffer = new List<WordModel>(batchSize);

        // goes through every line from word lists and adds it to database
        await foreach (var line in File.ReadLinesAsync("word-list.txt"))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            buffer.Add(new WordModel { Word = line.Trim().ToLower() });

            if (buffer.Count >= batchSize)
            {
                await context.Words.AddRangeAsync(buffer);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
                buffer.Clear();
            }
        }

        // Save remaining words
        if (buffer.Count > 0)
        {
            await context.Words.AddRangeAsync(buffer);
            await context.SaveChangesAsync();
        }
    }
}