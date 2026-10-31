using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The chief kits and the kneel (Jacob, 20 Oct 2026, later, DESIGN_LOCKS "Chief fights"; 21 Oct: "go do the chief
    /// build"; ChiefKits). One quick test per piece, only to prove it runs. Nothing here is tuning.
    public class ChiefKitTests
    {
        private const ChiefMove All = ChiefMove.Cleave | ChiefMove.Brand | ChiefMove.Eclipse;

        private static UnitSpec U(string id, UnitClass unitClass)
        {
            return new UnitSpec(id, unitClass);
        }

        // The Bloodridge chief leads from the back, a Marksman, as in the campaign.
        private static UnitSpec Chief(string id = "chief", int healthPercent = 100)
        {
            return new UnitSpec(id, UnitClass.Marksman, Row.Back, UnitRole.Chief, 100, healthPercent);
        }

        private static UnitSpec Pal(string id)
        {
            return new UnitSpec(id, UnitClass.Marksman, Row.Back, UnitRole.Pal, 100, 100);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        // His side plays his kit with these moves and no heavy swings; ours has a grade 1 Eclipse on its 5th button, and combos.
        // With tactics, his forecast too (FightRules.ChiefTactics, 22 Oct 2026).
        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, ChiefMove moves,
            bool nerve = true, bool tactics = false)
        {
            var kit = new SideRules(2, (moves & ChiefMove.Eclipse) != 0 ? ChiefKits.EclipseEveryTicks : 0, false, 0, ChiefSpell.Eclipse,
                kit: ChiefKit.Bloodridge, moves: moves);
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, nerve ? NerveRules.On() : NerveRules.Off, 10, 400,
                new SideRules(1, 0, true), kit, tactics ? FightRules.CampaignChiefTactics : FightRules.CampaignChiefKit));
        }

        private static bool Order(SimSession session, string directive, string target = "")
        {
            return session.TryIssue(Directives.CommandKind, "0", target, 0, 0, directive, out _);
        }

        // Ticks until an event matches, calling before ahead of each tick; events from before the call are not looked at.
        private static BattleEvent? Next(BattleSimulation sim, SimSession session, Func<BattleEvent, bool> match, Action before = null,
            long limit = BattleSetup.DefaultTickLimit)
        {
            int seen = sim.Events.Count;
            while (!sim.IsFinished && sim.CurrentTick < limit)
            {
                before?.Invoke();
                session.Tick();
                for (; seen < sim.Events.Count; seen++)
                    if (match(sim.Events[seen])) return sim.Events[seen];
            }
            for (; seen < sim.Events.Count; seen++)
                if (match(sim.Events[seen])) return sim.Events[seen];
            return null;
        }

        // Hold in the last moment before his Cleave falls.
        private static void HoldForCleave(BattleSimulation sim, SimSession session)
        {
            if (sim.CleaveTicks(1) > 0 && sim.CleaveTicks(1) <= 10 && sim.HoldTicksLeft(0) == 0) Order(session, "Hold");
        }

        private static string Landed(string move, string how)
        {
            return move + "." + how;
        }

        // ------------------------------------------------------------ his moves

        [Test]
        public void HisCleaveGlowsThenFallsAcrossFromHimAndATimedHoldParriesIt()
        {
            var sim = Fight("kit.cleave", 41, Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("c", UnitClass.Marksman)),
                Side(U("x", UnitClass.Skirmisher), Chief(healthPercent: 400)), ChiefMove.Cleave);
            var session = new SimSession(sim);
            Assert.AreEqual(ChiefKit.Bloodridge, sim.Kit(1));
            Assert.AreEqual(ChiefMove.Cleave, sim.KitMoves(1));

            // His blade glows for a second over whoever stands across from him in your front row, then it falls on them.
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.CleaveName, tell.Detail);
            Assert.AreEqual("chief", tell.ActorId);
            Assert.GreaterOrEqual(tell.Tick, ChiefKits.CleaveEveryTicks);
            Assert.Less(tell.Tick, ChiefKits.CleaveEveryTicks + 60, "every six seconds, or as soon after as he is free");
            Assert.AreEqual(ChiefKits.CleaveWindupTicks, tell.Amount);
            Assert.AreEqual(Row.Front, sim.FindById(tell.TargetId).Row);
            BattleEvent land = Next(sim, session, e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual(Landed(ChiefKits.CleaveName, ChiefKits.Full), land.Detail);
            Assert.AreEqual(tell.TargetId, land.TargetId);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.CleaveDetail && e.TargetId == land.TargetId));
            Assert.AreEqual(0, sim.Glory(0), "eating a Cleave is no answer");

            // The next one: Hold as it falls parries it. He staggers, your Glory climbs and his nerve takes a chunk.
            int nerve = 0;
            BattleEvent parried = Next(sim, session, e => e.Kind == BattleEventKind.Land, () =>
            {
                if (sim.CleaveTicks(1) > 0 && sim.CleaveTicks(1) <= 10 && sim.HoldTicksLeft(0) == 0)
                {
                    nerve = sim.Nerve(1);
                    Order(session, "Hold");
                }
            }).Value;
            Assert.AreEqual(Landed(ChiefKits.CleaveName, ChiefKits.Parried), parried.Detail);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Parry && e.ActorSide == 0 && e.TargetId == "chief" && e.Tick == parried.Tick),
                "he staggers");
            Assert.AreEqual(ChiefKits.ParryGlory, sim.Glory(0), "the fight's first answer, so no mixing yet");
            Assert.GreaterOrEqual(nerve - sim.Nerve(1), ChiefKits.ParryNerve, "the parry took a chunk of his nerve");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Line && e.ActorId == "chief" && e.Detail == ChiefKits.ParryLine));
            Assert.AreEqual("Lucky.", ChiefKits.Line(ChiefKit.Bloodridge, ChiefKits.ParryLine, false));
        }

        [Test]
        public void AWardenCoveringWhoeverStandsAcrossFromHimTakesTheCleave()
        {
            // Both of ours stand as far from his lane, so he cleaves the first of the row, whom our Warden covers.
            var sim = Fight("kit.cleave.guard", 42, Side(U("a", UnitClass.Skirmisher), U("w", UnitClass.Warden)),
                Side(U("x", UnitClass.Skirmisher), Chief(healthPercent: 400)), ChiefMove.Cleave);
            var session = new SimSession(sim);
            BattleEvent land = Next(sim, session, e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual("a", land.TargetId);
            Assert.AreEqual(Landed(ChiefKits.CleaveName, ChiefKits.Guarded), land.Detail);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Guard && e.ActorId == "w" && e.TargetId == "a" && e.Tick == land.Tick));
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.CleaveDetail && e.TargetId == "w" &&
                e.Tick == land.Tick), "he takes it");
            Assert.AreEqual(ChiefKits.GuardGlory, sim.Glory(0));
        }

        [Test]
        public void HisBrandBurstsOnThemAndThoseBesideThemUnlessYouAnswerIt()
        {
            var ours = Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("c", UnitClass.Skirmisher), U("d", UnitClass.Marksman));
            List<UnitSpec> Theirs() => Side(U("x", UnitClass.Skirmisher), Chief(healthPercent: 400));

            // Eaten: nobody covers, no Medic, no Fall Back. It bursts on the branded one and whoever stands next to them.
            var sim = Fight("kit.brand", 43, ours, Theirs(), ChiefMove.Brand);
            var session = new SimSession(sim);
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.BrandName, tell.Detail);
            Assert.AreEqual(ChiefKits.BrandEveryTicks, tell.Tick);
            Assert.AreEqual(ChiefKits.BrandRingTicks, tell.Amount);
            Assert.AreEqual(ChiefKits.BrandRingTicks - 1, sim.FindById(tell.TargetId).BrandTicks, "the ring runs from its first tick");
            BattleEvent burst = Next(sim, session, e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual(Landed(ChiefKits.BrandName, ChiefKits.Full), burst.Detail);
            Assert.AreEqual(tell.TargetId, burst.TargetId);
            List<string> row = ours.Where(u => u.Row == sim.FindById(tell.TargetId).Row).Select(u => u.Id).ToList();
            int at = row.IndexOf(tell.TargetId);
            CollectionAssert.AreEquivalent(row.Where((id, i) => Math.Abs(i - at) <= 1).ToList(),
                sim.Events.Where(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.BrandDetail && e.Tick == burst.Tick)
                    .Select(e => e.TargetId).ToList());
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.EatenDetail && e.Tick == burst.Tick));

            // Dodged: Fall Back as it bursts gets everyone clear.
            var dodge = Fight("kit.brand.dodge", 43, ours, Theirs(), ChiefMove.Brand);
            var ds = new SimSession(dodge);
            BattleEvent dodged = Next(dodge, ds, e => e.Kind == BattleEventKind.Land, () =>
            {
                if (dodge.Units.Any(u => u.BrandTicks > 0 && u.BrandTicks <= 20) && dodge.FallBackTicksLeft(0) == 0) Order(ds, "FallBack");
            }).Value;
            Assert.AreEqual(Landed(ChiefKits.BrandName, ChiefKits.Dodged), dodged.Detail);
            Assert.IsFalse(dodge.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.BrandDetail));
            Assert.AreEqual(ChiefKits.DodgeGlory, dodge.Glory(0));

            // Healed through: our Medic stands and nobody falls to it.
            var heal = Fight("kit.brand.heal", 43, Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("m", UnitClass.Medic)),
                Theirs(), ChiefMove.Brand);
            BattleEvent healed = Next(heal, new SimSession(heal), e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual(Landed(ChiefKits.BrandName, ChiefKits.Healed), healed.Detail);
            Assert.AreEqual(ChiefKits.HealGlory, heal.Glory(0));

            // Guarded: our Warden covers the branded one and takes their share halved. He cannot cover himself, so this
            // looks for a fight whose Brand went on the other one.
            for (ulong seed = 1; seed < 40; seed++)
            {
                var guard = Fight("kit.brand.guard", seed, Side(U("a", UnitClass.Skirmisher), U("w", UnitClass.Warden)), Theirs(), ChiefMove.Brand);
                var gs = new SimSession(guard);
                BattleEvent mark = Next(guard, gs, e => e.Kind == BattleEventKind.Tell).Value;
                if (mark.TargetId != "a") continue;
                BattleEvent covered = Next(guard, gs, e => e.Kind == BattleEventKind.Land).Value;
                Assert.AreEqual(Landed(ChiefKits.BrandName, ChiefKits.Guarded), covered.Detail);
                Assert.IsTrue(guard.Events.Any(e => e.Kind == BattleEventKind.Guard && e.ActorId == "w" && e.TargetId == "a" && e.Tick == covered.Tick));
                Assert.IsFalse(guard.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.BrandDetail && e.TargetId == "a"),
                    "he took her share");
                Assert.AreEqual(ChiefKits.GuardGlory, guard.Glory(0));
                return;
            }
            Assert.Fail("in forty fights the Brand never went on the one the Warden could cover");
        }

        [Test]
        public void HisEclipseComesEveryTwentyFiveSeconds()
        {
            // Two Wardens against him alone, with no nerve to break, so the fight lasts.
            var sim = Fight("kit.eclipse", 44, Side(U("v", UnitClass.Warden), U("w", UnitClass.Warden)), Side(Chief(healthPercent: 1000)),
                ChiefMove.Eclipse, nerve: false);
            var session = new SimSession(sim);
            BattleEvent first = Next(sim, session, e => e.Kind == BattleEventKind.Windup && e.ActorId == "chief").Value;
            Assert.AreEqual(ChiefKits.EclipseEveryTicks, first.Tick);
            Assert.AreEqual("Eclipse", first.Detail);
            BattleEvent second = Next(sim, session, e => e.Kind == BattleEventKind.Windup && e.ActorId == "chief").Value;
            Assert.AreEqual(2 * ChiefKits.EclipseEveryTicks, second.Tick);
        }

        [Test]
        public void AtHalfHealthBloodSunMakesEveryMoveWorse()
        {
            // Alone, so no band guards him and our blades go for him.
            var sim = Fight("kit.bloodsun", 45, Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("c", UnitClass.Skirmisher),
                U("d", UnitClass.Marksman)), Side(Chief(healthPercent: 300)), All, nerve: false);
            var session = new SimSession(sim);
            BattleEvent phase = Next(sim, session, e => e.Kind == BattleEventKind.Phase).Value;
            Assert.AreEqual(ChiefKits.PhaseName(ChiefKit.Bloodridge), phase.Detail);
            Assert.IsTrue(sim.BloodSun(1));
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Line && e.Detail == ChiefKits.BloodSunLine && e.Tick == phase.Tick));
            Assert.AreEqual("Now you burn.", ChiefKits.Line(ChiefKit.Bloodridge, ChiefKits.BloodSunLine, false));

            // He casts his Eclipse again at once, and it is the first of his Eclipses to hit a fifth harder than the last.
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Windup && e.ActorId == "chief" && e.Tick == phase.Tick));
            BattleEvent first = After(sim, session, phase.Tick, e => e.Kind == BattleEventKind.Release && e.ActorId == "chief").Value;
            Assert.AreEqual(Moves.EclipsePercent(2) * ChiefKits.BloodSunEclipsePercent / 100, first.Amount);

            // His Brand marks two, and a Cleave that lands leaves its victim bleeding.
            BattleEvent brand = After(sim, session, phase.Tick, e => e.Kind == BattleEventKind.Tell && e.Detail == ChiefKits.BrandName).Value;
            Assert.AreEqual(ChiefKits.BloodSunBrands,
                sim.Events.Count(e => e.Kind == BattleEventKind.Tell && e.Detail == ChiefKits.BrandName && e.Tick == brand.Tick));
            BattleEvent cleave = After(sim, session, phase.Tick,
                e => e.Kind == BattleEventKind.Land && e.Detail == Landed(ChiefKits.CleaveName, ChiefKits.Full)).Value;
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Mark && e.ActorId == "chief" && e.Detail == ClassSpells.BleedMark &&
                e.TargetId == cleave.TargetId && e.Tick == cleave.Tick));
        }

        // The first event after this tick that matches, ticking on until one comes or the fight ends.
        private static BattleEvent? After(BattleSimulation sim, SimSession session, long tick, Func<BattleEvent, bool> match)
        {
            int seen = 0;
            while (true)
            {
                for (; seen < sim.Events.Count; seen++)
                    if (sim.Events[seen].Tick > tick && match(sim.Events[seen])) return sim.Events[seen];
                if (sim.IsFinished || sim.CurrentTick >= BattleSetup.DefaultTickLimit) return null;
                session.Tick();
            }
        }

        // ------------------------------------------------------------ Glory and his hidden chain

        [Test]
        public void GloryClimbsFasterForMixedAnswersAndDropsARankWhenYouEatABigMove()
        {
            var sim = Fight("kit.glory", 46, Side(U("a", UnitClass.Skirmisher), U("w", UnitClass.Warden)),
                Side(U("x", UnitClass.Skirmisher), Chief(healthPercent: 400)), ChiefMove.Cleave | ChiefMove.Eclipse, nerve: false);
            var session = new SimSession(sim);

            // Guard, parry (a different answer: half again), parry (the same twice: half), guard (different again).
            int guard = ChiefKits.GuardGlory;
            int parry = guard + ChiefKits.ParryGlory * ChiefKits.MixedPercent / 100;
            int again = parry + ChiefKits.ParryGlory * ChiefKits.RepeatPercent / 100;
            int back = again + ChiefKits.GuardGlory * ChiefKits.MixedPercent / 100;
            int[] expected = { guard, parry, again, back };
            for (int n = 0; n < expected.Length; n++)
            {
                bool hold = n == 1 || n == 2;
                BattleEvent land = Next(sim, session, e => e.Kind == BattleEventKind.Land, () =>
                {
                    if (hold) HoldForCleave(sim, session);
                }).Value;
                Assert.AreEqual(Landed(ChiefKits.CleaveName, hold ? ChiefKits.Parried : ChiefKits.Guarded), land.Detail, "Cleave " + (n + 1));
                Assert.AreEqual(expected[n], sim.Glory(0), "after Cleave " + (n + 1));
            }

            // His Eclipse, eaten in full: a rank down, and he says so.
            BattleEvent eclipse = Next(sim, session, e => e.Kind == BattleEventKind.Release && e.ActorId == "chief").Value;
            Assert.AreEqual(Moves.Full, eclipse.Detail);
            Assert.AreEqual(back - ChiefKits.GloryPerRank, sim.Glory(0));
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Line && e.Detail == ChiefKits.EclipseLine && e.Tick == eclipse.Tick));
            Assert.AreEqual("Kneel.", ChiefKits.Line(ChiefKit.Bloodridge, ChiefKits.EclipseLine, false));
        }

        [Test]
        public void GloryPaysHalfAgainAtAAndAtSYourFifthButtonHitsTwiceAsHard()
        {
            Assert.AreEqual(0, ChiefKits.GloryBonus(GloryRank.B, 2));
            Assert.AreEqual(1, ChiefKits.GloryBonus(GloryRank.A, 1));
            Assert.AreEqual(1, ChiefKits.GloryBonus(GloryRank.A, 2));
            Assert.AreEqual(2, ChiefKits.GloryBonus(GloryRank.S, 2));
            Assert.AreEqual(GloryRank.D, ChiefKits.Rank(0));
            Assert.AreEqual(GloryRank.A, ChiefKits.Rank(3 * ChiefKits.GloryPerRank));
            Assert.AreEqual(GloryRank.S, ChiefKits.Rank(ChiefKits.GloryMax));

            // We parry every Cleave until S, then press the 5th button. The same answer each time climbs by half, so it takes ten,
            // and two Wardens, who hit softly, so he lives that long.
            var sim = Fight("kit.glory.s", 47, Side(U("v", UnitClass.Warden), U("w", UnitClass.Warden), Pal("p")),
                Side(U("x", UnitClass.Skirmisher), Chief(healthPercent: 600)), ChiefMove.Cleave, nerve: false);
            var session = new SimSession(sim);
            for (int n = 0; n < 12 && sim.GloryRankOf(0) != GloryRank.S; n++)
            {
                BattleEvent land = Next(sim, session, e => e.Kind == BattleEventKind.Land, () => HoldForCleave(sim, session)).Value;
                Assert.AreEqual(Landed(ChiefKits.CleaveName, ChiefKits.Parried), land.Detail, "Cleave " + (n + 1));
            }
            Assert.AreEqual(GloryRank.S, sim.GloryRankOf(0));
            Assert.AreEqual(ChiefKits.ParryGlory + 9 * ChiefKits.ParryGlory * ChiefKits.RepeatPercent / 100, sim.Glory(0));
            while (sim.DirectivePoints(0) < Directives.Cost(Directive.Eclipse)) session.Tick();
            Assert.IsTrue(Order(session, "Eclipse"));
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Empowered && e.ActorSide == 0 && e.Amount == Moves.EmpoweredPercent));
            BattleEvent ours = Next(sim, session, e => e.Kind == BattleEventKind.Release && e.ActorSide == 0).Value;
            Assert.AreEqual(Moves.EclipsePercent(1) * Moves.EmpoweredPercent / 100, ours.Amount);
        }

        [Test]
        public void ParryTwoCleavesThenHisEclipseAndYourWardenThrowsItBack()
        {
            BattleSimulation Chain(string id, bool nerve) => Fight(id, 48, Side(U("a", UnitClass.Skirmisher), U("w", UnitClass.Warden)),
                Side(U("x", UnitClass.Skirmisher), Chief(healthPercent: 400)), ChiefMove.Cleave | ChiefMove.Eclipse, nerve);

            // His first two Cleaves land; we parry the two before his Eclipse, then the Eclipse, each at the last moment.
            BattleEvent Throw(BattleSimulation sim, SimSession session, Action held) => Next(sim, session,
                e => e.Kind == BattleEventKind.Combo && e.Detail == ChiefKits.Sunbreaker, () =>
                {
                    int landed = sim.Events.Count(e => e.Kind == BattleEventKind.Land);
                    bool cleave = landed >= 2 && sim.CleaveTicks(1) > 0 && sim.CleaveTicks(1) <= 10;
                    bool eclipse = sim.WindupTicks(1) > 0 && sim.WindupTicks(1) <= 10;
                    if (!(cleave || eclipse) || sim.HoldTicksLeft(0) > 0) return;
                    held();
                    Order(session, "Hold");
                }).Value;

            // The throw is a massive hit to his nerve.
            BattleSimulation nerved = Chain("kit.sunbreaker", true);
            var ns = new SimSession(nerved);
            int nerve = 0;
            Throw(nerved, ns, () => nerve = nerved.Nerve(1));
            Assert.GreaterOrEqual(nerve - nerved.Nerve(1), ChiefKits.SunbreakerNerve);

            // Without nerve, so nothing breaks: it breaks him, it never kills him, and he is stunned for four seconds.
            BattleSimulation sim = Chain("kit.sunbreaker.stun", false);
            var session = new SimSession(sim);
            BattleUnit chief = sim.FindById("chief");
            BattleEvent thrown = Throw(sim, session, () => { });
            Assert.AreEqual(2, sim.Events.Count(e => e.Kind == BattleEventKind.Land && e.Detail == Landed(ChiefKits.CleaveName, ChiefKits.Parried)));
            Assert.AreEqual("w", thrown.ActorId, "your Warden throws it");
            Assert.AreEqual("chief", thrown.TargetId);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.SunbreakerDetail && e.TargetId == "chief"));
            Assert.IsTrue(chief.Alive);
            Assert.Greater(chief.StunTicks, 0, "stunned");
            BattleEvent? acted = Next(sim, session, e => e.ActorId == "chief" &&
                (e.Kind == BattleEventKind.Tell || e.Kind == BattleEventKind.Windup || e.Kind == BattleEventKind.Hit || e.Kind == BattleEventKind.Aim));
            if (acted.HasValue) Assert.GreaterOrEqual(acted.Value.Tick, thrown.Tick + ChiefKits.SunbreakerStunTicks - 1);
        }

        // ------------------------------------------------------------ the Bloodridge chief finished (22 Oct 2026)

        private static int Count(ChiefMove moves)
        {
            int n = 0;
            for (int bit = 0; bit < 30; bit++)
                if (((int)moves & (1 << bit)) != 0) n++;
            return n;
        }

        // A fighter at these percents of power and health, in its class's row.
        private static UnitSpec U(string id, UnitClass unitClass, int power, int health)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, power, health);
        }

        [Test]
        public void HisBurningRingSpinsThenGoesDownOneLaneThroughBothRows()
        {
            // Two of ours in each row; him alone and tough, with no nerve to break, so the fight lasts.
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Warden), U("b", UnitClass.Warden), U("m", UnitClass.Medic), U("t", UnitClass.Trapper));
            var sim = Fight("kit.ring", 61, Ours(), Side(Chief(healthPercent: 1000)), ChiefMove.BurningRing, nerve: false);
            var session = new SimSession(sim);
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.RingName, tell.Detail);
            Assert.GreaterOrEqual(tell.Tick, ChiefKits.RingEveryTicks);
            Assert.Less(tell.Tick, ChiefKits.RingEveryTicks + 60, "every twelve seconds, or as soon after as he is free");
            Assert.AreEqual(ChiefKits.RingSpinTicks, tell.Amount);
            Assert.AreEqual(tell.TargetId, sim.RingTargetId(1));

            // Eaten: it hits whoever stands nearest its lane in each row, and Glory drops.
            BattleEvent land = Next(sim, session, e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual(Landed(ChiefKits.RingName, ChiefKits.Full), land.Detail);
            Assert.AreEqual(tell.Tick + ChiefKits.RingSpinTicks - 1, land.Tick);
            List<BattleUnit> hit = sim.Events.Where(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.RingDetail && e.Tick == land.Tick)
                .Select(e => sim.FindById(e.TargetId)).ToList();
            Assert.AreEqual(1, hit.Count(u => u.Row == Row.Front), "one in the front row");
            Assert.AreEqual(1, hit.Count(u => u.Row == Row.Back), "one in the back row");
            Assert.IsTrue(hit.Any(u => u.Id == tell.TargetId), "down its mark's lane");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.EatenDetail && e.Tick == land.Tick));

            // Dodged: Fall Back as it flies gets everyone clear.
            var dodge = Fight("kit.ring.dodge", 61, Ours(), Side(Chief(healthPercent: 1000)), ChiefMove.BurningRing, nerve: false);
            var ds = new SimSession(dodge);
            BattleEvent dodged = Next(dodge, ds, e => e.Kind == BattleEventKind.Land, () =>
            {
                if (dodge.RingTicks(1) > 0 && dodge.RingTicks(1) <= 20 && dodge.FallBackTicksLeft(0) == 0) Order(ds, "FallBack");
            }).Value;
            Assert.AreEqual(Landed(ChiefKits.RingName, ChiefKits.Dodged), dodged.Detail);
            Assert.IsFalse(dodge.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.RingDetail));
            Assert.AreEqual(ChiefKits.DodgeGlory, dodge.Glory(0));
        }

        [Test]
        public void FocusFireHimWhileHisRingSpinsAndYourMarksmansShotKnocksItOutOfHisHand()
        {
            // Three Marksmen, so one of them can shoot in the second and a half it spins; we focus him as it starts.
            for (ulong seed = 1; seed < 40; seed++)
            {
                var sim = Fight("kit.ring.knock", seed, Side(U("w", UnitClass.Warden), U("m1", UnitClass.Marksman), U("m2", UnitClass.Marksman),
                    U("m3", UnitClass.Marksman)), Side(Chief(healthPercent: 1000)), ChiefMove.BurningRing, nerve: false);
                var session = new SimSession(sim);
                BattleEvent land = Next(sim, session, e => e.Kind == BattleEventKind.Land, () =>
                {
                    if (sim.RingTicks(1) > 0 && sim.FocusTicksLeft(0) == 0) Order(session, "FocusFire", "chief");
                }).Value;
                if (land.Detail != Landed(ChiefKits.RingName, ChiefKits.Broken)) continue;
                Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Broken && e.Detail == ChiefKits.RingDetail && e.TargetId == "chief" &&
                    e.ActorClass == (int)UnitClass.Marksman && e.Tick == land.Tick), "a Marksman's shot knocked it out");
                Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.RingDetail));
                Assert.AreEqual(0, sim.RingTicks(1));
                Assert.AreEqual(ChiefKits.BreakGlory, sim.Glory(0));
                return;
            }
            Assert.Fail("in forty fights no Marksman's shot landed on him while the ring spun");
        }

        [Test]
        public void HisSunMantleBurnsWhoeverStrikesHimUnlessYouFallBackOrSwitchToHisBand()
        {
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher));

            // Eaten: our blades keep striking him while it burns, and each blow burns its striker.
            var sim = Fight("kit.mantle", 62, Ours(), Side(Chief(healthPercent: 1000)), ChiefMove.SunMantle, nerve: false);
            var session = new SimSession(sim);
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.MantleName, tell.Detail);
            Assert.AreEqual(ChiefKits.MantleEveryTicks, tell.Tick);
            Assert.AreEqual(ChiefKits.MantleTicks, tell.Amount);
            BattleEvent land = Next(sim, session, e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual(Landed(ChiefKits.MantleName, ChiefKits.Full), land.Detail);
            Assert.AreEqual(tell.Tick + ChiefKits.MantleTicks - 1, land.Tick);
            List<BattleEvent> burns = sim.Events.Where(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.MantleDetail).ToList();
            Assert.IsNotEmpty(burns);
            Assert.IsTrue(burns.All(e => e.ActorId == "chief" && (e.TargetId == "a" || e.TargetId == "b") && e.Tick >= tell.Tick && e.Tick <= land.Tick));
            Assert.AreEqual(burns.Sum(e => e.Amount), land.Amount);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.EatenDetail && e.Tick == land.Tick));

            // Dodged: Fall Back all the while it burns, so our blades never reach him.
            var dodge = Fight("kit.mantle.dodge", 62, Ours(), Side(Chief(healthPercent: 1000)), ChiefMove.SunMantle, nerve: false);
            var ds = new SimSession(dodge);
            BattleEvent dodged = Next(dodge, ds, e => e.Kind == BattleEventKind.Land, () =>
            {
                if (dodge.CurrentTick >= ChiefKits.MantleEveryTicks - 1 && dodge.FallBackTicksLeft(0) == 0) Order(ds, "FallBack");
            }).Value;
            Assert.AreEqual(Landed(ChiefKits.MantleName, ChiefKits.Dodged), dodged.Detail);
            Assert.IsFalse(dodge.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.MantleDetail));
            Assert.AreEqual(ChiefKits.DodgeGlory, dodge.Glory(0));

            // Switched: our Focus Fire stays on his band's tough Skirmisher, so nobody strikes him.
            var swap = Fight("kit.mantle.switch", 62, Ours(), Side(U("x", UnitClass.Skirmisher, 100, 1000), Chief(healthPercent: 1000)),
                ChiefMove.SunMantle, nerve: false);
            var ss = new SimSession(swap);
            BattleEvent switched = Next(swap, ss, e => e.Kind == BattleEventKind.Land, () =>
            {
                if (swap.FocusTicksLeft(0) == 0) Order(ss, "FocusFire", "x");
            }).Value;
            Assert.AreEqual(Landed(ChiefKits.MantleName, ChiefKits.Switched), switched.Detail);
            Assert.IsFalse(swap.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.MantleDetail));
            Assert.AreEqual(ChiefKits.SwitchGlory, swap.Glory(0));
        }

        [Test]
        public void HisForecastShowsEachMoveItsMarkAndHowHardSixSecondsBeforeItIsDue()
        {
            // Our blades barely scratch anyone and nobody falls, so every mark still stands when its move comes.
            var sim = Fight("kit.forecast", 63, Side(U("a", UnitClass.Skirmisher, 10, 1000), U("b", UnitClass.Skirmisher, 10, 1000),
                    U("c", UnitClass.Skirmisher, 10, 1000), U("d", UnitClass.Marksman, 10, 1000)),
                Side(U("x", UnitClass.Skirmisher, 10, 1000), Chief(healthPercent: 1000)),
                ChiefMove.Cleave | ChiefMove.Brand | ChiefMove.BurningRing, nerve: false, tactics: true);
            var session = new SimSession(sim);
            Assert.IsTrue(sim.ChiefTacticsOn);
            Assert.IsTrue(sim.ForecastOn(1));
            Assert.IsFalse(sim.ForecastOn(0));

            // On the first tick his first Cleave shows: on whoever stands across from him, as hard as his heavy hit.
            session.Tick();
            BattleEvent first = sim.Events.Single(e => e.Kind == BattleEventKind.Forecast);
            Assert.AreEqual(0, first.Tick);
            Assert.AreEqual(ChiefKits.CleaveName, first.Detail);
            Assert.AreEqual(sim.FindById("chief").Power * HeavyRules.DamagePercent / 100, first.Amount);
            ChiefForecast shown = sim.Forecasts(1).Single();
            Assert.AreEqual(ChiefMove.Cleave, shown.Move);
            Assert.AreEqual(ChiefKits.CleaveEveryTicks - 1, shown.TicksLeft);
            Assert.AreEqual(first.TargetId, shown.TargetId);
            Assert.AreEqual(first.Amount, shown.Amount);

            // Every move he starts showed at least six seconds before, on the same mark, and the list stays soonest first.
            while (sim.CurrentTick < 3 * ChiefKits.BrandEveryTicks && !sim.IsFinished)
            {
                session.Tick();
                IReadOnlyList<ChiefForecast> list = sim.Forecasts(1);
                for (int i = 1; i < list.Count; i++) Assert.LessOrEqual(list[i - 1].TicksLeft, list[i].TicksLeft);
            }
            List<BattleEvent> tells = sim.Events.Where(e => e.Kind == BattleEventKind.Tell).ToList();
            Assert.IsTrue(tells.Any(e => e.Detail == ChiefKits.CleaveName) && tells.Any(e => e.Detail == ChiefKits.BrandName) &&
                tells.Any(e => e.Detail == ChiefKits.RingName));
            for (int i = 0; i < sim.Events.Count; i++)
            {
                BattleEvent tell = sim.Events[i];
                if (tell.Kind != BattleEventKind.Tell) continue;
                // The forecast before it in the history: the next one of a move shows on the tick it starts, after it.
                int j = i - 1;
                while (j >= 0 && !(sim.Events[j].Kind == BattleEventKind.Forecast && sim.Events[j].Detail == tell.Detail)) j--;
                Assert.GreaterOrEqual(j, 0, tell.Detail + " at " + tell.Tick + " never showed");
                BattleEvent fore = sim.Events[j];
                Assert.AreEqual(fore.TargetId, tell.TargetId, tell.Detail + " at " + tell.Tick);
                Assert.GreaterOrEqual(tell.Tick - fore.Tick, ChiefKits.ForecastTicks, tell.Detail + " at " + tell.Tick);
            }
        }

        [Test]
        public void EachRunHeTakesThreeOfHisFiveMoves()
        {
            Assert.AreEqual(5, Count(ChiefKits.Pool(ChiefKit.Bloodridge)));
            var seen = new HashSet<ChiefMove>(); // membership only
            for (ulong seed = 1; seed <= 20; seed++)
            {
                Campaign c = Campaign.New(seed, new CampaignTuning());
                ChiefMove moves = c.ChiefMovesThisRun(CampaignTribes.Bloodridge);
                Assert.AreEqual(ChiefKits.Takes, Count(moves));
                Assert.AreEqual(moves, moves & ChiefKits.Pool(ChiefKit.Bloodridge));
                seen.Add(moves);
            }
            Assert.Greater(seen.Count, 3, "different runs take different moves");
        }

        // ------------------------------------------------------------ the kneel

        [Test]
        public void WhenHisNerveRunsOutHeKneelsAndNothingMovesUntilYouChoose()
        {
            // His Warden guards him; our blades and Unleash break their nerve.
            BattleSimulation Brawl() => Fight("kit.kneel", 51, Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher),
                U("c", UnitClass.Skirmisher), U("d", UnitClass.Marksman)), Side(U("w", UnitClass.Warden), Chief()), ChiefMove.None);
            BattleEvent Kneel(BattleSimulation fight, SimSession session) => Next(fight, session, e => e.Kind == BattleEventKind.Yield, () =>
            {
                if (fight.UnleashTicksLeft(0) == 0) Order(session, "Unleash");
            }).Value;

            BattleSimulation sim = Brawl();
            var s = new SimSession(sim);
            BattleEvent kneel = Kneel(sim, s);
            BattleUnit chief = sim.FindById("chief");
            Assert.AreEqual(ChiefKits.KneelDetail, kneel.Detail);
            Assert.AreEqual(0, sim.Nerve(1));
            Assert.IsTrue(chief.Alive && chief.Yielded);
            Assert.IsTrue(sim.AwaitingVerdict);
            Assert.AreSame(chief, sim.KneelingChief);
            Assert.IsFalse(sim.IsFinished, "no break yet: you choose");
            Assert.IsNull(sim.CoveredBy(chief), "his Warden stepped back");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Line && e.Detail == ChiefKits.KneelLine && e.Tick == kneel.Tick));

            // Nothing moves while he kneels, and nothing runs out on its own.
            int events = sim.Events.Count, health = chief.Hp;
            long tick = sim.CurrentTick;
            for (int i = 0; i < 300; i++) s.Tick();
            Assert.AreEqual(events, sim.Events.Count);
            Assert.AreEqual(tick + 300, sim.CurrentTick);
            Assert.AreEqual(health, chief.Hp);

            // Only the verdict goes, and it is free.
            Assert.IsFalse(Order(s, "Unleash"));
            Assert.IsFalse(Order(s, "FallBack"));
            Assert.IsFalse(Order(s, "FocusFire", "w"));
            Assert.IsTrue(sim.CanUse(0, Directive.Hold, out _) && sim.CanUse(0, Directive.FocusFire, out _));
            Assert.IsFalse(sim.CanUse(0, Directive.Unleash, out string why));
            Assert.AreEqual(ChiefKits.KneelFailure, why);

            // Hold spares him: his side breaks and he lives, so the campaign takes him and his tribe.
            Assert.IsTrue(Order(s, "Hold"));
            Assert.IsTrue(sim.IsFinished);
            Assert.AreEqual(0, sim.Winner);
            Assert.AreEqual(1, sim.BrokenSide);
            Assert.IsTrue(chief.Alive);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Spare && e.ActorSide == 0 && e.TargetId == "chief"));

            // Focus Fire on him executes him instead.
            BattleSimulation other = Brawl();
            var os = new SimSession(other);
            Kneel(other, os);
            Assert.IsTrue(Order(os, "FocusFire", "chief"));
            Assert.IsTrue(other.IsFinished);
            Assert.AreEqual(0, other.Winner);
            Assert.IsFalse(other.FindById("chief").Alive);
            Assert.IsTrue(other.Events.Any(e => e.Kind == BattleEventKind.Execute && e.TargetId == "chief"));

            // Nobody chooses: when the fight's clock runs out, he is spared.
            BattleSimulation late = Brawl();
            var ls = new SimSession(late);
            Kneel(late, ls);
            while (!late.IsFinished) ls.Tick();
            Assert.AreEqual(BattleSetup.DefaultTickLimit - 1, late.EndTick);
            Assert.IsTrue(late.Events.Any(e => e.Kind == BattleEventKind.Spare && e.ActorId == ChiefKits.TimeActor));
            Assert.IsTrue(late.FindById("chief").Alive);
        }

        // ------------------------------------------------------------ the campaign

        private static Campaign AtTheChief(ulong seed, CampaignTuning tuning)
        {
            Campaign c = Campaign.New(seed, tuning);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            c.ChooseTribe(CampaignTribes.BloodridgeId);
            Assert.AreEqual(EncounterKind.Chief, c.Offers[0].Kind);
            return c;
        }

        [Test]
        public void TheBloodridgeChiefFightsWithHisKitAndHisCodexPageKeepsYourBestGrade()
        {
            var t = new CampaignTuning { PathFights = 0, EnemyScaleTier1 = 40 };
            for (ulong seed = 4100; seed < 4140; seed++)
            {
                Campaign c = AtTheChief(seed, t);
                Assert.IsFalse(c.MetChief(CampaignTribes.BloodridgeId));
                CampaignFight fight = c.StartEncounter(0);
                Assert.AreEqual(Campaign.RulesVersion, fight.RulesVersion);
                Assert.IsTrue(fight.Setup.Fight.ChiefKits);
                Assert.IsTrue(fight.Setup.Fight.ChiefTactics, "his forecast comes with rules 5");
                Assert.IsTrue(fight.Sim.ForecastOn(1));
                Assert.AreEqual(ChiefKit.Bloodridge, fight.Sim.Kit(1));
                ChiefMove moves = fight.Sim.KitMoves(1);
                Assert.AreEqual(c.ChiefMovesThisRun(CampaignTribes.Bloodridge), moves);
                Assert.AreEqual(ChiefKits.Takes, Count(moves), "three of his five");
                Assert.AreEqual(moves, moves & ChiefKits.Pool(ChiefKit.Bloodridge));
                Assert.AreEqual((moves & ChiefMove.Eclipse) != 0 ? ChiefKits.EclipseEveryTicks : 0, fight.Setup.Rules(1).AutoEclipseTicks);
                Assert.IsFalse(fight.MetBefore);
                Assert.AreEqual("Finish it.", fight.ChiefLine(ChiefKits.KneelLine));

                EncounterResult r = CampaignPilot.PlayFight(c, fight, new CampaignPilot(PilotStyle.Reference, seed));
                Assert.IsTrue(c.MetChief(CampaignTribes.BloodridgeId), "you have met him now, win or lose");
                Assert.IsTrue(c.Archive.TryReplay(fight.Setup.FightId, out _, out string failure), failure);
                if (!r.Won) continue;

                // The grade card: the rank you held at the end, more materials for more, and his page keeps your best.
                Assert.IsTrue(r.Graded);
                Assert.IsTrue(r.NewBestGrade);
                Assert.AreEqual(fight.Sim.GloryRankOf(0), r.Grade);
                Assert.AreEqual(t.MaterialsPerChief * (100 + ChiefKits.GradeMaterialsPercent * (int)r.Grade) / 100, r.MaterialsEarned);
                Assert.IsTrue(c.BestGrade(CampaignTribes.BloodridgeId, out GloryRank best));
                Assert.AreEqual(r.Grade, best);
                Assert.IsTrue(r.ChiefCaptured || r.ChiefKilled);

                // The save keeps his page, and the recorded fight still plays back exactly.
                Campaign read = CampaignSave.Read(CampaignSave.Write(c));
                Assert.IsEmpty(read.LoadNote);
                Assert.IsTrue(read.MetChief(CampaignTribes.BloodridgeId));
                Assert.IsTrue(read.BestGrade(CampaignTribes.BloodridgeId, out GloryRank kept));
                Assert.AreEqual(best, kept);
                Assert.IsTrue(read.Archive.TryReplay(fight.Setup.FightId, out _, out failure), failure);
                return;
            }
            Assert.Fail("no chief fight was won in forty campaigns");
        }

        [Test]
        public void AfterYouMeetHimHisKneelLineChanges()
        {
            // Strong enough that giving no orders loses, so he is offered again.
            var t = new CampaignTuning { PathFights = 0, EnemyScaleTier1 = 200 };
            for (ulong seed = 4300; seed < 4320; seed++)
            {
                Campaign c = AtTheChief(seed, new CampaignTuning { PathFights = 0, EnemyScaleTier1 = 40 });
                c = CampaignSave.Read(CampaignSave.Write(c), t);
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Passive, seed));
                if (r.Won || c.Stage != CampaignStage.InRun || c.Offers.Count == 0 || c.Offers[0].Kind != EncounterKind.Chief) continue;
                CampaignFight again = c.StartEncounter(0);
                Assert.IsTrue(again.MetBefore);
                Assert.AreEqual("You again.", again.ChiefLine(ChiefKits.KneelLine));
                return;
            }
            Assert.Fail("giving no orders never lost to him in twenty campaigns");
        }

        [Test]
        public void AKitFightSavedHalfwayOrOnHisKneesResumesExactly()
        {
            // At full strength, so the fight lasts past his first Brand.
            var t = new CampaignTuning { PathFights = 0 };
            for (ulong seed = 4200; seed < 4240; seed++)
            {
                Campaign c = AtTheChief(seed, t);
                CampaignFight fight = c.StartEncounter(0);
                var pilot = new CampaignPilot(PilotStyle.Reference, seed);

                // Halfway: after his first Brand.
                while (!fight.Over && fight.Sim.CurrentTick < ChiefKits.BrandEveryTicks + 30)
                {
                    pilot.Step(fight);
                    fight.Tick();
                }
                if (fight.Over) continue;
                Campaign half = CampaignSave.Read(CampaignSave.Write(c), t);
                Assert.IsEmpty(half.LoadNote);
                Assert.AreEqual(fight.Session.Checksum, half.Active.Session.Checksum);

                // On his knees: the save holds him there, and the verdict still goes.
                while (!fight.Over && !fight.Sim.AwaitingVerdict)
                {
                    pilot.Step(fight);
                    fight.Tick();
                }
                if (!fight.Sim.AwaitingVerdict) continue;
                Campaign knelt = CampaignSave.Read(CampaignSave.Write(c), t);
                Assert.IsEmpty(knelt.LoadNote);
                Assert.IsTrue(knelt.Active.Sim.AwaitingVerdict, "he still kneels");
                Assert.AreEqual(fight.Session.Checksum, knelt.Active.Session.Checksum);
                Assert.IsTrue(knelt.Active.TryOrder(Directive.Hold, null, out string why), why);
                EncounterResult r = knelt.Finish(knelt.Active);
                Assert.IsTrue(r.Won);
                Assert.IsTrue(r.ChiefCaptured, "spared, he and his tribe join you");
                return;
            }
            Assert.Fail("no chief fight reached the kneel in forty campaigns");
        }
    }
}
