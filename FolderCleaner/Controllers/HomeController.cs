using FolderCleaner.Models;
using FolderCleaner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FolderCleaner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICleanerService _cleanerService;
        private readonly ICleanerHistoryService _cleanerHistoryService;
        private readonly IFileService _fileService;
        public HomeController(ICleanerService cleanerService, ICleanerHistoryService cleanerHistoryService, IFileService fileService)
        {
            _cleanerService = cleanerService;
            _cleanerHistoryService = cleanerHistoryService;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var list = await _cleanerService.GetAllAsync(cancellationToken);
            return View(list);
        }

        public async Task<IActionResult> History(CancellationToken cancellationToken)
        {
            
            var allHistory = await _cleanerHistoryService.GetAllAsync(cancellationToken);
            var statusReadyHistories = await _cleanerHistoryService.GetActiveStatusReadyRecordsAsync(cancellationToken);
            return View(new List<IEnumerable<CleanerHistoryModel>> { statusReadyHistories, allHistory });
        }

        [HttpPost]
        public async Task<IActionResult> AddCleaner(CleanerModel cleanerModel, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                await _cleanerService.CreateAsync(cleanerModel, cancellationToken);
                TempData["SuccessMessage"] = "Success!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCleaner(int id, CancellationToken cancellationToken)
        {
            await _cleanerService.DeleteByIdAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Success!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> SwitchCleaner(int id, CancellationToken cancellationToken)
        {
            await _cleanerService.SwitchActivityAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Success!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ManualCleanUp(int id, CancellationToken cancellationToken)
        {
            var entity = await _cleanerService.GetByIdAsync(id, cancellationToken);
            if(entity != null)
            {
                try
                {
                    _fileService.Delete(entity.Path);
                    TempData["SuccessMessage"] = "Success!";
                }
                catch(Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}