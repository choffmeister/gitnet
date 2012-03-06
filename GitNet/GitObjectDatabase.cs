using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitNet.VirtualizedGitFolder;

namespace GitNet
{
    public class GitObjectDatabase
    {
        private readonly IGitFolder _gitFolder;
        private readonly Dictionary<GitObjectId, GitRawObject> _rawCache;
        private readonly Dictionary<GitObjectId, GitObject> _cache;

        public GitObjectDatabase(IGitFolder gitfolder)
        {
            _gitFolder = gitfolder;

            _rawCache = new Dictionary<GitObjectId, GitRawObject>();
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
                GitObject newObject = this.ParseRaw(id, this.RetrieveRaw(id));
                _cache[id] = newObject;
                return newObject;
            }
        }

        public GitRawObject RetrieveRaw(GitObjectId id)
        {
            if (_rawCache.ContainsKey(id))
            {
                return _rawCache[id];
            }
            else
            {
                GitRawObject newRawObject = this.RetrieveRawUncached(id);
                _rawCache[id] = newRawObject;
                return newRawObject;
            }
        }

        private GitRawObject RetrieveRawUncached(GitObjectId id)
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
            else
            {
                List<string> parsedPackFiles = new List<string>();

                if (_gitFolder.FileExists("objects/info/packs"))
                {
                    using (Stream file = _gitFolder.ReadFile("objects/info/packs"))
                    {
                        GitBinaryReaderWriter rw = new GitBinaryReaderWriter(file);

                        string line;
                        while ((line = rw.ReadLine()) != null)
                        {
                            if (line.Length >= 2 && line.StartsWith("P "))
                            {
                                string packName = line.Substring(2, line.Length - 7);
                                parsedPackFiles.Add(packName);

                                GitPack pack = new GitPack(this, _gitFolder, packName);
                                GitRawObject gro = pack.RetrieveRawObject(id);

                                if (gro != null) return gro;
                            }
                        }
                    }
                }

                foreach (string packName in _gitFolder.ListFiles("objects/pack").Where(n => n.EndsWith(".pack")).Select(n => n.Substring(13, n.Length - 18)).Except(parsedPackFiles))
                {
                    GitPack pack = new GitPack(this, _gitFolder, packName);
                    GitRawObject gro = pack.RetrieveRawObject(id);

                    if (gro != null) return gro;
                }
            }

            return null;
        }

        private GitObject ParseRaw(GitObjectId id, GitRawObject raw)
        {
            if (raw != null)
            {
                switch (raw.Type)
                {
                    case GitRawObjectType.Commit:
                        return new GitCommit(id, new MemoryStream(raw.Data));
                    case GitRawObjectType.Tree:
                        return new GitTree(id, new MemoryStream(raw.Data));
                    case GitRawObjectType.Blob:
                        return new GitBlob(id, new MemoryStream(raw.Data));
                    case GitRawObjectType.Tag:
                        return new GitTag(id, new MemoryStream(raw.Data));
                    default:
                        throw new NotSupportedException();
                }
            }

            return null;
        }
    }
}