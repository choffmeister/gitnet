using System.Collections.Generic;
using System.IO;

namespace GitNet.VirtualizedGitFolder
{
    public interface IGitFolder
    {
        Stream ReadFile(string path);

        List<string> ListFiles(string path);

        List<string> ListSubdirectories(string path);
    }
}