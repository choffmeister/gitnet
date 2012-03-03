using System.Collections.Generic;
using System.Linq;
using GitNet.Binary;

namespace GitNet
{
    public sealed class GitTree : GitObject
    {
        private readonly GitTreeEntry[] _entries;

        public GitTreeEntry[] Entries
        {
            get { return _entries; }
        }

        public GitTree(GitObjectId id, byte[] rawContent)
            : base(id)
        {
            List<GitTreeEntry> entries = new List<GitTreeEntry>();

            int i = 0;
            while (i < rawContent.Length)
            {
                string entryMode = GitBinaryHelper.Encoding.GetString(rawContent, i, GitBinaryHelper.FindNextOccurence(rawContent, ref i, 32));
                string entryName = GitBinaryHelper.Encoding.GetString(rawContent, i, GitBinaryHelper.FindNextOccurence(rawContent, ref i, 0));
                GitObjectId entryId = new GitObjectId(rawContent.Skip(i).Take(20).ToArray());
                i += 20;

                entries.Add(new GitTreeEntry(entryId, entryName, entryMode));
            }

            _entries = entries.ToArray();
        }
    }
}