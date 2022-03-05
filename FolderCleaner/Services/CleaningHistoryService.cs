using FolderCleaner.Worker.DataContext;
using FolderCleaner.Worker.Enums;
using FolderCleaner.Worker.Models;
using FolderCleaner.Worker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Worker.Services
{
    public class CleaningHistoryService : ICleaningHistoryService
    {
        private readonly Context _context;
        private readonly IFileService _fileService;

        public CleaningHistoryService(Context context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task StartCleaningAsync(CleaningHistory activeHistory, TriggeredBy triggeredBy, CancellationToken cancellationToken)
        {
            try
            {
                await UpdateAsync(new CleaningHistory
                {
                    Id = activeHistory.Id,
                    CleaningStatus = CleaningStatus.Started,
                    CleaningRecordId = activeHistory.CleaningRecordId,
                    TriggeredBy = triggeredBy,
                    CleaningRecord = activeHistory.CleaningRecord
                }, cancellationToken);
                int? totalFiles = activeHistory.CleaningRecord.TotalFiles;
                long? cleaningSize = activeHistory.CleaningRecord.CleaningSize;
                _fileService.Delete(activeHistory.CleaningRecord.Path);

                await UpdateAsync(new CleaningHistory
                {
                    Id = activeHistory.Id,
                    CleaningStatus = CleaningStatus.Success,
                    CleaningRecordId = activeHistory.CleaningRecordId,
                    TriggeredBy = triggeredBy,
                    CleaningRecord = activeHistory.CleaningRecord,
                    TotalFiles = totalFiles,
                    CleaningSize = cleaningSize
                }, cancellationToken);

                if (activeHistory.CleaningRecord.Repeat)
                {
                    var cleaningHistory = new CleaningHistory
                    {
                        RunsAt = activeHistory.RunsAt.AddSeconds(activeHistory.CleaningRecord.RepeatRange.Value),
                        CleaningStatus = CleaningStatus.Ready,
                        CleaningRecordId = activeHistory.CleaningRecordId,
                        TriggeredBy = triggeredBy,
                    };
                    while (DateTime.Now > cleaningHistory.RunsAt)
                    {
                        cleaningHistory.RunsAt = cleaningHistory.RunsAt.AddSeconds(activeHistory.CleaningRecord.RepeatRange.Value);
                    }
                    await CreateAsync(cleaningHistory, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                var errorModel = new CleaningHistory
                {
                    Id = activeHistory.Id,
                    CleaningStatus = CleaningStatus.Failure,
                    CleaningRecordId = activeHistory.CleaningRecordId,
                    TriggeredBy = triggeredBy,
                    CleaningRecord = activeHistory.CleaningRecord
                };
                errorModel.AppendExtraMessageToStatusDescription(ex.Message);
                errorModel = await UpdateAsync(errorModel, cancellationToken);

                await CreateAsync(new CleaningHistory
                {
                    RunsAt = errorModel.RunsAt,
                    CleaningStatus = CleaningStatus.Ready,
                    CleaningRecordId = activeHistory.CleaningRecordId,
                    TriggeredBy = triggeredBy,
                }, cancellationToken);
            }
        }

        public async Task<CleaningHistory> CreateAsync(CleaningHistory cleanerHistoryModel, CancellationToken cancellationToken)
        {
            var cleaningHistoryEntry = await _context.CleaningHistory.AddAsync(cleanerHistoryModel, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return cleaningHistoryEntry.Entity;
        }

        public async Task<IEnumerable<CleaningHistory>> GetAllAsync(CancellationToken cancellationToken)
        {
            var cleaningHistory = await _context.CleaningHistory.AsNoTracking().Include(nameof(CleaningHistory.CleaningRecord)).Where(x => x.CleaningStatus != CleaningStatus.Ready).OrderByDescending(x => x.CreatedOn).ToListAsync(cancellationToken);
            return cleaningHistory;
        }

        public async Task<IEnumerable<CleaningHistory>> GetActiveTriggeredByWorkerAllAsync(CancellationToken cancellationToken)
        {
            var cleaningHistory = await _context.CleaningHistory.AsNoTracking()?.Include(nameof(CleaningHistory.CleaningRecord))
                .Where(x => x.CleaningRecord.IsActive && x.TriggeredBy == TriggeredBy.Worker).OrderByDescending(x => x.CreatedOn).ToListAsync(cancellationToken);
            return cleaningHistory.GroupBy(x => x.CleaningRecordId).Select(x => x.FirstOrDefault());
        }

        public async Task<IEnumerable<CleaningHistory>> GetActiveStatusReadyRecordsAsync(CancellationToken cancellationToken)
        {
            var cleaningHistory = await _context.CleaningHistory.AsNoTracking()?.Include(nameof(CleaningHistory.CleaningRecord))
                .Where(x => x.CleaningRecord.IsActive && x.CleaningStatus == CleaningStatus.Ready).OrderByDescending(x => x.CreatedOn).ToListAsync(cancellationToken);
            return cleaningHistory.GroupBy(x => x.CleaningRecordId).Select(x => x.FirstOrDefault());
        }


        public async Task<IEnumerable<CleaningHistory>> GetActivePendingRecordsAsync(CancellationToken cancellationToken)
        {
            var cleaningHistory = await _context.CleaningHistory.AsNoTracking()?.Include(nameof(CleaningHistory.CleaningRecord))
                .Where(x => x.CleaningRecord.IsActive && x.CleaningStatus == CleaningStatus.Ready && DateTime.Now >= x.RunsAt).OrderByDescending(x => x.CreatedOn).ToListAsync(cancellationToken);
            return cleaningHistory.GroupBy(x => x.CleaningRecordId).Select(x => x.FirstOrDefault());
        }

        public async Task<CleaningHistory> UpdateAsync(CleaningHistory cleanerHistoryModel, CancellationToken cancellationToken)
        {
            var cleaningHistory = await _context.CleaningHistory.FindAsync(new object[] { cleanerHistoryModel.Id }, cancellationToken);
            if (cleaningHistory != null)
            {
                cleaningHistory.CleaningStatus = cleanerHistoryModel.CleaningStatus;
                cleaningHistory.TriggeredBy = cleanerHistoryModel.TriggeredBy;

                cleaningHistory.CleaningSize = cleanerHistoryModel.CleaningSize;
                cleaningHistory.TotalFiles = cleanerHistoryModel.TotalFiles;
                cleaningHistory.UpdatedOn = DateTime.Now;

                _context.Entry(cleaningHistory).State = EntityState.Modified;
                await _context.SaveChangesAsync(cancellationToken);
            }
            return cleaningHistory;
        }

        public async Task ClearTheHistoryAsync(CancellationToken cancellationToken)
        {
            var cleaningHistory = await _context.CleaningHistory.AsNoTracking()?.Where(x => x.CleaningStatus != CleaningStatus.Ready).ToListAsync(cancellationToken);
            _context.CleaningHistory.RemoveRange(cleaningHistory);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}