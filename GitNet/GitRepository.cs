using GitNet.VirtualizedGitFolder;

namespace GitNet
{
    public class GitRepository
    {
        private readonly IGitFolder _gitFolder;

        public GitRepository(IGitFolder gitFolder)
        {
            _gitFolder = gitFolder;
        }

        public GitObject Lookup(GitObjectId id)
        {
            return GitObject.CreateFromRaw(id, _gitFolder.ReadFile("objects/" + id.Sha.Substring(0, 2) + "/" + id.Sha.Substring(2)));
        }
    }
}