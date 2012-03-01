namespace GitNet
{
    public sealed class GitCommit : GitObject
    {
        public GitCommit(GitObjectId id, byte[] rawContent)
            : base(id, rawContent)
        {
        }
    }
}