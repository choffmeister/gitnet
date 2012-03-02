﻿using GitNet.VirtualizedGitFolder;
using NUnit.Framework;

namespace GitNet.Tests
{
    [TestFixture]
    public class GitRepositoryTests
    {
        private GitRepository _repo;
        private IGitFolder _folder;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _folder = new WindowsFileSystemGitFolder("TestRepository");
            _repo = new GitRepository(_folder);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _repo.Dispose();
        }

        [Test]
        public void Lookup()
        {
            GitObject commit = _repo.Lookup("3ea91f0a360b8288b46d064e5cd4296a26020cfd");
            Assert.IsInstanceOf(typeof(GitCommit), commit);

            GitObject tree = _repo.Lookup("2f22b42434938c3dc11695064ecf0c04add85711");
            Assert.IsInstanceOf(typeof(GitTree), tree);

            GitObject blob = _repo.Lookup("01e79c32a8c99c557f0757da7cb6d65b3414466d");
            Assert.IsInstanceOf(typeof(GitBlob), blob);

            GitObject tag = _repo.Lookup("6402a88dd851421f4a6d6e0baf2b7b0ed17e0048");
            Assert.IsInstanceOf(typeof(GitTag), tag);
        }

        [Test]
        public void LookupAll()
        {
            int count = 0;

            foreach (var objFolder in _folder.ListSubdirectories("objects"))
            {
                foreach (var objFile in _folder.ListFiles(objFolder))
                {
                    GitObject obj = _repo.Lookup(objFile.Substring(8, 2) + objFile.Substring(11));

                    count++;
                }
            }

            Assert.AreEqual(12, count);
        }

        [Test]
        public void ResolveReferences()
        {
            Assert.AreEqual(new GitObjectId("3ea91f0a360b8288b46d064e5cd4296a26020cfd"), _repo.ResolveReference("ref: HEAD"));
        }
    }
}