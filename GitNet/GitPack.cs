using System;
using System.IO;
using GitNet.VirtualizedGitFolder;

namespace GitNet
{
    public class GitPack
    {
        private readonly IGitFolder _gitFolder;
        private readonly string _name;

        private readonly string _indexFile;
        private readonly string _packFile;

        public GitPack(IGitFolder gitFolder, string name)
        {
            _gitFolder = gitFolder;
            _name = name;

            _indexFile = string.Format("objects/pack/{0}.idx", _name);
            _packFile = string.Format("objects/pack/{0}.pack", _name);
        }

        public GitObject RetrieveObject(GitObjectId id)
        {
            using (Stream index = _gitFolder.ReadFile(_indexFile))
            {
                GitBinaryReaderWriter rw = new GitBinaryReaderWriter(index);

                int version = rw.ReadPackIndexFileVersion();
                int[] fanOutTable = rw.ReadInt32List(256);

                int a = id.Raw[0] == 0 ? 0 : fanOutTable[id.Raw[0] - 1];
                int b = fanOutTable[id.Raw[0]];

                if (a < b)
                {
                    index.Seek(a * 20, SeekOrigin.Current);

                    for (int i = a; i < b; i++)
                    {
                        if (rw.ReadObjectId() == id)
                        {
                            index.Seek(i * 4 + fanOutTable[255] * 24 + 1024 + 8, SeekOrigin.Begin);
                            int offset = rw.ReadInt32();
                            return this.ExtractObject(id, offset);
                        }
                    }
                }
            }

            return null;
        }

        private GitObject ExtractObject(GitObjectId id, int offset)
        {
            using (Stream pack = _gitFolder.ReadFile(_packFile))
            {
                GitBinaryReaderWriter rw = new GitBinaryReaderWriter(pack);

                int version = rw.ReadPackFileVersion();
                int totalObjects = rw.ReadInt32();
                pack.Seek(offset, SeekOrigin.Begin);
                int expandedSize;
                int type = rw.ReadPackFileChunkHeader(out expandedSize);

                switch (type)
                {
                    case 1:
                        return rw.ReadUnheaderedGitObject(id, "commit");
                    case 2:
                        return rw.ReadUnheaderedGitObject(id, "tree");
                    case 3:
                        return rw.ReadUnheaderedGitObject(id, "blob");
                    case 4:
                        return rw.ReadUnheaderedGitObject(id, "tag");
                    // case 6: OBJ_OFS_DELTA
                    // case 7: OBJ_REF_DELTA
                    default:
                        throw new NotImplementedException(string.Format("Pack chunk type '{0}' not yet implemented", type));
                }
            }
        }
    }
}