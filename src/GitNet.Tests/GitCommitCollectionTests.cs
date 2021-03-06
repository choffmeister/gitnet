﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace GitNet.Tests
{
    [TestFixture]
    public class GitCommitCollectionTests
    {
        [Test]
        public void Test1()
        {
            IGitFolder folder = new FileSystemGitFolder("TestRepository2");
            GitRepository repo = new GitRepository(folder);
            List<GitCommit> commits = new GitCommitCollection(repo.ObjectDatabase, repo.Head).ToList();

            Assert.AreEqual(39, commits.Count);

            Assert.AreEqual("79dcc06", commits [0].Id.Sha.Substring(0, 7));
            Assert.AreEqual("b4651e5", commits [1].Id.Sha.Substring(0, 7));
            Assert.AreEqual("0e3f2a9", commits [2].Id.Sha.Substring(0, 7));
            Assert.AreEqual("076c18d", commits [3].Id.Sha.Substring(0, 7));
            Assert.AreEqual("2e2f5e4", commits [4].Id.Sha.Substring(0, 7));
            Assert.AreEqual("31e7821", commits [5].Id.Sha.Substring(0, 7));
            Assert.AreEqual("14c1d8a", commits [6].Id.Sha.Substring(0, 7));
            Assert.AreEqual("5506ba3", commits [7].Id.Sha.Substring(0, 7));
            Assert.AreEqual("07d5468", commits [8].Id.Sha.Substring(0, 7));
            Assert.AreEqual("43cd0f3", commits [9].Id.Sha.Substring(0, 7));
            Assert.AreEqual("a095150", commits [10].Id.Sha.Substring(0, 7));
            Assert.AreEqual("50ec113", commits [11].Id.Sha.Substring(0, 7));
            Assert.AreEqual("b5198b9", commits [12].Id.Sha.Substring(0, 7));
            Assert.AreEqual("c5958cd", commits [13].Id.Sha.Substring(0, 7));
            Assert.AreEqual("c7ac193", commits [14].Id.Sha.Substring(0, 7));
        }

        [Test]
        public void Test2()
        {
            IGitFolder folder = new FileSystemGitFolder("TestRepository3");
            GitRepository repo = new GitRepository(folder);
            List<GitCommit> commits = new GitCommitCollection(repo.ObjectDatabase, repo.Head).ToList();
        }
    }
}