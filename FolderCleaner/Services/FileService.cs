using FolderCleaner.Services.Interfaces;
using System.IO;

namespace FolderCleaner.Services
{
    public class FileService : IFileService
    {
        public void Delete(string path)
        {
            var fileAttributes = File.GetAttributes(path);

            if (fileAttributes.HasFlag(FileAttributes.Directory))
            {
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
            else
            {
                File.Delete(path);
            }
        }
    }
}