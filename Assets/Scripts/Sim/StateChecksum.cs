using System.Globalization;
using System.Text;

namespace KindredSiege.Sim
{
    /// FNV-1a 64 over an ordered state snapshot string. Same function as Desk 42's OfficeStateChecksum.
    /// The snapshot must be built in a fixed field order or the checksum means nothing.
    public static class StateChecksum
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static ulong ComputeRaw(string orderedSnapshot)
        {
            ulong hash = OffsetBasis;
            string s = orderedSnapshot ?? string.Empty;
            for (int i = 0; i < s.Length; i++)
            {
                hash ^= s[i];
                hash *= Prime;
            }
            return hash;
        }

        public static string Compute(string orderedSnapshot)
        {
            return ComputeRaw(orderedSnapshot).ToString("X16", CultureInfo.InvariantCulture);
        }
    }

    /// Builds the ordered snapshot. Call Field in the same order every time.
    public sealed class SnapshotBuilder
    {
        private readonly StringBuilder _b = new StringBuilder(256);

        public SnapshotBuilder Section(string name)
        {
            _b.Append('[').Append(name).Append(']');
            return this;
        }

        public SnapshotBuilder Field(string name, long value)
        {
            _b.Append(name).Append('=').Append(value.ToString(CultureInfo.InvariantCulture)).Append(';');
            return this;
        }

        public SnapshotBuilder Field(string name, ulong value)
        {
            _b.Append(name).Append('=').Append(value.ToString(CultureInfo.InvariantCulture)).Append(';');
            return this;
        }

        public SnapshotBuilder Field(string name, bool value)
        {
            _b.Append(name).Append('=').Append(value ? '1' : '0').Append(';');
            return this;
        }

        public SnapshotBuilder Field(string name, string value)
        {
            _b.Append(name).Append('=').Append(value ?? string.Empty).Append(';');
            return this;
        }

        public override string ToString()
        {
            return _b.ToString();
        }
    }
}
