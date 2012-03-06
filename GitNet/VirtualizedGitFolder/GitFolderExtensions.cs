using System;
using System.IO;

namespace GitNet.VirtualizedGitFolder
{
    public static class GitFolderExtensions
    {
        public static string ReadAllText(this IGitFolder gitFolder, string path)
        {
            using (StreamReader reader = new StreamReader(gitFolder.ReadFile(path), GitBinaryReaderWriter.Encoding))
            {
                return reader.ReadToEnd();
            }
        }

        public static string[] ReadAllLines(this IGitFolder gitFolder, string path)
        {
            return gitFolder.ReadAllText(path).Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        }

        public static byte[] ReadAllBytes(this IGitFolder gitFolder, string path)
        {
            using (Stream file = gitFolder.ReadFile(path))
            {
                GitBinaryReaderWriter rw = new GitBinaryReaderWriter(file);

                return rw.ReadBytes();
            }
        }
    }
}