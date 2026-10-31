using System;

namespace KindredSiege.Sim.Battle
{
    /// Danger (Jacob, 31 Oct 2026: "Go, lock and build"; DESIGN_LOCKS, "Jacob's playtest fixes 3, 7 and 9"; FightRules.Danger,
    /// campaign rules 21). His rules 19 run had no challenge: a Hold soaked everything for free, so Fall Back had no use, road
    /// bands carried nothing until the fourth row, and the chief barely hurt. So big moves are swings or blasts: a Hold soaks a
    /// swing (Cleave, the heavies) as before, but a Hold that is not a parry soaks nothing from a blast (the Eclipse, the
    /// Brand, the Burning Ring); only a parry or a Fall Back saves you. Bloodridge's road bands carry their chief's moves from
    /// the first fight, each a third of the health of whoever it hits. The chief's Cleave waits for one of his blasts between
    /// two Cleaves, and his Brand and Ring hit harder. Every number is a starting value.
    public static class DangerRules
    {
        /// A carried Cleave, Brand or Burning Ring you don't answer: this percent of the health of each fighter it hits.
        public const int CarriedPercent = 33;

        /// The chief's Brand and Burning Ring, unanswered: this percent of each fighter's health (ChiefKits: 20 and 25).
        public const int ChiefBrandPercent = 35;
        public const int ChiefRingPercent = 35;
    }

    public sealed partial class BattleSimulation
    {
        private readonly bool _dangerOn;

        /// Whether this fight plays danger: campaign rules 21.
        public bool DangerOn => _dangerOn;

        // How a blast was met: from rules 21 a Hold that is not a parry is no answer, so the blast lands in full.
        private BeatMet AsBlast(BeatMet met) => _dangerOn && met == BeatMet.Block ? BeatMet.Open : met;

        // The chief's Brand and Burning Ring, unanswered, in percent of each one's health.
        private int ChiefBrandPercent => _dangerOn ? DangerRules.ChiefBrandPercent : ChiefKits.BrandPercent;
        private int ChiefRingPercent => _dangerOn ? DangerRules.ChiefRingPercent : ChiefKits.RingPercent;

        // A carried Cleave: twice the carrier's blow, or from rules 21 a third of its target's health.
        private int CarriedCleave(BattleUnit carrier, BattleUnit target) => _dangerOn
            ? Math.Max(1, target.MaxHp * DangerRules.CarriedPercent / 100)
            : Math.Max(1, carrier.Power * CarryRules.CleavePercent / 100);

        // What a telling carried Cleave shows on the screen: its hit on whoever it is going for.
        private int CarriedCleaveShown(BattleUnit carrier)
        {
            BattleUnit target = FindById(carrier.CarryTargetId);
            return target != null ? CarriedCleave(carrier, target) : Math.Max(1, carrier.Power * CarryRules.CleavePercent / 100);
        }

        // A carried Brand and Burning Ring, in percent of each one's health.
        private int CarriedBrandPercent => _dangerOn ? DangerRules.CarriedPercent : CarryRules.BrandPercent;
        private int CarriedRingPercent => _dangerOn ? DangerRules.CarriedPercent : CarryRules.RingPercent;

        // From rules 21 the chief's swings and blasts take turns: after a Cleave his next Cleave waits until one of his blasts
        // has gone off. A chief with no blast never waits.
        private bool CleaveWaitsForBlast(int side) => _dangerOn && _sides[side].SwungLast &&
            (HasMove(side, ChiefMove.Brand) || HasMove(side, ChiefMove.BurningRing) || HasMove(side, ChiefMove.Eclipse));

        // One of his blasts went off, parried or not: his Cleave may come again.
        private void BlastWent(int side)
        {
            if (_dangerOn) _sides[side].SwungLast = false;
        }
    }
}
