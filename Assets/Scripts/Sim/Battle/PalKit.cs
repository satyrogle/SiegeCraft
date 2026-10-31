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

        /// The routes the road offers from rules 25: the three built so far. The other five wait for their forms.
        public static bool Built(ShadeRoute route)
        {
            return route == ShadeRoute.Sanctuary || route == ShadeRoute.Backstab || route == ShadeRoute.Fear;
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
