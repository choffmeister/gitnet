using System;

namespace GitNet
{
    public sealed class GitCommit : GitObject
    {
        private readonly GitObjectId _treeId;
        private readonly GitObjectId[] _parentIds;
        private readonly GitAuthor _author;
        private readonly DateTime _authorTimestamp;
        private readonly GitAuthor _committer;
        private readonly DateTime _committerTimestamp;

        public GitCommit(GitObjectId id, byte[] rawContent)
            : base(id, rawContent)
        {
            // TODO: implement
        }
    }
}