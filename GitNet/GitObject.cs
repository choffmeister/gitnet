namespace GitNet
{
    [System.Diagnostics.DebuggerDisplay("Id={Id.Sha}")]
    public class GitObject
    {
        private GitObjectId _id;

        public GitObjectId Id
        {
            get { return _id; }
        }

        internal GitObject(GitObjectId id)
        {
            _id = id;
        }
    }
}