using System.IO;

namespace GitNet.Binary
{
    public class GitStreamReader
    {
        private readonly Stream _stream;

        public GitStreamReader(Stream stream)
        {
            _stream = stream;
        }

        public string ReadUntil(byte border)
        {
            return null;
        }
    }
}