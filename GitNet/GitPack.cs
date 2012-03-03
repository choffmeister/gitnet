using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                byte[] buffer = new byte[8192];
                index.Read(buffer, 0, 8);

                // ensure magic number and version
                if (buffer[0] != 0xff || buffer[1] != 0x74 || buffer[2] != 0x4f || buffer[3] != 0x63)
                    throw new Exception(string.Format("Expected magic number 0xff744763 in packet index file '{0}'", _indexFile));
                if (ToInt32(buffer, 4) != 2)
                    throw new NotSupportedException(string.Format("Version '{0}' index file is not supported", ToInt32(buffer, 4)));

                // read fan-out table
                index.Read(buffer, 0, 1024);
                int totalObjects = ToInt32(buffer, 1020);
                int remainingToSkip = totalObjects * 20;
                int indexOfId = -1;

                // read maximal 20 shas at once
                for (int i = 0; i < totalObjects; i += 400)
                {
                    int shasToRead = Math.Min(totalObjects - i, 400);
                    index.Read(buffer, 0, shasToRead * 20);
                    remainingToSkip -= shasToRead * 20;

                    List<GitObjectId> ids = Enumerable.Range(0, shasToRead).Select(n => buffer.Skip(n * 20).Take(20).ToArray()).Select(n => new GitObjectId(n)).ToList();

                    indexOfId = ids.IndexOf(id);
                    if (indexOfId >= 0)
                    {
                        indexOfId += i;
                        index.Seek(remainingToSkip, SeekOrigin.Current);
                    }
                }

                if (indexOfId >= 0)
                {
                    // skip CRC32 table for now
                    index.Seek(totalObjects * 4, SeekOrigin.Current);

                    // jump to offset value
                    index.Seek(indexOfId * 4, SeekOrigin.Current);

                    index.Read(buffer, 0, 4);
                    int offset = ToInt32(buffer, 0);
                    int nextOffset = -1;

                    if (indexOfId < totalObjects - 1)
                    {
                        index.Read(buffer, 0, 4);
                        nextOffset = ToInt32(buffer, 0);
                    }

                    return this.ExtractObject(id, offset, nextOffset);
                }
            }

            return null;
        }

        private GitObject ExtractObject(GitObjectId id, int offset, int nextOffset)
        {
            using (Stream pack = _gitFolder.ReadFile(_packFile))
            {
                byte[] buffer = new byte[8192];
                pack.Read(buffer, 0, 12);

                // ensure magic number and version
                if (buffer[0] != 0x50 || buffer[1] != 0x41 || buffer[2] != 0x43 || buffer[3] != 0x4b)
                    throw new Exception(string.Format("Expected magic number 0x5041434b in packet index file '{0}'", _packFile));
                if (ToInt32(buffer, 4) != 2)
                    throw new NotSupportedException(string.Format("Version '{0}' index file is not supported", ToInt32(buffer, 4)));

                int totalObjects = ToInt32(buffer, 8);

                pack.Seek(offset - 12, SeekOrigin.Current);

                int i = 0;
                do pack.Read(buffer, i, 1); while ((buffer[i++] & (byte)128) != 0);
                int type = (buffer[0] & 112) >> 4;
                int expandedSize = ToInt4(buffer, 0);
                for (int j = 1; j < i; j++) expandedSize |= ToInt7(buffer, j) << (4 + (7 * (j - 1)));

                int size = nextOffset > -1 ? nextOffset - offset - i : (int)pack.Length - offset - i - 20;

                byte[] data = new byte[size];
                pack.Read(data, 0, size);

                switch (type)
                {
                    case 1:
                        return GitObject.CreateFromRaw(id, data.ToStream(), "commit");
                    case 2:
                        return GitObject.CreateFromRaw(id, data.ToStream(), "tree");
                    case 3:
                        return GitObject.CreateFromRaw(id, data.ToStream(), "blob");
                    case 4:
                        return GitObject.CreateFromRaw(id, data.ToStream(), "tag");
                    // case 6: OBJ_OFS_DELTA
                    // case 7: OBJ_REF_DELTA
                    default:
                        throw new NotImplementedException(string.Format("Pack chunk type '{0}' not yet implemented", type));
                }
            }
        }

        private static int ToInt4(byte[] bytes, int offset)
        {
            int a = bytes[offset] & 15;

            return a;
        }

        private static int ToInt7(byte[] bytes, int offset)
        {
            int a = bytes[offset] & 127;

            return a;
        }

        private static int ToInt32(byte[] bytes, int offset)
        {
            int a = bytes[offset + 3];
            int b = bytes[offset + 2] << 8;
            int c = bytes[offset + 1] << 16;
            int d = bytes[offset + 0] << 24;

            return a | b | c | d;
        }
    }
}