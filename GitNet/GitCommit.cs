using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GitNet
{
    public sealed class GitCommit : GitObject
    {
        private static readonly Regex _authorCommitterRegex = new Regex(@"^(?<Name>.+)\s\<(?<MailAddress>.+)\>\s(?<Timestamp>\d+)\s(?<TimeOffset>[+|\-]\d{4})$");

        private readonly GitObjectId _treeId;
        private readonly GitObjectId[] _parentIds;
        private readonly GitAuthor _author;
        private readonly GitAuthor _committer;
        private readonly string _message;

        public GitObjectId TreeId
        {
            get { return _treeId; }
        }

        public GitObjectId[] ParentId
        {
            get { return _parentIds; }
        }

        public GitAuthor Author
        {
            get { return _author; }
        }

        public GitAuthor Committer
        {
            get { return _committer; }
        }

        public string Message
        {
            get { return _message; }
        }

        public GitCommit(GitObjectId id, byte[] rawContent)
            : base(id, rawContent)
        {
            List<GitObjectId> parentIds = new List<GitObjectId>();

            int i = 0;
            while (i < rawContent.Length)
            {
                string line = GetNextLine(rawContent, ref i);

                if (line.StartsWith("tree "))
                {
                    _treeId = new GitObjectId(line.Substring(5));
                }
                else if (line.StartsWith("parent "))
                {
                    parentIds.Add(new GitObjectId(line.Substring(7)));
                }
                else if (line.StartsWith("author "))
                {
                    Match match = _authorCommitterRegex.Match(line.Substring(7));

                    if (match.Success)
                    {
                        DateTime authorDate = ConvertUnixTimestampToDateTime(match.Groups["Timestamp"].Value, match.Groups["TimeOffset"].Value);
                        _author = new GitAuthor(match.Groups["Name"].Value, match.Groups["MailAddress"].Value, authorDate);
                    }
                    else
                    {
                        throw new Exception("Invalid author line format");
                    }
                }
                else if (line.StartsWith("committer "))
                {
                    Match match = _authorCommitterRegex.Match(line.Substring(10));

                    if (match.Success)
                    {
                        DateTime committerDate = ConvertUnixTimestampToDateTime(match.Groups["Timestamp"].Value, match.Groups["TimeOffset"].Value);
                        _committer = new GitAuthor(match.Groups["Name"].Value, match.Groups["MailAddress"].Value, committerDate);
                    }
                    else
                    {
                        throw new Exception("Invalid committer line format");
                    }
                }
                else if (line == "")
                {
                    _message = GitObject.Encoding.GetString(rawContent, i, rawContent.Length - i);
                    break;
                }
            }

            _parentIds = parentIds.ToArray();
        }

        private static DateTime ConvertUnixTimestampToDateTime(string seconds, string offset)
        {
            int offsetInt = Convert.ToInt32(offset);

            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(long.Parse(seconds))
                .Add(new TimeSpan(offsetInt / 100, offsetInt % 100, 0));
        }

        private static string GetNextLine(byte[] rawContent, ref int i)
        {
            int start = i;

            while (i < rawContent.Length)
            {
                if (rawContent[i++] == 10)
                {
                    return GitObject.Encoding.GetString(rawContent, start, i - start - 1);
                }
            }

            return GitObject.Encoding.GetString(rawContent, start, rawContent.Length - start);
        }
    }
}