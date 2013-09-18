namespace GitNet
{
    public class GitReference
    {
        private readonly string _name;
        private readonly GitObjectId _id;

        public string Name
        {
            get { return _name; }
        }

        public GitObjectId Id
        {
            get { return _id; }
        }

        public GitReference(string name, GitObjectId id)
        {
            _name = name;
            _id = id;
        }
    }
}