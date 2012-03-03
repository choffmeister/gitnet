﻿using System;
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

        public GitCommit Head
        {
            get { return _head.Value; }
        }

        public GitRepository(IGitFolder gitFolder)
        {
            _gitFolder = gitFolder;
            _objectCache = new Dictionary<GitObjectId, GitObject>();
            _referenceCache = new Dictionary<string, GitObjectId>();

            _head = new Lazy<GitCommit>(() => this.RetrieveObject<GitCommit>(this.ResolveReference("ref: HEAD")), true);
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
                    // TODO: look into packed objects
                    newObject = null;
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