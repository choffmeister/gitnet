namespace GitNet
{
    public sealed class GitBlob : GitObject
    {
        private readonly byte[] _content;

        public byte[] Content
        {
            get { return _content; }
        }

        public GitBlob(GitObjectId id, byte[] rawContent)
            : base(id, rawContent)
        {
            _content = rawContent;
        }
    }
}