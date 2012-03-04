using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GitNet.Binary
{
    public sealed class GitBinaryReaderWriter
    {
        private static readonly Encoding _encoding = Encoding.UTF8;

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
    }
}