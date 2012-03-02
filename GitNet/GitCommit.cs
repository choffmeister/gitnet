using System;
using System.Collections.Generic;

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

        public GitCommit(GitObjectId id, byte[] rawContent)
            : base(id, rawContent)
        {
            List<GitObjectId> parentIds = new List<GitObjectId>();

            int i = 0;
            while (i < rawContent.Length)
            {
                string line = GetNextLine(rawContent, ref i);

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
                    _message = GitObject.Encoding.GetString(rawContent, i, rawContent.Length - i);
                    break;
                }
            }

            _parentIds = parentIds.ToArray();
        }

        private static string GetNextLine(byte[] rawContent, ref int i)
        {
            int start = i;

            while (i < rawContent.Length)
            {
                if (rawContent[i++] == 10)
                {
                    return GitObject.Encoding.GetString(rawContent, start, i - start - 1);
                }
            }

            return GitObject.Encoding.GetString(rawContent, start, rawContent.Length - start);
        }
    }
}