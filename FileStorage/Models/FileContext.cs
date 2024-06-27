using Microsoft.EntityFrameworkCore;
namespace FileStorage.Models;

public class FileContext: DbContext
{
    public FileContext(DbContextOptions<FileContext> options) : base(options)
    {
    }

    public DbSet<File> Documents { get; set; }
    
}