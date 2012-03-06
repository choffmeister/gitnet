﻿using System.Collections.Generic;
using System.IO;
using GitNet.VirtualizedGitFolder;

namespace GitNet
{
    public class GitObjectDatabase
    {
        private readonly IGitFolder _gitFolder;
        private readonly Dictionary<GitObjectId, GitObject> _cache;

        public GitObjectDatabase(IGitFolder gitfolder)
        {
            _gitFolder = gitfolder;

            _cache = new Dictionary<GitObjectId, GitObject>();
        }

        public GitObject Retrieve(GitObjectId id)
        {
            if (_cache.ContainsKey(id))
            {
                return _cache[id];
            }
            else
            {
                GitObject newObject = this.RetrieveUncached(id);
                _cache[id] = newObject;
                return newObject;
            }
        }

        private GitObject RetrieveUncached(GitObjectId id)
        {
            string fileName = "objects/" + id.Sha.Substring(0, 2) + "/" + id.Sha.Substring(2);

            if (_gitFolder.FileExists(fileName))
            {
                using (Stream file = _gitFolder.ReadFile(fileName))
                {
                    GitBinaryReaderWriter rw = new GitBinaryReaderWriter(file);

                    return rw.ReadHeaderedGitObject(id);
                }
            }
            else if (_gitFolder.FileExists("objects/info/packs"))
            {
                using (Stream file = _gitFolder.ReadFile("objects/info/packs"))
                {
                    GitBinaryReaderWriter rw = new GitBinaryReaderWriter(file);

                    string line;
                    while ((line = rw.ReadLine()) != null)
                    {
                        if (line.Length >= 2 && line.StartsWith("P "))
                        {
                            GitPack pack = new GitPack(_gitFolder, line.Substring(2, line.Length - 7));
                            GitObject go = pack.RetrieveObject(id);

                            if (go != null) return go;
                        }
                    }
                }
            }

            return null;
        }
    }
}