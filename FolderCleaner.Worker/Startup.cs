﻿using FolderCleaner.Worker.BackgroundServices;
using FolderCleaner.Worker.DataContext;
using FolderCleaner.Worker.Extensions;
using FolderCleaner.Worker.Services;
using FolderCleaner.Worker.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace FolderCleaner.Worker.Worker
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            SetDatabase(services);
            SetServices(services);
            services.AddControllersWithViews();
            services.AddDirectoryBrowser();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider("C:\\"),
                RequestPath = "/C:",
                Formatter = new MyDirectoryFormatter(),
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
        public void SetDatabase(IServiceCollection services)
        {
            services.AddDbContextPool<Context>(s => s.UseSqlite("Data Source=foldercleaner.db"));
        }
        public void SetServices(IServiceCollection services)
        {
            services.AddScoped<ICleaningHistoryService, CleaningHistoryService>();
            services.AddScoped<ICleaningRecordService, CleaningRecordService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddHostedService<CleaningHostedService>();
        }
    }
}