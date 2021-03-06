﻿using System.Collections.Generic;
namespace GitNet
{
    public static class GitRepositoryExtensions
    {
        public static GitObject RetrieveObject(this GitRepository repository, string id)
        {
            return repository.RetrieveObject(new GitObjectId(id));
        }

        public static GitObject RetrieveObject(this GitRepository repository, byte[] id)
        {
            return repository.RetrieveObject(new GitObjectId(id));
        }

        public static T RetrieveObject<T>(this GitRepository repository, GitObjectId id)
            where T : GitObject
        {
            return (T)repository.RetrieveObject(id);
        }

        public static T RetrieveObject<T>(this GitRepository repository, string id)
            where T : GitObject
        {
            return (T)repository.RetrieveObject(id);
        }

        public static T RetrieveObject<T>(this GitRepository repository, byte[] id)
            where T : GitObject
        {
            return (T)repository.RetrieveObject(id);
        }

        public static IEnumerable<GitCommit> GetCommits(this GitRepository repository)
        {
            return repository.GetCommits("ref: refs/heads/master");
        }

        public static IEnumerable<GitCommit> GetCommits(this GitRepository repository, string reference)
        {
            return repository.GetCommits(repository.RetrieveObject<GitCommit>(repository.ResolveReference(reference)));
        }
    }
}