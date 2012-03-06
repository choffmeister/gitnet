using System.IO;

namespace GitNet
{
    public sealed class GitTag : GitObject
    {
        private readonly GitObjectId _objectId;
        private readonly string _type;
        private readonly string _tag;
        private readonly GitSignature _tagger;
        private readonly string _message;

        public GitTag(GitObjectId id, Stream raw)
            : base(id)
        {
            GitBinaryReaderWriter rw = new GitBinaryReaderWriter(raw);

            string line;
            while ((line = rw.ReadLine()) != null)
            {
                if (line.StartsWith("object "))
                {
                    _objectId = new GitObjectId(line.Substring(7));
                }
                else if (line.StartsWith("type "))
                {
                    _type = line.Substring(5);
                }
                else if (line.StartsWith("tag "))
                {
                    _tag = line.Substring(4);
                }
                else if (line.StartsWith("tagger "))
                {
                    _tagger = new GitSignature(line.Substring(7));
                }
                else if (line == "")
                {
                    _message = rw.ReadToEnd();
                    break;
                }
            }
        }
    }
}