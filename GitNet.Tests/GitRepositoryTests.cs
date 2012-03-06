using System.Collections.Generic;
using System.Linq;
using GitNet.VirtualizedGitFolder;
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
            _folder = new FileSystemGitFolder("TestRepository");
            _repo = new GitRepository(_folder);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _repo.Dispose();
        }

        [Test]
        public void RetrieveLooseObjects()
        {
            GitObject commit = _repo.RetrieveObject("3ea91f0a360b8288b46d064e5cd4296a26020cfd");
            Assert.IsInstanceOf(typeof(GitCommit), commit);
            Assert.AreEqual("Test User", ((GitCommit)commit).Committer.Name);
            Assert.AreEqual("test.user@invalid.domain.tld", ((GitCommit)commit).Committer.MailAddress);

            GitObject tree = _repo.RetrieveObject("2f22b42434938c3dc11695064ecf0c04add85711");
            Assert.IsInstanceOf(typeof(GitTree), tree);

            GitObject blob = _repo.RetrieveObject("01e79c32a8c99c557f0757da7cb6d65b3414466d");
            Assert.IsInstanceOf(typeof(GitBlob), blob);

            GitObject tag = _repo.RetrieveObject("6402a88dd851421f4a6d6e0baf2b7b0ed17e0048");
            Assert.IsInstanceOf(typeof(GitTag), tag);

            GitObject nonExistent = _repo.RetrieveObject(GitObjectId.Zero);
            Assert.IsNull(nonExistent);
        }

        [Test]
        public void RetrieveAllLooseObjects()
        {
            int count = 0;

            foreach (var objFolder in _folder.ListSubdirectories("objects").Where(n => n != "objects/info" && n != "objects/pack"))
            {
                foreach (var objFile in _folder.ListFiles(objFolder))
                {
                    GitObject obj = _repo.RetrieveObject(objFile.Substring(8, 2) + objFile.Substring(11));

                    count++;
                }
            }

            Assert.AreEqual(12, count);
        }

        [Test]
        public void RetrievePackedObject()
        {
            GitObject commit = _repo.RetrieveObject("2d350445a98ac37a1735e00a3fc546e58d757896");

            Assert.IsNotNull(commit);
            Assert.IsInstanceOf(typeof(GitCommit), commit);
            Assert.AreEqual("choffmeister", ((GitCommit)commit).Author.Name);
            Assert.AreEqual("fifth\n", ((GitCommit)commit).Message);
        }

        [Test]
        public void ResolveReferences()
        {
            Assert.AreEqual(new GitObjectId("3ea91f0a360b8288b46d064e5cd4296a26020cfd"), _repo.ResolveReference("ref: HEAD"));
            Assert.IsNull(_repo.ResolveReference("ref: refs/heads/nonexistentbranch"));
        }

        [Test]
        public void Head()
        {
            GitCommit head = _repo.Head;
        }

        [Test]
        public void References()
        {
            List<GitReference> references = _repo.References;

            Assert.AreEqual(3, references.Count);
            Assert.AreEqual("HEAD", references[0].Name);
            Assert.AreEqual("refs/heads/master", references[1].Name);
            Assert.AreEqual("refs/tags/foo-tag", references[2].Name);
        }
    }
}