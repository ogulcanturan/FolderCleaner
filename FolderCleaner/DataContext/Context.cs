using FolderCleaner.Models;
using Microsoft.EntityFrameworkCore;

namespace FolderCleaner.DataContext
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> opts) : base(opts) { }

        public DbSet<CleanerModel> Cleaners { get; set; }
        public DbSet<CleanerHistoryModel> CleanerHistories { get; set; }
    }
}