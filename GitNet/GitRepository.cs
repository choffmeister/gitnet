using System;
using System.Collections.Generic;
using System.Linq;
using GitNet.VirtualizedGitFolder;

namespace GitNet
{
    public sealed class GitRepository : IDisposable
    {
        private readonly IGitFolder _gitFolder;
        private readonly Dictionary<GitObjectId, GitObject> _objectCache;
        private readonly Dictionary<string, GitObjectId> _referenceCache;

        private Lazy<GitCommit> _head;
        private Lazy<GitPackList> _packList;
        private Lazy<GitReference[]> _references;

        public GitCommit Head
        {
            get { return _head.Value; }
        }

        public GitPackList PackList
        {
            get { return _packList.Value; }
        }

        public GitReference[] References
        {
            get { return _references.Value; }
        }

        public GitRepository(IGitFolder gitFolder)
        {
            _gitFolder = gitFolder;
            _objectCache = new Dictionary<GitObjectId, GitObject>();
            _referenceCache = new Dictionary<string, GitObjectId>();

            _head = new Lazy<GitCommit>(() => this.RetrieveObject<GitCommit>(this.ResolveReference("ref: HEAD")), true);
            _packList = new Lazy<GitPackList>(() => new GitPackList(_gitFolder), true);
            _references = new Lazy<GitReference[]>(() =>
            {
                List<string> referenceNames = _gitFolder.ListFiles("refs", true).ToList();

                if (_gitFolder.FileExists("HEAD"))
                    referenceNames.Insert(0, "HEAD");

                return referenceNames.Select(n => new GitReference(n, this.ResolveReference("ref: " + n))).ToArray();
            });
        }

        public GitObject RetrieveObject(GitObjectId id)
        {
            if (_objectCache.ContainsKey(id))
            {
                return _objectCache[id];
            }
            else
            {
                GitObject newObject = null;

                string fileName = "objects/" + id.Sha.Substring(0, 2) + "/" + id.Sha.Substring(2);

                if (_gitFolder.FileExists(fileName))
                {
                    newObject = GitObject.CreateFromRaw(id, _gitFolder.ReadFile(fileName));
                }
                else
                {
                    newObject = _packList.Value.RetrieveObject(id);
                }

                _objectCache[id] = newObject;
                return newObject;
            }
        }

        public GitObjectId ResolveReference(string reference)
        {
            if (_referenceCache.ContainsKey(reference))
            {
                return _referenceCache[reference];
            }
            else
            {
                GitObjectId newId = null;

                if (reference.StartsWith("ref: "))
                {
                    if (_gitFolder.FileExists(reference.Substring(5)))
                    {
                        newId = this.ResolveReference(_gitFolder.ReadAllLines(reference.Substring(5)).First());
                    }
                    else
                    {
                        // TODO: look into packed-refs
                        newId = null;
                    }
                }
                else
                {
                    newId = new GitObjectId(reference);
                }

                _referenceCache[reference] = newId;
                return newId;
            }
        }

        public void Dispose()
        {
            _gitFolder.Dispose();
        }
    }
}