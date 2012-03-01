using System.IO;

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
            return File.OpenRead(Path.Combine(_baseFolder, path.Replace("/", "\\")));
        }
    }
}