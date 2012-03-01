namespace GitNet
{
    public sealed class GitBlob : GitObject
    {
        public GitBlob(GitObjectId id, byte[] rawContent)
            : base(id, rawContent)
        {
        }
    }
}