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
        private static int numberOfActiveJobs = 0;
        private const int maxNumberOfActiveJobs = 1;
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
            _timer = new Timer(async x => await WorkAsync(x), null, startOn, period);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public async Task WorkAsync(object state)
        {
            if (numberOfActiveJobs < maxNumberOfActiveJobs)
            {
                try
                {
                    Interlocked.Increment(ref numberOfActiveJobs);
                    await _cleaningHistoryService.StartCleaningAsync(TriggeredBy.Worker, default).ConfigureAwait(false);
                }
                finally
                {
                    Interlocked.Decrement(ref numberOfActiveJobs);
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}