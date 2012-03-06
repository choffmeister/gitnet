using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GitNet
{
    public class GitCommitCollection : IEnumerable<GitCommit>
    {
        private readonly GitObjectDatabase _objectDatabase;
        private readonly GitCommit _head;

        public GitCommitCollection(GitObjectDatabase objectDatabase, GitCommit head)
        {
            _objectDatabase = objectDatabase;
            _head = head;
        }

        public IEnumerator<GitCommit> GetEnumerator()
        {
            return new GitCommitCollectionEnumerator(_objectDatabase, _head);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new GitCommitCollectionEnumerator(_objectDatabase, _head);
        }
    }

    public class GitCommitCollectionEnumerator : IEnumerator<GitCommit>
    {
        private readonly GitObjectDatabase _objectDatabase;
        private readonly GitCommit _head;

        private List<GitCommit> _currents;
        private GitCommit _current;

        public GitCommitCollectionEnumerator(GitObjectDatabase objectDatabase, GitCommit head)
        {
            _objectDatabase = objectDatabase;
            _head = head;

            _currents = new List<GitCommit>();
            _currents.Add(_head);
            _current = null;
        }

        public GitCommit Current
        {
            get { return _current; }
        }

        object IEnumerator.Current
        {
            get { return _current; }
        }

        public bool MoveNext()
        {
            _current = _currents.FirstOrDefault();

            if (_current != null)
            {
                _currents.Remove(_current);
                _currents = _currents.Union(_current.ParentId.Select(n => (GitCommit)_objectDatabase.Retrieve(n))).OrderByDescending(n => n.Committer.Date).ToList();

                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
        }
    }
}