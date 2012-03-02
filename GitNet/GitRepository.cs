using System.Collections.Generic;
using System.Linq;
using GitNet.VirtualizedGitFolder;

namespace GitNet
{
    public sealed class GitRepository
    {
        private readonly IGitFolder _gitFolder;
        private readonly Dictionary<GitObjectId, GitObject> _cache;

        public GitRepository(IGitFolder gitFolder)
        {
            _gitFolder = gitFolder;
            _cache = new Dictionary<GitObjectId, GitObject>();
        }

        public GitObject Lookup(GitObjectId id)
        {
            if (_cache.ContainsKey(id))
            {
                return _cache[id];
            }
            else
            {
                GitObject go = GitObject.CreateFromRaw(id, _gitFolder.ReadFile("objects/" + id.Sha.Substring(0, 2) + "/" + id.Sha.Substring(2)));
                _cache[id] = go;

                return go;
            }
        }

        public GitObjectId ResolveReference(string reference)
        {
            if (reference.StartsWith("ref: "))
            {
                return this.ResolveReference(_gitFolder.ReadAllLines(reference.Substring(5)).First());
            }
            else
            {
                return new GitObjectId(reference);
            }
        }
    }
}