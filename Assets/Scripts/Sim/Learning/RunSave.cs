using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// Writes a RunMemory down and reads it back, so what the chiefs learned about you, your own
    /// fighters' reflexes and the codex survive closing the game (Codex audit, 6 Oct 2026: they
    /// only lived as long as the process). The screen owns the file; this only turns the memory
    /// into text and back. A run in progress is not saved: losing or quitting starts a new run.
    ///
    /// The text is versioned, one record a line, fields split by tabs, with backslash escapes
    /// for tabs, line breaks and backslashes. Lessons keep their sentences and evidence, but their
    /// fights are not saved, so a lesson from before a reload can be read and not replayed.
    public static class RunSave
    {
        public const string Header = "kindred-siege-memory";
        public const int Version = 1;

        public static string Write(RunMemory memory)
        {
            if (memory == null) throw new ArgumentNullException(nameof(memory));
            var b = new StringBuilder();
            Line(b, Header, N(Version));
            Line(b, "runs", N(memory.RunsStarted), N(memory.RunsWon));
            for (int i = 0; i < memory.Codex.Count; i++) Line(b, "codex", memory.Codex[i]);
            WriteMind(b, memory.ClarthMind);
            WriteMind(b, memory.RaiderMind);
            WriteMind(b, memory.SkullpeakMind);
            Line(b, "end");
            return b.ToString();
        }

        /// The memory saved in this text. Throws FormatException if the text is not a save this
        /// version can read.
        public static RunMemory Read(string text)
        {
            if (!TryRead(text, out RunMemory memory, out string failure)) throw new FormatException(failure);
            return memory;
        }

        public static bool TryRead(string text, out RunMemory memory, out string failure)
        {
            memory = null;
            failure = string.Empty;
            if (string.IsNullOrEmpty(text))
            {
                failure = "The save is empty.";
                return false;
            }
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            var read = new RunMemory();
            TribeMind mind = null;
            bool headerSeen = false;
            bool ended = false;
            for (int n = 0; n < lines.Length; n++)
            {
                if (lines[n].Length == 0) continue;
                string[] f = lines[n].Split('\t');
                for (int i = 0; i < f.Length; i++) f[i] = Unescape(f[i]);
                string where = "Line " + (n + 1).ToString(CultureInfo.InvariantCulture) + ": ";
                if (ended)
                {
                    failure = where + "text after the end of the save.";
                    return false;
                }
                if (!headerSeen)
                {
                    if (f.Length != 2 || f[0] != Header)
                    {
                        failure = where + "this is not a Kindred Siege memory save.";
                        return false;
                    }
                    if (!TryInt(f[1], out int version) || version != Version)
                    {
                        failure = where + "saved as version " + f[1] + ", this game reads version " + N(Version) + ".";
                        return false;
                    }
                    headerSeen = true;
                    continue;
                }
                switch (f[0])
                {
                    case "runs":
                        if (f.Length != 3 || !TryInt(f[1], out int started) || !TryInt(f[2], out int won))
                            return Fail(where + "bad runs record.", out failure);
                        read.RunsStarted = started;
                        read.RunsWon = won;
                        break;
                    case "codex":
                        if (f.Length != 2) return Fail(where + "bad codex record.", out failure);
                        read.Discover(f[1]);
                        break;
                    case "mind":
                        if (f.Length != 2) return Fail(where + "bad mind record.", out failure);
                        mind = MindFor(read, f[1]);
                        if (mind == null) return Fail(where + "no tribe called " + f[1] + ".", out failure);
                        break;
                    case "habit":
                        if (mind == null || f.Length != 3 || !TryInt(f[1], out int habit) || habit < 0 ||
                            habit >= TribeMind.HabitCount || !TryInt(f[2], out int value))
                            return Fail(where + "bad habit record.", out failure);
                        mind.SetHabit((Habit)habit, value);
                        break;
                    case "threat":
                        if (mind == null || f.Length != 3 || !TryInt(f[1], out int unitClass) || unitClass < 0 ||
                            unitClass >= SliceClasses.Count || !TryInt(f[2], out int threat))
                            return Fail(where + "bad threat record.", out failure);
                        mind.SetThreatMemory((UnitClass)unitClass, threat);
                        break;
                    case "learned":
                        if (mind == null || f.Length != 2) return Fail(where + "bad learned record.", out failure);
                        mind.MarkLearned(f[1]);
                        break;
                    case "lesson":
                        if (mind == null || f.Length != 11 || !TryInt(f[2], out int side) || !TryInt(f[4], out int subject) ||
                            subject < 0 || subject >= SliceClasses.Count || !TryInt(f[5], out int before) ||
                            !TryInt(f[6], out int after) || !TryInt(f[7], out int evidence) ||
                            !TryLong(f[8], out long from) || !TryLong(f[9], out long to))
                            return Fail(where + "bad lesson record.", out failure);
                        mind.Record(new Lesson(mind.TribeId, f[1], side, f[3], (UnitClass)subject, before, after, evidence, from, to, f[10]));
                        break;
                    case "end":
                        if (f.Length != 1) return Fail(where + "bad end record.", out failure);
                        ended = true;
                        break;
                    default:
                        return Fail(where + "unknown record " + f[0] + ".", out failure);
                }
            }
            if (!headerSeen) return Fail("This is not a Kindred Siege memory save.", out failure);
            if (!ended) return Fail("The save stops before its end, so it was cut short.", out failure);
            memory = read;
            return true;
        }

        private static void WriteMind(StringBuilder b, TribeMind mind)
        {
            Line(b, "mind", mind.TribeId);
            for (int i = 0; i < TribeMind.HabitCount; i++) Line(b, "habit", N(i), N(mind.HabitValue((Habit)i)));
            for (int i = 0; i < SliceClasses.Count; i++) Line(b, "threat", N(i), N(mind.ThreatMemory((UnitClass)i)));
            for (int i = 0; i < mind.FightsLearned.Count; i++) Line(b, "learned", mind.FightsLearned[i]);
            for (int i = 0; i < mind.Lessons.Count; i++)
            {
                Lesson l = mind.Lessons[i];
                Line(b, "lesson", l.FightId, N(l.LearnerSide), l.Rule, N((int)l.Subject), N(l.ValueBefore), N(l.ValueAfter),
                    N(l.Evidence), l.FromTick.ToString(CultureInfo.InvariantCulture), l.ToTick.ToString(CultureInfo.InvariantCulture), l.Why);
            }
        }

        private static TribeMind MindFor(RunMemory memory, string tribeId)
        {
            if (tribeId == memory.ClarthMind.TribeId) return memory.ClarthMind;
            if (tribeId == memory.RaiderMind.TribeId) return memory.RaiderMind;
            if (tribeId == memory.SkullpeakMind.TribeId) return memory.SkullpeakMind;
            return null;
        }

        private static void Line(StringBuilder b, params string[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0) b.Append('\t');
                b.Append(Escape(fields[i]));
            }
            b.Append('\n');
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var b = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': b.Append("\\\\"); break;
                    case '\t': b.Append("\\t"); break;
                    case '\n': b.Append("\\n"); break;
                    case '\r': b.Append("\\r"); break;
                    default: b.Append(c); break;
                }
            }
            return b.ToString();
        }

        private static string Unescape(string s)
        {
            if (s.IndexOf('\\') < 0) return s;
            var b = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c != '\\' || i + 1 >= s.Length)
                {
                    b.Append(c);
                    continue;
                }
                char next = s[++i];
                b.Append(next == 't' ? '\t' : next == 'n' ? '\n' : next == 'r' ? '\r' : next);
            }
            return b.ToString();
        }

        private static bool Fail(string why, out string failure)
        {
            failure = why;
            return false;
        }

        private static bool TryInt(string s, out int value)
        {
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryLong(string s, out long value)
        {
            return long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static string N(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
