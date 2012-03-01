using GitNet.VirtualizedGitFolder;
using NUnit.Framework;

namespace GitNet.Tests
{
    [TestFixture]
    public class GitRepositoryTests
    {
        [Test]
        public void Constructor()
        {
            GitRepository repo = new GitRepository(new WindowsFileSystemGitFolder("TestRepository"));
        }

        [Test]
        public void Lookup()
        {
            GitRepository repo = new GitRepository(new WindowsFileSystemGitFolder("TestRepository"));

            GitObject commit = repo.Lookup("3ea91f0a360b8288b46d064e5cd4296a26020cfd");
            Assert.IsInstanceOf(typeof(GitCommit), commit);

            GitObject tree = repo.Lookup("2f22b42434938c3dc11695064ecf0c04add85711");
            Assert.IsInstanceOf(typeof(GitTree), tree);

            GitObject blob = repo.Lookup("01e79c32a8c99c557f0757da7cb6d65b3414466d");
            Assert.IsInstanceOf(typeof(GitBlob), blob);

            GitObject tag = repo.Lookup("6402a88dd851421f4a6d6e0baf2b7b0ed17e0048");
            Assert.IsInstanceOf(typeof(GitTag), tag);
        }
    }
}