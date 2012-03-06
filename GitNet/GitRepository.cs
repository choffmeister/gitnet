using System;
using System.Collections.Generic;
using GitNet.VirtualizedGitFolder;

namespace GitNet
{
    public sealed class GitRepository : IDisposable
    {
        private readonly IGitFolder _gitFolder;

        private Lazy<GitCommit> _head;
        private Lazy<GitObjectDatabase> _objectDatabase;
        private Lazy<GitReferenceDatabase> _referenceDatabase;

        public GitCommit Head
        {
            get { return _head.Value; }
        }

        public GitObjectDatabase ObjectDatabase
        {
            get { return _objectDatabase.Value; }
        }

        public GitReferenceDatabase ReferenceDatabase
        {
            get { return _referenceDatabase.Value; }
        }

        public List<GitReference> References
        {
            get { return _referenceDatabase.Value.List(); }
        }

        public GitRepository(IGitFolder gitFolder)
        {
            _gitFolder = gitFolder;

            _head = new Lazy<GitCommit>(() => this.RetrieveObject<GitCommit>(this.ResolveReference("ref: HEAD")), true);
            _objectDatabase = new Lazy<GitObjectDatabase>(() => new GitObjectDatabase(_gitFolder), true);
            _referenceDatabase = new Lazy<GitReferenceDatabase>(() => new GitReferenceDatabase(_gitFolder), true);
        }

        public IEnumerable<GitCommit> GetCommits()
        {
            return new GitCommitCollection(_objectDatabase.Value, _head.Value);
        }

        public GitObject RetrieveObject(GitObjectId id)
        {
            return _objectDatabase.Value.Retrieve(id);
        }

        public GitObjectId ResolveReference(string reference)
        {
            return _referenceDatabase.Value.Resolve(reference);
        }

        public void Dispose()
        {
            _gitFolder.Dispose();
        }
    }
}