using System.Collections.Generic;
using System.Linq;

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
            : base(id, rawContent)
        {
            List<GitTreeEntry> entries = new List<GitTreeEntry>();

            int i = 0;
            while (i < rawContent.Length)
            {
                string entryMode = GitObject.Encoding.GetString(rawContent, i, FindNextOccurence(rawContent, ref i, 32));
                string entryName = GitObject.Encoding.GetString(rawContent, i, FindNextOccurence(rawContent, ref i, 0));
                GitObjectId entryId = new GitObjectId(rawContent.Skip(i).Take(20).ToArray());
                i += 20;

                entries.Add(new GitTreeEntry(entryId, entryName, entryMode));
            }

            _entries = entries.ToArray();
        }

        private static int FindNextOccurence(byte[] rawContent, ref int offset, byte b)
        {
            int i = 0;
            while (offset < rawContent.Length)
            {
                if (rawContent[offset + i] == b)
                {
                    offset += i + 1;
                    return i;
                }

                i++;
            }

            return -1;
        }
    }
}