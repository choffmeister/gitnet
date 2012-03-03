using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace GitNet
{
    [System.Diagnostics.DebuggerDisplay("Id={Id.Sha}")]
    public class GitObject
    {
        private static readonly Encoding _encoding = Encoding.UTF8;
        private static readonly Dictionary<string, Func<GitObjectId, byte[], GitObject>> _typeMap = new Dictionary<string, Func<GitObjectId, byte[], GitObject>>() {
            { "commit", (id, rawContent) => new GitCommit(id, rawContent) },
            { "tree", (id, rawContent) => new GitTree(id, rawContent) },
            { "blob", (id, rawContent) => new GitBlob(id, rawContent) },
            { "tag", (id, rawContent) => new GitTag(id, rawContent) }
        };

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
            byte[] rawContent = Parse(raw, out type);

            return _typeMap[type](id, rawContent);
        }

        public static GitObject CreateFromRaw(GitObjectId id, Stream raw, string type)
        {
            byte[] rawContent = Deflate(raw);

            return _typeMap[type](id, rawContent);
        }

        private static byte[] Parse(Stream raw, out string type)
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

            // parse header
            type = headerParts[0];
            byte[] result = new byte[int.Parse(headerParts[1])];
            Array.Copy(deflatedRaw, headerContentBorder + 1, result, 0, int.Parse(headerParts[1]));

            return result;
        }

        private static byte[] Deflate(Stream raw)
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