using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
            byte[] deflatedRaw = Deflate(raw);

            // extract header
            int headerContentBorder = 0;
            while (headerContentBorder < deflatedRaw.Length && deflatedRaw[headerContentBorder] != 0)
            {
                headerContentBorder++;
            }
            string header = _encoding.GetString(deflatedRaw, 0, headerContentBorder);
            string[] headerParts = header.Split(new char[] { ' ' });

            string type = headerParts[0];
            int size = int.Parse(headerParts[1]);
            byte[] data = deflatedRaw.Skip(headerContentBorder + 1).Take(size).ToArray();

            switch (type)
            {
                case "commit": return new GitCommit(id, data);
                case "tree": return new GitTree(id, data);
                case "blob": return new GitBlob(id, data);
                case "tag": return new GitTag(id, data);
                default: throw new NotSupportedException(string.Format("Object of type '{0}' is not supported", type));
            }
        }

        public static GitObject DeserializeUnheaderedGitObject(GitObjectId id, Stream raw, string type)
        {
            byte[] data = Deflate(raw);

            switch (type)
            {
                case "commit": return new GitCommit(id, data);
                case "tree": return new GitTree(id, data);
                case "blob": return new GitBlob(id, data);
                case "tag": return new GitTag(id, data);
                default: throw new NotSupportedException(string.Format("Object of type '{0}' is not supported", type));
            }
        }

        public static byte[] Deflate(Stream raw)
        {
            // check for zlib header
            byte[] zlibHeader = new byte[2];
            raw.Read(zlibHeader, 0, 2);
            if (zlibHeader[0] != 120 || (zlibHeader[1] != 1 && zlibHeader[1] != 156))
            {
                throw new Exception("Not a valid zlib deflate stream");
            }

            // deflate raw stream
            DeflateStream gzip = new DeflateStream(raw, CompressionMode.Decompress);
            return gzip.ToByteArray();
        }

        public static int FindNextOccurence(byte[] rawContent, ref int offset, byte b)
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

        public static string GetNextLine(byte[] rawContent, ref int i)
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