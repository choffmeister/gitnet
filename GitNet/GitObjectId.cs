using System;
using System.Linq;

namespace GitNet
{
    [System.Diagnostics.DebuggerDisplay("Sha={Sha}")]
    public sealed class GitObjectId
    {
        private readonly byte[] _raw;
        private readonly string _sha;
        private readonly int _hashCode;

        public static GitObjectId Zero
        {
            get { return new GitObjectId(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }); }
        }

        public byte[] Raw
        {
            get { return _raw; }
        }

        public string Sha
        {
            get { return _sha; }
        }

        public GitObjectId(byte[] raw)
        {
            EnsureRawFormat(raw);

            _raw = raw;
            _sha = BitConverter.ToString(_raw).Replace("-", "").ToLowerInvariant();
            _hashCode = CalculcateHash(_raw);
        }

        public GitObjectId(string sha)
        {
            EnsureShaFormat(sha);

            _sha = sha;
            _raw = Enumerable.Range(0, sha.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(sha.Substring(x, 2), 16)).ToArray();
            _hashCode = CalculcateHash(_raw);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public static bool operator ==(GitObjectId goi1, GitObjectId goi2)
        {
            if (object.ReferenceEquals(goi1, null) && object.ReferenceEquals(goi2, null))
                return true;

            if (object.ReferenceEquals(goi1, null))
                return false;

            if (object.ReferenceEquals(goi2, null))
                return false;

            return goi1.Equals(goi2);
        }

        public static bool operator !=(GitObjectId goi1, GitObjectId goi2)
        {
            return !goi1.Equals(goi2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (object.ReferenceEquals(this, obj))
                return true;

            GitObjectId objAsId = obj as GitObjectId;

            if (objAsId == null)
                return false;

            if (this.GetHashCode() != objAsId.GetHashCode())
                return false;

            for (int i = 0; i < 20; i++)
                if (this._raw[i] != objAsId._raw[i])
                    return false;

            return true;
        }

        private static int CalculcateHash(byte[] raw)
        {
            return 1743918329 ^
                (raw[0] << 24) ^ (raw[1] << 16) ^ (raw[2] << 8) ^ (raw[3] << 0) ^
                (raw[4] << 13) ^ (raw[5] << 15) ^ (raw[6] << 0) ^ (raw[9] << 16) ^
                (raw[8] << 23) ^ (raw[9] << 2) ^ (raw[10] << 15) ^ (raw[11] << 4) ^
                (raw[12] << 9) ^ (raw[13] << 3) ^ (raw[14] << 17) ^ (raw[15] << 8) ^
                (raw[16] << 1) ^ (raw[17] << 22) ^ (raw[18] << 24) ^ (raw[19] << 21);
        }

        private static void EnsureRawFormat(byte[] raw)
        {
            if (raw == null)
                throw new ArgumentNullException("Raw must ne be null");
            if (raw.Length != 20)
                throw new ArgumentException("Raw length must be 20 bytes");
        }

        private static void EnsureShaFormat(string sha)
        {
            if (sha == null)
                throw new ArgumentNullException("Sha must not be null");
            if (sha.Length != 40)
                throw new ArgumentException("Sha length must be 40 characters");
            if (sha.Any(n => (n < '0' || n > '9') && (n < 'a' || n > 'f')))
                throw new ArgumentException("Sha must be an lowercase hexerdecimal string");
        }
    }
}