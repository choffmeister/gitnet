namespace GitNet
{
    public sealed class GitTree : GitObject
    {
        public GitTree(GitObjectId id, byte[] rawContent)
            : base(id, rawContent)
        {
        }
    }
}