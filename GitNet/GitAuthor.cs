using System;

namespace GitNet
{
    public class GitAuthor
    {
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

        public GitAuthor(string name, string mailAddress, DateTime date)
        {
            _name = name;
            _mailAddress = mailAddress;
            _date = date;
        }
    }
}