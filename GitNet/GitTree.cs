using System.Collections.Generic;
using System.IO;
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

        public GitTree(GitObjectId id, Stream raw)
            : base(id)
        {
            GitBinaryReaderWriter rw = new GitBinaryReaderWriter(raw);
            List<GitTreeEntry> entries = new List<GitTreeEntry>();

            while (true)
            {
                string entryMode = rw.ReadSpaceTerminatedString();
                string entryName = rw.ReadNullTerminatedString();
                byte[] entryId = rw.ReadBytes(20);

                if (entryMode == null || entryName == null || entryId == null)
                {
                    break;
                }

                entries.Add(new GitTreeEntry(new GitObjectId(entryId), entryName, entryMode));
            }

            _entries = entries.ToArray();
        }
    }
}