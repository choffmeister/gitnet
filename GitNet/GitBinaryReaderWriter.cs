using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GitNet
{
    internal sealed class GitBinaryReaderWriter
    {
        private static readonly Encoding _encoding = Encoding.UTF8;

        public static Encoding Encoding
        {
            get { return _encoding; }
        }

        private readonly Stream _stream;

        public GitBinaryReaderWriter(Stream stream)
        {
            _stream = stream;
        }

        public byte[] ReadBytesUntil(byte endByte)
        {
            List<byte> result = new List<byte>();

            for (int current = _stream.ReadByte(); current != endByte; current = _stream.ReadByte())
            {
                if (current == -1 && result.Count == 0)
                {
                    return null;
                }

                result.Add((byte)current);
            }

            return result.ToArray();
        }

        public byte[] ReadBytes(int count)
        {
            byte[] result = new byte[count];

            int read = _stream.Read(result, 0, count);

            return read == count ? result : null;
        }

        public byte[] ReadBytes()
        {
            return ToByteArray(_stream);
        }

        public string ReadLine()
        {
            byte[] bytes = this.ReadBytesUntil((byte)10);

            return bytes != null ? _encoding.GetString(bytes) : null;
        }

        public string ReadNullTerminatedString()
        {
            byte[] bytes = this.ReadBytesUntil((byte)0);

            return bytes != null ? _encoding.GetString(bytes) : null;
        }

        public string ReadSpaceTerminatedString()
        {
            byte[] bytes = this.ReadBytesUntil((byte)32);

            return bytes != null ? _encoding.GetString(bytes) : null;
        }

        public string ReadToEnd()
        {
            StringBuilder sb = new StringBuilder();
            byte[] buffer = new byte[8192];

            for (int read = _stream.Read(buffer, 0, 8192); read > 0; read = _stream.Read(buffer, 0, 8192))
            {
                sb.Append(_encoding.GetString(buffer, 0, read));
            }

            return sb.ToString();
        }

        public int ReadInt32()
        {
            byte[] buffer = new byte[4];
            _stream.Read(buffer, 0, 4);

            return buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
        }

        public int[] ReadInt32List(int count)
        {
            byte[] buffer = new byte[count * 4];
            _stream.Read(buffer, 0, count * 4);

            int[] result = new int[count];

            for (int i = 0; i < count * 4; i += 4)
            {
                result[i / 4] = buffer[i + 0] << 24 | buffer[i + 1] << 16 | buffer[i + 2] << 8 | buffer[i + 3];
            }

            return result;
        }

        public GitObjectId ReadObjectId()
        {
            byte[] buffer = new byte[20];
            _stream.Read(buffer, 0, 20);

            return new GitObjectId(buffer);
        }

        public int ReadPackFileChunkHeader(out int expandedSize)
        {
            byte b = (byte)_stream.ReadByte();
            int type = (b & 112) >> 4;
            expandedSize = b & 15 + this.ReadDynamicIntLittleEndian() << 4;

            return type;
        }

        public int ReadPackFileVersion()
        {
            byte[] buffer = new byte[4];
            _stream.Read(buffer, 0, 4);

            // ensure magic number and version
            if (buffer[0] != 0x50 || buffer[1] != 0x41 || buffer[2] != 0x43 || buffer[3] != 0x4b)
                throw new Exception("Expected magic number 0x5041434b in packet index file");

            int version = ReadInt32();

            if (version != 2)
                throw new NotSupportedException(string.Format("Version '{0}' index file is not supported", version));

            return version;
        }

        public int ReadPackIndexFileVersion()
        {
            byte[] buffer = new byte[4];
            _stream.Read(buffer, 0, 4);

            // ensure magic number and version
            if (buffer[0] != 0xff || buffer[1] != 0x74 || buffer[2] != 0x4f || buffer[3] != 0x63)
                throw new Exception("Expected magic number 0xff744763 in packet index file");

            int version = ReadInt32();

            if (version != 2)
                throw new NotSupportedException(string.Format("Version '{0}' index file is not supported", version));

            return version;
        }

        public GitObject ReadHeaderedGitObject(GitObjectId id)
        {
            Stream deflatedRaw = ToDeflatedStream(_stream);

            GitBinaryReaderWriter rw = new GitBinaryReaderWriter(deflatedRaw);
            string header = rw.ReadNullTerminatedString();
            string[] headerParts = header.Split(new char[] { ' ' });

            string type = headerParts[0];
            int size = int.Parse(headerParts[1]);

            switch (type)
            {
                case "commit": return new GitCommit(id, deflatedRaw);
                case "tree": return new GitTree(id, deflatedRaw);
                case "blob": return new GitBlob(id, deflatedRaw);
                case "tag": return new GitTag(id, deflatedRaw);
                default: throw new NotSupportedException(string.Format("Object of type '{0}' is not supported", type));
            }
        }

        public Tuple<int, byte[]> ReadObjectDelta()
        {
            int offset = -1;
            byte b;

            do
            {
                offset++;
                b = (byte)_stream.ReadByte();
                offset = (offset << 7) + (b & 127);
            }
            while ((b & (byte)128) != 0);

            byte[] delta = this.ReadDeflated();

            return Tuple.Create(offset, delta);
        }

        public int ReadDynamicIntLittleEndian()
        {
            int result = 0;
            int j = 0;
            byte b;

            do
            {
                b = (byte)_stream.ReadByte();
                result |= (b & 127) << (7 * j++);
            } while ((b & (byte)128) != 0);

            return result;
        }

        public int ReadDynamicIntBigEndian()
        {
            int result = 0;
            int j = 0;
            byte b;

            do
            {
                b = (byte)_stream.ReadByte();
                result = (result << (7 * j++)) | (b & 127);
            } while ((b & (byte)128) != 0);

            return result;
        }

        public byte[] ReadDeflated()
        {
            return ToByteArray(ToDeflatedStream(_stream));
        }

        private static Stream ToDeflatedStream(Stream raw)
        {
            // check for zlib header
            byte[] zlibHeader = new byte[2];
            raw.Read(zlibHeader, 0, 2);
            if (zlibHeader[0] != 120 || (zlibHeader[1] != 1 && zlibHeader[1] != 156))
            {
                throw new Exception("Not a valid zlib deflate stream");
            }

            return new DeflateStream(raw, CompressionMode.Decompress);
        }

        private static byte[] ToByteArray(Stream raw)
        {
            List<Tuple<byte[], int>> buffers = new List<Tuple<byte[], int>>();

            int acutallyRead = -1;
            while (acutallyRead != 0)
            {
                byte[] buffer = new byte[4096];
                acutallyRead = raw.Read(buffer, 0, 4096);
                buffers.Add(Tuple.Create(buffer, acutallyRead));
            }

            int i = 0;
            int j = 0;
            byte[] byteArray = new byte[buffers.Sum(n => n.Item2)];

            while (i < buffers.Count)
            {
                Array.Copy(buffers[i].Item1, 0, byteArray, j, buffers[i].Item2);
                j += buffers[i].Item2;
                i += 1;
            }

            return byteArray;
        }
    }
}