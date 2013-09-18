namespace GitNet
{
    public class GitRawObjectReferenceDelta
    {
        private readonly GitObjectId _referenceId;
        private readonly byte[] _data;

        public GitObjectId ReferenceId
        {
            get { return _referenceId; }
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public GitRawObjectReferenceDelta(GitObjectId referenceId, byte[] data)
        {
            _referenceId = referenceId;
            _data = data;
        }
    }
}