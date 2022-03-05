using FolderCleaner.Worker.Enums;
using FolderCleaner.Worker.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Worker.BackgroundServices
{
    public class CleaningHostedService : IHostedService, IDisposable
    {
        private readonly ICleaningHistoryService _cleaningHistoryService;
        private Timer _timer;
        private int _cleaningHostedServicePeriod = 60;
        public CleaningHostedService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _cleaningHistoryService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ICleaningHistoryService>();
            var section = configuration.GetSection("CleaningHostedServicePeriod");
            if (section.Exists())
                int.TryParse(section.Value, out _cleaningHostedServicePeriod);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var activeHistoryList = await _cleaningHistoryService.GetActiveTriggeredByWorkerAllAsync(cancellationToken);
            foreach(var activeHistory in activeHistoryList)
            {
                var isReady = activeHistory.CleaningStatus == CleaningStatus.Ready;
                
                if (!isReady && (activeHistory.CleaningRecord.Repeat || activeHistory.CleaningStatus != CleaningStatus.Success))
                { // Is status isn't ready and (record is repeatable or (is'nt success (covers failure and started status)))
                    await _cleaningHistoryService.CreateAsync(new Models.CleaningHistory
                    {
                        RunsAt = activeHistory.RunsAt,
                        CleaningStatus = CleaningStatus.Ready,
                        CleaningRecordId = activeHistory.CleaningRecordId,
                        TriggeredBy = TriggeredBy.Worker
                    }, cancellationToken);
                }
            }
            var startOn = TimeSpan.FromSeconds(5);
            var period = TimeSpan.FromSeconds(_cleaningHostedServicePeriod);
            _timer = new Timer(Work, null, startOn, period);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Work(object state)
        {
            var activeHistoryList = _cleaningHistoryService.GetActivePendingRecordsAsync(default).Result;
            foreach (var activeHistory in activeHistoryList)
            {
                _cleaningHistoryService.StartCleaningAsync(activeHistory, TriggeredBy.Worker, default).Wait();
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}