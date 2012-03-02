namespace GitNet
{
    public sealed class GitTag : GitObject
    {
        private readonly GitObjectId _objectId;
        private readonly string _type;
        private readonly string _tag;
        private readonly GitAuthor _tagger;
        private readonly string _message;

        public GitTag(GitObjectId id, byte[] rawContent)
            : base(id, rawContent)
        {
            var a = GitObject.Encoding.GetString(rawContent);

            int i = 0;
            while (i < rawContent.Length)
            {
                string line = GitObject.GetNextLine(rawContent, ref i);

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
                    _tagger = new GitAuthor(line.Substring(7));
                }
                else if (line == "")
                {
                    _message = GitObject.Encoding.GetString(rawContent, i, rawContent.Length - i);
                    break;
                }
            }
        }
    }
}