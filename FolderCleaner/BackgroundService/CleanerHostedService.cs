using FolderCleaner.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.BackgroundService
{
    public class CleanerHostedService : IHostedService
    {
        private readonly IFileService _fileService;
        private readonly ICleanerHistoryService _cleanerHistoryService;
        private Timer _timer;
        public CleanerHostedService(IFileService fileService, IServiceProvider serviceProvider)
        {
            _fileService = fileService;
            _cleanerHistoryService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ICleanerHistoryService>();
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var activeHistoryList = await _cleanerHistoryService.GetActiveAllAsync(cancellationToken);
            foreach(var activeHistory in activeHistoryList)
            {
                var isReady = activeHistory.CleanerStatus == Models.CleanerStatus.Ready;
                if (!isReady)
                {
                    await _cleanerHistoryService.CreateAsync(new Models.CleanerHistoryModel
                    {
                        CleanerStatus = Models.CleanerStatus.Ready,
                        CleanerId = activeHistory.CleanerId,
                    }, cancellationToken);
                }
            }
            var startOn = TimeSpan.FromSeconds(5);
            var period = TimeSpan.FromMinutes(1);
            _timer = new Timer(Work, null, startOn, period);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Work(object state)
        {
            var activeHistoryList = _cleanerHistoryService.GetActiveAllAsync(default).Result;
            foreach (var activeHistory in activeHistoryList.Where(x => x.CleanerStatus == Models.CleanerStatus.Ready && x.CleanerModel.WorksAt.ToString("HH:mm") == DateTime.Now.ToString("HH:mm")))
            {
                _cleanerHistoryService.CreateAsync(new Models.CleanerHistoryModel
                {
                    CleanerStatus = Models.CleanerStatus.Started,
                    CleanerId = activeHistory.CleanerId,
                }, default).Wait();

                try
                {
                    _fileService.Delete(activeHistory.CleanerModel.Path);

                    _cleanerHistoryService.CreateAsync(new Models.CleanerHistoryModel
                    {
                        CleanerStatus = Models.CleanerStatus.Finished,
                        CleanerId = activeHistory.CleanerId,
                    }, default).Wait();
                }
                catch
                {
                    _cleanerHistoryService.CreateAsync(new Models.CleanerHistoryModel
                    {
                        CleanerStatus = Models.CleanerStatus.Failed,
                        CleanerId = activeHistory.CleanerId,
                    }, default).Wait();
                }
                finally
                {
                    _cleanerHistoryService.CreateAsync(new Models.CleanerHistoryModel
                    {
                        CleanerStatus = Models.CleanerStatus.Ready,
                        CleanerId = activeHistory.CleanerId,
                    }, default).Wait();
                }
            }
        }
    }
}
