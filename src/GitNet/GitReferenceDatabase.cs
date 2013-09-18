using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitNet
{
    public class GitReferenceDatabase
    {
        private readonly IGitFolder _gitFolder;
        private readonly Dictionary<string, GitObjectId> _cache;

        public GitReferenceDatabase(IGitFolder gitfolder)
        {
            _gitFolder = gitfolder;

            _cache = new Dictionary<string, GitObjectId>();
        }

        public List<GitReference> List()
        {
            List<string> referenceNames = _gitFolder.ListFiles("refs", true).ToList();

            if (_gitFolder.FileExists("HEAD"))
                referenceNames.Insert(0, "HEAD");

            if (_gitFolder.FileExists("packed-refs"))
            {
                using (Stream file = _gitFolder.ReadFile("packed-refs"))
                {
                    GitBinaryReaderWriter rw = new GitBinaryReaderWriter(file);

                    string line;
                    while ((line = rw.ReadLine()) != null)
                    {
                        if (line.Length > 0 && line.First() != '^' && line.First() != '#')
                        {
                            string referenceName = line.Substring(41);

                            if (!referenceNames.Contains(referenceName))
                            {
                                referenceNames.Add(referenceName);
                            }
                        }
                    }
                }
            }

            return referenceNames.Select(n => new GitReference(n, this.Resolve("ref: " + n))).ToList();
        }

        public GitObjectId Resolve(string reference)
        {
            if (_cache.ContainsKey(reference))
            {
                return _cache [reference];
            }
            else
            {
                GitObjectId newObjectId = this.ResolveUncached(reference);
                _cache [reference] = newObjectId;
                return newObjectId;
            }
        }

        private GitObjectId ResolveUncached(string reference)
        {
            if (reference.StartsWith("ref: "))
            {
                if (_gitFolder.FileExists(reference.Substring(5)))
                {
                    return this.Resolve(_gitFolder.ReadAllLines(reference.Substring(5)).First());
                }
                else if (_gitFolder.FileExists("packed-refs"))
                {
                    using (Stream file = _gitFolder.ReadFile("packed-refs"))
                    {
                        GitBinaryReaderWriter rw = new GitBinaryReaderWriter(file);

                        string line;
                        while ((line = rw.ReadLine()) != null)
                        {
                            if (line.Length > 0 && line.First() != '^' && line.First() != '#' && line.EndsWith(reference.Substring(4)))
                            {
                                return new GitObjectId(line.Substring(0, 40));
                            }
                        }
                    }
                }
            }
            else
            {
                return new GitObjectId(reference);
            }

            return null;
        }
    }
}