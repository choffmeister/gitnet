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

        public override int GetHashCode()
        {
            return 2091067123 ^ this.Id.GetHashCode();
        }

        public static bool operator ==(GitObject goi1, GitObject goi2)
        {
            if (object.ReferenceEquals(goi1, null) && object.ReferenceEquals(goi2, null))
                return true;

            if (object.ReferenceEquals(goi1, null))
                return false;

            if (object.ReferenceEquals(goi2, null))
                return false;

            return goi1.Equals(goi2);
        }

        public static bool operator !=(GitObject goi1, GitObject goi2)
        {
            return !(goi1 == goi2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (object.ReferenceEquals(this, obj))
                return true;

            GitObject objAsGitObject = obj as GitObject;

            if (objAsGitObject == null)
                return false;

            if (this.GetHashCode() != objAsGitObject.GetHashCode())
                return false;

            if (this.Id != objAsGitObject.Id)
                return false;

            return true;
        }
    }
}