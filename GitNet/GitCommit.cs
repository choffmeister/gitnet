using System.Collections.Generic;
using System.IO;

namespace GitNet
{
    public sealed class GitCommit : GitObject
    {
        private readonly GitObjectId _treeId;
        private readonly GitObjectId[] _parentIds;
        private readonly GitAuthor _author;
        private readonly GitAuthor _committer;
        private readonly string _message;

        public GitObjectId TreeId
        {
            get { return _treeId; }
        }

        public GitObjectId[] ParentId
        {
            get { return _parentIds; }
        }

        public GitAuthor Author
        {
            get { return _author; }
        }

        public GitAuthor Committer
        {
            get { return _committer; }
        }

        public string Message
        {
            get { return _message; }
        }

        public GitCommit(GitObjectId id, Stream raw)
            : base(id)
        {
            GitBinaryReaderWriter rw = new GitBinaryReaderWriter(raw);
            List<GitObjectId> parentIds = new List<GitObjectId>();

            string line;
            while ((line = rw.ReadLine()) != null)
            {
                if (line.StartsWith("tree "))
                {
                    _treeId = new GitObjectId(line.Substring(5));
                }
                else if (line.StartsWith("parent "))
                {
                    parentIds.Add(new GitObjectId(line.Substring(7)));
                }
                else if (line.StartsWith("author "))
                {
                    _author = new GitAuthor(line.Substring(7));
                }
                else if (line.StartsWith("committer "))
                {
                    _committer = new GitAuthor(line.Substring(10));
                }
                else if (line == "")
                {
                    _message = rw.ReadToEnd();
                    break;
                }
            }

            _parentIds = parentIds.ToArray();
        }
    }
}