using FolderCleaner.Worker.Models;
using Microsoft.EntityFrameworkCore;

namespace FolderCleaner.Worker.DataContext
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> opts) : base(opts) { }

        public DbSet<CleaningRecord> CleaningRecords { get; set; }
        public DbSet<CleaningHistory> CleaningHistory { get; set; }
    }
}