using GitNet.VirtualizedGitFolder;

namespace GitNet
{
    public class GitPack
    {
        private readonly IGitFolder _gitFolder;

        public GitPack(IGitFolder gitFolder)
        {
            _gitFolder = gitFolder;
        }
    }
}