using System;
using System.Collections.Generic;

namespace KindredSiege.Sim.Battle
{
    /// What one of a side's orders did (OrderEffect.Kind). Covered, ScareBlocked, Pulled, Shattered, OutOfReach, StaleBlock,
    /// Feinted and WaitedOut went against it.
    public enum OrderEffectKind
    {
        /// A Hold parried a big hit: nothing landed, and the striker staggered.
        Parried = 0,

        /// A Hold soaked a hit, so it landed lighter. OrderEffect.CouldParry says whether a live Hold given within
        /// FightRules.PerfectHoldTicks before it would have parried it instead.
        Soaked = 1,

        /// A Hold braced against a march or through her Overclock, pressed open veins, stood fast against a riptide, held
        /// through a surge, or held the line against the hunters' Drive.
        Braced = 2,

        /// A Hold blocked the cage bear's Roar, or the other side's Unleash scare.
        Blocked = 3,

        /// Fall Back got clear of it.
        Dodged = 4,

        /// Focus Fire broke a cast, a move or a shooter's aim.
        Broke = 5,

        /// Focus Fire on his band while his Sun Mantle burned, so nobody struck him.
        Switched = 6,

        /// Focus Fire on the cage bear's maw cut its Roar off.
        Cut = 7,

        /// Unleash smashed a wall, his Basin or his Sluice Gate.
        Smashed = 8,

        /// Unleash staggered a march.
        Staggered = 9,

        /// Unleash tore the hunters' net off its prey.
        Tore = 10,

        /// Unleash finished a combo (OrderEffect.Move: Moves.Ambush or FightRules.Riposte).
        Combo = 11,

        /// Focus Fire brought its target down, or broke the beast's part it was on.
        BroughtDown = 12,

        /// The damage your side dealt while it ran: to its target for Focus Fire, to anyone for Unleash.
        Damage = 13,

        /// A Warden of theirs covered your Focus Fire's target and took the blows meant for them (OrderEffect.TargetId is that
        /// Warden, Amount what his Guard took off).
        Covered = 14,

        /// Their Hold blocked your Unleash's scare.
        ScareBlocked = 15,

        /// The Thundercoast chief's Riptide pulled your side into the deep as you fell back (Amount: the health it took).
        Pulled = 16,

        /// A Shieldbreaker's heavy swing broke your Hold, and landed in full.
        Shattered = 17,

        /// Under the parry at the center: a move landed in full on one of yours a Hold does not cover (the back row) while it ran
        /// (Amount: the health it took).
        OutOfReach = 18,

        /// Under the parry at the center before the squad first (campaign rules 8): a stale Hold met a beat, so it only soaked and
        /// cost your nerve (Amount: the nerve).
        StaleBlock = 19,

        /// Under the parry at the center: his beat was a fake while it ran, so the real one came later (Amount: the ticks until
        /// it).
        Feinted = 20,

        /// Under the parry at the center: his beat waited until your Hold went stale (Amount: the ticks it waited).
        WaitedOut = 21
    }

    /// Why an order did nothing (OrderResult.Miss), once its result is final.
    public enum OrderMiss
    {
        None = 0,

        /// Nothing came at you while it ran.
        NothingCame = 1,

        /// OrderResult.MissMove landed in full while it ran, and this order does not meet it; for Fall Back, also the
        /// Thundercoast chief's Riptide pulling you in.
        WrongOrder = 2,

        /// OrderResult.MissMove landed when this order could no longer meet it: a Hold too old to parry a hit a timed one would
        /// have parried, or a Hold or Fall Back that ran out within a second before a hit it meets.
        TooEarly = 3,

        /// Focus Fire: their Warden (OrderResult.MissMove, his id) covered your target and took every blow.
        Covered = 4,

        /// Focus Fire or Unleash: no blow of yours landed while it ran. A Hold, under the parry at the center: OrderResult.MissMove
        /// landed in full on one of yours it does not cover (the back row) while it ran.
        OutOfReach = 5,

        /// Unleash: their Hold blocked its scare, and it did nothing else.
        Blocked = 6,

        /// OrderResult.MissMove, a move that runs a while, had already hit you in full before this order came.
        TooLate = 7,

        /// OrderResult.MissMove came while it ran and this order meets it, but something else of yours met it first: a Fall
        /// Back's dodge (which goes before a Hold), a Warden's cover, a Medic's treatment, a break.
        MetFirst = 8,

        /// A Hold: OrderResult.MissMove's beat was a fake while it ran (OrderEffectKind.Feinted).
        Feinted = 9,

        /// A Hold: OrderResult.MissMove's beat waited until it went stale (OrderEffectKind.WaitedOut).
        WaitedOut = 10
    }

    /// One thing an order did, at the tick it happened.
    public readonly struct OrderEffect
    {
        /// The name for a Marksman's Aimed Shot broken by your Focus Fire, which has no Tell of its own.
        public const string AimedShot = "Aimed Shot";

        public OrderEffect(long tick, OrderEffectKind kind, string move, string targetId, int amount, bool couldParry)
        {
            Tick = tick;
            Kind = kind;
            Move = move ?? string.Empty;
            TargetId = targetId ?? string.Empty;
            Amount = amount;
            CouldParry = couldParry;
        }

        public long Tick { get; }
        public OrderEffectKind Kind { get; }

        /// What it met, by the name on that move's Tell, Windup or Land ("Eclipse", "Heavy", "Cleave", "Brand", "Bite" and so
        /// on; "Unleash" for a scare; "Steel Storm" or "Lockdown" for a wall; AimedShot); empty for Damage, Covered and
        /// BroughtDown.
        public string Move { get; }

        /// The enemy it was about: whoever did the move it met, the target it hit or brought down, or the Warden who covered
        /// that target.
        public string TargetId { get; }

        /// The damage dealt (Damage), what a covering Warden's Guard took off (Covered), the health the Riptide took (Pulled)
        /// or a move out of a Hold's reach took (OutOfReach), the nerve a stale Hold cost (StaleBlock), or the ticks a fake
        /// beat or a wait out put off the real one (Feinted, WaitedOut); 0 otherwise.
        public int Amount { get; }

        /// For a Soaked hit: a live Hold given within FightRules.PerfectHoldTicks before it would have parried it.
        public bool CouldParry { get; }

        /// True unless it went against the order (Covered, ScareBlocked, Pulled, Shattered, OutOfReach, StaleBlock, Feinted,
        /// WaitedOut).
        public bool Helped => Kind != OrderEffectKind.Covered && Kind != OrderEffectKind.ScareBlocked &&
            Kind != OrderEffectKind.Pulled && Kind != OrderEffectKind.Shattered && Kind != OrderEffectKind.OutOfReach &&
            Kind != OrderEffectKind.StaleBlock && Kind != OrderEffectKind.Feinted && Kind != OrderEffectKind.WaitedOut;
    }

    /// An order a side tried that did not go (BattleSimulation.LastRejection, CampaignFight.LastRejection): what it was, when,
    /// at whom, and why, in the words the check gave. Not part of the fight: nothing of it is in the snapshot or the log.
    public readonly struct OrderRejection
    {
        public static readonly OrderRejection None = new OrderRejection(Directive.Hold, -1, string.Empty, string.Empty);

        public OrderRejection(Directive order, long tick, string targetId, string reason)
        {
            Order = order;
            Tick = tick;
            TargetId = targetId ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public Directive Order { get; }
        public long Tick { get; }
        public string TargetId { get; }
        public string Reason { get; }
        public bool IsNone => Tick < 0;
    }

    /// What one Hold, Fall Back, Focus Fire or Unleash of a side did (BattleSimulation.OrderResults).
    public readonly struct OrderResult
    {
        public static readonly OrderResult None = new OrderResult(Directive.Hold, -1, -1, string.Empty, false, false, false,
            new OrderEffect[0], OrderMiss.None, string.Empty);

        public OrderResult(Directive order, long tick, long endTick, string targetId, bool paused, bool running, bool final,
            IReadOnlyList<OrderEffect> effects, OrderMiss miss, string missMove)
        {
            Order = order;
            Tick = tick;
            EndTick = endTick;
            TargetId = targetId ?? string.Empty;
            Paused = paused;
            Running = running;
            Final = final;
            Effects = effects ?? new OrderEffect[0];
            Miss = miss;
            MissMove = missMove ?? string.Empty;
        }

        public Directive Order { get; }

        /// The tick it was given on, and the tick it stops on (in force from Tick up to, not including, EndTick): its length,
        /// cut short by a newer order of the same kind (your side's reflexes too), by a parry that spent a Hold or a
        /// Shieldbreaker who broke one, by a Focus Fire's target falling, or by the fight's end.
        public long Tick { get; }
        public long EndTick { get; }

        /// Focus Fire's target, else empty.
        public string TargetId { get; }

        /// Given while the fight was paused: a Hold given so soaks but never parries (HeavyRules.PausedOrderArg).
        public bool Paused { get; }

        /// Still in force.
        public bool Running { get; }

        /// Its result will not change: it has stopped; a Hold or Fall Back that ran out with nothing to show has waited a second
        /// for a hit it came too early for; and no move of theirs that runs a while and is judged when it ends (the Sun Mantle,
        /// her Overclock, Open Veins, the Riptide, the Tidal Bore, the Storm Surge) that it was up against is still running.
        public bool Final { get; }

        /// What it did, in the order it happened. A move that runs a while can add one after the order stopped.
        public IReadOnlyList<OrderEffect> Effects { get; }

        /// Why it did nothing, once it is Final with no effect that helped; OrderMiss.None otherwise.
        public OrderMiss Miss { get; }

        /// The move or the Warden the miss is about, else empty.
        public string MissMove { get; }

        public bool IsNone => Tick < 0;

        /// True when any of its effects helped.
        public bool Helped
        {
            get
            {
                for (int i = 0; i < Effects.Count; i++) if (Effects[i].Helped) return true;
                return false;
            }
        }
    }

    /// The order results (Jacob, 23 Oct 2026: "go, build the order results in the sim and hand them to Codex"), for Gate 2 of
    /// Codex's plan: for every Hold, Fall Back, Focus Fire and Unleash a side gave, what it met and what it did, or why it did
    /// nothing, so the screen can show it where the order landed and a player learns each button by seeing it work or fail. It
    /// reads the fight's own history (its events) and changes nothing: a fight that asks plays exactly as one that never does.
    /// The one thing it keeps is which orders were given while the fight was paused, which the command log brings back on a
    /// replay or a resumed save.
    ///
    /// Claude's calls while building, Jacob's to change: only the orders you give count, not your side's reflexes, though a
    /// reflex takes over from your order of its kind as it does in the fight; a soak counts as something it did, with
    /// CouldParry saying a timed Hold would have parried it; an answer that needs one of your fighters as well as the order (a
    /// Marksman's shot through your Focus Fire, say) counts for the order; a move that runs a while and lands when it ends
    /// counts for every order of the kind that meets it that was up for any of it; a Hold or Fall Back that ran out within a
    /// second before a hit it meets came too early; a Hold softening ordinary blows on the front row is not counted, so
    /// NothingCame means no move or spell came; an Unleash is judged on its own tick (smash, stagger, tear, combo, a blocked
    /// scare) and by the damage your side dealt while it ran; the Damage of a Focus Fire or an Unleash counts every blow as
    /// dealt, before it is capped at the health left; an execution is the verdict's, not the order's.
    public sealed partial class BattleSimulation
    {
        // The longest a move that runs a while takes from its start to its Land: the Storm Surge's high water.
        private const int SpanReachTicks = ChiefKits.SurgeTicks;

        private static readonly string EclipseMove = Directives.Name(Directive.Eclipse);

        // The Directive events of orders given while the fight was paused, by index (TryApply). Not in the snapshot: the
        // command log brings it back.
        private readonly List<int> _pausedOrders = new List<int>();

        // Each side's last order that did not go (TryApply). Not part of the fight.
        private readonly OrderRejection[] _rejected = { OrderRejection.None, OrderRejection.None };

        /// This side's last order that did not go, and why, or OrderRejection.None.
        public OrderRejection LastRejection(int side)
        {
            return side >= 0 && side <= 1 ? _rejected[side] : OrderRejection.None;
        }

        /// What every Hold, Fall Back, Focus Fire and Unleash this side gave did, oldest first. It reads the whole fight, so
        /// ask when something happens (an order given, a Land, a Release), not every frame.
        public IReadOnlyList<OrderResult> OrderResults(int side)
        {
            var list = new List<OrderResult>();
            if (side < 0 || side > 1) return list;
            for (int i = 0; i < _events.Count; i++)
                if (TryOrderAt(side, i, out Directive order)) list.Add(ResultOf(side, i, order));
            return list;
        }

        /// What this side's latest Hold, Fall Back, Focus Fire or Unleash did, or OrderResult.None.
        public OrderResult LastOrderResult(int side)
        {
            if (side < 0 || side > 1) return OrderResult.None;
            for (int i = _events.Count - 1; i >= 0; i--)
                if (TryOrderAt(side, i, out Directive order)) return ResultOf(side, i, order);
            return OrderResult.None;
        }

        // ------------------------------------------------------------ which events are orders

        // One of the four orders this side gave itself, not a reflex.
        private bool TryOrderAt(int side, int at, out Directive order)
        {
            order = Directive.Hold;
            BattleEvent e = _events[at];
            if (e.Kind != BattleEventKind.Directive || e.ActorSide != side ||
                !string.Equals(e.ActorId, side == 0 ? "side0" : "side1", StringComparison.Ordinal))
                return false;
            return Directives.TryParse(e.Detail, out order) &&
                (order == Directive.Hold || order == Directive.FallBack || order == Directive.FocusFire || order == Directive.Unleash);
        }

        // Any order of this kind from this side, its reflexes too: it takes over from the one before.
        private static bool SameOrder(BattleEvent e, int side, Directive order)
        {
            return e.Kind == BattleEventKind.Directive && e.ActorSide == side && Directives.TryParse(e.Detail, out Directive d) && d == order;
        }

        // How long an order runs, as ApplyDirective set it: longer when an interrupt empowered it, and by a favour.
        private int OrderLength(int side, Directive order, int at)
        {
            int percent = 100;
            if (at + 1 < _events.Count)
            {
                BattleEvent e = _events[at + 1];
                if (e.Kind == BattleEventKind.Empowered && e.Tick == _events[at].Tick && e.ActorSide == side &&
                    e.Detail == Directives.Name(order))
                    percent = e.Amount;
            }
            switch (order)
            {
                case Directive.Hold: return HoldTicks * percent / 100;
                case Directive.FallBack: return Longer(FallBackTicks * percent / 100, _rules[side].Favours.FallBackLongerPercent);
                case Directive.FocusFire: return Longer(FocusTicks * percent / 100, _rules[side].Favours.FocusLongerPercent);
                default: return Longer(UnleashTicks * percent / 100, _rules[side].Favours.UnleashLongerPercent);
            }
        }

        // ------------------------------------------------------------ one order's result

        private OrderResult ResultOf(int side, int at, Directive order)
        {
            BattleEvent given = _events[at];
            long tick = given.Tick;
            string target = order == Directive.FocusFire ? given.TargetId : string.Empty;
            long end = tick + OrderLength(side, order, at);
            bool ranOut = true;
            for (int j = at + 1; j < _events.Count && _events[j].Tick < end; j++)
            {
                BattleEvent x = _events[j];
                bool stop = SameOrder(x, side, order);
                if (stop) end = x.Tick;
                else if (order == Directive.Hold && ((x.Kind == BattleEventKind.Parry && x.ActorSide == side) || Shatters(x, side)))
                {
                    // A parry spends a Hold, and a Shieldbreaker breaks one.
                    end = x.Tick + 1;
                    stop = true;
                }
                else if (order == Directive.FocusFire && x.Kind == BattleEventKind.Kill &&
                    string.Equals(x.TargetId, target, StringComparison.Ordinal))
                {
                    // A Focus Fire has nothing left to hit once its target falls.
                    end = x.Tick + 1;
                    stop = true;
                }
                if (!stop) continue;
                ranOut = false;
                break;
            }
            if (_finished && end > _endTick + 1)
            {
                end = _endTick + 1;
                ranOut = false;
            }

            var effects = new List<OrderEffect>();
            switch (order)
            {
                case Directive.Hold:
                    HoldEffects(side, at, end, effects);
                    break;
                case Directive.FallBack:
                    FallBackEffects(side, at, end, effects);
                    break;
                case Directive.FocusFire:
                    FocusEffects(side, at, end, target, effects);
                    break;
                default:
                    UnleashEffects(side, at, end, effects);
                    break;
            }
            // In the order they happened; a tie keeps the order found.
            for (int k = 1; k < effects.Count; k++)
            {
                OrderEffect e = effects[k];
                int m = k;
                for (; m > 0 && effects[m - 1].Tick > e.Tick; m--) effects[m] = effects[m - 1];
                effects[m] = e;
            }
            bool running = !_finished && end > _tick;
            bool helped = false;
            for (int k = 0; k < effects.Count && !helped; k++) helped = effects[k].Helped;
            // A Hold or Fall Back that ran out with nothing to show waits a second: a hit right after it came too early.
            bool watching = !helped && ranOut && !_finished && (order == Directive.Hold || order == Directive.FallBack) &&
                _tick < end + SimClock.TicksPerSecond;
            bool final = !running && !watching && !AwaitsSpan(side, order, end);
            OrderMiss miss = OrderMiss.None;
            string missMove = string.Empty;
            if (final && !helped) miss = MissOf(side, at, end, ranOut, order, effects, out missMove);
            return new OrderResult(order, tick, end, target, _pausedOrders.Contains(at), running, final, effects, miss, missMove);
        }

        // ------------------------------------------------------------ what each order did

        // A Hold: parries (which spend it), soaks, braces and blocks, and a Shieldbreaker breaking it. Under the parry at the
        // center also what went against it: a move landing out of its reach, a stale Hold's nerve, a fake beat, a wait out.
        private void HoldEffects(int side, int at, long end, List<OrderEffect> effects)
        {
            int foe = 1 - side;
            long start = _events[at].Tick;
            for (int j = at + 1; j < _events.Count && _events[j].Tick < end + SpanReachTicks; j++)
            {
                BattleEvent x = _events[j];
                bool inside = x.Tick < end;
                if (x.Kind == BattleEventKind.Land && x.ActorSide == foe)
                {
                    SplitLand(x.Detail, out string move, out string how);
                    if (Spans(move) ? !Overlaps(start, end, SpanStart(j, move), x.Tick) : !inside) continue;
                    if (how == ChiefKits.Braced)
                        effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Braced, move, x.ActorId, 0, false));
                    else if (how == ChiefKits.Soaked)
                        effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Soaked, move, x.ActorId, 0, Parryable(move, foe, x.Tick)));
                    else if (how == BeastKits.Blocked)
                        effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Blocked, move, x.ActorId, 0, false));
                    else if (_parryOn && how == ChiefKits.Full && !Spans(move) && OutOfHoldReach(move, x.TargetId, foe))
                        effects.Add(new OrderEffect(x.Tick, OrderEffectKind.OutOfReach, move, x.ActorId, x.Amount, false));
                    continue;
                }
                if (!inside) continue;
                if (x.Kind == BattleEventKind.Parry && x.ActorSide == side)
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Parried, x.Detail.Length > 0 ? x.Detail : ParriedMove(j),
                        x.TargetId, 0, false));
                else if (x.Kind == BattleEventKind.Release && x.ActorSide == foe && x.Detail == Moves.Soaked && !Summed(j))
                {
                    string move = WindupMove(j);
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Soaked, move, x.ActorId, 0, Parryable(move, foe, x.Tick)));
                }
                else if (x.Kind == BattleEventKind.Directive && x.ActorSide == side && x.Detail == BlockedDetail)
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Blocked, Directives.Name(Directive.Unleash), string.Empty, 0, false));
                else if (Shatters(x, side))
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Shattered, HeavyRules.Heavy, x.ActorId, 0, false));
                else if (!_parryOn) continue;
                else if (x.Kind == BattleEventKind.Release && x.ActorSide == foe && x.Detail == Moves.Full && !Summed(j) &&
                         OutOfHoldReach(WindupMove(j), x.TargetId, foe))
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.OutOfReach, WindupMove(j), x.ActorId, x.Amount, false));
                else if (x.Kind == BattleEventKind.Reaction && x.ActorSide == side && x.Detail == ParryRules.BlockDetail)
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.StaleBlock, BeatMoveOf(j, x.TargetId), x.TargetId, x.Amount, false));
                else if (x.Kind == BattleEventKind.Reaction && x.ActorSide == foe && x.Detail == ParryRules.FeintDetail)
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Feinted, BeatMoveOf(j, x.ActorId), x.ActorId, x.Amount, false));
                else if (x.Kind == BattleEventKind.Reaction && x.ActorSide == foe && x.Detail == WaitOutDetail)
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.WaitedOut, BeatMoveOf(j, x.ActorId), x.ActorId, x.Amount, false));
            }
            FlurryBraced(side, at, end, effects);
        }

        // A move a Hold soaks or parries landing on one of yours it does not cover: someone outside the front row.
        private bool OutOfHoldReach(string move, string targetId, int foe)
        {
            if (targetId.Length == 0 || !Meets(Directive.Hold, move, foe)) return false;
            BattleUnit target = FindById(targetId);
            return target != null && target.Side != foe && target.Row != Row.Front;
        }

        // The move a beat belongs to (at a fake, a wait out, or a stale Hold's block): the latest of this fighter's Windups and
        // Tells that was due by then, so the receipt names it the moment it happens. Empty when there is none.
        private string BeatMoveOf(int at, string doerId)
        {
            long tick = _events[at].Tick;
            for (int j = at - 1; j >= 0; j--)
            {
                BattleEvent x = _events[j];
                if ((x.Kind == BattleEventKind.Windup || x.Kind == BattleEventKind.Tell) && string.Equals(x.ActorId, doerId, StringComparison.Ordinal) &&
                    x.Tick + x.Amount <= tick + 1)
                    return x.Detail;
            }
            return string.Empty;
        }

        // The Skullpeak chief's Overclock: her jab's Land starts the flurry, and a Hold up for any of it braces it, which the
        // sim answers when it ends with nothing landing then.
        private void FlurryBraced(int side, int at, long end, List<OrderEffect> effects)
        {
            int foe = 1 - side;
            long start = _events[at].Tick;
            string jab = ChiefKits.OverclockName + "." + ChiefKits.Full;
            int from = at;
            while (from > 0 && _events[from - 1].Tick > start - ChiefKits.OverclockTicks) from--;
            for (int j = from; j < _events.Count && _events[j].Tick < end; j++)
            {
                BattleEvent x = _events[j];
                if (x.Kind != BattleEventKind.Land || x.ActorSide != foe || x.Detail != jab) continue;
                long over = x.Tick + ChiefKits.OverclockTicks - 1;
                if (over >= _tick || !Overlaps(start, end, x.Tick, over)) continue;
                BattleUnit chief = KitChief(foe);
                effects.Add(new OrderEffect(over, OrderEffectKind.Braced, ChiefKits.OverclockName, chief == null ? x.ActorId : chief.Id, 0,
                    false));
            }
        }

        // A Fall Back: dodges, and the Riptide's pull.
        private void FallBackEffects(int side, int at, long end, List<OrderEffect> effects)
        {
            int foe = 1 - side;
            long start = _events[at].Tick;
            for (int j = at + 1; j < _events.Count && _events[j].Tick < end + SpanReachTicks; j++)
            {
                BattleEvent x = _events[j];
                if (x.ActorSide != foe) continue;
                bool inside = x.Tick < end;
                if (x.Kind == BattleEventKind.Land)
                {
                    SplitLand(x.Detail, out string move, out string how);
                    if (move == ChiefKits.RiptideName)
                    {
                        if (inside && how == ChiefKits.Full)
                            effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Pulled, move, x.ActorId, x.Amount, false));
                    }
                    else if (how == ChiefKits.Dodged && (Spans(move) ? Overlaps(start, end, SpanStart(j, move), x.Tick) : inside))
                        effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Dodged, move, x.ActorId, 0, false));
                }
                else if (inside && x.Kind == BattleEventKind.Release && x.Detail == Moves.Dodged && !Summed(j))
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Dodged, WindupMove(j), x.ActorId, 0, false));
            }
        }

        // A Focus Fire on its target: the casts, moves and aims it broke, its target brought down, the damage dealt to them,
        // and a Warden of theirs covering them.
        private void FocusEffects(int side, int at, long end, string target, List<OrderEffect> effects)
        {
            int foe = 1 - side;
            long start = _events[at].Tick;
            int damage = 0, covered = 0;
            long lastHit = -1, lastCover = -1;
            string warden = string.Empty;
            for (int j = at + 1; j < _events.Count && _events[j].Tick < end + SpanReachTicks; j++)
            {
                BattleEvent x = _events[j];
                bool inside = x.Tick < end;
                bool onTarget = string.Equals(x.TargetId, target, StringComparison.Ordinal);
                if (x.Kind == BattleEventKind.Land && x.ActorSide == foe)
                {
                    SplitLand(x.Detail, out string move, out string how);
                    if (how == ChiefKits.Switched)
                    {
                        if (Overlaps(start, end, SpanStart(j, move), x.Tick) && OnHisBand(target, x.ActorId))
                            effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Switched, move, x.ActorId, 0, false));
                    }
                    else if (inside && how == BeastKits.Cut && string.Equals(x.ActorId, target, StringComparison.Ordinal))
                        effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Cut, move, x.ActorId, 0, false));
                    else if (inside && how == ChiefKits.Broken && BrokeToFocus(move, x.ActorId, target, foe))
                        effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Broke, move, x.ActorId, 0, false));
                    continue;
                }
                if (!inside) continue;
                if (x.Kind == BattleEventKind.Interrupt && x.ActorSide == side && onTarget)
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Broke, x.Detail, target, 0, false));
                else if (x.Kind == BattleEventKind.Broken && x.ActorSide == side && onTarget &&
                    (x.Detail == ClassSpells.AimDetail || x.Detail == ChiefKits.RifleDetail))
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Broke,
                        x.Detail == ChiefKits.RifleDetail ? ChiefKits.RifleName : OrderEffect.AimedShot, target, 0, false));
                else if (x.Kind == BattleEventKind.Kill && x.ActorSide == side && onTarget && x.Detail != ChiefKits.ExecuteDetail)
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.BroughtDown, string.Empty, target, 0, false));
                else if (x.Kind == BattleEventKind.Hit && x.ActorSide == side && onTarget && x.Detail != ChiefKits.ExecuteDetail)
                {
                    damage += x.Amount;
                    lastHit = x.Tick;
                }
                else if (x.Kind == BattleEventKind.Guard && x.ActorSide == foe && onTarget)
                {
                    covered += x.Amount;
                    lastCover = x.Tick;
                    warden = x.ActorId;
                }
            }
            if (damage > 0) effects.Add(new OrderEffect(lastHit, OrderEffectKind.Damage, string.Empty, target, damage, false));
            if (lastCover >= 0) effects.Add(new OrderEffect(lastCover, OrderEffectKind.Covered, string.Empty, warden, covered, false));
        }

        // A move of theirs that broke while your Focus Fire was on this target broke to it: the Drive breaks only to a Focus
        // Fire on his front row, and the Corner to one on his net thrower (his Trapper, or him); the Rifle Line's lost shots
        // count one by one (its Broken events); anything else breaks to a Focus Fire on whoever does it.
        private bool BrokeToFocus(string move, string actorId, string target, int foe)
        {
            if (move == ChiefKits.RifleName) return false;
            if (move == ChiefKits.Name(HuntStep.Drive)) return true;
            if (string.Equals(actorId, target, StringComparison.Ordinal)) return true;
            if (move != ChiefKits.Name(HuntStep.Corner)) return false;
            BattleUnit u = FindById(target);
            return u != null && u.Side == foe && u.Class == UnitClass.Trapper;
        }

        // Your Focus Fire's target is one of his own side but not him: his Sun Mantle switches away from it.
        private bool OnHisBand(string target, string chiefId)
        {
            BattleUnit u = FindById(target);
            BattleUnit chief = FindById(chiefId);
            return u != null && chief != null && u.Side == chief.Side && !ReferenceEquals(u, chief);
        }

        // An Unleash, on its own tick: the walls it smashed, the marches it staggered, the nets it tore, the combos it finished
        // and a scare their Hold blocked; then the damage your side dealt while it ran.
        private void UnleashEffects(int side, int at, long end, List<OrderEffect> effects)
        {
            int foe = 1 - side;
            long tick = _events[at].Tick;
            int damage = 0;
            long lastHit = -1;
            for (int j = at + 1; j < _events.Count && _events[j].Tick < end; j++)
            {
                BattleEvent x = _events[j];
                if (x.Kind == BattleEventKind.Hit && x.ActorSide == side && x.Detail != ChiefKits.ExecuteDetail)
                {
                    damage += x.Amount;
                    lastHit = x.Tick;
                    continue;
                }
                if (x.Tick != tick) continue;
                if (x.Kind == BattleEventKind.Wall && x.ActorSide == foe && x.Detail == ChiefSpells.WallSmashed)
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Smashed, WallName(foe, j), string.Empty, 0, false));
                else if (x.Kind == BattleEventKind.Land && x.ActorSide == foe)
                {
                    SplitLand(x.Detail, out string move, out string how);
                    if (how == ChiefKits.Staggered)
                        effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Staggered, move, x.ActorId, 0, false));
                    else if (how == ChiefKits.Smashed)
                        effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Smashed, move, x.ActorId, 0, false));
                    else if (how == ChiefKits.Torn)
                        effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Tore, move, x.TargetId, 0, false));
                }
                else if (x.Kind == BattleEventKind.Combo && x.ActorSide == side && (x.Detail == Moves.Ambush || x.Detail == FightRules.Riposte))
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.Combo, x.Detail, x.TargetId, 0, false));
                else if (x.Kind == BattleEventKind.Directive && x.ActorSide == foe && x.Detail == BlockedDetail)
                    effects.Add(new OrderEffect(x.Tick, OrderEffectKind.ScareBlocked, Directives.Name(Directive.Unleash), string.Empty, 0, false));
            }
            if (damage > 0) effects.Add(new OrderEffect(lastHit, OrderEffectKind.Damage, string.Empty, string.Empty, damage, false));
        }

        // The wall an Unleash smashed: the Skullpeak chief's Lockdown once she is behind it, else the Steel Storm's.
        private string WallName(int foe, int smash)
        {
            for (int j = smash - 1; j >= 0; j--)
            {
                BattleEvent w = _events[j];
                if (w.Kind != BattleEventKind.Wall || w.ActorSide != foe) continue;
                if (w.Detail == ChiefKits.WallLockdown || w.Detail == ChiefKits.WallRebuilt) return ChiefKits.PhaseName(ChiefKit.Skullpeak);
                if (w.Detail == ChiefSpells.WallUp || w.Detail == ChiefKits.WallHalf) break;
            }
            return ChiefSpells.Name(ChiefSpell.SteelStorm);
        }

        // ------------------------------------------------------------ why it did nothing

        private OrderMiss MissOf(int side, int at, long end, bool ranOut, Directive order, List<OrderEffect> effects, out string missMove)
        {
            missMove = string.Empty;
            if (order == Directive.FocusFire)
            {
                for (int k = 0; k < effects.Count; k++)
                    if (effects[k].Kind == OrderEffectKind.Covered)
                    {
                        missMove = effects[k].TargetId;
                        return OrderMiss.Covered;
                    }
                return OrderMiss.OutOfReach;
            }
            if (order == Directive.Unleash)
            {
                for (int k = 0; k < effects.Count; k++)
                    if (effects[k].Kind == OrderEffectKind.ScareBlocked) return OrderMiss.Blocked;
                return OrderMiss.OutOfReach;
            }

            // A Hold or a Fall Back. Falling back into a Riptide is the wrong order.
            int foe = 1 - side;
            for (int k = 0; k < effects.Count; k++)
                if (effects[k].Kind == OrderEffectKind.Pulled)
                {
                    missMove = effects[k].Move;
                    return OrderMiss.WrongOrder;
                }
            // Under the parry at the center a Hold says first what went against it: a move out of its reach, a fake, a wait out.
            if (order == Directive.Hold && _parryOn)
            {
                OrderEffectKind[] against = { OrderEffectKind.OutOfReach, OrderEffectKind.Feinted, OrderEffectKind.WaitedOut };
                OrderMiss[] misses = { OrderMiss.OutOfReach, OrderMiss.Feinted, OrderMiss.WaitedOut };
                for (int a = 0; a < against.Length; a++)
                    for (int k = 0; k < effects.Count; k++)
                        if (effects[k].Kind == against[a])
                        {
                            missMove = effects[k].Move;
                            return misses[a];
                        }
            }
            // Something of theirs landed in full while it ran.
            int j = at + 1;
            for (; j < _events.Count && _events[j].Tick < end; j++)
            {
                if (!LandedInFull(j, foe, out string move)) continue;
                missMove = move;
                if (Spans(move)) return Meets(order, move, foe) ? OrderMiss.TooLate : OrderMiss.WrongOrder;
                return order == Directive.Hold && Parryable(move, foe, _events[j].Tick) ? OrderMiss.TooEarly : OrderMiss.WrongOrder;
            }
            // Or it ran out within a second before a hit it meets.
            if (ranOut)
                for (; j < _events.Count && _events[j].Tick < end + SimClock.TicksPerSecond; j++)
                {
                    if (!LandedInFull(j, foe, out string move) || Spans(move) || !Meets(order, move, foe)) continue;
                    missMove = move;
                    return OrderMiss.TooEarly;
                }
            // Or something it meets came while it ran, and something else of yours met it first.
            if (MetFirst(side, at, end, order, out string other))
            {
                missMove = other;
                return OrderMiss.MetFirst;
            }
            return OrderMiss.NothingCame;
        }

        // A move of theirs this order meets came while it ran and was met another way: dodged by a Fall Back, covered by a
        // Warden, treated by a Medic, or broken, snared, cut, staggered, torn or smashed.
        private bool MetFirst(int side, int at, long end, Directive order, out string move)
        {
            move = string.Empty;
            int foe = 1 - side;
            long start = _events[at].Tick;
            for (int j = at + 1; j < _events.Count && _events[j].Tick < end + SpanReachTicks; j++)
            {
                BattleEvent x = _events[j];
                if (x.ActorSide != foe) continue;
                string name, how;
                if (x.Kind == BattleEventKind.Release && !Summed(j))
                {
                    name = WindupMove(j);
                    how = x.Detail;
                }
                else if (x.Kind == BattleEventKind.Land) SplitLand(x.Detail, out name, out how);
                else continue;
                bool metElse = order == Directive.Hold
                    ? how == ChiefKits.Dodged || how == ChiefKits.Guarded || how == ChiefKits.Healed || how == ChiefKits.Broken ||
                      how == ChiefKits.Staggered || how == ChiefKits.Torn || how == BeastKits.Snared || how == BeastKits.Cut
                    : how == ChiefKits.Parried || how == ChiefKits.Soaked || how == ChiefKits.Braced || how == ChiefKits.Guarded ||
                      how == ChiefKits.Healed || how == ChiefKits.Broken || how == ChiefKits.Switched || how == BeastKits.Snared;
                if (!metElse || !Meets(order, name, foe)) continue;
                if (Spans(name) ? !Overlaps(start, end, SpanStart(j, name), x.Tick) : x.Tick >= end) continue;
                move = name;
                return true;
            }
            return false;
        }

        // A move of theirs that landed in full at this event, by its name.
        private bool LandedInFull(int at, int foe, out string move)
        {
            move = string.Empty;
            BattleEvent x = _events[at];
            if (x.ActorSide != foe) return false;
            if (x.Kind == BattleEventKind.Release && x.Detail == Moves.Full && !Summed(at)) move = WindupMove(at);
            else if (x.Kind == BattleEventKind.Land)
            {
                SplitLand(x.Detail, out string name, out string how);
                if (how == ChiefKits.Full) move = name;
            }
            return move.Length > 0;
        }

        // Whether this order meets this move of theirs by itself: a Hold soaks, parries, braces or blocks it; Fall Back gets
        // clear of it.
        private bool Meets(Directive order, string move, int foe)
        {
            if (order == Directive.Hold)
                return move == EclipseMove || move == Moves.Pyre || move == HeavyRules.Heavy || move == ChiefKits.CleaveName ||
                    move == ChiefKits.HookName || move == ChiefKits.Name(HuntStep.Kill) || move == ChiefKits.Name(HuntStep.Drive) ||
                    move == ChiefKits.AdvanceName || move == ChiefKits.VeinsName || move == ChiefKits.RiptideName ||
                    move == ChiefKits.BoreName || move == ChiefKits.SurgeName || move == BeastKits.BiteName ||
                    move == BeastKits.RoarName || move == BeastKits.PulseName;
            if (order == Directive.FallBack)
                return move == EclipseMove || move == Moves.Pyre || move == ChiefSpells.Name(ChiefSpell.Firebomb) ||
                    move == ChiefSpells.Name(ChiefSpell.DustShark) || move == ChiefSpells.Name(ChiefSpell.Tide) ||
                    move == ChiefSpells.Name(ChiefSpell.BloodCall) || (move == HeavyRules.Heavy && !HasBand(foe, BandTrait.LongReach)) ||
                    move == ChiefKits.BrandName || move == ChiefKits.RingName || move == ChiefKits.MantleName ||
                    move == ChiefKits.RifleName || move == ChiefKits.AdvanceName || move == ChiefKits.LeechName ||
                    move == ChiefKits.BoreName || move == BeastKits.DiveName || move == BeastKits.VolleyName ||
                    move == BeastKits.SweepName || move == BeastKits.GrabName;
            return false;
        }

        // Whether a live Hold given within FightRules.PerfectHoldTicks before this move of theirs landed would have parried it.
        private bool Parryable(string move, int foe, long tick)
        {
            if (!_fight.PerfectHold) return false;
            if (move == EclipseMove || move == Moves.Pyre || move == BeastKits.BiteName || move == ChiefKits.HookName ||
                move == ChiefKits.Name(HuntStep.Kill))
                return true;
            return (move == HeavyRules.Heavy || move == ChiefKits.CleaveName) && !HasBand(foe, BandTrait.Unstoppable) && !BerserkAt(foe, tick);
        }

        // This side's band was berserk on this tick (BandTrait.Berserk), when nothing of theirs can be parried.
        private bool BerserkAt(int side, long tick)
        {
            if (!_bandOn || !HasBand(side, BandTrait.Berserk)) return false;
            for (int i = 0; i < _events.Count && _events[i].Tick <= tick; i++)
            {
                BattleEvent e = _events[i];
                if (e.Kind == BattleEventKind.Reaction && e.ActorSide == side && e.Detail == BandRules.BerserkDetail && tick < e.Tick + e.Amount)
                    return true;
            }
            return false;
        }

        // ------------------------------------------------------------ moves that run a while

        // The moves that run a while and land when they end, saying how they were met over all of it.
        private static bool Spans(string move)
        {
            return move == ChiefKits.MantleName || move == ChiefKits.VeinsName || move == ChiefKits.RiptideName ||
                move == ChiefKits.BoreName || move == ChiefKits.SurgeName;
        }

        // When such a move began, for its Land at this event: his Tell for it, or for the Storm Surge the water rising as its
        // horn ends.
        private long SpanStart(int land, string move)
        {
            BattleEvent l = _events[land];
            if (move == ChiefKits.SurgeName) return l.Tick - (ChiefKits.SurgeTicks - 1);
            for (int j = land - 1; j >= 0 && l.Tick - _events[j].Tick <= SpanReachTicks; j--)
            {
                BattleEvent t = _events[j];
                if (t.Kind == BattleEventKind.Tell && t.ActorSide == l.ActorSide && t.Detail == move &&
                    string.Equals(t.ActorId, l.ActorId, StringComparison.Ordinal))
                    return t.Tick;
            }
            return l.Tick;
        }

        private static bool Overlaps(long start, long end, long from, long to)
        {
            return start <= to && end > from;
        }

        // A move of theirs that runs a while is running now, began before this order stopped, and is one this order meets: what
        // the order did about it comes when it lands.
        private bool AwaitsSpan(int side, Directive order, long end)
        {
            if (_finished || order == Directive.Unleash) return false;
            int foe = 1 - side;
            SideState his = _sides[foe];
            if (order != Directive.Hold && his.MantleTicks > 0 && SpanBegan(foe, ChiefKits.MantleName) < end) return true;
            if (order != Directive.FocusFire && his.BoreTicks > 0 && SpanBegan(foe, ChiefKits.BoreName) < end) return true;
            if (order != Directive.Hold) return false;
            if (his.OverclockTicks > 0 && _tick - (ChiefKits.OverclockTicks - his.OverclockTicks) < end) return true;
            if (his.VeinsTicks > 0 && SpanBegan(foe, ChiefKits.VeinsName) < end) return true;
            if (his.RipTicks > 0 && SpanBegan(foe, ChiefKits.RiptideName) < end) return true;
            return his.SurgeTicks > 0 && _tick - (ChiefKits.SurgeTicks - his.SurgeTicks) < end;
        }

        // When the move of theirs running now began: his latest Tell for it.
        private long SpanBegan(int foe, string move)
        {
            for (int j = _events.Count - 1; j >= 0 && _tick - _events[j].Tick <= SpanReachTicks; j--)
            {
                BattleEvent t = _events[j];
                if (t.Kind == BattleEventKind.Tell && t.ActorSide == foe && t.Detail == move) return t.Tick;
            }
            return long.MaxValue;
        }

        // ------------------------------------------------------------ reading the history

        // A Land's detail is the move, a dot and how it went.
        private static void SplitLand(string detail, out string move, out string how)
        {
            int dot = detail.LastIndexOf('.');
            move = dot < 0 ? detail : detail.Substring(0, dot);
            how = dot < 0 ? string.Empty : detail.Substring(dot + 1);
        }

        // A Shieldbreaker's heavy swing broke this side's Hold (BandTrait.Shieldbreaker).
        private static bool Shatters(BattleEvent e, int side)
        {
            return e.Kind == BattleEventKind.Reaction && e.ActorSide == 1 - side && e.Detail == BandRules.ShieldbreakDetail;
        }

        // The move a Release finished: the Windup the same actor of that side started last before it.
        private string WindupMove(int release)
        {
            BattleEvent r = _events[release];
            for (int j = release - 1; j >= 0; j--)
            {
                BattleEvent w = _events[j];
                if (w.Kind == BattleEventKind.Windup && w.ActorSide == r.ActorSide && string.Equals(w.ActorId, r.ActorId, StringComparison.Ordinal))
                    return w.Detail;
            }
            return string.Empty;
        }

        // A Release that the Land right after it sums up (the Stoneveil chief's Blood Call): the Land is read instead.
        private bool Summed(int release)
        {
            BattleEvent r = _events[release];
            string prefix = WindupMove(release) + ".";
            for (int j = release + 1; j < _events.Count && _events[j].Tick == r.Tick; j++)
            {
                BattleEvent l = _events[j];
                if (l.Kind == BattleEventKind.Land && string.Equals(l.ActorId, r.ActorId, StringComparison.Ordinal) &&
                    l.Detail.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }

        // A parry with no move named on it (the Eclipse's, a heavy swing's): the move of the parried Release just before it.
        private string ParriedMove(int parry)
        {
            BattleEvent p = _events[parry];
            for (int j = parry - 1; j >= 0 && _events[j].Tick == p.Tick; j--)
            {
                BattleEvent r = _events[j];
                if (r.Kind == BattleEventKind.Release && r.Detail == FightRules.Parried && string.Equals(r.ActorId, p.TargetId, StringComparison.Ordinal))
                    return WindupMove(j);
            }
            return string.Empty;
        }
    }
}
