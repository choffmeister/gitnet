using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GitNet
{
    [System.Diagnostics.DebuggerDisplay("Id={Id.Sha}")]
    public class GitObject
    {
        private static readonly Encoding _encoding = Encoding.UTF8;
        private GitObjectId _id;
        private byte[] _rawContent;

        public static Encoding Encoding
        {
            get { return _encoding; }
        }

        public GitObjectId Id
        {
            get { return _id; }
        }

        internal GitObject(GitObjectId id, byte[] rawContent)
        {
            _id = id;
            _rawContent = rawContent;
        }

        public static GitObject CreateFromRaw(GitObjectId id, Stream raw)
        {
            string type = null;
            byte[] rawContent = ParseRaw(raw, out type);

            switch (type)
            {
                case "commit":
                    return new GitCommit(id, rawContent);
                case "tree":
                    return new GitTree(id, rawContent);
                case "tag":
                    return new GitTag(id, rawContent);
                case "blob":
                    return new GitBlob(id, rawContent);
                default:
                    throw new NotSupportedException(string.Format("Object type '{0}' handling not supported", type));
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

        protected static int FindNextOccurence(byte[] rawContent, ref int offset, byte b)
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

        protected static string GetNextLine(byte[] rawContent, ref int i)
        {
            int start = i;

            while (i < rawContent.Length)
            {
                if (rawContent[i++] == 10)
                {
                    return Encoding.GetString(rawContent, start, i - start - 1);
                }
            }

            return Encoding.GetString(rawContent, start, rawContent.Length - start);
        }
    }
}