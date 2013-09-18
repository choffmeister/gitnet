using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitNet
{
    public class FileSystemGitFolder : IGitFolder
    {
        private static bool _isRunningOnLinux;

        static FileSystemGitFolder()
        {
            // see http://mono-project.com/FAQ%3a_Technical#Mono_Platforms
            int p = (int)Environment.OSVersion.Platform;
            _isRunningOnLinux = (p == 4) || (p == 6) || (p == 128);
        }

        private readonly string _baseFolder;

        public FileSystemGitFolder(string baseFolder)
        {
            _baseFolder = baseFolder;
        }

        public Stream ReadFile(string path)
        {
            return File.OpenRead(this.ToAbsolutePath(path));
        }

        public List<string> ListFiles(string path, bool recursive = false)
        {
            return Directory.EnumerateFiles(this.ToAbsolutePath(path), "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(n => this.ToVirtualPath(n)).ToList();
        }

        public List<string> ListSubdirectories(string path, bool recursive = false)
        {
            return Directory.EnumerateDirectories(this.ToAbsolutePath(path), "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(n => this.ToVirtualPath(n)).ToList();
        }

        public bool FileExists(string path)
        {
            return File.Exists(this.ToAbsolutePath(path));
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(this.ToAbsolutePath(path));
        }

        protected virtual string ToAbsolutePath(string path)
        {
            return _isRunningOnLinux ? Path.Combine(_baseFolder) : Path.Combine(_baseFolder, path.Replace("/", "\\"));
        }

        protected virtual string ToVirtualPath(string path)
        {
            return _isRunningOnLinux ? path.Substring(_baseFolder.Length + 1) : path.Substring(_baseFolder.Length + 1).Replace("\\", "/");
        }

        public void Dispose()
        {
        }
    }
}