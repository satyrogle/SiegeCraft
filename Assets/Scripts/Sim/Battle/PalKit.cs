using System;
using System.Collections.Generic;

namespace KindredSiege.Sim.Battle
{
    /// The Shade's eight routes, two on each of four buttons (DESIGN_LOCKS, "The Shade's core" and "The floor under the three
    /// Pals"): Ghost Step is now Backstab and Death Warrant is now Fear. Saves and replays keep the numbers; never reorder.
    public enum ShadeRoute
    {
        None = 0,
        Mirror = 1,
        Sanctuary = 2,
        Backstab = 3,
        RiftMine = 4,
        Rupture = 5,
        Fear = 6,
        Breach = 7,
        Echo = 8
    }

    /// The numbers behind the Shade's tricks built so far, each one's first form (campaign rules 23; BattleSimulation.Shade.cs).
    /// Every number is a starting value.
    public static class ShadeRules
    {
        /// None and the eight routes.
        public const int RouteCount = 9;

        /// She enhances at most two of the four buttons, one route each, and a route has three forms.
        public const int MaxButtons = 2;
        public const int MaxForm = 3;

        /// Sanctuary: a clean parry gives each of your standing fighters a shield of this share of his health, for
        /// ShieldTicks. The shield soaks hits before his health, then breaks.
        public const int ShieldPercent = 15;
        public const int ShieldTicks = 3 * SimClock.TicksPerSecond;

        /// Backstab: a Fall Back leaves each of your standing fighters' next hit doubling, in the Cash layer, for
        /// BackstabTicks.
        public const int BackstabCashPercent = 200;
        public const int BackstabTicks = 6 * SimClock.TicksPerSecond;

        /// Fear: killing the enemy your Focus Fire is on takes FearNerve from each enemy beside him, and of those the one
        /// left with the least nerve panics for FearPanicTicks.
        public const int FearNerve = 150;
        public const int FearPanicTicks = 2 * SimClock.TicksPerSecond;

        /// The Trick, Shield and Cash events' details.
        public const string SanctuaryDetail = "sanctuary";
        public const string BackstabDetail = "backstab";
        public const string FearDetail = "fear";

        // ------------------------------------------------------------ the forms (campaign rules 25)
        // Jacob in chat, 31 Oct 2026: "go" on Claude's Shade design (HANDOFF_LOG, "Claim: the Shade's forms"). Every number is a
        // starting value.

        /// Sanctuary 2, Relay: the field also makes one shared pool of this share of the shields it gave. When a hit breaks a
        /// fighter's shield, the pool soaks what is left of it, until the pool is gone or the field ends (ShieldTicks).
        public const int RelayPercent = 25;

        /// Sanctuary 3, Last Refuge: once a fight, a hit that would kill one of the side while its field is up leaves him at 1
        /// health, and every shield of the field shatters.
        public const int RefugeHealth = 1;

        // Sanctuary's price from rules 25: the side's parries do not stagger (BattleSimulation.ParryLands).

        /// Backstab 2, Twin Knives: each fighter's next this many hits land double, not one.
        public const int TwinKnivesHits = 2;

        /// Backstab 3, Shadow Strike: the side keeps attacking while it falls back. Backstab's price from rules 25: the next Fall
        /// Back waits until this long after the last one ran out.
        public const int BackstabLockTicks = 2 * SimClock.TicksPerSecond;

        /// Fear 2, Spreading Dread: an enemy Fear breaks sets Fear off again around him, at most this many more times for one
        /// fall.
        public const int DreadSpreads = 2;

        /// Fear's price from rules 25: Focus Fire lasts this percent as long.
        public const int FearFocusPercent = 75;

        /// The events' details for the forms: Relay's pool soaking (Shield), Last Refuge (Trick, its target the one saved),
        /// Fear spreading (Trick, its target the one who broke), and Turncoat's blow (Trick, then the Hit's detail).
        public const string RelayDetail = "relay";
        public const string RefugeDetail = "refuge";
        public const string DreadDetail = "dread";
        public const string TurncoatDetail = "turncoat";

        // ------------------------------------------------------------ the other five routes (campaign rules 25)
        // Jacob, 1 Nov 2026: "finish shade". Their forms are the ones locked on 31 Oct ("The Shade's core": "as in Codex's paper
        // and the 28 Oct names"), in the sim's timing versions (finding F4: the sim has no positions, so a push is a delay and
        // "nearby" is the one beside him). Every number is a starting value inside the paper's ranges.

        /// Mirror 1, Return: a clean parry sends this share of the parried one's health back at him (Trick "mirror", then a Hit
        /// with the same detail). 2, Catch: times the parries in a row, up to MirrorCatchMax. 3, Pierce: the return also goes
        /// through him into the first of theirs in the row behind him, at PiercePercent of it. Price: only a clean parry returns.
        public const int MirrorPercent = 5;
        public const int MirrorCatchMax = 3;
        public const int PiercePercent = 50;

        /// Rift Mine 1, Mine: a Fall Back leaves a mine for MineTicks; the first of theirs who strikes one of yours meanwhile
        /// sets it off, MinePercent of his health (Trick "mine"). 2, Shock: his next turn waits ShockTicks longer. 3, Wide
        /// Blast: the one or two beside him take it too. Price: your side's attacks wait ReturnPercent of the Fall Back.
        public const int MineTicks = 5 * SimClock.TicksPerSecond;
        public const int MinePercent = 8;
        public const int ShockTicks = SimClock.TicksPerSecond;
        public const int ReturnPercent = 150;

        /// Rupture 1, Weak point: each blow of your side that lands on your Focus Fire's target opens it one step, up to
        /// RuptureSteps; press Focus Fire on him again to rupture it, RupturePercent of his health a step (Trick "rupture").
        /// The press is free, needs a step, and ends the Focus. 2, Crack: a full rupture also cracks him for CrackTicks, his
        /// armour CrackPercent thinner. 3, Finish: each of your fighters but a Medic whose turn comes within FinishTicks takes it
        /// now, and his next turn waits that much longer. Price: changing target, or the Focus running out, loses the steps.
        public const int RuptureSteps = 3;
        public const int RupturePercent = 4;
        public const int CrackTicks = 5 * SimClock.TicksPerSecond / 2;
        public const int CrackPercent = 40;
        public const int FinishTicks = SimClock.TicksPerSecond;

        /// Breach 1, Impact: an Unleash hits up to BreachTargets of their front row (their back row if it is empty) for
        /// ImpactPercent of each one's health, and their next turns wait ShockTicks longer (Trick "breach"). 2, Gap: those it
        /// hit cannot cover anyone for GapTicks. 3, Ram: press Unleash again while it runs to end it and ram the same targets
        /// for RamPercent of each one's health, scaled by the Unleash left (Trick "ram"); at least RamMinTicks must be left.
        /// Price: your side takes ExposedPercent more damage while it runs and ExposedAfterTicks after.
        public const int BreachTargets = 3;
        public const int ImpactPercent = 4;
        public const int GapTicks = 3 * SimClock.TicksPerSecond / 2;
        public const int RamPercent = 12;
        public const int RamMinTicks = SimClock.TicksPerSecond;
        public const int ExposedPercent = 20;
        public const int ExposedAfterTicks = 3 * SimClock.TicksPerSecond / 2;

        /// Echo 1, Record: while an Unleash runs each of your fighters keeps EchoPercent of every blow he lands, and when it ends
        /// his echo hits the first one he recorded for all of it (Trick "echo", then a Hit with the same detail). 2, Release:
        /// press Unleash again to let the echoes go at once, ending it. 3, Converge: that press sends them all at your Focus
        /// Fire's target, if he stands. Price: Unleash's own bonus is halved.
        public const int EchoPercent = 35;

        /// The forms' details: on Trick events, and on the Hit their damage lands as.
        public const string MirrorDetail = "mirror";
        public const string MineDetail = "mine";
        public const string RuptureDetail = "rupture";
        public const string BreachDetail = "breach";
        public const string RamDetail = "ram";
        public const string EchoDetail = "echo";

        /// Rupture's weak point opening a step (Trick, amount the steps), and its Finish (Trick, amount the fighters it pulled).
        public const string WeakPointDetail = "weakpoint";
        public const string FinishDetail = "finish";

        /// True for the detail of damage one of her tricks dealt: it is the trick's, never a fighter's blow, so it opens no weak
        /// point and records no echo.
        public static bool IsTrickDetail(string detail)
        {
            return detail == MirrorDetail || detail == MineDetail || detail == RuptureDetail || detail == BreachDetail ||
                detail == RamDetail || detail == EchoDetail || detail == TurncoatDetail;
        }

        /// The routes the road offers from rules 25: all eight.
        public static bool Built(ShadeRoute route)
        {
            return route > ShadeRoute.None && (int)route < RouteCount;
        }

        /// A route's name, as the road card and the codex show it.
        public static string Name(ShadeRoute route)
        {
            switch (route)
            {
                case ShadeRoute.RiftMine: return "Rift Mine";
                case ShadeRoute.None: return string.Empty;
                default: return route.ToString();
            }
        }

        /// A form's name: the route's name at form 1.
        public static string FormName(ShadeRoute route, int form)
        {
            switch (route)
            {
                case ShadeRoute.Sanctuary: return form >= 3 ? "Last Refuge" : form == 2 ? "Relay" : "Sanctuary";
                case ShadeRoute.Backstab: return form >= 3 ? "Shadow Strike" : form == 2 ? "Twin Knives" : "Backstab";
                case ShadeRoute.Fear: return form >= 3 ? "Turncoat" : form == 2 ? "Spreading Dread" : "Fear";
                case ShadeRoute.Mirror: return form >= 3 ? "Pierce" : form == 2 ? "Catch" : "Mirror";
                case ShadeRoute.RiftMine: return form >= 3 ? "Wide Blast" : form == 2 ? "Shock" : "Rift Mine";
                case ShadeRoute.Rupture: return form >= 3 ? "Finish" : form == 2 ? "Crack" : "Rupture";
                case ShadeRoute.Breach: return form >= 3 ? "Ram" : form == 2 ? "Gap" : "Breach";
                case ShadeRoute.Echo: return form >= 3 ? "Converge" : form == 2 ? "Release" : "Echo";
                default: return Name(route);
            }
        }

        /// What a form adds, in one line, for the road card.
        public static string Describe(ShadeRoute route, int form)
        {
            switch (route)
            {
                case ShadeRoute.Sanctuary:
                    if (form >= 3) return "Once a fight, a killing blow on one of yours under the field leaves him at 1 health.";
                    if (form == 2) return "The field also makes a shared pool that soaks what a broken shield could not.";
                    return "A clean parry shields every fighter. Your parries no longer stagger.";
                case ShadeRoute.Backstab:
                    if (form >= 3) return "Fall Back no longer stops your attacks.";
                    if (form == 2) return "After a Fall Back, each fighter's next " + TwinKnivesHits + " hits land double.";
                    return "After a Fall Back, each fighter's next hit lands double. Fall Back waits " +
                        BackstabLockTicks / SimClock.TicksPerSecond + " s longer.";
                case ShadeRoute.Fear:
                    if (form >= 3) return "The enemy Fear panics hits the enemy beside him once.";
                    if (form == 2) return "An enemy Fear breaks sets Fear off again around him, up to " + DreadSpreads + " more times.";
                    return "When your Focus Fire target falls, those beside him lose nerve and the weakest panics. Focus Fire is a quarter shorter.";
                case ShadeRoute.Mirror:
                    if (form >= 3) return "The return also goes through him into the one behind him.";
                    if (form == 2) return "The return grows with each parry in a row, up to " + MirrorCatchMax + ".";
                    return "A clean parry sends a hit back at whoever you parried. Only a clean parry returns.";
                case ShadeRoute.RiftMine:
                    if (form >= 3) return "The blast also hits the ones beside him.";
                    if (form == 2) return "The blast also makes his next turn wait.";
                    return "Fall Back leaves a mine that blasts the first enemy who strikes one of yours. Your attacks wait longer after a Fall Back.";
                case ShadeRoute.Rupture:
                    if (form >= 3) return "A full rupture makes your fighters about to act act now, then wait that much longer.";
                    if (form == 2) return "A full rupture also cracks his armour.";
                    return "Your hits on your Focus Fire target open a weak point; press Focus Fire on him again to rupture it. Changing target loses it.";
                case ShadeRoute.Breach:
                    if (form >= 3) return "Press Unleash again to end it and ram their front line, harder the more is left.";
                    if (form == 2) return "Those it hits cannot cover anyone for a moment.";
                    return "Unleash hits their front line and makes them wait. Your side takes more damage while it runs.";
                case ShadeRoute.Echo:
                    if (form >= 3) return "Letting them go sends every echo at your Focus Fire target.";
                    if (form == 2) return "Press Unleash again to let the echoes go at once.";
                    return "While Unleash runs your fighters record their hits, and replay part of them when it ends. Unleash's own bonus is halved.";
                default: return string.Empty;
            }
        }

        /// The button a route sits on.
        public static Directive ButtonOf(ShadeRoute route)
        {
            switch (route)
            {
                case ShadeRoute.Mirror:
                case ShadeRoute.Sanctuary: return Directive.Hold;
                case ShadeRoute.Backstab:
                case ShadeRoute.RiftMine: return Directive.FallBack;
                case ShadeRoute.Rupture:
                case ShadeRoute.Fear: return Directive.FocusFire;
                case ShadeRoute.Breach:
                case ShadeRoute.Echo: return Directive.Unleash;
                default: throw new ArgumentOutOfRangeException(nameof(route));
            }
        }
    }

    /// The Whisperer's habits, two a class, with the 28 Oct audit's names (campaign rules 24; DESIGN_LOCKS, "Habits come with
    /// the game, the Whisperer upgrades them"). Saves and replays keep the numbers; never reorder.
    public enum WhispererHabit
    {
        None = 0,
        Bodyguard = 1, Rescuer = 2,
        Breaker = 3, Avenger = 4,
        CounterShot = 5, Spotter = 6,
        Ambusher = 7, Gatekeeper = 8,
        Triage = 9, FrontFirst = 10
    }

    /// The squad's pairs (28 Oct, fix 2). Saves and replays keep the numbers; never reorder.
    public enum SquadPair
    {
        None = 0,
        PinAndShoot = 1,
        OpenAndCut = 2,
        SnareAndRip = 3,
        MendAndHold = 4
    }

    /// The Whisperer in the fight (campaign rules 24; FightRules.Whisper; BattleSimulation.Whisperer.cs). Every number is a
    /// starting value.
    public static class WhisperRules
    {
        public const int HabitCount = 10;
        public const int PairCount = 4;

        /// Triage waits until someone is under this much of his health, in thousandths.
        public const int TriageBelowPermille = 250;

        /// An upgraded Triage's saved treatment lands twice as big, in the status layer, which no other heal pays into.
        public const int TriageUpPercent = 200;

        /// Rank 2: his class spell happens one more time.
        public const int RankTwo = 2;

        /// The Whisper event's details: a pair's free answer or an upgraded Avenger's free strike; a Medic keeping a
        /// treatment in hand; an Ambusher's trap springing on a heavy blow, and an upgraded one stopping it; an upgraded
        /// Counter Shot fired with no aim.
        public const string FreeDetail = "free";
        public const string InHandDetail = "inhand";
        public const string AmbushDetail = "ambush";
        public const string StoppedDetail = "stopped";
        public const string CounterDetail = "counter";

        /// The class a habit belongs to.
        public static UnitClass ClassOf(WhispererHabit habit)
        {
            switch (habit)
            {
                case WhispererHabit.Bodyguard:
                case WhispererHabit.Rescuer: return UnitClass.Warden;
                case WhispererHabit.Breaker:
                case WhispererHabit.Avenger: return UnitClass.Skirmisher;
                case WhispererHabit.CounterShot:
                case WhispererHabit.Spotter: return UnitClass.Marksman;
                case WhispererHabit.Ambusher:
                case WhispererHabit.Gatekeeper: return UnitClass.Trapper;
                case WhispererHabit.Triage:
                case WhispererHabit.FrontFirst: return UnitClass.Medic;
                default: throw new ArgumentOutOfRangeException(nameof(habit));
            }
        }

        /// A pair's two classes: the one whose spell lands first, and the one who answers it.
        public static UnitClass FirstOf(SquadPair pair)
        {
            switch (pair)
            {
                case SquadPair.PinAndShoot:
                case SquadPair.SnareAndRip: return UnitClass.Trapper;
                case SquadPair.OpenAndCut: return UnitClass.Marksman;
                case SquadPair.MendAndHold: return UnitClass.Medic;
                default: throw new ArgumentOutOfRangeException(nameof(pair));
            }
        }

        public static UnitClass AnswerOf(SquadPair pair)
        {
            switch (pair)
            {
                case SquadPair.PinAndShoot: return UnitClass.Marksman;
                case SquadPair.OpenAndCut:
                case SquadPair.SnareAndRip: return UnitClass.Skirmisher;
                case SquadPair.MendAndHold: return UnitClass.Warden;
                default: throw new ArgumentOutOfRangeException(nameof(pair));
            }
        }
    }

    /// What a side brings into a fight from the Pals' stores (campaign rules 23; DESIGN_LOCKS, "The floor under the three
    /// Pals"): the Shade's routes on its buttons, each at its form, and one percent on its order costs, the cheaper hook (a
    /// talent that makes orders cheaper sets it, below 0). From rules 24 the Whisperer's too: each class's habit, upgraded or
    /// not, and the squad's pair (WithWhisper). None for a side that brings nothing.
    public sealed class PalKit
    {
        public static readonly PalKit None = new PalKit(null, 0);

        public const int MinPercent = -90;
        public const int MaxPercent = 300;

        private readonly int[] _forms = new int[ShadeRules.RouteCount];

        // Each habit: 0 not set, 1 set, 2 set and upgraded, at most one a class.
        private readonly int[] _habits = new int[WhisperRules.HabitCount + 1];

        private PalKit(PalKit routes, int[] habits, SquadPair pair)
        {
            Array.Copy(routes._forms, _forms, _forms.Length);
            OrderCostPercent = routes.OrderCostPercent;
            var taken = new bool[(int)UnitClass.Medic + 1];
            for (int h = 1; habits != null && h < habits.Length && h <= WhisperRules.HabitCount; h++)
            {
                if (habits[h] <= 0) continue;
                int cls = (int)WhisperRules.ClassOf((WhispererHabit)h);
                if (taken[cls]) continue;
                taken[cls] = true;
                _habits[h] = Math.Min(2, habits[h]);
            }
            Pair = pair < SquadPair.None || (int)pair > WhisperRules.PairCount ? SquadPair.None : pair;
        }

        /// The same kit with the Whisperer's picks (rules 24): habits as Habits gives them (0, 1 set, 2 upgraded), and the pair.
        public PalKit WithWhisper(IReadOnlyList<int> habits, SquadPair pair)
        {
            int[] copy = new int[WhisperRules.HabitCount + 1];
            for (int h = 1; habits != null && h < habits.Count && h <= WhisperRules.HabitCount; h++) copy[h] = habits[h];
            return new PalKit(this, copy, pair);
        }

        /// The squad's pair, None for none.
        public SquadPair Pair { get; }

        /// This class's habit on this side, None for its plain behaviour.
        public WhispererHabit HabitOf(UnitClass cls)
        {
            for (int h = 1; h <= WhisperRules.HabitCount; h++)
                if (_habits[h] > 0 && WhisperRules.ClassOf((WhispererHabit)h) == cls) return (WhispererHabit)h;
            return WhispererHabit.None;
        }

        /// Whether this habit is set and upgraded.
        public bool Upgraded(WhispererHabit habit)
        {
            return habit > WhispererHabit.None && (int)habit <= WhisperRules.HabitCount && _habits[(int)habit] >= 2;
        }

        /// Every habit's state, in WhispererHabit's order, for the replay evidence.
        public int[] Habits()
        {
            return (int[])_habits.Clone();
        }

        /// routes: each route and its form, in order. A route on a button already taken, or past the second button, is
        /// left out; a form is kept between 1 and ShadeRules.MaxForm.
        public PalKit(IReadOnlyList<KeyValuePair<ShadeRoute, int>> routes, int orderCostPercent)
        {
            int buttons = 0;
            for (int i = 0; routes != null && i < routes.Count; i++)
            {
                ShadeRoute route = routes[i].Key;
                if (route <= ShadeRoute.None || (int)route >= ShadeRules.RouteCount || routes[i].Value <= 0) continue;
                if (buttons >= ShadeRules.MaxButtons || RouteOn(ShadeRules.ButtonOf(route)) != ShadeRoute.None) continue;
                _forms[(int)route] = Math.Min(ShadeRules.MaxForm, routes[i].Value);
                buttons++;
            }
            OrderCostPercent = Math.Max(MinPercent, Math.Min(MaxPercent, orderCostPercent));
        }

        /// The cheaper hook: a percent on every one of this side's order costs. 0 for none.
        public int OrderCostPercent { get; }

        /// A route's form on this side, 0 without it.
        public int FormOf(ShadeRoute route)
        {
            return route <= ShadeRoute.None || (int)route >= ShadeRules.RouteCount ? 0 : _forms[(int)route];
        }

        public bool Has(ShadeRoute route) => FormOf(route) > 0;

        /// The route on this button, None for a plain button.
        public ShadeRoute RouteOn(Directive button)
        {
            for (int r = 1; r < ShadeRules.RouteCount; r++)
                if (_forms[r] > 0 && ShadeRules.ButtonOf((ShadeRoute)r) == button) return (ShadeRoute)r;
            return ShadeRoute.None;
        }

        public bool IsNone
        {
            get
            {
                if (OrderCostPercent != 0 || Pair != SquadPair.None) return false;
                for (int r = 1; r < ShadeRules.RouteCount; r++) if (_forms[r] > 0) return false;
                for (int h = 1; h <= WhisperRules.HabitCount; h++) if (_habits[h] > 0) return false;
                return true;
            }
        }

        /// Every route's form, in ShadeRoute's order, for the replay evidence.
        public int[] Forms()
        {
            return (int[])_forms.Clone();
        }

        /// The same kit back from Forms and the percent.
        public static PalKit FromForms(IReadOnlyList<int> forms, int orderCostPercent)
        {
            var routes = new List<KeyValuePair<ShadeRoute, int>>();
            for (int r = 1; forms != null && r < forms.Count && r < ShadeRules.RouteCount; r++)
                if (forms[r] > 0) routes.Add(new KeyValuePair<ShadeRoute, int>((ShadeRoute)r, forms[r]));
            return new PalKit(routes, orderCostPercent);
        }

        public void AppendTo(SnapshotBuilder b, string prefix)
        {
            for (int r = 1; r < ShadeRules.RouteCount; r++)
                if (_forms[r] > 0) b.Field(prefix + "shade." + ((ShadeRoute)r).ToString().ToLowerInvariant(), _forms[r]);
            if (OrderCostPercent != 0) b.Field(prefix + "orderCost", OrderCostPercent);
            // The Whisperer's (rules 24), written only when set, so every older fight keeps its checksum.
            for (int h = 1; h <= WhisperRules.HabitCount; h++)
                if (_habits[h] > 0) b.Field(prefix + "habit." + ((WhispererHabit)h).ToString().ToLowerInvariant(), _habits[h]);
            if (Pair != SquadPair.None) b.Field(prefix + "pair", (int)Pair);
        }
    }
}
