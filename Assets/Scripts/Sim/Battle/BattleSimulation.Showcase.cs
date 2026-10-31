using System;
using System.Collections.Generic;

namespace KindredSiege.Sim.Battle
{
    /// A rival's lesson acting in a fight (Jacob, 24 Oct 2026, DESIGN_LOCKS "The parry at the center", part 3: "the moment each
    /// lesson fires"; the mark on whoever carries it flashes then). Rule names the lesson: a tribe's habit by its LessonRules id,
    /// or a hunter's trait. Only fights under the parry at the center keep them.
    public readonly struct LessonFiring
    {
        /// The tribes': waits out your parry, feints, covers whoever you focus, holds when you Unleash, unleashes when you Fall Back.
        public const string WaitOut = "parry";
        public const string Feint = "feint";
        public const string Cover = "focus";
        public const string HoldOnUnleash = "unleash";
        public const string PressOnFallBack = "fallback";

        /// The hunters': Delay waited out your parry, a Double swing's light first swing went, Long Reach reached through your Fall
        /// Back, Thick Hide braced against your Focus Fire, Unshakable shrugged off your Unleash. A hunter's Feint is Feint.
        public const string Delay = "delay";
        public const string Double = "double";
        public const string LongReach = "longreach";
        public const string ThickHide = "thickhide";
        public const string Unshakable = "unshakable";

        public LessonFiring(long tick, int side, string rule, string actorId, string targetId)
        {
            Tick = tick;
            Side = side;
            Rule = rule ?? string.Empty;
            ActorId = actorId ?? string.Empty;
            TargetId = targetId ?? string.Empty;
        }

        public long Tick { get; }

        /// The side whose lesson it is: the rival's.
        public int Side { get; }

        public string Rule { get; }

        /// Who carries it and acted: the one who waited or feinted, the Warden who covered, the braced hunter. Empty for a side's
        /// own order (a Hold, an Unleash).
        public string ActorId { get; }

        /// Whom it was about, when one: the one you focused, the one a double swing falls on.
        public string TargetId { get; }
    }

    /// A mark on an enemy and who cashes it (DESIGN_LOCKS "The parry at the center", part 4: "every mark on every enemy with who
    /// cashes it"): a snare the Marksman's Aimed Shot crits (Focus Fire on him sends it there), an Opening the Skirmisher's strike
    /// hits twice as hard, a bleeding wound the Skirmisher's strike rips open.
    public readonly struct MarkRead
    {
        public MarkRead(string unitId, string mark, int ticksLeft, UnitClass casher, IReadOnlyList<string> casherIds)
        {
            UnitId = unitId ?? string.Empty;
            Mark = mark ?? string.Empty;
            TicksLeft = Math.Max(0, ticksLeft);
            Casher = casher;
            CasherIds = casherIds ?? new string[0];
        }

        /// The enemy who carries it.
        public string UnitId { get; }

        /// ClassSpells.SnareMark, OpeningMark or BleedMark.
        public string Mark { get; }

        public int TicksLeft { get; }

        /// The class that cashes it: its colour on the screen.
        public UnitClass Casher { get; }

        /// Your standing fighters of that class, who glow with it.
        public IReadOnlyList<string> CasherIds { get; }
    }

    // The showcase's reads (Jacob, 24 Oct 2026, DESIGN_LOCKS "The parry at the center", parts 3 and 4): when each rival lesson
    // fires, the lesson behind an order's miss, when his cast is due, every combo window open, and every mark with who cashes
    // it. Read only, like the threat read: asking changes nothing. The lesson firings are kept as they happen and are not
    // state, so a fight replayed from its orders keeps the same ones.
    public sealed partial class BattleSimulation
    {
        private readonly List<LessonFiring> _firings = new List<LessonFiring>();

        /// Every moment one of this side's lessons fired this fight, in order. Empty outside the parry at the center.
        public IReadOnlyList<LessonFiring> LessonFirings(int side)
        {
            var list = new List<LessonFiring>();
            for (int i = 0; i < _firings.Count; i++) if (_firings[i].Side == side) list.Add(_firings[i]);
            return list;
        }

        /// The rival lesson behind one of this side's orders that missed (OrderResults), or empty: a Hold whose beat waited it out,
        /// came late (Delay) or feinted it; a Focus Fire their learned cover or a braced Thick Hide met; an Unleash their learned
        /// Hold met or Unshakable shrugged off; a Fall Back their learned Unleash met, or Long Reach reached through.
        public string LessonBehind(int side, OrderResult result)
        {
            if (side < 0 || side > 1 || result.IsNone || result.Miss == OrderMiss.None) return string.Empty;
            int rival = 1 - side;
            long end = result.EndTick < 0 ? _tick : result.EndTick;
            for (int i = 0; i < _firings.Count; i++)
            {
                LessonFiring f = _firings[i];
                if (f.Side != rival || f.Tick < result.Tick || f.Tick > end) continue;
                bool atOrder = f.Tick == result.Tick;
                switch (result.Order)
                {
                    case Directive.Hold:
                        if (f.Rule == LessonFiring.WaitOut || f.Rule == LessonFiring.Delay || f.Rule == LessonFiring.Feint) return f.Rule;
                        break;
                    case Directive.FocusFire:
                        if (atOrder && (f.Rule == LessonFiring.Cover || f.Rule == LessonFiring.ThickHide)) return f.Rule;
                        break;
                    case Directive.Unleash:
                        if (atOrder && (f.Rule == LessonFiring.HoldOnUnleash || f.Rule == LessonFiring.Unshakable)) return f.Rule;
                        break;
                    case Directive.FallBack:
                        if ((atOrder && f.Rule == LessonFiring.PressOnFallBack) || f.Rule == LessonFiring.LongReach) return f.Rule;
                        break;
                }
            }
            return string.Empty;
        }

        /// Ticks until a side's chief casts on his own clock (his cast needs Directives.Cost(Directive.Eclipse) points, and
        /// DirectivePoints gives his points): 0 when it is due and he waits for the points or a free moment, -1 when he never
        /// casts on a clock.
        public int NextCastTicks(int side)
        {
            if (side < 0 || side > 1 || _rules[side].AutoEclipseTicks <= 0 || _rules[side].EclipseGrade <= 0) return -1;
            return (int)Math.Max(0L, _sides[side].NextAutoEclipse - _tick);
        }

        /// Every combo this side can finish now, with what finishes it and how long it has: the Riposte after a parry, the Ambush
        /// after a Fall Back, the Pyre after a Focus Fire (then an Unleash). NextCombo gives the first of them.
        public IReadOnlyList<ComboHint> OpenCombos(int side)
        {
            var list = new List<ComboHint>();
            if (_finished || side < 0 || side > 1 || !_rules[side].Combos) return list;
            SideState s = _sides[side];
            if (_fight.PerfectHold && s.LastParryTick >= 0)
            {
                long left = FightRules.RiposteWindowTicks - (_tick - s.LastParryTick);
                BattleUnit caster = FindById(s.ParriedCasterId);
                if (left >= 0 && caster != null && caster.Alive) list.Add(new ComboHint(FightRules.Riposte, Directive.Unleash, (int)left, 1, 2));
            }
            if (!s.AmbushUsed && s.LastFallBackTick >= 0)
            {
                long left = Moves.AmbushWindowTicks - (_tick - s.LastFallBackTick);
                if (left >= 0) list.Add(new ComboHint(Moves.Ambush, Directive.Unleash, (int)left, 1, 2));
            }
            if (!s.PyreUsed && s.LastFocusTick >= 0 && _rules[side].EclipseGrade > 0 && Leader(side) != null)
            {
                long left = Moves.PyreWindowTicks - (_tick - s.LastFocusTick);
                BattleUnit focused = FindById(s.LastFocusId);
                if (left >= 0 && focused != null && focused.Alive)
                {
                    bool unleashed = s.LastUnleashTick >= s.LastFocusTick;
                    list.Add(new ComboHint(Moves.Pyre, unleashed ? Directive.Eclipse : Directive.Unleash, (int)left, unleashed ? 2 : 1, 3));
                }
            }
            return list;
        }

        /// Every mark on the other side's standing fighters and who of this side cashes it (MarkRead), in unit order. Empty without
        /// class spells, whose marks they are.
        public IReadOnlyList<MarkRead> EnemyMarks(int side)
        {
            var list = new List<MarkRead>();
            if (!_classOn || side < 0 || side > 1) return list;
            IReadOnlyList<string> marksmen = IdsOf(side, UnitClass.Marksman);
            IReadOnlyList<string> skirmishers = IdsOf(side, UnitClass.Skirmisher);
            for (int i = 0; i < _units.Count; i++)
            {
                BattleUnit u = _units[i];
                if (!u.Alive || u.Side == side) continue;
                if (u.SnareTicks > 0) list.Add(new MarkRead(u.Id, ClassSpells.SnareMark, u.SnareTicks, UnitClass.Marksman, marksmen));
                if (u.OpeningTicks > 0) list.Add(new MarkRead(u.Id, ClassSpells.OpeningMark, u.OpeningTicks, UnitClass.Skirmisher, skirmishers));
                if (u.BleedTicks > 0) list.Add(new MarkRead(u.Id, ClassSpells.BleedMark, u.BleedTicks, UnitClass.Skirmisher, skirmishers));
            }
            return list;
        }

        private IReadOnlyList<string> IdsOf(int side, UnitClass unitClass)
        {
            var ids = new List<string>();
            for (int i = 0; i < _units.Count; i++)
                if (_units[i].Alive && _units[i].Side == side && _units[i].Class == unitClass) ids.Add(_units[i].Id);
            return ids;
        }

        // A lesson of this side fired: kept for the screen under the parry at the center.
        private void Fired(int side, string rule, BattleUnit actor, BattleUnit target)
        {
            if (!_parryOn) return;
            _firings.Add(new LessonFiring(_tick, side, rule, actor == null ? string.Empty : actor.Id, target == null ? string.Empty : target.Id));
        }
    }
}
