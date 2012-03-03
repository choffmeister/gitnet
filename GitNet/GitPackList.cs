using System.Linq;
using GitNet.VirtualizedGitFolder;

namespace GitNet
{
    public class GitPackList
    {
        private readonly IGitFolder _gitFolder;
        private readonly string[] _packNames;

        public string[] PackNames
        {
            get { return _packNames; }
        }

        public GitPackList(IGitFolder gitFolder)
        {
            _gitFolder = gitFolder;

            if (_gitFolder.FileExists("objects/info/packs"))
            {
                _packNames = _gitFolder.ReadAllLines("objects/info/packs").Where(n => n.StartsWith("P ")).Select(n => n.Substring(2, n.Length - 7)).ToArray();
            }
            else
            {
                _packNames = new string[0];
            }
        }

        public GitObject RetrieveObject(GitObjectId id)
        {
            foreach (string packName in _packNames)
            {
                GitPack pack = new GitPack(_gitFolder, packName);
                GitObject go = pack.RetrieveObject(id);

                if (go != null)
                {
                    return go;
                }
            }

            return null;
        }
    }
}