namespace GitNet
{
    public static class GitRepositoryExtensions
    {
        public static GitObject Lookup(this GitRepository repository, string id)
        {
            return repository.Lookup(new GitObjectId(id));
        }

        public static GitObject Lookup(this GitRepository repository, byte[] id)
        {
            return repository.Lookup(new GitObjectId(id));
        }

        public static T Lookup<T>(this GitRepository repository, GitObjectId id)
            where T : GitObject
        {
            return (T)repository.Lookup(id);
        }

        public static T Lookup<T>(this GitRepository repository, string id)
            where T : GitObject
        {
            return (T)repository.Lookup(id);
        }

        public static T Lookup<T>(this GitRepository repository, byte[] id)
            where T : GitObject
        {
            return (T)repository.Lookup(id);
        }
    }
}