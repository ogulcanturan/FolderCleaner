using FolderCleaner.Worker.DataContext;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FolderCleaner.Worker.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHostBuilder = CreateWebHostBuilder(args).Build();
            GenerateDbIfNotExists(webHostBuilder);
            webHostBuilder.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static void GenerateDbIfNotExists(IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var context = serviceProvider.GetRequiredService<Context>();
                context.Database.Migrate(); // Applies any pending migrations and also updates.
            }
        }
    }
}