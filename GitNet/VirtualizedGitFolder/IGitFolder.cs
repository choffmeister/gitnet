using System.IO;

namespace GitNet.VirtualizedGitFolder
{
    public interface IGitFolder
    {
        Stream ReadFile(string path);
    }
}