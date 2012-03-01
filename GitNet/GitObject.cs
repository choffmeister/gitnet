using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GitNet
{
    public class GitObject
    {
        private static readonly Encoding _encoding = Encoding.UTF8;
        private GitObjectId _id;
        private byte[] _rawContent;

        protected static Encoding Encoding
        {
            get { return _encoding; }
        }

        public GitObjectId Id
        {
            get { return _id; }
        }

        protected byte[] RawContent
        {
            get { return _rawContent; }
        }

        internal GitObject(GitObjectId id, byte[] rawContent)
        {
            _id = id;
            _rawContent = rawContent;
        }

        public static GitObject CreateFromRaw(string id, Stream raw)
        {
            return CreateFromRaw(new GitObjectId(id), raw);
        }

        public static GitObject CreateFromRaw(byte[] id, Stream raw)
        {
            return CreateFromRaw(new GitObjectId(id), raw);
        }

        public static GitObject CreateFromRaw(GitObjectId id, Stream raw)
        {
            string type = null;
            byte[] rawContent = ParseRaw(raw, out type);

            switch (type)
            {
                case "commit":
                    return new GitCommit(id, rawContent);
                default:
                    throw new NotImplementedException(string.Format("Object type '{0}' handling not yet implemented", type));
            }
        }

        private static byte[] ParseRaw(Stream raw, out string type)
        {
            // check for zlib header
            byte[] zlibHeader = new byte[2];
            raw.Read(zlibHeader, 0, 2);
            if (zlibHeader[0] != 120 || zlibHeader[1] != 1)
            {
                throw new Exception("Not a valid zlib deflate stream");
            }

            // deflate raw stream
            DeflateStream gzip = new DeflateStream(raw, CompressionMode.Decompress);
            byte[] deflatedRaw = gzip.ToByteArray();

            // extract header
            int headerContentBorder = 0;
            while (headerContentBorder < deflatedRaw.Length && deflatedRaw[headerContentBorder] != 0)
            {
                headerContentBorder++;
            }
            string header = _encoding.GetString(deflatedRaw, 0, headerContentBorder);
            string[] headerParts = header.Split(new char[] { ' ' });

            type = headerParts[0];
            byte[] result = new byte[int.Parse(headerParts[1])];
            Array.Copy(deflatedRaw, headerContentBorder + 1, result, 0, int.Parse(headerParts[1]));

            return result;
        }
    }
}