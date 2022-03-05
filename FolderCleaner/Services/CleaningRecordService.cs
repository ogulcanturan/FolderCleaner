using FolderCleaner.Worker.DataContext;
using FolderCleaner.Worker.Enums;
using FolderCleaner.Worker.Models;
using FolderCleaner.Worker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Worker.Services
{
    public class CleaningRecordService : ICleaningRecordService
    {
        private readonly Context _context;

        public CleaningRecordService(Context context)
        {
            _context = context;
        }

        public async Task<CleaningRecord> CreateAsync(CleaningRecord cleaningRecord, TriggeredBy triggeredBy, CancellationToken cancellationToken)
        {
            if (cleaningRecord.IsActive)
            {
                cleaningRecord.CleaningHistory = new List<CleaningHistory>
                {
                    new CleaningHistory
                    {
                        RunsAt = cleaningRecord.RunsAt,
                        CleaningStatus = CleaningStatus.Ready,
                        TriggeredBy = triggeredBy,
                    }
                };
            }
            var cleaningRecordEntry = await _context.CleaningRecords.AddAsync(cleaningRecord, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return cleaningRecordEntry.Entity;
        }

        public async Task DeleteByIdAsync(int id, CancellationToken cancellationToken)
        {
            _context.CleaningRecords.Remove(new CleaningRecord { Id = id });
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<CleaningRecord>> GetAllAsync(CancellationToken cancellationToken)
        {
            var cleaners = await _context.CleaningRecords.AsNoTracking().ToListAsync(cancellationToken);
            return cleaners;
        }

        public async Task<CleaningRecord> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var cleaner = await _context.CleaningRecords.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return cleaner;
        }

        public async Task<CleaningRecord> UpdateAsync(CleaningRecord cleaningRecord, CancellationToken cancellationToken)
        {
            var record = await _context.CleaningRecords.Include(nameof(CleaningRecord.CleaningHistory)).FirstOrDefaultAsync(x => x.Id == cleaningRecord.Id, cancellationToken);
            if (!record.IsActive && cleaningRecord.IsActive)
            {
                cleaningRecord.CleaningHistory.Add(new CleaningHistory
                {
                    CleaningStatus = CleaningStatus.Ready
                });
            }
            else
            {
                cleaningRecord.CleaningHistory?.Clear();
            }
            record.IsActive = cleaningRecord.IsActive;
            record.Path = cleaningRecord.Path;
            cleaningRecord.RunsAt = cleaningRecord.RunsAt;
            await _context.SaveChangesAsync(cancellationToken);
            return record;
        }

        public async Task<CleaningRecord> SwitchActivityAsync(int id, CancellationToken cancellationToken)
        {
            var record = await _context.CleaningRecords.Include(nameof(CleaningRecord.CleaningHistory)).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            bool nextStatus = !record.IsActive;
            if (!record.IsActive && nextStatus && (record.Repeat || record.RunsAt >= DateTime.Now))
            {
                var cleaningHistory = new CleaningHistory
                {
                    RunsAt = record.RunsAt,
                    CleaningStatus = CleaningStatus.Ready,
                    TriggeredBy = TriggeredBy.Worker,
                };
                if (record.Repeat)
                {
                    while (DateTime.Now > cleaningHistory.RunsAt)
                    {
                        cleaningHistory.RunsAt = cleaningHistory.RunsAt.AddSeconds(record.RepeatRange.Value);
                    }
                }
                record.CleaningHistory = new List<CleaningHistory>
                {
                    cleaningHistory
                };
            }
            else
            {
                record.CleaningHistory?.Clear();
            }
            record.IsActive = nextStatus;
            await _context.SaveChangesAsync(cancellationToken);
            return record;
        }
    }
}