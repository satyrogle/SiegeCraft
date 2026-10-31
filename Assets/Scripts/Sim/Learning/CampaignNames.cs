using System;
using System.Collections.Generic;
using System.Globalization;

namespace KindredSiege.Sim.Learning
{
    /// A chief's name and title. The same all campaign: a chief who runs comes back as the same person.
    public sealed class CampaignChief
    {
        internal CampaignChief(string tribeId, string name, string title)
        {
            TribeId = tribeId;
            Name = name;
            Title = title;
        }

        public string TribeId { get; }

        /// Their name alone, for a label: "Sekhemre".
        public string Name { get; }

        /// Their deadpan title: "Twice Buried".
        public string Title { get; }

        /// Both, for a card or a line: "Sekhemre, Twice Buried".
        public string FullName => Name + ", " + Title;
    }

    /// Generated names (Jacob, 11 Oct 2026, answer 27 to Codex: "a random name generator", the chiefs
    /// included; his four calls on 12 Oct 2026, DESIGN_LOCKS.md "Name calls"). Each people has its own
    /// sound: Bloodridge real ancient Egyptian names after the Medjay they are based on, the Clarth short
    /// hard hunter names, Skullpeak clipped metal ones. Every chief has a deadpan title, about one fighter
    /// in CheekyOneIn gets a cheeky name, and a name given to a second fighter of yours makes them "the
    /// Second". Enemies are named too and keep their names when they join you, and nobody you meet on one
    /// tribe's path in a run shares a name. The Sovereign's hunters and your cult have none. Names come from
    /// the campaign seed and a key through their own dice, so naming never moves a draw that decides a fight.
    public static class CampaignNames
    {
        /// About one generated fighter in this many gets a cheeky name instead.
        public const int CheekyOneIn = 20;

        // A tribe's path in a run is its fights, then its chief: three today (CampaignTuning.PathFights is 2),
        // with room for four. A fight has at most eight named fighters, so each people needs 32 plain names
        // for nobody on a path to share one.
        private const int SlotsPerFight = 8;
        private const int FightsPerPath = 4;

        // A stream no campaign or battle draw uses.
        private const ulong Stream = 7;

        private static readonly string[] Ordinals =
        {
            "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Ninth", "Tenth", "Eleventh", "Twelfth"
        };

        private sealed class People
        {
            public People(string[] names, string[] cheeky, string[] chiefs, string[] titles)
            {
                Names = names;
                Cheeky = cheeky;
                Chiefs = chiefs;
                Titles = titles;
            }

            public string[] Names { get; }
            public string[] Cheeky { get; }
            public string[] Chiefs { get; }
            public string[] Titles { get; }
        }

        // Real ancient Egyptian names: the Medjay are their base (Jacob, 3 and 11 Oct 2026). Jacob made this
        // tribe truly scary, so its cheeky names and titles stay grim.
        private static readonly People Bloodridge = new People(
            new[]
            {
                "Nakht", "Meryt", "Ahmose", "Senebi", "Neferu", "Khety", "Ipi", "Tjeti", "Rensi", "Hapu", "Iuny", "Sitre",
                "Bebi", "Nebet", "Henut", "Paneb", "Satiah", "Ankhu", "Weni", "Harkhuf", "Sabni", "Mekhu", "Nefret", "Kawit",
                "Ashayet", "Henhenet", "Ipuy", "Wah", "Intef", "Renseneb", "Iymery", "Senet"
            },
            new[] { "Still Warm", "Mostly Ash", "Spare Bandage", "Lid" },
            new[] { "Sekhemre", "Khasekhem", "Djedkare", "Menkhepre", "Neferkare", "Sobekhotep", "Seankhkare", "Wahibre" },
            new[]
            {
                "Twice Buried", "Who Outlived the Tomb", "Keeper of the Ash Jars", "Who Was Sealed In", "Who Counts the Bones",
                "Never Properly Mourned", "Who Wraps the Living", "Who Returns the Dead"
            });

        // Made up, short and hard: war hunters of the Broken Wastes who hunt the sand shark (Jacob, 3 Oct 2026).
        private static readonly People Clarth = new People(
            new[]
            {
                "Karru", "Teshka", "Vahn", "Iska", "Brakka", "Oshe", "Ruk", "Tamo", "Kelta", "Duro", "Sefa", "Orra", "Haska",
                "Tuvo", "Zeka", "Marru", "Varo", "Kesh", "Anka", "Rekka", "Uma", "Fenno", "Yarra", "Gesh", "Lokka", "Pira",
                "Dakko", "Senna", "Ikko", "Torra", "Bosk", "Nalla"
            },
            new[] { "Bait", "Chum", "Toothless", "Lucky" },
            new[] { "Vahnu", "Karrak", "Teshkar", "Oruk", "Haskan", "Dursa", "Kelvaru", "Ishkai" },
            new[]
            {
                "Who Speared the Moon", "Who Bit the Shark Back", "Who Was Swallowed and Got Out", "Who Never Misses Twice",
                "Who Talks to Fins", "Who Outran the Dunes", "Of the Long Spear", "Who Wrestled the Sand"
            });

        // Made up, clipped and metal: people who grew machine parts into their bodies (Jacob, 3 Oct 2026). Their
        // chief is a woman with steel ribs and a bionic leg.
        private static readonly People Skullpeak = new People(
            new[]
            {
                "Vessa", "Orsk", "Dagna", "Velka", "Strann", "Ixa", "Brask", "Kova", "Tesk", "Ulla", "Grem", "Sorva", "Dren",
                "Ilsa", "Vark", "Reska", "Tovin", "Holt", "Yeva", "Krell", "Saska", "Vonn", "Nix", "Borra", "Kett", "Lise",
                "Drava", "Zorna", "Haldis", "Senka", "Ruvik", "Elsk"
            },
            new[] { "Rivet", "Sprocket", "Spare", "Hinge" },
            new[] { "Mirka", "Zelka", "Ottra", "Varna", "Ilka", "Brenna", "Sevka", "Tarja" },
            new[]
            {
                "Mostly Original Parts", "Some Assembly Required", "Warranty Void", "Spare Ribs Included", "Who Grew Her Own Leg",
                "Of the Steel Ribs", "Never Recalled", "Her Own Spare Parts"
            });

        // Made up, deep and ritual: the cave tribe that lives for blood rites (Jacob, 18 Oct 2026). Grim, like Bloodridge.
        // Claude's placeholders.
        private static readonly People Stoneveil = new People(
            new[]
            {
                "Vhaal", "Ishmir", "Kethra", "Morra", "Sulka", "Drekh", "Ouma", "Zehra", "Thessk", "Ravik", "Ulmara", "Hesk", "Nyra",
                "Grul", "Vessiq", "Kaal", "Ithra", "Murrak", "Seshi", "Dovak", "Yrra", "Bhal", "Kesmir", "Talu", "Orrin", "Shavra",
                "Muun", "Lekra", "Varrish", "Iru", "Zhol", "Neska"
            },
            new[] { "Thin Blood", "Leech", "Clot", "Spare Vein" },
            new[] { "Hemrakh", "Vasshira", "Ulgoth", "Ishvaar", "Dravesh", "Kaalmir", "Sethrul", "Nyvok" },
            new[]
            {
                "Who Drinks First", "Never Clots", "Keeper of the Red Basin", "Who Bled the Cave Dry", "Who Counts the Drops",
                "Of the Open Vein", "Who Was Bled and Liked It", "Who Paints the Walls"
            });

        // Made up, long and liquid: the ancient sea tribe who think everyone above the water is beneath them (Jacob,
        // 18 Oct 2026). Their titles look down on you. Claude's placeholders.
        private static readonly People Thundercoast = new People(
            new[]
            {
                "Olun", "Merai", "Thalo", "Ishel", "Noru", "Vasha", "Liru", "Oshen", "Pelai", "Quorra", "Seluun", "Ameth", "Ruul",
                "Nalei", "Tiruun", "Ovessa", "Kaimu", "Eloa", "Mirruk", "Sahle", "Uvai", "Toloth", "Neryss", "Aqueth", "Lomai",
                "Ishun", "Vael", "Corru", "Halun", "Meeli", "Oruun", "Taavi"
            },
            new[] { "Damp", "Gills", "Brine", "Sinker" },
            new[] { "Ulathir", "Merovai", "Tholassa", "Nauruun", "Ishmeth", "Vossaria", "Oculon", "Kelphis" },
            new[]
            {
                "Who Pities the Dry", "Never Once Rained On", "Who Breathes Better Than You", "Of the Lower Tide",
                "Who Corrected Evolution", "Patient With Air Breathers", "Who Drowned a Desert", "Too Good for Land"
            });

        /// Whether this people has names. The Sovereign's hunters and your cult do not.
        public static bool Named(string tribeId)
        {
            return Of(tribeId) != null;
        }

        /// A tribe's chief in the campaign with this seed, the same every time. Null for a people with no names.
        public static CampaignChief Chief(ulong seed, string tribeId)
        {
            People p = Of(tribeId);
            if (p == null) return null;
            DeterministicRng dice = Dice(seed, "chief", tribeId);
            string name = p.Chiefs[dice.Next(p.Chiefs.Length)];
            return new CampaignChief(tribeId, name, p.Titles[dice.Next(p.Titles.Length)]);
        }

        /// Names for one side of one fight, in unit order: the fight on this step of a tribe's path in this run
        /// (the tutorial is run 0). Each step takes its own eight places in that run's order, so the same fight
        /// always gets the same names and nobody on one path shares a name. Empty for a people with no names.
        public static List<string> ForFight(ulong seed, int run, int step, string tribeId, int count)
        {
            var names = new List<string>();
            People p = Of(tribeId);
            string order = "run." + run.ToString(CultureInfo.InvariantCulture);
            int start = (step % FightsPerPath + FightsPerPath) % FightsPerPath * SlotsPerFight;
            var given = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < count; i++)
            {
                if (p == null)
                {
                    names.Add(string.Empty);
                    continue;
                }
                string name = At(p, seed, order, tribeId, start + i);
                given.TryGetValue(name, out int n);
                given[name] = ++n;
                names.Add(WithOrdinal(name, n));
            }
            return names;
        }

        /// The index-th name in a people's own order for this campaign. The Clarth you command in the tutorial
        /// are the first five and every fresh one after them takes the next, so a squad shares no name until the
        /// names run out and come round again.
        internal static string Roster(ulong seed, string tribeId, int index)
        {
            People p = Of(tribeId);
            return p == null || index < 0 ? string.Empty : At(p, seed, "roster", tribeId, index);
        }

        /// A name for a fighter saved before names (version 5 and older), picked from their id, so that save
        /// opens with the same names every time.
        internal static string FromKey(ulong seed, string tribeId, string key)
        {
            People p = Of(tribeId);
            if (p == null) return string.Empty;
            DeterministicRng dice = Dice(seed, "saved." + key, tribeId);
            return dice.Next(CheekyOneIn) == 0 ? p.Cheeky[dice.Next(p.Cheeky.Length)] : p.Names[dice.Next(p.Names.Length)];
        }

        /// "Nakht" the first time a name is given, then "Nakht the Second", "Nakht the Third" and on; past the
        /// twelfth, "Nakht 13".
        public static string WithOrdinal(string name, int nth)
        {
            if (string.IsNullOrEmpty(name) || nth <= 1) return name ?? string.Empty;
            return nth <= Ordinals.Length
                ? name + " the " + Ordinals[nth - 1]
                : name + " " + nth.ToString(CultureInfo.InvariantCulture);
        }

        /// A name without its "the Second": the name it was given from.
        public static string BaseOf(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            for (int i = 1; i < Ordinals.Length; i++)
            {
                string tail = " the " + Ordinals[i];
                if (name.EndsWith(tail, StringComparison.Ordinal)) return name.Substring(0, name.Length - tail.Length);
            }
            int space = name.LastIndexOf(' ');
            if (space > 0 && int.TryParse(name.Substring(space + 1), NumberStyles.None, CultureInfo.InvariantCulture, out int nth) &&
                nth > Ordinals.Length)
                return name.Substring(0, space);
            return name;
        }

        private static People Of(string tribeId)
        {
            switch (tribeId)
            {
                case CampaignTribes.BloodridgeId: return Bloodridge;
                case CampaignTribes.ClarthId: return Clarth;
                case CampaignTribes.SkullpeakId: return Skullpeak;
                case CampaignTribes.StoneveilId: return Stoneveil;
                case CampaignTribes.ThundercoastId: return Thundercoast;
                default: return null;
            }
        }

        // The name in one place of an order: its plain name, or about one place in CheekyOneIn a cheeky one,
        // each cheeky place taking the next cheeky name.
        private static string At(People p, ulong seed, string order, string tribeId, int index)
        {
            if (!Rare(seed, order, tribeId, index))
                return p.Names[Shuffled(p.Names.Length, Dice(seed, order, tribeId))[index % p.Names.Length]];
            int before = 0;
            for (int k = 0; k < index; k++) if (Rare(seed, order, tribeId, k)) before++;
            return p.Cheeky[Shuffled(p.Cheeky.Length, Dice(seed, order + ".cheeky", tribeId))[before % p.Cheeky.Length]];
        }

        private static bool Rare(ulong seed, string order, string tribeId, int index)
        {
            return Dice(seed, order + "." + index.ToString(CultureInfo.InvariantCulture), tribeId).Next(CheekyOneIn) == 0;
        }

        // Fresh dice for one key: FNV 1a over the key, mixed into the seed. Never one of the campaign's streams.
        private static DeterministicRng Dice(ulong seed, string what, string tribeId)
        {
            string key = what + "|" + tribeId;
            ulong hash = 14695981039346656037UL;
            for (int i = 0; i < key.Length; i++)
            {
                hash ^= key[i];
                hash *= 1099511628211UL;
            }
            return new DeterministicRng(seed ^ hash, Stream);
        }

        // Fisher and Yates over 0 to count minus 1.
        private static int[] Shuffled(int count, DeterministicRng dice)
        {
            var order = new int[count];
            for (int i = 0; i < count; i++) order[i] = i;
            for (int i = count - 1; i > 0; i--)
            {
                int j = dice.Next(i + 1);
                int keep = order[i];
                order[i] = order[j];
                order[j] = keep;
            }
            return order;
        }
    }
}
