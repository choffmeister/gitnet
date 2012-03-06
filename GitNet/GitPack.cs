using System;
using System.IO;
using GitNet.VirtualizedGitFolder;

namespace GitNet
{
    public class GitPack
    {
        const int OBJ_COMMIT = 1;
        const int OBJ_TREE = 2;
        const int OBJ_BLOB = 3;
        const int OBJ_TAG = 4;
        const int OBJ_OFS_DELTA = 6;
        const int OBJ_REF_DELTA = 7;

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
                            var result = this.UnpackObject(offset);

                            switch (result.Item2)
                            {
                                case OBJ_COMMIT:
                                    return new GitCommit(id, new MemoryStream(result.Item1));
                                case OBJ_TREE:
                                    return new GitTree(id, new MemoryStream(result.Item1));
                                case OBJ_BLOB:
                                    return new GitBlob(id, new MemoryStream(result.Item1));
                                case OBJ_TAG:
                                    return new GitTag(id, new MemoryStream(result.Item1));
                                default:
                                    throw new NotSupportedException();
                            }
                        }
                    }
                }
            }

            return null;
        }

        private Tuple<byte[], int> UnpackObject(int offset)
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
                    case OBJ_COMMIT:
                        goto case OBJ_TAG;
                    case OBJ_TREE:
                        goto case OBJ_TAG;
                    case OBJ_BLOB:
                        goto case OBJ_TAG;
                    case OBJ_TAG:
                        return Tuple.Create(rw.ReadDeflated(), type);
                    case OBJ_OFS_DELTA:
                        {
                            var delta = rw.ReadObjectDelta();
                            var origin = this.UnpackObject(offset - delta.Item1);

                            return Tuple.Create(this.ApplyPatch(delta.Item2, origin.Item1), origin.Item2);
                        }
                    case OBJ_REF_DELTA:
                        {
                            GitObjectId originId = rw.ReadObjectId();
                            var delta = rw.ReadObjectDelta();

                            // TODO: receive origin data and type
                            throw new NotImplementedException();

                            //return Tuple.Create(this.ApplyPatch(delta.Item2, origin.Item1), origin.Item2);
                        }
                    default:
                        throw new NotSupportedException(string.Format("Pack chunk type '{0}' not yet implemented", type));
                }
            }
        }

        private byte[] ApplyPatch(byte[] delta, byte[] origin)
        {
            MemoryStream deltaStream = new MemoryStream(delta);
            GitBinaryReaderWriter deltaReader = new GitBinaryReaderWriter(deltaStream);

            int a = deltaReader.ReadDynamicIntLittleEndian();
            int b = deltaReader.ReadDynamicIntLittleEndian();

            byte[] result = new byte[b];
            int position = 0;

            while (deltaStream.Position < deltaStream.Length)
            {
                int opCode = deltaStream.ReadByte();

                if ((opCode & 128) != 0)
                {
                    int offset = 0;
                    if ((opCode & 0x01) != 0) offset |= deltaStream.ReadByte();
                    if ((opCode & 0x02) != 0) offset |= deltaStream.ReadByte() << 8;
                    if ((opCode & 0x04) != 0) offset |= deltaStream.ReadByte() << 16;
                    if ((opCode & 0x08) != 0) offset |= deltaStream.ReadByte() << 24;

                    int length = 0;
                    if ((opCode & 0x10) != 0) length |= deltaStream.ReadByte();
                    if ((opCode & 0x20) != 0) length |= deltaStream.ReadByte() << 8;
                    if ((opCode & 0x40) != 0) length |= deltaStream.ReadByte() << 16;

                    // TODO: check if this is correct
                    if (length == 0) length = 0x10000;

                    Array.Copy(origin, offset, result, position, length);
                    position += length;
                }
                else
                {
                    Array.Copy(deltaReader.ReadBytes(opCode), 0, result, position, opCode);
                    position += opCode;
                }
            }

            return result;
        }
    }
}