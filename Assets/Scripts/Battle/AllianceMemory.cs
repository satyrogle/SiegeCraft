using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KindredSiege.Battle
{
    // Prototype-only factual history. No global tribe mood and no invented off-screen battles.
    public enum AllianceFact { Rescued, Abandoned, Fell, Covered, Salvaged }

    [Serializable]
    public sealed class AllianceMemoryEvent
    {
        public int Fight;
        public int Second;
        public string Subject;
        public string Other;
        public AllianceFact Fact;
    }

    [Serializable]
    public sealed class AllianceSurvivor
    {
        public string Id;
        public int Health;
        public int Sanity;
    }

    [Serializable]
    public sealed class AllianceMemory
    {
        public int Version = 1;
        public bool FirstFightComplete;
        public bool HasSalvage;
        public List<AllianceMemoryEvent> Events = new List<AllianceMemoryEvent>();
        public List<AllianceSurvivor> Survivors = new List<AllianceSurvivor>();

        public bool Record(int fight, int second, string subject, AllianceFact fact, string other = "Commander")
        {
            if (fight < 1 || fight > 2 || second < 0 || !KnownId(subject) || !KnownId(other) ||
                !Enum.IsDefined(typeof(AllianceFact), fact))
                throw new ArgumentException("Invalid prototype memory event.");
            // One meaningful event of each type per subject per fight, even if callbacks repeat.
            for (int i = 0; i < Events.Count; i++)
                if (Events[i].Fight == fight && Events[i].Subject == subject && Events[i].Fact == fact)
                    return false;
            Events.Add(new AllianceMemoryEvent { Fight = fight, Second = second, Subject = subject, Other = other, Fact = fact });
            return true;
        }

        public bool WillCover(string id)
        {
            bool trust = false;
            for (int i = 0; i < Events.Count; i++)
            {
                var e = Events[i];
                if (e.Subject != id) continue;
                if (e.Fact == AllianceFact.Rescued || e.Fact == AllianceFact.Covered) trust = true;
                if (e.Fact == AllianceFact.Abandoned || e.Fact == AllianceFact.Fell) trust = false;
            }
            return trust;
        }

        public static bool KnownId(string id) => id == "Commander" || id == "Ember scout" ||
            id == "Ash guard 1" || id == "Ash guard 2";

        static bool IsGuardId(string id) => id.StartsWith("Ash guard", StringComparison.Ordinal);

        // Trusting guards shield the commander, whoever freed them, so the history says so.
        public static string Describe(AllianceMemoryEvent e)
        {
            string action;
            switch (e.Fact)
            {
                case AllianceFact.Rescued: action = "was freed while your fighters were there. Will shield your commander."; break;
                case AllianceFact.Abandoned: action = "escaped without your help. Keeps distance."; break;
                case AllianceFact.Fell: action = e.Fight == 1 ? "fell. Will not return for fight two." : "fell in fight two."; break;
                case AllianceFact.Covered: action = "saw your commander take the pulse for them. Will now shield your commander."; break;
                case AllianceFact.Salvaged: action = "recovered a disruptor. Two pulse-cancelling charges."; break;
                default: action = "has an unknown record."; break;
            }
            return "F" + e.Fight + " / " + e.Second + "s  " + e.Subject + " " + action;
        }

        // Starting health of each prototype fighter. Spawning and the checkpoint check both read it.
        public static int MaxHealth(string id) => id == "Commander" ? 220 : id == "Ember scout" ? 130 : 180;

        public bool IsValid()
        {
            if (Version != 1 || !FirstFightComplete || Events == null || Survivors == null ||
                Events.Count > 32 || Survivors.Count != 4) return false;
            var seen = new HashSet<string>();
            foreach (var s in Survivors)
                // Zero sanity means Lost, and the Lost are dead, so nobody standing can have it.
                if (s == null || !KnownId(s.Id) || !seen.Add(s.Id) || s.Health < 0 || s.Health > MaxHealth(s.Id) ||
                    s.Sanity < 0 || s.Sanity > 100 || s.Health > 0 && s.Sanity == 0) return false;
            var facts = new HashSet<string>();
            string rescuer = null;
            int rescueSecond = -1;
            foreach (var e in Events)
            {
                if (e == null || e.Fight != 1 || e.Second < 0 || e.Second > 60 || !KnownId(e.Subject) ||
                    !KnownId(e.Other) || !Enum.IsDefined(typeof(AllianceFact), e.Fact) ||
                    !facts.Add(e.Subject + "/" + e.Fact)) return false;
                // Only history the first fight can produce: rescue and abandonment happen to guards,
                // salvage to the commander, and nobody is covered before fight two.
                bool guard = IsGuardId(e.Subject);
                if (e.Fact == AllianceFact.Covered || !guard && (e.Fact == AllianceFact.Rescued || e.Fact == AllianceFact.Abandoned) ||
                    e.Fact == AllianceFact.Salvaged && e.Subject != "Commander") return false;
                if (e.Fact == AllianceFact.Rescued)
                {
                    // One rescue moment, credited to one fighter who took part in it: the commander or the scout.
                    if (e.Other != "Commander" && e.Other != "Ember scout" ||
                        rescuer != null && (e.Other != rescuer || e.Second != rescueSecond)) return false;
                    rescuer = e.Other;
                    rescueSecond = e.Second;
                }
                else if (e.Other != "Commander") return false;   // every other first fight fact is recorded against the commander
            }
            bool salvaged = facts.Contains("Commander/" + AllianceFact.Salvaged);
            if (HasSalvage != salvaged || rescuer != null && salvaged) return false;
            foreach (var s in Survivors)
            {
                bool fell = facts.Contains(s.Id + "/" + AllianceFact.Fell);
                bool rescued = facts.Contains(s.Id + "/" + AllianceFact.Rescued);
                bool abandoned = facts.Contains(s.Id + "/" + AllianceFact.Abandoned);
                // Falling means zero health, and a fallen guard has no rescue or abandonment on record.
                // Every guard still standing shares the fight's one outcome: all rescued, or all left behind.
                if (fell != (s.Health == 0) ||
                    (fell ? rescued || abandoned : IsGuardId(s.Id) && (rescued == abandoned || rescued != (rescuer != null))) ||
                    s.Id == rescuer && s.Health == 0) return false;
            }
            return Survivors.Exists(s => s.Id == "Commander" && s.Health > 0);
        }
    }

    public static class AllianceCheckpoint
    {
        // Deliberately NOT SaveManager's campaign.json, and never loaded by the campaign.
        public static string PathName => Path.Combine(Application.persistentDataPath, "alliance-prototype-v1.json");

        public static bool Save(AllianceMemory memory, out string error, string path = null)
        {
            error = "";
            if (memory == null || !memory.IsValid()) { error = "Invalid checkpoint; previous file preserved."; return false; }
            path = path ?? PathName;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path + ".tmp", JsonUtility.ToJson(memory, true));
                if (!File.Exists(path)) File.Move(path + ".tmp", path);
                // Only a checkpoint that still loads may become the backup. A damaged one is set aside
                // as .rejected, so it can never overwrite the last good copy.
                else File.Replace(path + ".tmp", path, TryLoad(path, out _, out _) ? path + ".bak" : path + ".rejected");
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            { error = "Checkpoint could not be saved: " + e.Message; return false; }
        }

        public static bool Load(out AllianceMemory memory, out string error, string path = null)
        {
            path = path ?? PathName;
            if (TryLoad(path, out memory, out error)) return true;
            // Any failed load of the main file, including a missing one, falls back to the copy Save kept.
            if (TryLoad(path + ".bak", out memory, out _))
            {
                error = File.Exists(path) ? "Checkpoint was damaged, so its backup was loaded." : "Checkpoint was missing, so its backup was loaded.";
                return true;
            }
            return false;
        }

        static bool TryLoad(string path, out AllianceMemory memory, out string error)
        {
            memory = null;
            error = "";
            if (!File.Exists(path)) { error = "No prototype checkpoint yet."; return false; }
            try
            {
                if (new FileInfo(path).Length > 65536) throw new InvalidDataException("Checkpoint is too large.");
                var loaded = JsonUtility.FromJson<AllianceMemory>(File.ReadAllText(path));
                if (loaded == null || !loaded.IsValid()) throw new InvalidDataException("Unsupported or incomplete checkpoint.");
                memory = loaded;
                return true;
            }
            catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is ArgumentException)
            { error = "Checkpoint not loaded; file preserved: " + e.Message; return false; }
        }
    }
}
