using System;

namespace KindredSiege.Sim.Battle
{
    public sealed partial class BattleSimulation
    {
        // The damage layers (Hits.cs; DESIGN_LOCKS, "Builds: one theme per Pal", step 2; FightRules.Layers, campaign rules 17):
        // every hit and heal goes through one pipeline, rounded once, the leftover kept on the fighter for the next.
        private readonly bool _layersOn;

        /// Whether this fight plays the damage layers (campaign rules 17).
        public bool LayersOn => _layersOn;

        // A hit through the pipeline, as ApplyDamage takes one: the victim's side of it (a wall, the cage bear's ribcage, an
        // Eclipse that wore a chief out, a Turned Hunt, gear against big hits) as layers, then rounded once with his leftover.
        private void ApplyHit(BattleUnit source, int sourceSide, BattleUnit victim, Hit hit, string detail, bool leaveOne = false)
        {
            if (_finished || !victim.Alive || hit.Base <= 0) return;
            if (LeaderProtected(victim)) return;
            if (_halted) return;
            if (_comboOn && (victim.EvacTicks > 0 || FlurryKeeps(victim, sourceSide, hit.Points, detail))) return;

            bool foe = sourceSide == 1 - victim.Side;
            // From rules 22 held parry bars make your payoffs stronger (BattleSimulation.Meter.cs).
            if (foe) MeterBoost(sourceSide, hit);
            // From rules 26 a side that whiffed a parry takes more for a second (BattleSimulation.Tight.cs).
            if (_tightOn) WhiffHit(victim, foe, hit);
            if (_spellsOn && _sides[victim.Side].WallTicks > 0 && foe) hit.Add(HitLayer.Guard, WallPercentOf(victim.Side) - 100);
            else if (_kitsTwoOn && foe && LockdownHolds(victim.Side)) hit.Add(HitLayer.Guard, ChiefSpells.WallDamagePercent - 100);
            if (_beastOn && victim.Side == _beastSide && foe && !CageHit(victim, sourceSide, hit)) return;
            if (victim.Role == UnitRole.Chief && WornOutTicksLeft(victim.Side) > 0) hit.Add(HitLayer.Kit, Moves.WornOutDamagePercent - 100);
            if (_huntOn && Turned(victim) && foe) hit.Add(HitLayer.Kit, ChiefKits.TurnedPercent - 100);
            int guard = SpecOf(victim).BigHitArmorPercent;
            if (guard > 0 && detail == Moves.EclipseDetail) hit.Add(HitLayer.Armor, -guard);

            long left = victim.HitLeftover;
            int amount = hit.Resolve(ref left);
            victim.HitLeftover = left;
            if (leaveOne) amount = Math.Min(amount, victim.Hp - 1);
            // A shield soaks what it can first (rules 23, BattleSimulation.Shade.cs).
            int unshielded = amount;
            amount = Shielded(victim, amount);
            if (amount <= 0 && unshielded > 0) return;
            if (amount <= 0) return;
            // Who hit one of yours, for Avenger and Counter Shot (rules 24, BattleSimulation.Whisperer.cs).
            if (_whisperOn && foe && source != null && source.Side == sourceSide) Struck(source, victim);
            Land(source, sourceSide, victim, amount, detail);
        }

        // A heal through the pipeline, rounded once with the patient's own leftover.
        private void HealHit(BattleUnit healer, BattleUnit patient, Hit heal, string detail)
        {
            // Facing the Stoneveil chief your Medic's treatment plucks his Leeches off or stitches a wound shut, healed or not
            // (BattleSimulation.Stoneveil.cs).
            if (_riteOn)
            {
                RiteTreated(healer, patient);
                if (Stopped) return;
            }
            long left = patient.HealLeftover;
            int amount = heal.ResolveHeal(ref left);
            patient.HealLeftover = left;
            Restore(healer, patient, amount, detail);
        }

        // A blow's own bonuses as layers, as Bonuses takes them one rounding after another before rules 17. Its base is the
        // attacker's power and the roll, never under a point.
        private Hit BlowHit(BattleUnit attacker, BattleUnit target, int power, int roll, BlowKind kind)
        {
            Hit hit = Hit.Percent(power, roll);
            if (hit.Base < Hit.Unit) hit = new Hit(1);
            SideState mine = _sides[attacker.Side];
            SideState theirs = _sides[target.Side];
            if (mine.UnleashTicks > 0) hit.Add(HitLayer.Orders, UnleashDamagePercent - 100);
            if (mine.FrenzyTicks > 0)
                hit.Add(HitLayer.Orders, (attacker.Role == UnitRole.Pal ? Moves.FrenzyPalPowerPercent : Moves.FrenzyAllyPowerPercent) - 100);
            if (mine.RallyTicks > 0) hit.Add(HitLayer.Orders, RallyDamagePercent - 100);
            if (attacker.RiseTicks > 0) hit.Add(HitLayer.Orders, FightRules.RiseDamagePercent - 100);
            if (theirs.HoldTicks > 0 && target.Row == Row.Front) hit.Add(HitLayer.Guard, HoldSoak(target.Side) - 100);
            if (_bandOn) BandHit(attacker, target, hit);
            if (_floodOn) FloodHit(attacker.Side, hit);
            if (_huntOn) DriveHit(attacker, target, hit);
            if (_riteOn) DrinkHit(attacker, hit);
            int wounded = SpecOf(attacker).WoundedBonusPercent;
            if (wounded > 0 && target.HpPermille < WoundedBelowPermille) hit.Add(HitLayer.Increased, wounded);
            if (kind != BlowKind.Auto) SpellHit(attacker, kind, hit);
            if (kind == BlowKind.AimedShot && _shotsAtOnce > 1) hit.Times(_shotsAtOnce);
            if (_beastOn && kind != BlowKind.Auto && IsPart(target) && SpecOf(attacker).BeastScars > 0)
                hit.Add(HitLayer.Increased, BeastKits.ScarPercent - 100);
            return hit;
        }

        // A Warden takes the blow for his ward: his Guard's cut and his armour on the hit, the Guard event showing in whole
        // points what the cut kept off.
        private void GuardedHit(BattleUnit attacker, BattleUnit target, BattleUnit warden, Hit hit, int cut)
        {
            int before = hit.Points;
            hit.Add(HitLayer.Guard, -cut);
            Emit(EventFor(BattleEventKind.Guard, warden, target, before - hit.Points, string.Empty));
            ApplyHit(attacker, attacker.Side, warden, Armor(warden, hit), GuardedDetail);
        }

        // A unit's armour as a cut on the hit (Armored, before rules 17).
        private Hit Armor(BattleUnit victim, Hit hit)
        {
            int armor = SpecOf(victim).ArmorPercent;
            return armor > 0 ? hit.Add(HitLayer.Armor, -armor) : hit;
        }

        // One rounding step before rules 17 or one layer from them: the damage taken this percent, or the percent on its layer.
        private static int More(int damage, Hit hit, HitLayer layer, int percent)
        {
            if (hit == null) return damage * percent / 100;
            hit.Add(layer, percent - 100);
            return damage;
        }
    }
}
