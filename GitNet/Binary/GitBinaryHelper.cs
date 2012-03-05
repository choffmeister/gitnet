using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GitNet.Binary
{
    public static class GitBinaryHelper
    {
        private static readonly Encoding _encoding = Encoding.UTF8;

        public static Encoding Encoding
        {
            get { return _encoding; }
        }

        public static GitObject DeserializeHeaderedGitObject(GitObjectId id, Stream raw)
        {
            Stream deflatedRaw = Deflate(raw);

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

        public static GitObject DeserializeUnheaderedGitObject(GitObjectId id, Stream raw, string type)
        {
            Stream data = Deflate(raw);

            switch (type)
            {
                case "commit": return new GitCommit(id, data);
                case "tree": return new GitTree(id, data);
                case "blob": return new GitBlob(id, data);
                case "tag": return new GitTag(id, data);
                default: throw new NotSupportedException(string.Format("Object of type '{0}' is not supported", type));
            }
        }

        public static Stream Deflate(Stream raw)
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
    }
}