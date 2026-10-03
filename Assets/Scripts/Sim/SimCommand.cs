using System;

namespace KindredSiege.Sim
{
    /// One player or system input, stamped with the tick it applies on.
    /// Same field shape as Desk 42's OfficeCommand so one replay checker can read both games.
    public sealed class SimCommand
    {
        public SimCommand(
            int schemaVersion,
            long tick,
            int sequence,
            string kind,
            string actorId,
            string targetId,
            int arg0,
            int arg1,
            string textArg)
        {
            SchemaVersion = schemaVersion;
            Tick = tick;
            Sequence = sequence;
            Kind = kind ?? string.Empty;
            ActorId = actorId ?? string.Empty;
            TargetId = targetId ?? string.Empty;
            Arg0 = arg0;
            Arg1 = arg1;
            TextArg = textArg ?? string.Empty;
        }

        public int SchemaVersion { get; }
        public long Tick { get; }
        public int Sequence { get; }
        public string Kind { get; }
        public string ActorId { get; }
        public string TargetId { get; }
        public int Arg0 { get; }
        public int Arg1 { get; }
        public string TextArg { get; }

        public bool SameAs(SimCommand other)
        {
            return other != null &&
                SchemaVersion == other.SchemaVersion &&
                Tick == other.Tick &&
                Sequence == other.Sequence &&
                string.Equals(Kind, other.Kind, StringComparison.Ordinal) &&
                string.Equals(ActorId, other.ActorId, StringComparison.Ordinal) &&
                string.Equals(TargetId, other.TargetId, StringComparison.Ordinal) &&
                Arg0 == other.Arg0 &&
                Arg1 == other.Arg1 &&
                string.Equals(TextArg, other.TextArg, StringComparison.Ordinal);
        }
    }
}
