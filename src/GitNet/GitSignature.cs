using System;
using System.Text.RegularExpressions;

namespace GitNet
{
    [System.Diagnostics.DebuggerDisplay("Name={Name}, MailAddress={MailAddress}")]
    public class GitSignature
    {
        private static readonly Regex _regex = new Regex(@"^(?<Name>.+)\s\<(?<MailAddress>.+)\>\s(?<Timestamp>\d+)\s(?<TimeOffset>[+|\-]\d{4})$");

        private readonly string _name;
        private readonly string _mailAddress;
        private readonly DateTime _date;

        public string Name
        {
            get { return _name; }
        }

        public string MailAddress
        {
            get { return _mailAddress; }
        }

        public DateTime Date
        {
            get { return _date; }
        }

        public GitSignature(string name, string mailAddress, DateTime date)
        {
            _name = name;
            _mailAddress = mailAddress;
            _date = date;
        }

        public GitSignature(string rawString)
        {
            Match match = _regex.Match(rawString);

            if (match.Success)
            {
                _name = match.Groups["Name"].Value;
                _mailAddress = match.Groups["MailAddress"].Value;
                _date = ConvertUnixTimestampToDateTime(match.Groups["Timestamp"].Value, match.Groups["TimeOffset"].Value);
            }
            else
            {
                throw new Exception("Invalid signature raw format");
            }
        }

        private static DateTime ConvertUnixTimestampToDateTime(string seconds, string offset)
        {
            int offsetInt = Convert.ToInt32(offset);

            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(long.Parse(seconds))
                .Add(new TimeSpan(offsetInt / 100, offsetInt % 100, 0));
        }
    }
}