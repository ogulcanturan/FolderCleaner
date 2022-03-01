using FolderCleaner.DataContext;
using FolderCleaner.Models;
using FolderCleaner.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Services
{
    public class CleanerService : ICleanerService
    {
        private readonly Context _context;

        public CleanerService(Context context)
        {
            _context = context;
        }

        public async Task<CleanerModel> CreateAsync(CleanerModel cleanerModel, CancellationToken cancellationToken)
        {
            if (cleanerModel.IsActive)
            {
                cleanerModel.CleanerHistories = new List<CleanerHistoryModel>
                {
                    new CleanerHistoryModel
                    {
                        CleanerStatus = CleanerStatus.Ready
                    }
                };
            }
            var cleanerEntry = await _context.Cleaners.AddAsync(cleanerModel, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return cleanerEntry.Entity;
        }

        public async Task DeleteByIdAsync(int id, CancellationToken cancellationToken)
        {
            _context.Cleaners.Remove(new CleanerModel { Id = id });
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<CleanerModel>> GetAllAsync(CancellationToken cancellationToken)
        {
            var cleaners = await _context.Cleaners.AsNoTracking().ToListAsync(cancellationToken);
            return cleaners;
        }

        public async Task<CleanerModel> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var cleaner = await _context.Cleaners.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            return cleaner;
        }

        public async Task<CleanerModel> UpdateAsync(CleanerModel cleanerModel, CancellationToken cancellationToken)
        {
            var cleaner = await _context.Cleaners.Include("CleanerHistories").FirstOrDefaultAsync(x => x.Id == cleanerModel.Id, cancellationToken);
            if (!cleaner.IsActive && cleanerModel.IsActive)
            {
                cleanerModel.CleanerHistories.Add(new CleanerHistoryModel
                {
                    CleanerStatus = CleanerStatus.Ready
                });
            }
            else
            {
                cleanerModel.CleanerHistories?.Clear();
            }
            cleaner.IsActive = cleanerModel.IsActive;
            cleaner.Path = cleanerModel.Path;
            cleanerModel.WorksAt = cleanerModel.WorksAt;
            await _context.SaveChangesAsync(cancellationToken);
            return cleaner;
        }

        public async Task<CleanerModel> SwitchActivityAsync(int id, CancellationToken cancellationToken)
        {
            var cleaner = await _context.Cleaners.Include("CleanerHistories").FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            bool nextStatus = !cleaner.IsActive;
            if (!cleaner.IsActive && nextStatus)
            {
                cleaner.CleanerHistories = new List<CleanerHistoryModel>
                {
                    new CleanerHistoryModel
                    {
                        CleanerStatus = CleanerStatus.Ready
                    }
                };
            }
            else
            {
                cleaner.CleanerHistories?.Clear();
            }
            cleaner.IsActive = nextStatus;
            await _context.SaveChangesAsync(cancellationToken);
            return cleaner;
        }
    }
}