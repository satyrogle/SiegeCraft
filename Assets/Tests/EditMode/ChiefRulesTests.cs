using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The chief rules (Jacob, 20 Oct 2026, later: chiefs harder "by adding more rules and gameplay"; ChiefFight). One
    /// quick test each, only to prove they run.
    public class ChiefRulesTests
    {
        private static UnitSpec U(string id, UnitClass unitClass)
        {
            return new UnitSpec(id, unitClass);
        }

        private static UnitSpec Chief(string id, UnitClass unitClass, int healthPercent = 100)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Chief, 100, healthPercent);
        }

        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, NerveRules nerve = null,
            SideRules theirRules = null)
        {
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, nerve ?? NerveRules.Off, 10, 400, null, theirRules,
                FightRules.CampaignChief));
        }

        private static bool Order(SimSession session, string directive, string target = "")
        {
            return session.TryIssue(Directives.CommandKind, "0", target, 0, 0, directive, out _);
        }

        [Test]
        public void AChiefYieldsAtAQuarterAndHoldSparesHimSoHisNerveBreaks()
        {
            var ours = new List<UnitSpec> { U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("c", UnitClass.Marksman) };
            var theirs = new List<UnitSpec> { Chief("chief", UnitClass.Skirmisher), U("x", UnitClass.Warden) };
            var sim = Fight("yield", 31, ours, theirs, NerveRules.On());
            var session = new SimSession(sim);
            while (!sim.IsFinished && sim.CurrentTick < 3000 && !sim.Events.Any(e => e.Kind == BattleEventKind.Yield)) session.Tick();
            BattleUnit chief = sim.FindById("chief");
            Assert.IsTrue(chief.Yielded, "he never yielded");
            Assert.LessOrEqual(chief.Hp * 100, chief.MaxHp * ChiefFight.YieldPercent);

            // We hold until their nerve breaks: none of ours strikes him, and he does nothing more.
            int from = sim.Events.Count;
            while (!sim.IsFinished && sim.CurrentTick < 3000)
            {
                if (sim.HoldTicksLeft(0) == 0) Order(session, "Hold");
                session.Tick();
            }
            var after = sim.Events.Skip(from).ToList();
            Assert.IsFalse(after.Any(e => e.ActorSide == 0 && e.ActorId.Length > 0 && e.TargetId == "chief" &&
                (e.Kind == BattleEventKind.Hit || e.Kind == BattleEventKind.Miss)), "spared while we hold");
            Assert.IsFalse(after.Any(e => e.ActorId == "chief"), "a kneeling chief does nothing");
            Assert.AreEqual(1, sim.BrokenSide, "their nerve broke");
            Assert.IsTrue(chief.Alive, "with him still standing: a capture");
        }

        [Test]
        public void HisBandGuardsHimTreatsHimFirstAndHeShrugsOffMarks()
        {
            // His Warden guards him.
            var guard = Fight("bodyguard", 32, new List<UnitSpec> { U("a", UnitClass.Marksman) },
                new List<UnitSpec> { U("w", UnitClass.Warden), U("m", UnitClass.Medic), Chief("chief", UnitClass.Marksman) });
            while (guard.CurrentTick < 120) guard.Tick();
            Assert.AreEqual("w", guard.CoveredBy(guard.FindById("chief")).Id, "his Warden guards him, not his Medic");

            // His Medic treats him first while he is hurt, and our Aimed Shots leave no Opening on him while his band stands.
            var sim = Fight("bodyguard.medic", 33, new List<UnitSpec> { U("a", UnitClass.Marksman), U("k", UnitClass.Skirmisher) },
                new List<UnitSpec> { U("m", UnitClass.Medic), Chief("chief", UnitClass.Marksman) });
            var session = new SimSession(sim);
            BattleUnit chief = sim.FindById("chief"), medic = sim.FindById("m");
            int treatedHim = 0, aimedAtHim = 0;
            while (!sim.IsFinished && sim.CurrentTick < 2400 && medic.Alive)
            {
                if (sim.FocusTicksLeft(0) == 0) Order(session, "FocusFire", "chief");
                bool hurt = chief.Alive && chief.HpPermille < BattleSimulation.MedicHealsBelowPermille;
                int before = sim.Events.Count;
                session.Tick();
                foreach (BattleEvent e in sim.Events.Skip(before))
                {
                    if (e.Kind == BattleEventKind.Heal && e.ActorId == "m" && hurt)
                    {
                        Assert.AreEqual("chief", e.TargetId, "she treats him first");
                        treatedHim++;
                    }
                    if (e.Kind == BattleEventKind.Hit && e.Detail.StartsWith(ClassSpells.AimedDetail) && e.TargetId == "chief") aimedAtHim++;
                    if (e.Kind == BattleEventKind.Mark && e.TargetId == "chief" && medic.Alive)
                        Assert.Fail("a mark took on him while his band stood: " + e.Detail);
                }
            }
            Assert.Greater(treatedHim, 0, "she treated him");
            Assert.Greater(aimedAtHim, 0, "Aimed Shots hit him");
        }

        [Test]
        public void HisSecondPhaseBringsHisReservesAndHisSpellAgain()
        {
            var ours = new List<UnitSpec> { U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("c", UnitClass.Marksman) };
            var theirs = new List<UnitSpec>
            {
                U("x", UnitClass.Warden), Chief("chief", UnitClass.Skirmisher),
                U("r1", UnitClass.Skirmisher).AsReserve(), U("r2", UnitClass.Marksman).AsReserve()
            };
            var sim = Fight("phase", 34, ours, theirs, null, new SideRules(2, 3000, false));
            BattleUnit r1 = sim.FindById("r1"), r2 = sim.FindById("r2"), chief = sim.FindById("chief");
            Assert.IsTrue(r1.Benched && !r1.Alive && r2.Benched && !r2.Alive, "his reserves wait out of the fight");
            Assert.AreEqual(2, sim.AliveCount(1));

            int at = -1;
            while (!sim.IsFinished && sim.CurrentTick < 3000 && at < 0)
            {
                sim.Tick();
                for (int i = 0; i < sim.Events.Count; i++) if (sim.Events[i].Kind == BattleEventKind.Phase) at = i;
            }
            Assert.GreaterOrEqual(at, 0, "his second phase never came");
            BattleEvent phase = sim.Events[at];
            Assert.AreEqual("chief", phase.ActorId);
            Assert.AreEqual(2, phase.Amount);
            CollectionAssert.AreEquivalent(new[] { "r1", "r2" },
                sim.Events.Where(e => e.Kind == BattleEventKind.Join).Select(e => e.TargetId).ToList());
            Assert.IsFalse(r1.Benched || r2.Benched, "they joined");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Windup && e.ActorId == "chief" && e.Tick == phase.Tick),
                "he started his spell again at once");
            Assert.IsTrue(chief.PhaseDone);
        }
    }
}
