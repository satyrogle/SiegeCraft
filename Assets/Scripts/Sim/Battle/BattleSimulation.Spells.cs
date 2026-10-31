using System;

namespace KindredSiege.Sim.Battle
{
    // Chief spells (ChiefSpells, Jacob 18 Oct 2026). The Eclipse plays as it always has. Steel Storm winds up in the
    // same slot, with the same tell, but goes off as a wall on its own side: the enemy's blows hit it at
    // WallDamagePercent until it falls or an Unleash smashes it open. It cannot be parried, dodged or interrupted:
    // Unleash is its one answer. A fight where neither side has a spell but the Eclipse never meets any of this.
    public sealed partial class BattleSimulation
    {
        /// True when either side's chief order is a spell other than the Eclipse.
        private readonly bool _spellsOn;

        /// What a side's chief order does (the Eclipse unless its rules say otherwise). Read only, for the screen.
        public ChiefSpell Spell(int side)
        {
            return side >= 0 && side <= 1 ? _rules[side].Spell : ChiefSpell.Eclipse;
        }

        /// The one button that answers a side's chief spell: Hold for the Eclipse, Unleash for Steel Storm. For
        /// the screen's cut in and its colour.
        public Directive SpellAnswer(int side)
        {
            return ChiefSpells.Answer(Spell(side));
        }

        /// Ticks a side's Steel Storm wall still stands, or 0.
        public int WallTicks(int side)
        {
            return side >= 0 && side <= 1 ? _sides[side].WallTicks : 0;
        }

        /// The wound up Steel Storm goes off: the wall goes up on the caster's side. The Skullpeak chief's goes up half built
        /// when your Trapper snared one of her Wardens while they locked, which answers it; whole, she says her line
        /// (ChiefKits, 22 Oct 2026).
        private void RaiseWall(int side, BattleUnit caster)
        {
            SideState state = _sides[side];
            state.WallTicks = ChiefSpells.WallTicks;
            bool half = false;
            if (_kitsTwoOn)
            {
                half = KitOn(side) && state.LockSnared;
                state.WallPercent = half ? ChiefKits.HalfWallPercent : 0;
                state.WallUpTick = _tick;
                state.LockSnared = false;
            }
            Emit(new BattleEvent(_tick, BattleEventKind.Wall, side, caster.Id, (int)caster.Class, string.Empty, -1,
                ChiefSpells.WallTicks, half ? ChiefKits.WallHalf : ChiefSpells.WallUp));
            if (!_kitsTwoOn || !KitOn(side)) return;
            if (half) Answered(1 - side, ChiefAnswer.Snare, caster, ChiefKits.SnarePoints, ChiefKits.Name(ChiefAnswer.Snare));
            else Say(caster, ChiefKits.EclipseLine);
        }

        /// What the other side's blows do on this side while its Steel Storm wall stands, in percent: a tenth, or half
        /// through the Skullpeak chief's half built one.
        private int WallPercentOf(int side)
        {
            int percent = _sides[side].WallPercent;
            return percent > 0 ? percent : ChiefSpells.WallDamagePercent;
        }

        /// A side unleashed: a standing wall on the other side is smashed open, and that side loses nerve. The Skullpeak
        /// chief's walls are smashed her own way (BattleSimulation.Skullpeak.cs).
        private void TrySmashWall(int side)
        {
            int enemy = 1 - side;
            if (!_finished && _kitsTwoOn && KitOn(enemy))
            {
                SmashKitWall(side);
                return;
            }
            if (_finished || _sides[enemy].WallTicks <= 0) return;
            _sides[enemy].WallTicks = 0;
            Emit(new BattleEvent(_tick, BattleEventKind.Wall, enemy, string.Empty, -1, string.Empty, -1,
                ChiefSpells.WallSmashNerve, ChiefSpells.WallSmashed));
            Earn(side, ChiefSpells.SmashPoints, ChiefSpells.SmashReason);
            DrainNerve(enemy, ChiefSpells.WallSmashNerve, side);
        }

        /// The wound up War Cry goes off: the caster's side gets nerve back, never above full. Its Release event's
        /// amount is the nerve it gave back.
        private void CryOut(int side, BattleUnit caster)
        {
            SideState state = _sides[side];
            int before = state.Nerve;
            if (_nerveOn) state.Nerve = Math.Min(NerveRules.Full, state.Nerve + ChiefSpells.WarCryNerve);
            Emit(new BattleEvent(_tick, BattleEventKind.Release, side, caster.Id, (int)caster.Class, string.Empty, -1,
                state.Nerve - before, ChiefSpells.Cried));
        }

        /// Walls count down after the tick's actions, like every order; one that runs out falls. A Skullpeak chief's wall
        /// that falls unsmashed breaks the other side's run of quick smashes (her hidden chain, ChiefKits).
        private void CountDownWalls()
        {
            for (int s = 0; s < 2; s++)
            {
                SideState state = _sides[s];
                if (state.WallTicks <= 0) continue;
                state.WallTicks--;
                if (state.WallTicks > 0) continue;
                Emit(new BattleEvent(_tick, BattleEventKind.Wall, s, string.Empty, -1, string.Empty, -1, 0, ChiefSpells.WallFell));
                if (!_kitsTwoOn) continue;
                state.WallPercent = 0;
                if (KitOn(s)) _sides[1 - s].Chain = 0;
            }
        }
    }
}
