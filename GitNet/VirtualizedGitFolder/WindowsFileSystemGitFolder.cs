using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitNet.VirtualizedGitFolder
{
    public class WindowsFileSystemGitFolder : IGitFolder
    {
        private readonly string _baseFolder;

        public WindowsFileSystemGitFolder(string baseFolder)
        {
            _baseFolder = baseFolder;
        }

        public Stream ReadFile(string path)
        {
            return File.OpenRead(this.ToAbsolutePath(path));
        }

        public List<string> ListFiles(string path)
        {
            return Directory.EnumerateFiles(this.ToAbsolutePath(path)).Select(n => n.Substring(_baseFolder.Length + 1).Replace("\\", "/")).ToList();
        }

        public List<string> ListSubdirectories(string path)
        {
            return Directory.EnumerateDirectories(this.ToAbsolutePath(path)).Select(n => n.Substring(_baseFolder.Length + 1).Replace("\\", "/")).ToList();
        }

        private string ToAbsolutePath(string path)
        {
            return Path.Combine(_baseFolder, path.Replace("/", "\\"));
        }
    }
}