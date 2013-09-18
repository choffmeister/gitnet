using System;
using System.Collections.Generic;
using System.IO;

namespace GitNet
{
    public interface IGitFolder : IDisposable
    {
        Stream ReadFile(string path);

        List<string> ListFiles(string path, bool recursive = false);

        List<string> ListSubdirectories(string path, bool recursive = false);

        bool FileExists(string path);

        bool DirectoryExists(string path);
    }
}