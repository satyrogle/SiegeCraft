using System;
using System.Collections.Generic;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// What a fight can show a tribe (Jacob, 8 Oct 2026). Each tribe has a fixed code: three
    /// things it respects and one it hates. Feats are measured from the fight's own events, so
    /// a replay always agrees with them. Since 10 Oct 2026 (Jacob: no respect bar, no taboos)
    /// nothing scores them; the code stays as each tribe's character.
    public enum Feat
    {
        AnsweredThreeWays = 0,
        ScaredThem = 1,
        BrokeByFear = 2,
        CleanWin = 3,
        QuickWin = 4,
        Outnumbered = 5,
        Hid = 6,
        Loud = 7,

        /// What every tribe hates since taboos were dropped (Jacob, 10 Oct 2026): nothing.
        None = 8
    }

    public enum EncounterKind
    {
        WarParty = 0,
        HoldTheLine = 1,
        Duel = 2,
        Trial = 3,
        Chief = 4,
        Tutorial = 5,

        /// The hunters, the last fight of a run (Jacob, 11 Oct 2026, call J2; AlphaHunt). Since 18 Oct 2026 the hunters
        /// are their own tribe and the rival, not the Sovereign's, and a named hunter leads them (HunterCompany).
        AlphaHunt = 6,

        /// A named hunter and a few of the company waiting on a road node (Jacob's go, 18 Oct 2026), from bounty 2 on
        /// (BountyRules). Beat them to climb the hunters' ladder.
        Ambush = 7,

        /// The beast beside a road, which you choose to hunt (Jacob, 20 Oct 2026, DESIGN_LOCKS "Beasts"; built 21 Oct): the
        /// Broken Wastes' sand shark beside a run's first road, Skullpeak's cage bear beside Skullpeak's (22 Oct;
        /// Battle.BeastKits, Encounter.Beast). Not one of the road's offers: Campaign.BeastOffer and Campaign.StartBeast.
        Beast = 8
    }

    /// Where the campaign stands. Since 10 Oct 2026 ChooseTribe means at camp between runs, where
    /// the next run starts; Finished is no longer reached, because every run ends back at camp.
    public enum CampaignStage
    {
        Tutorial = 0,
        ChooseTribe = 1,
        InRun = 2,
        Finished = 3
    }

    public static class Feats
    {
        /// A win this fast counts as quick (40 seconds at 30 ticks a second).
        public const int QuickWinTicks = 1200;

        /// Unleashes that must land unblocked to scare a tribe.
        public const int ScaresNeeded = 2;

        /// Broken by fear means at least this many of them still standing when they gave up.
        public const int BrokeByFearStanding = 3;

        /// Different answers to their Eclipse needed in one fight.
        public const int EclipseWaysNeeded = 3;

        public static string Name(Feat feat)
        {
            switch (feat)
            {
                case Feat.AnsweredThreeWays: return "Answer their Eclipse three ways";
                case Feat.ScaredThem: return "Scare them";
                case Feat.BrokeByFear: return "Break them by fear";
                case Feat.CleanWin: return "Lose nobody";
                case Feat.QuickWin: return "Win fast";
                case Feat.Outnumbered: return "Win outnumbered";
                case Feat.Hid: return "Hiding";
                case Feat.Loud: return "Noise";
                case Feat.None: return "Nothing";
                default: return "Unknown";
            }
        }

        public static string Describe(Feat feat)
        {
            switch (feat)
            {
                case Feat.AnsweredThreeWays:
                    return "In one fight, answer their Eclipse " + EclipseWaysNeeded +
                        " different ways: dodge it, break the cast, punish the caster, or soak it.";
                case Feat.ScaredThem: return "Land " + ScaresNeeded + " Unleashes they do not block.";
                case Feat.BrokeByFear: return "Win with " + BrokeByFearStanding + " or more of them still standing.";
                case Feat.CleanWin: return "Win without losing anyone.";
                case Feat.QuickWin: return "Win inside 40 seconds.";
                case Feat.Outnumbered: return "Win with fewer fighters than they bring.";
                case Feat.Hid: return "Every Hold you order costs respect.";
                case Feat.Loud: return "Every Unleash you order costs respect.";
                default: return string.Empty;
            }
        }

        public static bool IsTaboo(Feat feat)
        {
            return feat == Feat.Hid || feat == Feat.Loud;
        }

        /// How many times the fight showed this: 0 or 1 for feats, the number of acts for a taboo.
        public static int Count(Feat feat, FightRecord record, BattleSetup setup, bool won)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (setup == null) throw new ArgumentNullException(nameof(setup));
            IReadOnlyList<BattleEvent> ev = record.Events;
            switch (feat)
            {
                case Feat.AnsweredThreeWays:
                    return EclipseWays(ev) >= EclipseWaysNeeded ? 1 : 0;
                case Feat.ScaredThem:
                    return ScaresLanded(ev) >= ScaresNeeded ? 1 : 0;
                case Feat.BrokeByFear:
                    if (!won) return 0;
                    for (int i = 0; i < ev.Count; i++)
                        if (ev[i].Kind == BattleEventKind.Break && ev[i].ActorSide == 1 &&
                            ev[i].Detail == BattleSimulation.BreakDetail && ev[i].Amount >= BrokeByFearStanding) return 1;
                    return 0;
                case Feat.CleanWin:
                    return won && record.Survivors(0) == setup.Side(0).Count ? 1 : 0;
                case Feat.QuickWin:
                    return won && record.EndTick <= QuickWinTicks ? 1 : 0;
                case Feat.Outnumbered:
                    return won && Fighters(setup.Side(0)) < Fighters(setup.Side(1)) ? 1 : 0;
                case Feat.Hid:
                    return Ordered(ev, Directive.Hold);
                case Feat.Loud:
                    return Ordered(ev, Directive.Unleash);
                default:
                    return 0;
            }
        }

        /// How many different ways you answered the enemy's Eclipse this fight, 0 to 4: dodged it
        /// with Fall Back, soaked it with Hold, broke the cast by hitting the caster in the windup,
        /// or punished the caster while he was worn out by his own cast.
        public static int EclipseWays(IReadOnlyList<BattleEvent> ev)
        {
            bool dodged = false, soaked = false, broke = false, punished = false;
            for (int i = 0; i < ev.Count; i++)
            {
                BattleEvent e = ev[i];
                if (e.Kind == BattleEventKind.Interrupt && e.ActorSide == 0) broke = true;
                if (e.Kind != BattleEventKind.Release || e.ActorSide != 1) continue;
                if (e.Detail == Moves.Dodged) dodged = true;
                else if (e.Detail == Moves.Soaked) soaked = true;
                for (int j = i + 1; j < ev.Count && !punished; j++)
                {
                    BattleEvent h = ev[j];
                    if (h.Tick - e.Tick > Moves.WornOutTicks) break;
                    if (h.Kind == BattleEventKind.Hit && h.ActorSide == 0 && h.TargetId == e.ActorId) punished = true;
                }
            }
            return (dodged ? 1 : 0) + (soaked ? 1 : 0) + (broke ? 1 : 0) + (punished ? 1 : 0);
        }

        /// The player's ordered Unleashes the enemy did not block with its shields.
        public static int ScaresLanded(IReadOnlyList<BattleEvent> ev)
        {
            int ordered = Ordered(ev, Directive.Unleash);
            int blocked = 0;
            for (int i = 0; i < ev.Count; i++)
                if (ev[i].Kind == BattleEventKind.Directive && ev[i].ActorSide == 1 && ev[i].Detail == BattleSimulation.BlockedDetail)
                    blocked++;
            return Math.Max(0, ordered - blocked);
        }

        /// Orders of this kind the player gave (not reflexes).
        public static int Ordered(IReadOnlyList<BattleEvent> ev, Directive directive)
        {
            string name = Directives.Name(directive);
            int count = 0;
            for (int i = 0; i < ev.Count; i++)
            {
                BattleEvent e = ev[i];
                if (e.Kind == BattleEventKind.Directive && e.ActorSide == 0 && e.ActorId == "side0" && e.Detail == name) count++;
            }
            return count;
        }

        private static int Fighters(IReadOnlyList<UnitSpec> specs)
        {
            int count = 0;
            for (int i = 0; i < specs.Count; i++) if (specs[i].Role == UnitRole.Fighter) count++;
            return count;
        }
    }

    /// Respect (Jacob, 8 Oct 2026): the bar is short for early tribes and longer as they scale up,
    /// and a full bar makes the chieftain call you out. Unused since 10 Oct 2026, when Jacob
    /// dropped the respect bar and taboos; kept so screens that still mention it compile.
    public static class RespectRules
    {
        public const int WinPoints = 10;
        public const int FirstFeatPoints = 20;
        public const int RepeatFeatPoints = 5;
        public const int TabooPoints = 10;

        /// No cap: every act a tribe hates costs TabooPoints (Jacob's go, 10 Oct 2026). Capped at
        /// 30 a fight, spamming Skullpeak's hated Unleash still won their fights at a profit.
        public const int TabooCapPerFight = int.MaxValue;

        /// Tier 1: 100. Tier 2: 150.
        public static int Needed(int tier)
        {
            return 50 + 50 * Math.Max(1, tier);
        }
    }

    /// A tribe of the slice. Data, hand made and unchanging (lock 4, 1 Oct 2026).
    public sealed class CampaignTribe
    {
        private static readonly UnitClass[] NoLineup = new UnitClass[0];

        internal CampaignTribe(string id, string name, int tier, UnitClass[] roster, UnitClass chiefClass, string chiefName,
            string assistName, int assistGrade, int chiefEclipseGrade, Feat[] respects, Feat hates, Habit[] warTaught,
            string trialName, string trialText)
        {
            Id = id;
            Name = name;
            Tier = tier;
            Roster = roster;
            ChiefClass = chiefClass;
            ChiefName = chiefName;
            AssistName = assistName;
            AssistGrade = assistGrade;
            ChiefEclipseGrade = chiefEclipseGrade;
            Respects = respects;
            Hates = hates;
            WarTaught = warTaught;
            TrialName = trialName;
            TrialText = trialText;
        }

        public string Id { get; }
        public string Name { get; }
        public int Tier { get; }
        public IReadOnlyList<UnitClass> Roster { get; }
        public UnitClass ChiefClass { get; }
        public string ChiefName { get; }

        /// The chief's move once he kneels: it becomes your assist.
        public string AssistName { get; }
        public int AssistGrade { get; }
        public int ChiefEclipseGrade { get; }

        /// The code: three things it respects and one it hates. Since 10 Oct 2026 nothing scores
        /// it and every tribe hates Feat.None (Jacob dropped the respect bar and taboos).
        public IReadOnlyList<Feat> Respects { get; }
        public Feat Hates { get; }

        /// Habits it arrives with, taught by its wars with other tribes, so a first fight cannot be spammed.
        public IReadOnlyList<Habit> WarTaught { get; }

        public string TrialName { get; }
        public string TrialText { get; }

        /// What this tribe's chief order does (ChiefSpells, 18 Oct 2026): its casts and, once captured, your 5th button.
        public ChiefSpell Spell { get; internal set; } = ChiefSpell.Eclipse;

        /// Its trial is a caster winding up the tribe's spell again and again, as in Bloodridge's Eclipse trial; otherwise
        /// it is a precision trial, as in Skullpeak's.
        public bool TrialCasts { get; internal set; }

        /// It floods every fight it is in (Battle.FloodRules; the sea tribe, Jacob, 18 Oct 2026).
        public bool Floods { get; internal set; }

        /// It takes no prisoners: when it beats you, your most hurt fighter dies instead of being captured (Claude's
        /// addition for the sea tribe, kept in the plan Jacob said go to, 18 Oct 2026).
        public bool NoPrisoners { get; internal set; }

        /// Its chief's own moveset (Battle.ChiefKits; Jacob, 20 Oct 2026, "Chief fights"), or None while his kit is not
        /// written. Bloodridge's and Skullpeak's are (22 Oct 2026).
        public ChiefKit Kit { get; internal set; } = ChiefKit.None;

        /// Who the tribe sends to each kind of fight. Each lineup was picked with the bots (11 Oct 2026) so that doing
        /// nothing loses where it allows and spam does not win, and sent every time so each fight was one test you could
        /// learn (Jacob, "fixed per fight"); before that the same fight swung from about 1% to 98% for a sensible bot on
        /// how many fights came before it. Since 18 Oct 2026 each is the first of the tribe's three squads for that kind
        /// of fight (WarPartyMore and the rest), and the road node shows which one comes. Longer fights take the lineup
        /// from its start again. Empty: the roster from its start.
        public IReadOnlyList<UnitClass> TutorialLineup { get; internal set; } = NoLineup;
        public IReadOnlyList<UnitClass> WarPartyLineup { get; internal set; } = NoLineup;
        public IReadOnlyList<UnitClass> HoldLineup { get; internal set; } = NoLineup;

        /// The trial's lineup. Until 10 Oct 2026 Skullpeak's changed with every fight, and its first
        /// trial, with no Warden in it, was won by doing nothing.
        public IReadOnlyList<UnitClass> TrialLineup { get; internal set; } = NoLineup;

        /// The chief's fighters, not counting the chief.
        public IReadOnlyList<UnitClass> ChiefLineup { get; internal set; } = NoLineup;

        /// The Sovereign's hunters, not counting their captain (AlphaHunt).
        public IReadOnlyList<UnitClass> HuntLineup { get; internal set; } = NoLineup;

        /// The lineup for this kind of fight.
        public IReadOnlyList<UnitClass> Lineup(EncounterKind kind)
        {
            IReadOnlyList<UnitClass> lineup;
            switch (kind)
            {
                case EncounterKind.Tutorial: lineup = TutorialLineup; break;
                case EncounterKind.WarParty: lineup = WarPartyLineup; break;
                case EncounterKind.HoldTheLine: lineup = HoldLineup; break;
                case EncounterKind.Trial: lineup = TrialLineup; break;
                case EncounterKind.Chief: lineup = ChiefLineup; break;
                case EncounterKind.AlphaHunt:
                case EncounterKind.Ambush: lineup = HuntLineup; break;
                default: lineup = NoLineup; break;
            }
            return lineup.Count > 0 ? lineup : Roster;
        }

        /// The tribe's other two squads for each kind of road fight and for its chief's band (Jacob, 18 Oct 2026: "three
        /// squads per fight shown on the node"). Each road node, and each run's chief, sends one of the three by its own
        /// seed; squad 0 is the lineup above. Claude's starting values, made from the classes the tribe already has art
        /// for and not picked with the bots (Jacob, 12 Oct 2026: build first, tune after he plays).
        public IReadOnlyList<IReadOnlyList<UnitClass>> WarPartyMore { get; internal set; } = NoSquads;
        public IReadOnlyList<IReadOnlyList<UnitClass>> HoldMore { get; internal set; } = NoSquads;
        public IReadOnlyList<IReadOnlyList<UnitClass>> TrialMore { get; internal set; } = NoSquads;
        public IReadOnlyList<IReadOnlyList<UnitClass>> ChiefMore { get; internal set; } = NoSquads;

        private static readonly IReadOnlyList<UnitClass>[] NoSquads = new IReadOnlyList<UnitClass>[0];

        /// How many squads the tribe sends to this kind of fight: 1 to 3. The tutorial, a duel and the hunters have one.
        public int SquadCount(EncounterKind kind)
        {
            return 1 + More(kind).Count;
        }

        /// One of its squads for this kind of fight. Squad 0, or one it does not have, is Lineup(kind).
        public IReadOnlyList<UnitClass> Lineup(EncounterKind kind, int squad)
        {
            IReadOnlyList<IReadOnlyList<UnitClass>> more = More(kind);
            return squad <= 0 || squad > more.Count || more[squad - 1].Count == 0 ? Lineup(kind) : more[squad - 1];
        }

        private IReadOnlyList<IReadOnlyList<UnitClass>> More(EncounterKind kind)
        {
            switch (kind)
            {
                case EncounterKind.WarParty: return WarPartyMore;
                case EncounterKind.HoldTheLine: return HoldMore;
                case EncounterKind.Trial: return TrialMore;
                case EncounterKind.Chief: return ChiefMore;
                default: return NoSquads;
            }
        }

        public bool IsStarter => Id == CampaignTribes.ClarthId;
    }

    public static class CampaignTribes
    {
        public const string ClarthId = SliceRun.ClarthId;
        public const string BloodridgeId = SliceRun.RaidersId;
        public const string SkullpeakId = SliceRun.SkullpeakId;

        /// The two new tribes (Jacob, 18 Oct 2026; DESIGN_LOCKS, "The two new tribes"). Claude's placeholder names, after
        /// two free map regions.
        public const string StoneveilId = "stoneveil";
        public const string ThundercoastId = "thundercoast";
        public const string CultId = "cult";

        /// Their order is Dust Shark (Jacob, 18 Oct 2026: "make a clarth spell and make them inherit this", then "2 Dust
        /// Shark"): your 5th button carries it from the tutorial on, until a captured chief's order takes the button.
        /// Grade 1, a step below a captured chief's.
        public static readonly CampaignTribe Clarth = new CampaignTribe(ClarthId, "Clarth", 1,
            new[] { UnitClass.Warden, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Medic },
            UnitClass.Warden, "the Clarth chief", "Dust Shark", 1, 0,
            new Feat[0], Feat.None, new Habit[0], string.Empty, string.Empty)
        {
            Spell = ChiefSpell.DustShark
        };

        /// Bloodridge's trial asks for three answers to their Eclipse (Jacob's go, 9 Oct 2026). Their hold
        /// the line is lost by holding with every lineup the bots tried (11 Oct 2026): Hold drains your own
        /// nerve faster than their attacks, so this lineup only keeps doing nothing from winning it most.
        public static readonly CampaignTribe Bloodridge = new CampaignTribe(BloodridgeId, "Bloodridge", 1,
            new[] { UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Medic,
                UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Warden },
            UnitClass.Marksman, "the Bloodridge chief", "Eclipse", 2, 2,
            new[] { Feat.AnsweredThreeWays, Feat.ScaredThem, Feat.BrokeByFear }, Feat.None,
            new[] { Habit.HoldOnUnleash },
            "Eclipse trial", "Their shaman casts Eclipse again and again.")
        {
            TrialCasts = true,
            // Cleave, Brand, Eclipse and Blood Sun (Jacob, 20 Oct 2026: "perfect", then "lock this direction"), and from 22 Oct
            // Burning Ring and Sun Mantle ("yes, lock both as written").
            Kit = ChiefKit.Bloodridge,
            TutorialLineup = new[] { UnitClass.Warden, UnitClass.Marksman, UnitClass.Skirmisher },
            WarPartyLineup = new[] { UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Medic,
                UnitClass.Marksman },
            HoldLineup = new[] { UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Medic, UnitClass.Marksman, UnitClass.Skirmisher,
                UnitClass.Warden, UnitClass.Skirmisher },
            TrialLineup = new[] { UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Marksman, UnitClass.Skirmisher },
            ChiefLineup = new[] { UnitClass.Medic, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Skirmisher },
            // Their other squads (18 Oct 2026): a blade rush with a flail and a shield line with rifles, and so on.
            WarPartyMore = new[]
            {
                new[] { UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Skirmisher, UnitClass.Marksman, UnitClass.Medic },
                new[] { UnitClass.Warden, UnitClass.Warden, UnitClass.Marksman, UnitClass.Marksman, UnitClass.Medic, UnitClass.Skirmisher }
            },
            HoldMore = new[]
            {
                new[]
                {
                    UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Medic, UnitClass.Marksman,
                    UnitClass.Warden
                },
                new[]
                {
                    UnitClass.Marksman, UnitClass.Marksman, UnitClass.Marksman, UnitClass.Warden, UnitClass.Warden, UnitClass.Medic,
                    UnitClass.Trapper
                }
            },
            TrialMore = new[]
            {
                new[] { UnitClass.Warden, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Medic },
                new[] { UnitClass.Trapper, UnitClass.Skirmisher, UnitClass.Marksman, UnitClass.Marksman }
            },
            ChiefMore = new[]
            {
                new[] { UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Medic },
                new[] { UnitClass.Warden, UnitClass.Warden, UnitClass.Marksman, UnitClass.Marksman, UnitClass.Medic }
            }
        };

        /// Their trial brings both Wardens, who step in front of whoever you focus: doing nothing
        /// loses it, and focusing anyone before them mostly does too (bots, 10 Oct 2026). The card
        /// says what they do, not how to beat it, and asks for no feat since 10 Oct 2026.
        public static readonly CampaignTribe Skullpeak = new CampaignTribe(SkullpeakId, "Skullpeak", 2,
            new[] { UnitClass.Warden, UnitClass.Trapper, UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Medic,
                UnitClass.Trapper, UnitClass.Marksman, UnitClass.Warden },
            UnitClass.Trapper, "the Skullpeak chief", "Steel Storm", 2, 2,
            new[] { Feat.CleanWin, Feat.QuickWin, Feat.Outnumbered }, Feat.None,
            new[] { Habit.CoverFocused, Habit.HoldOnUnleash },
            "Precision trial", "Fewer of yours than theirs, and both their Wardens step in front of whoever you focus.")
        {
            // Steel Storm is a wall that Unleash smashes, not a second Eclipse (Jacob, 18 Oct 2026: "i like 3 unleash").
            Spell = ChiefSpell.SteelStorm,
            // Steel Storm, Hook, Rifle Line, Iron Advance, Overclock and Lockdown, a slow grind behind walls (Jacob, 22 Oct
            // 2026: "yes, lock both as written").
            Kit = ChiefKit.Skullpeak,
            WarPartyLineup = new[] { UnitClass.Medic, UnitClass.Trapper, UnitClass.Marksman, UnitClass.Warden, UnitClass.Warden,
                UnitClass.Trapper },
            HoldLineup = new[] { UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Medic, UnitClass.Trapper, UnitClass.Marksman,
                UnitClass.Warden, UnitClass.Warden },
            TrialLineup = new[] { UnitClass.Trapper, UnitClass.Marksman, UnitClass.Warden, UnitClass.Warden, UnitClass.Trapper },
            ChiefLineup = new[] { UnitClass.Medic, UnitClass.Trapper, UnitClass.Marksman, UnitClass.Warden, UnitClass.Warden },
            // Their other squads (18 Oct 2026): a sharpshooters' nest and a steel wall, and so on. Every precision trial
            // squad keeps both Wardens, since stepping in front of whoever you focus is the trial.
            WarPartyMore = new[]
            {
                new[] { UnitClass.Marksman, UnitClass.Marksman, UnitClass.Marksman, UnitClass.Warden, UnitClass.Trapper, UnitClass.Medic },
                new[] { UnitClass.Warden, UnitClass.Warden, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Medic }
            },
            HoldMore = new[]
            {
                new[]
                {
                    UnitClass.Trapper, UnitClass.Trapper, UnitClass.Trapper, UnitClass.Warden, UnitClass.Marksman, UnitClass.Medic,
                    UnitClass.Skirmisher
                },
                new[]
                {
                    UnitClass.Warden, UnitClass.Warden, UnitClass.Warden, UnitClass.Marksman, UnitClass.Marksman, UnitClass.Medic,
                    UnitClass.Medic
                }
            },
            TrialMore = new[]
            {
                new[] { UnitClass.Marksman, UnitClass.Marksman, UnitClass.Warden, UnitClass.Warden, UnitClass.Medic },
                new[] { UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Warden, UnitClass.Warden, UnitClass.Skirmisher }
            },
            ChiefMore = new[]
            {
                new[] { UnitClass.Marksman, UnitClass.Marksman, UnitClass.Warden, UnitClass.Trapper, UnitClass.Medic },
                new[] { UnitClass.Warden, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Medic }
            }
        };

        /// The cave tribe of tier 1, beside Bloodridge (Jacob, 18 Oct 2026; DESIGN_LOCKS, "The two new tribes"): a cave
        /// species like Shaak Ti and Plo Koon fused, who live for blood rituals like WoW's blood trolls. Their blades leave
        /// bleeding wounds (TribeTraits.Bloodletters), and their chief, a blood priest, casts the blood call, which rips the
        /// blood out of every bleeding fighter of yours unless you Fall Back. Their trial is a priest calling again and again.
        /// The name, the chief and every squad are Claude's placeholders, made of blades and blood priests.
        public static readonly CampaignTribe Stoneveil = new CampaignTribe(StoneveilId, "Stoneveil", 1,
            new[]
            {
                UnitClass.Skirmisher, UnitClass.Medic, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Marksman,
                UnitClass.Skirmisher, UnitClass.Medic
            },
            UnitClass.Medic, "the Stoneveil chief", "Blood Call", 2, 2,
            new Feat[0], Feat.None, new[] { Habit.HoldOnUnleash },
            "Blood trial", "Their blood priest calls the blood again and again.")
        {
            Spell = ChiefSpell.BloodCall,
            TrialCasts = true,
            // Blood Call, the Basin, Leeches, Open Veins, Transfusion and the Red Rite, the blood rite (Jacob, 22 Oct 2026: "yes,
            // lock both kits as written").
            Kit = ChiefKit.Stoneveil,
            WarPartyLineup = new[] { UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Medic, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Trapper },
            HoldLineup = new[]
            {
                UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Medic, UnitClass.Trapper,
                UnitClass.Marksman
            },
            TrialLineup = new[] { UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Medic },
            ChiefLineup = new[] { UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Medic, UnitClass.Trapper },
            WarPartyMore = new[]
            {
                new[] { UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Medic, UnitClass.Marksman },
                new[] { UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Medic, UnitClass.Skirmisher, UnitClass.Medic, UnitClass.Skirmisher }
            },
            HoldMore = new[]
            {
                new[]
                {
                    UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Trapper, UnitClass.Medic, UnitClass.Medic,
                    UnitClass.Warden
                },
                new[]
                {
                    UnitClass.Warden, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Medic,
                    UnitClass.Marksman
                }
            },
            TrialMore = new[]
            {
                new[] { UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Trapper, UnitClass.Warden },
                new[] { UnitClass.Warden, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Trapper }
            },
            ChiefMore = new[]
            {
                new[] { UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Skirmisher, UnitClass.Medic, UnitClass.Warden },
                new[] { UnitClass.Warden, UnitClass.Trapper, UnitClass.Skirmisher, UnitClass.Medic, UnitClass.Marksman }
            }
        };

        /// The sea tribe of tier 2, beside Skullpeak (Jacob, 18 Oct 2026; DESIGN_LOCKS, "The two new tribes"): an ancient
        /// Nautolan tribe that lives underwater, made itself into fishmen with machines and DNA work, and sees everyone who
        /// lives above the water as beneath it. It floods every fight (Battle.FloodRules), its chief's tide drowns your back
        /// row unless you Fall Back, and it takes no prisoners. Its trial is a precision trial: both Wardens step in front of
        /// whoever you focus. The name, the chief and every squad are Claude's placeholders, made of rifles, harpoons and
        /// shields.
        public static readonly CampaignTribe Thundercoast = new CampaignTribe(ThundercoastId, "Thundercoast", 2,
            new[]
            {
                UnitClass.Marksman, UnitClass.Trapper, UnitClass.Warden, UnitClass.Marksman, UnitClass.Medic, UnitClass.Trapper,
                UnitClass.Warden, UnitClass.Skirmisher
            },
            UnitClass.Warden, "the Thundercoast chief", "Tide", 2, 2,
            new Feat[0], Feat.None, new[] { Habit.CoverFocused, Habit.HoldOnUnleash },
            "Depth trial", "Fewer of yours than theirs, the water rising, and both their Wardens step in front of whoever you focus.")
        {
            Spell = ChiefSpell.Tide,
            Floods = true,
            NoPrisoners = true,
            // Tide, Sluice Gate, Riptide, Tidal Bore, Storm Surge and Spring Tide, the tide's clock (Jacob, 22 Oct 2026: "yes, lock
            // both kits as written").
            Kit = ChiefKit.Thundercoast,
            WarPartyLineup = new[] { UnitClass.Marksman, UnitClass.Trapper, UnitClass.Warden, UnitClass.Marksman, UnitClass.Medic, UnitClass.Trapper },
            HoldLineup = new[]
            {
                UnitClass.Marksman, UnitClass.Marksman, UnitClass.Warden, UnitClass.Trapper, UnitClass.Medic, UnitClass.Warden,
                UnitClass.Skirmisher
            },
            TrialLineup = new[] { UnitClass.Marksman, UnitClass.Trapper, UnitClass.Warden, UnitClass.Warden, UnitClass.Marksman },
            ChiefLineup = new[] { UnitClass.Marksman, UnitClass.Trapper, UnitClass.Warden, UnitClass.Medic, UnitClass.Marksman },
            WarPartyMore = new[]
            {
                new[] { UnitClass.Warden, UnitClass.Warden, UnitClass.Marksman, UnitClass.Marksman, UnitClass.Marksman, UnitClass.Medic },
                new[] { UnitClass.Trapper, UnitClass.Trapper, UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Marksman, UnitClass.Medic }
            },
            HoldMore = new[]
            {
                new[]
                {
                    UnitClass.Trapper, UnitClass.Trapper, UnitClass.Trapper, UnitClass.Warden, UnitClass.Marksman, UnitClass.Medic,
                    UnitClass.Marksman
                },
                new[]
                {
                    UnitClass.Warden, UnitClass.Warden, UnitClass.Warden, UnitClass.Marksman, UnitClass.Medic, UnitClass.Medic,
                    UnitClass.Trapper
                }
            },
            TrialMore = new[]
            {
                new[] { UnitClass.Trapper, UnitClass.Trapper, UnitClass.Warden, UnitClass.Warden, UnitClass.Medic },
                new[] { UnitClass.Marksman, UnitClass.Skirmisher, UnitClass.Warden, UnitClass.Warden, UnitClass.Medic }
            },
            ChiefMore = new[]
            {
                new[] { UnitClass.Warden, UnitClass.Marksman, UnitClass.Marksman, UnitClass.Trapper, UnitClass.Medic },
                new[] { UnitClass.Trapper, UnitClass.Trapper, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Medic }
            }
        };

        /// Every tribe, in a fixed order: saves write their minds in it. New tribes go at the end.
        public static readonly CampaignTribe[] All = { Clarth, Bloodridge, Skullpeak, Stoneveil, Thundercoast };

        public static CampaignTribe Find(string id)
        {
            for (int i = 0; i < All.Length; i++) if (All[i].Id == id) return All[i];
            return null;
        }
    }

    /// The slice campaign's numbers. Measured against the bots on 9 and 10 Oct 2026
    /// (CampaignTests): placeholders until Jacob's playtest.
    public sealed class CampaignTuning
    {
        public int Tier1Percent { get; set; } = 110;
        public int Tier2Percent { get; set; } = 130;
        public int Tier3Percent { get; set; } = 140;

        /// Every enemy's strength on top of its tier, in percent, by tier. The calibration knobs.
        public int EnemyScaleTier1 { get; set; } = 95;
        public int EnemyScaleTier2 { get; set; } = 86;

        /// Each encounter's enemies, in percent of the scaled tier strength. The chief is a knife
        /// edge: at 90 the reference pilot won 90% of Bloodridge runs, at 95 it won 25%.
        public int WarPartyPercent { get; set; } = 110;
        public int HoldPercent { get; set; } = 100;

        /// Hold the line from tier 2 up. At 100 every bot held Skullpeak's line, even one giving no
        /// orders; at 122 doing nothing loses it and holding with Hold wins (10 Oct 2026).
        public int HoldPercentTier2 { get; set; } = 122;
        public int DuelPercent { get; set; } = 110;
        public int TrialPercent { get; set; } = 100;

        /// Skullpeak's trial lineup. At 108 doing nothing still won 10 to 18% of first trials; at
        /// 111 the reference pilot won 39% of its trials across a run, down from 93% (10 Oct 2026).
        public int PrecisionTrialPercent { get; set; } = 109;
        public int ChiefPercent { get; set; } = 92;

        /// The Sovereign's hunters (Jacob, 11 Oct 2026: tune to Medium, measured on the reading bot: about
        /// one first run in three won, most players won by run 3). A knife edge: at 100 the reading bot
        /// won 28% of hunts, at 98 80%, at 97 88%. At 98 it won 31% of first runs and 87% by run 3.
        /// A starting value. Test: Jacob's playtest. Too easy: raise; too hard: lower.
        public int HuntPercent { get; set; } = 98;

        public int WarPartyCount { get; set; } = 6;
        public int HoldCount { get; set; } = 7;
        public int HoldTicks { get; set; } = 1350;

        /// Ticks before any other fight runs out of time.
        public int FightTicks { get; set; } = BattleSetup.DefaultTickLimit;

        /// Encounters a respect run could last before the chief. Unused since 10 Oct 2026: each
        /// tribe's path to its chief is PathFights long.
        public int MaxSteps { get; set; } = 8;

        /// Rows on each tribe's road before its chief (Jacob, 13 Oct 2026, the path map: 3 lanes and 3 rows of
        /// fights and events, then the chief; until then, fights on the path). A starting value. Test: a whole slice
        /// run takes 20 to 30 minutes by hand. Longer: cut to 2. Shorter: raise to 4.
        public int PathFights { get; set; } = 3;

        /// Materials a won fight and a beaten chief send home for the Pals. They come home even from
        /// a lost run (Jacob, 10 Oct 2026). Placeholders: nothing spends them yet.
        public int MaterialsPerFight { get; set; } = 10;
        public int MaterialsPerChief { get; set; } = 30;

        /// Hold the line means not giving ground: Fall Back is refused there.
        public bool HoldForbidsFallBack { get; set; } = true;

        /// A fight that runs out of time is lost: they still hold the field. Hold the line is the
        /// exception. Without it, falling back until the clock ran out won on health left.
        public bool TimeOutLoses { get; set; } = true;

        /// Value a war taught habit starts at. It fades if you never give the tribe a reason to keep it.
        public int WarTaughtValue { get; set; } = 4;

        /// Every tribe has seen invaders run before: it arrives knowing to Unleash when you fall
        /// back. Without it, falling back over and over kept the lines apart and won (9 Oct 2026).
        public bool WarTaughtPressOnFallBack { get; set; } = true;

        /// Tribes whose wars taught them to cover what you focus arrive knowing it.
        public bool WarTaughtCover { get; set; } = true;

        /// Every run starts with at least this many fresh Clarth fighters.
        public int TopUpTo { get; set; } = 5;

        /// Their champion winds up a big swing you can see coming, like an Eclipse. Without it a
        /// Skullpeak duel had no answer but the Unleash they despise (9 Oct 2026).
        public bool DuelChampionCasts { get; set; } = true;
        public int DuelEclipseGrade { get; set; } = 1;
        public int DuelEclipseTicks { get; set; } = 300;

        public int TrialFighters { get; set; } = 4;
        public int TrialEclipseTicks { get; set; } = 270;
        public int LieutenantHealthPercent { get; set; } = 200;
        public int PrecisionTrialSquad { get; set; } = 4;
        public int PrecisionTrialEnemies { get; set; } = 5;
        public int ChiefFighters { get; set; } = 5;
        public int ChiefPowerPercent { get; set; } = 150;
        public int ChiefHealthPercent { get; set; } = 300;
        public int ChiefEclipseTicks { get; set; } = 480;
        public int TutorialFighters { get; set; } = 3;
        public int TutorialPercent { get; set; } = 80;
        public UnitClass LeaderClass { get; set; } = UnitClass.Marksman;
        public int LeaderPercent { get; set; } = 100;
        public int UnleashNerve { get; set; } = BattleSimulation.UnleashNerveCost;
        public int HoldDrainEveryTicks { get; set; } = 3;
        public int SquadSize { get; set; } = 5;

        public int TierPercent(int tier)
        {
            if (tier <= 1) return Tier1Percent;
            return tier == 2 ? Tier2Percent : Tier3Percent;
        }

        public int EnemyScale(int tier)
        {
            return tier <= 1 ? EnemyScaleTier1 : EnemyScaleTier2;
        }

        public int KindPercent(EncounterKind kind)
        {
            switch (kind)
            {
                case EncounterKind.WarParty: return WarPartyPercent;
                case EncounterKind.HoldTheLine: return HoldPercent;
                case EncounterKind.Duel: return DuelPercent;
                case EncounterKind.Trial: return TrialPercent;
                case EncounterKind.Chief: return ChiefPercent;
                case EncounterKind.AlphaHunt:
                case EncounterKind.Ambush: return HuntPercent;
                default: return 100;
            }
        }

        /// The newest campaign rules version a fight may play (Campaign.RulesVersion if higher): tests of an older rule, which
        /// old saves still play, keep testing it. Every version by default.
        public int RulesCap { get; set; } = int.MaxValue;

        public CampaignTuning Clone()
        {
            return (CampaignTuning)MemberwiseClone();
        }
    }
}
