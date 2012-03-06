namespace GitNet
{
    public class GitRawObjectOffsetDelta
    {
        private readonly int _offset;
        private readonly byte[] _data;

        public int Offset
        {
            get { return _offset; }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public GitRawObjectOffsetDelta(int offset, byte[] data)
        {
            _offset = offset;
            _data = data;
        }
    }
}