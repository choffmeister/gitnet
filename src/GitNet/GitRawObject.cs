namespace GitNet
{
    public class GitRawObject
    {
        private readonly GitRawObjectType _type;
        private readonly byte[] _data;

        public GitRawObjectType Type
        {
            get { return _type; }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public GitRawObject(GitRawObjectType type, byte[] data)
        {
            _type = type;
            _data = data;
        }
    }

    public enum GitRawObjectType
    {
        Commit = 1,
        Tree = 2,
        Blob = 3,
        Tag = 4
    }
}