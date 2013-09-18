namespace GitNet
{
    public class GitTreeEntry
    {
        private readonly GitObjectId _id;
        private readonly string _name;
        private readonly string _mode;

        public GitObjectId Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string Mode
        {
            get { return _mode; }
        }

        public GitTreeEntry(GitObjectId id, string name, string mode)
        {
            _id = id;
            _name = name;
            _mode = mode;
        }
    }
}