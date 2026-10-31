using System;

namespace KindredSiege.Sim.Battle
{
    /// Burn, the road kit's first element (Jacob, 25 Oct 2026: recruits bring their own element, "depends what adds more variety";
    /// FightRules.RoadKit, campaign rules version 12). A fighter whose spec burns (UnitSpec.BurnPercent, Bloodridge's fighters
    /// and your Bloodridge recruits) sets whoever his blows land on burning: one stack a blow, up to MaxStacks, each costing
    /// StackPercent of the victim's health a second, for Ticks after the last. An Aimed Shot on a burning enemy flares it: all
    /// the burn left lands at once, and it pays as a cashed mark. Nothing cleanses it; it runs out. Every number is a starting
    /// value.
    public static class BurnRules
    {
        public const int MaxPercent = 5;
        public const int MaxStacks = 3;
        public const int StackPercent = 1;
        public const int Ticks = 5 * SimClock.TicksPerSecond;

        /// The Mark event's detail when a stack is lit, and the Hit details of a tick of burn and of a flare.
        public const string Mark = "burn";
        public const string Detail = "burn";
        public const string FlareDetail = "flare";
    }

    public sealed partial class BattleSimulation
    {
        // A burning fighter's blow landed: another stack on whoever it hit.
        private void Burned(BattleUnit attacker, BattleUnit victim)
        {
            if (!_roadKitOn || _finished || attacker == null || victim == null || !victim.Alive || attacker.Side == victim.Side) return;
            if (SpecOf(attacker).BurnPercent <= 0) return;
            victim.Statuses.Apply(StatusKind.Burn, BurnRules.Ticks, sourceId: attacker.Id);
            MarkEvent(attacker.Side, attacker, victim, BurnRules.Ticks, BurnRules.Mark);
        }

        // Burn runs on, a hit every second.
        private void TickBurn()
        {
            for (int i = 0; i < _units.Count && !_finished; i++)
            {
                BattleUnit u = _units[i];
                Status burn = u.Statuses[StatusKind.Burn];
                if (burn.Ticks <= 0) continue;
                burn.Ticks--;
                if (!u.Alive)
                {
                    u.Statuses.Clear(StatusKind.Burn);
                    continue;
                }
                if (burn.Ticks % StatusRules.Of(StatusKind.Burn).Period == 0)
                {
                    BattleUnit burner = FindById(burn.SourceId);
                    // Through the damage layers a second of burn is its exact share (BattleSimulation.Layers.cs).
                    if (_layersOn) ApplyHit(burner, 1 - u.Side, u, Hit.Percent((long)u.MaxHp * burn.Stacks, BurnRules.StackPercent), BurnRules.Detail);
                    else ApplyDamage(burner, 1 - u.Side, u, BurnTick(u), BurnRules.Detail);
                }
                if (burn.Ticks <= 0) burn.Stacks = 0;
            }
        }

        private static int BurnTick(BattleUnit u)
        {
            return Math.Max(1, u.MaxHp * u.BurnStacks * BurnRules.StackPercent / 100);
        }

        // An Aimed Shot landed on a burning enemy: all the burn left lands at once, and pays as a cashed mark.
        private void Flare(BattleUnit marksman, BattleUnit target)
        {
            if (!_roadKitOn || _finished || !target.Alive || target.BurnStacks <= 0) return;
            int seconds = (target.BurnTicks + SimClock.TicksPerSecond - 1) / SimClock.TicksPerSecond;
            // Through the damage layers all the burn left, exactly (BattleSimulation.Layers.cs).
            Hit flared = _layersOn ? Hit.Percent((long)target.MaxHp * target.BurnStacks * seconds, BurnRules.StackPercent) : null;
            int flare = flared != null ? flared.Points : BurnTick(target) * seconds;
            string burner = target.BurnerId;
            target.Statuses.Clear(StatusKind.Burn);
            Emit(EventFor(BattleEventKind.Cash, marksman, target, flare, BurnRules.FlareDetail));
            if (flared != null) ApplyHit(marksman, marksman.Side, target, flared, BurnRules.FlareDetail);
            else ApplyDamage(marksman, marksman.Side, target, flare, BurnRules.FlareDetail);
            MarkCashed(marksman, target);
            if (_roadCombatOn) Credit(marksman, burner);
        }
    }
}
