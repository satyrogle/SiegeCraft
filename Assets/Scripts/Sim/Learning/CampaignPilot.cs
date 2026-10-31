using System.Collections.Generic;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    public enum PilotStyle
    {
        /// Reads the tribe. Dodges their Eclipse and punishes the worn out caster; in a trial for a
        /// tribe that respects three answers it keeps two points back and breaks the first cast,
        /// then dodges and punishes. Keeps the pressure on with Unleash where the tribe allows it
        /// (SliceAutopilot, measured 6 Oct 2026). Where Unleash is taboo, focus is its weapon:
        /// their Warden first if he covers what you focus, then their Medic, then the most hurt.
        /// Holds the line with Hold where the tribe allows it, calls the assist once they are
        /// shaken, and picks encounters for feats it has not shown this run.
        Reference = 0,
        Random = 1,
        UnleashSpam = 2,
        FocusSpam = 3,
        HoldSpam = 4,

        /// Only dodges their Eclipse.
        Turtle = 5,

        /// Written to break things: rushes whoever leads them (his fall breaks the side), dodges
        /// every Eclipse, calls the assist at once, and farms the encounter it wins most easily.
        Cheater = 6,

        /// Falls back whenever it can, to keep the lines apart.
        FallBackSpam = 7,

        /// Gives no orders at all.
        Passive = 8
    }

    /// Scripted players for the campaign. Tests and balance numbers play with them; they read the
    /// fight and give orders through CampaignFight, the way a player would. What they cannot see:
    /// whether any of it is readable or fun, and they never tire or misread a windup.
    public sealed class CampaignPilot
    {
        public const int DodgeAtWindup = 60;
        private readonly PilotStyle _style;
        private readonly DeterministicRng _rng;
        // Its own dice for a duel's parries, so a pilot set to parry draws nothing else differently.
        private readonly DeterministicRng _parryRng;
        private bool _answered;
        private bool _breaking;
        private bool _broke;
        private int _scanned;
        private int _lastWindup;
        private long _focusUntil = -1;
        private long _unleashUntil = -1;
        private long _lastHold = -1;
        private bool _read;
        private bool _theyCover;

        public CampaignPilot(PilotStyle style, ulong seed)
        {
            _style = style;
            _rng = new DeterministicRng(seed, 77);
            _parryRng = new DeterministicRng(seed, 78);
        }

        public PilotStyle Style => _style;

        /// In a champion duel (campaign rules 16, DESIGN_LOCKS "Duels"), how often in a thousand the pilot parries the
        /// champion's beat, a Hold on the tick before it; 0, the default, never, so every pilot plays as before. Whatever its
        /// style, how well a player parries, for measuring duels.
        public int DuelParryPermille { get; set; }

        /// Which offer to take.
        public int PickOffer(Campaign c)
        {
            if (c.Offers.Count <= 1) return 0;
            switch (_style)
            {
                case PilotStyle.Random:
                    return _rng.Next(c.Offers.Count);
                case PilotStyle.Cheater:
                {
                    for (int i = 0; i < c.Offers.Count; i++) if (c.Offers[i].Kind == EncounterKind.WarParty) return i;
                    for (int i = 0; i < c.Offers.Count; i++) if (c.Offers[i].Kind == EncounterKind.Duel) return i;
                    return 0;
                }
                case PilotStyle.Reference:
                {
                    int best = 0;
                    int bestScore = -1;
                    for (int i = 0; i < c.Offers.Count; i++)
                    {
                        Encounter e = c.Offers[i];
                        int score = 0;
                        for (int f = 0; f < e.CanShow.Count; f++)
                            score += Contains(c.FeatsThisRun, e.CanShow[f]) ? RespectRules.RepeatFeatPoints : RespectRules.FirstFeatPoints;
                        score = score * 10 + (e.Kind == EncounterKind.WarParty ? 2 : e.Kind == EncounterKind.Trial ? 1 : 0);
                        if (score > bestScore)
                        {
                            best = i;
                            bestScore = score;
                        }
                    }
                    return best;
                }
                default:
                    return 0;
            }
        }

        /// Call once before every tick.
        public void Step(CampaignFight fight)
        {
            BattleSimulation sim = fight.Sim;
            if (fight.Over || sim.CurrentTick < 20) return;

            // A chief kneels for your verdict (Battle.ChiefKits, 21 Oct 2026): the bots spare him, except the cheater, who
            // executes him, and the random bot, which flips a coin. The passive bot gives no order, so the clock spares him.
            BattleUnit kneeling = sim.KneelingChief;
            if (kneeling != null)
            {
                if (_style == PilotStyle.Passive) return;
                if (_style == PilotStyle.Cheater || (_style == PilotStyle.Random && _rng.Next(2) == 0))
                    fight.TryOrder(Directive.FocusFire, kneeling.Id, out _);
                else fight.TryOrder(Directive.Hold, null, out _);
                return;
            }

            long t = sim.CurrentTick;
            int w = sim.WindupTicks(1);
            if (w > _lastWindup)
            {
                _answered = false;
                _breaking = false;
            }
            _lastWindup = w;

            // A duel: Hold on the tick before the champion's beat, DuelParryPermille of the time, one roll a beat.
            if (DuelParryPermille > 0 && fight.Encounter.Kind == EncounterKind.Duel &&
                (_lastHold < 0 || t - _lastHold >= RaidRules.MashTicks))
            {
                foreach (Threat threat in sim.Threats(0))
                {
                    if (!threat.ParryWindow || threat.BeatTicks != 1) continue;
                    if (_parryRng.Next(1000) < DuelParryPermille && fight.TryOrder(Directive.Hold, null, out _))
                    {
                        _lastHold = t;
                        return;
                    }
                    break;
                }
            }

            switch (_style)
            {
                case PilotStyle.Passive:
                    return;
                case PilotStyle.Random:
                    if (_rng.Next(25) != 0) return;
                    switch (_rng.Next(5))
                    {
                        case 0: fight.TryOrder(Directive.Hold, null, out _); break;
                        case 1: fight.TryOrder(Directive.FallBack, null, out _); break;
                        case 2:
                            BattleUnit target = RandomEnemy(sim);
                            if (target != null) fight.TryOrder(Directive.FocusFire, target.Id, out _);
                            break;
                        case 3: fight.TryOrder(Directive.Unleash, null, out _); break;
                        default: fight.TryOrder(Directive.Eclipse, null, out _); break;
                    }
                    return;
                case PilotStyle.UnleashSpam:
                    if (sim.UnleashTicksLeft(0) == 0) fight.TryOrder(Directive.Unleash, null, out _);
                    return;
                case PilotStyle.HoldSpam:
                    if (sim.HoldTicksLeft(0) == 0) fight.TryOrder(Directive.Hold, null, out _);
                    return;
                case PilotStyle.FallBackSpam:
                    fight.TryOrder(Directive.FallBack, null, out _);
                    return;
                case PilotStyle.FocusSpam:
                    if (t >= _focusUntil)
                    {
                        BattleUnit target = FirstAlive(sim, 1, UnitClass.Medic) ?? MostHurt(sim, 1);
                        if (target != null && fight.TryOrder(Directive.FocusFire, target.Id, out _)) _focusUntil = t + BattleSimulation.FocusTicks;
                    }
                    return;
                case PilotStyle.Turtle:
                    if (w > 0 && w <= DodgeAtWindup && !_answered && fight.TryOrder(Directive.FallBack, null, out _)) _answered = true;
                    return;
                case PilotStyle.Cheater:
                {
                    if (w > 0 && !_answered)
                    {
                        if (w <= DodgeAtWindup && fight.TryOrder(Directive.FallBack, null, out _)) _answered = true;
                        return;
                    }
                    if (fight.CanCallAssist) fight.TryOrder(Directive.Eclipse, null, out _);
                    BattleUnit head = sim.Leader(1);
                    if (head != null && t >= _focusUntil && fight.TryOrder(Directive.FocusFire, head.Id, out _))
                        _focusUntil = t + BattleSimulation.FocusTicks;
                    if (t >= _unleashUntil && sim.DirectivePoints(0) >= 3 && fight.TryOrder(Directive.Unleash, null, out _))
                        _unleashUntil = t + BattleSimulation.UnleashTicks;
                    return;
                }
                default:
                    Reference(fight, sim, fight.Encounter.Tribe, t, w);
                    return;
            }
        }

        private void Reference(CampaignFight fight, BattleSimulation sim, CampaignTribe tribe, long t, int w)
        {
            if (!_read)
            {
                _theyCover = sim.Setup.CopyTactics(1).CoverFocused >= BattleSimulation.HabitThreshold;
                _read = true;
            }
            IReadOnlyList<BattleEvent> ev = sim.Events;
            for (; _scanned < ev.Count; _scanned++)
                if (ev[_scanned].Kind == BattleEventKind.Interrupt && ev[_scanned].ActorSide == 0) _broke = true;

            bool noHold = tribe.Hates == Feat.Hid;
            bool noUnleash = tribe.Hates == Feat.Loud;
            bool holdLine = fight.Encounter.NoFallBack;
            int focusCost = Directives.Cost(Directive.FocusFire);
            int dodgeCost = Directives.Cost(Directive.FallBack);

            // Breaking a cast costs a point held back all fight, which lost chief fights (9 Oct
            // 2026): only worth it in the trial, for a tribe that respects three answers.
            bool wantBreak = fight.Encounter.Kind == EncounterKind.Trial && Contains(tribe.Respects, Feat.AnsweredThreeWays) &&
                !_broke && sim.Leader(1) != null;
            int spare = fight.Encounter.EnemyCasts && !holdLine ? dodgeCost + (wantBreak ? focusCost : 0) : 0;
            int points = sim.DirectivePoints(0);

            // Against a raid chief your Hold is free (rules 10): parry every beat coming at you, one clean press each, never
            // sooner than RaidRules.MashTicks after the last.
            if (sim.RaidChiefOn && sim.Kit(1) == ChiefKit.Bloodridge && fight.Encounter.Kind == EncounterKind.Chief && !noHold &&
                (_lastHold < 0 || t - _lastHold >= RaidRules.MashTicks))
            {
                foreach (Threat threat in sim.Threats(0))
                {
                    if (!threat.ParryWindow || threat.BeatTicks != 1) continue;
                    if (fight.TryOrder(Directive.Hold, null, out _))
                    {
                        _lastHold = t;
                        return;
                    }
                    break;
                }
            }

            if (w > 0)
            {
                BattleUnit caster = sim.Leader(1);
                if (wantBreak && !_breaking && caster != null && w > DodgeAtWindup && points >= focusCost + dodgeCost &&
                    fight.TryOrder(Directive.FocusFire, caster.Id, out _))
                {
                    _breaking = true;
                    _focusUntil = t + BattleSimulation.FocusTicks;
                }
                if (!_answered && w <= DodgeAtWindup)
                {
                    if (!holdLine)
                    {
                        if (fight.TryOrder(Directive.FallBack, null, out _)) _answered = true;
                    }
                    else if (!noHold && fight.TryOrder(Directive.Hold, null, out _))
                    {
                        _answered = true;
                    }
                }
                return;
            }

            // Punish: their caster is worn out by his own Eclipse.
            if (sim.WornOutTicksLeft(1) > 0 && t >= _focusUntil)
            {
                BattleUnit chief = sim.Leader(1);
                if (chief != null && fight.TryOrder(Directive.FocusFire, chief.Id, out _))
                {
                    _focusUntil = t + BattleSimulation.FocusTicks;
                    return;
                }
            }
            if (fight.CanCallAssist && points >= Directives.Cost(Directive.Eclipse) + spare &&
                (sim.Nerve(1) <= 750 || sim.WornOutTicksLeft(1) > 0))
            {
                fight.TryOrder(Directive.Eclipse, null, out _);
                return;
            }

            // Hold the line with Hold, where the tribe does not despise it.
            if (holdLine && !noHold)
            {
                if (sim.HoldTicksLeft(0) == 0 && points >= Directives.Cost(Directive.Hold)) fight.TryOrder(Directive.Hold, null, out _);
                return;
            }

            // Their Unleash scares us unless our shields are up.
            if (!noHold && sim.UnleashTicksLeft(1) > 0 && sim.HoldTicksLeft(0) == 0 &&
                points >= Directives.Cost(Directive.Hold) + spare && fight.TryOrder(Directive.Hold, null, out _))
                points -= Directives.Cost(Directive.Hold);

            if (!noUnleash)
            {
                // Pressure, even into their shields.
                if (t >= _unleashUntil && points >= Directives.Cost(Directive.Unleash) + spare &&
                    fight.TryOrder(Directive.Unleash, null, out _))
                {
                    _unleashUntil = t + BattleSimulation.UnleashTicks;
                    points -= Directives.Cost(Directive.Unleash);
                }
                if (t < _focusUntil || !_theyCover) return;
                BattleUnit warden = FirstAlive(sim, 1, UnitClass.Warden);
                if (warden != null && points >= focusCost + spare + Directives.Cost(Directive.Unleash) &&
                    fight.TryOrder(Directive.FocusFire, warden.Id, out _))
                    _focusUntil = t + BattleSimulation.FocusTicks;
                return;
            }

            // No Unleash: focus is the weapon (focusing their Medic first lost Skullpeak war
            // parties that their Warden first won, 9 Oct 2026).
            if (t < _focusUntil || points < focusCost + spare) return;
            BattleUnit target = _theyCover ? FirstAlive(sim, 1, UnitClass.Warden) : null;
            if (target == null) target = FirstAlive(sim, 1, UnitClass.Medic);
            if (target == null) target = MostHurt(sim, 1);
            if (target != null && fight.TryOrder(Directive.FocusFire, target.Id, out _)) _focusUntil = t + BattleSimulation.FocusTicks;
        }

        private static bool Contains(IReadOnlyList<Feat> feats, Feat feat)
        {
            for (int i = 0; i < feats.Count; i++) if (feats[i] == feat) return true;
            return false;
        }

        private BattleUnit RandomEnemy(BattleSimulation sim)
        {
            var alive = new List<BattleUnit>();
            for (int i = 0; i < sim.Units.Count; i++) if (sim.Units[i].Side == 1 && sim.Units[i].Alive) alive.Add(sim.Units[i]);
            return alive.Count == 0 ? null : alive[_rng.Next(alive.Count)];
        }

        private static BattleUnit FirstAlive(BattleSimulation sim, int side, UnitClass unitClass)
        {
            for (int i = 0; i < sim.Units.Count; i++)
            {
                BattleUnit u = sim.Units[i];
                if (u.Side == side && u.Alive && u.Class == unitClass && !u.IsLeader) return u;
            }
            return null;
        }

        private static BattleUnit MostHurt(BattleSimulation sim, int side)
        {
            BattleUnit best = null;
            for (int i = 0; i < sim.Units.Count; i++)
            {
                BattleUnit u = sim.Units[i];
                if (u.Side == side && u.Alive && (best == null || u.HpPermille < best.HpPermille)) best = u;
            }
            return best;
        }

        // ------------------------------------------------------------ headless play

        public static EncounterResult PlayFight(Campaign c, CampaignFight fight, CampaignPilot pilot)
        {
            while (!fight.Over)
            {
                pilot.Step(fight);
                fight.Tick();
            }
            return c.Finish(fight);
        }

        /// Plays the tutorial until it is won, at most maxTries times.
        public static bool PlayTutorial(Campaign c, PilotStyle style, ulong seed, int maxTries = 5)
        {
            for (int i = 0; i < maxTries && c.Stage == CampaignStage.Tutorial; i++)
                PlayFight(c, c.StartTutorial(), new CampaignPilot(style, seed + (ulong)i));
            return c.TutorialDone;
        }

        /// Plays one whole run against a tribe. Returns every encounter's result in order.
        public static List<EncounterResult> PlayRun(Campaign c, string tribeId, PilotStyle style, ulong seed)
        {
            var results = new List<EncounterResult>();
            c.ChooseTribe(tribeId);
            var picker = new CampaignPilot(style, seed);
            for (int n = 0; c.Stage == CampaignStage.InRun; n++)
            {
                CampaignFight fight = c.StartEncounter(picker.PickOffer(c));
                results.Add(PlayFight(c, fight, new CampaignPilot(style, seed * 31UL + (ulong)n)));
            }
            return results;
        }
    }
}
