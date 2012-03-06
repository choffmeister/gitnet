using System.IO;

namespace GitNet
{
    public sealed class GitBlob : GitObject
    {
        private readonly byte[] _content;

        public byte[] Content
        {
            get { return _content; }
        }

        public GitBlob(GitObjectId id, Stream raw)
            : base(id)
        {
            GitBinaryReaderWriter rw = new GitBinaryReaderWriter(raw);

            _content = rw.ReadBytes();
        }
    }
}