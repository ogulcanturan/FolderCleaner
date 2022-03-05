using FolderCleaner.Worker.Services.Interfaces;
using System.IO;
using System.Linq;

namespace FolderCleaner.Worker.Services
{
    public class FileService : IFileService
    {
        public void Delete(string path)
        {
            if (IsPathExists(path, out bool isDirectory))
            {
                if (isDirectory)
                {
                    var directoryInfo = new DirectoryInfo(path);
                    foreach(var file in directoryInfo.EnumerateFiles())
                        file.Delete();
                    foreach (var directory in directoryInfo.EnumerateDirectories())
                        directory.Delete();

                    //Directory.Delete(path, true);
                    //Directory.CreateDirectory(path);
                }
                else
                {
                    File.Delete(path);
                }
            }
        }

        public static bool IsPathExists(string path, out bool isDirectory)
        {
            isDirectory = false;
            if ((!File.Exists(path)) && (!Directory.Exists(path)))
                return false;

            isDirectory = File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            return true;
        }

        public static int? TotalFiles(string path)
        {
            if (IsPathExists(path, out bool isDirectory))
            {
                if (isDirectory)
                    return Directory.GetFileSystemEntries(path).Length;

                return 1;
            }
            return null;
        }

        public static long? GetSize(string path)
        {
            if (IsPathExists(path, out bool isDirectory))
                return isDirectory ? new DirectoryInfo(path).EnumerateFiles().Sum(x => x.Length) : new System.IO.FileInfo(path).Length;
            return null;
        }
    }
}