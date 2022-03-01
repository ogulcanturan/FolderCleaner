using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FolderCleaner.Services.Interfaces
{
    public interface IFileService
    {
        void Delete(string path);
    }
}
