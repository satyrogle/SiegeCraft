using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Road combat (Jacob, 25 Oct 2026: "go, build rules 11 in the sim as written in 11263, 11358 and 11406";
    /// FightRules.RoadCombat, BattleSimulation.Road.cs, Chains.cs and Defence.cs; campaign rules version 11). One quick test
    /// per piece, only to prove it runs. Nothing here is tuning.
    public class RoadCombatTests
    {
        private static UnitSpec U(string id, UnitClass unitClass, int health = 1000)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, 100, health);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, SideRules theirRules = null,
            List<BondPair> bonds = null)
        {
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.On(NerveRules.Full, NerveRules.Full),
                10, 400, new SideRules(1, 0, true), theirRules ?? new SideRules(0, 0, false), FightRules.Campaign.WithRoadCombat(),
                bonds));
        }

        private static void PlayOut(BattleSimulation sim)
        {
            var session = new SimSession(sim);
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit) session.Tick();
        }

        [Test]
        public void ARoadFightEndsOnlyWhenOneSideHasNobodyLeftFighting()
        {
            for (ulong seed = 1; seed <= 6; seed++)
            {
                BattleSimulation sim = Fight("end" + seed, seed,
                    Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher), U("t", UnitClass.Trapper), U("m", UnitClass.Marksman), U("h", UnitClass.Medic)),
                    Side(U("x.w", UnitClass.Warden), U("x.s", UnitClass.Skirmisher), U("x.m", UnitClass.Marksman), U("x.h", UnitClass.Medic)));
                Assert.IsTrue(sim.UnitNerveOn);
                PlayOut(sim);
                Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Break), "no side breaks on a shared bar");
                if (sim.CurrentTick >= BattleSetup.DefaultTickLimit) continue;
                Assert.IsTrue(sim.StillFighting(0) == 0 || sim.StillFighting(1) == 0, "the fight ended with one side gone");
                foreach (BattleEvent e in sim.Events.Where(e => e.Kind == BattleEventKind.Snap && e.Detail == RoadRules.Fled))
                    Assert.AreEqual(1, e.ActorSide, "only their side flees");
                foreach (BattleEvent e in sim.Events.Where(e => e.Kind == BattleEventKind.Snap && e.Detail == RoadRules.Frenzy))
                    Assert.IsFalse(SliceClasses.Stats((UnitClass)e.ActorClass).Ranged, "only melee fighters frenzy");
                foreach (BattleEvent e in sim.Events.Where(e => e.Kind == BattleEventKind.Snap && e.Detail == RoadRules.Freeze))
                    Assert.AreEqual(0, e.ActorSide, "only yours freeze");
            }
        }

        [Test]
        public void ASnareReadiesItsMarksmanAndABondedOneShootsWithoutAiming()
        {
            List<UnitSpec> ours = Side(U("w", UnitClass.Warden, 4000), U("t", UnitClass.Trapper), U("m", UnitClass.Marksman));
            BattleSimulation plain = Fight("chain", 3, ours, Side(U("x.s", UnitClass.Skirmisher, 4000)));
            PlayOut(plain);
            BattleEvent react = plain.Events.FirstOrDefault(e => e.Kind == BattleEventKind.Chain && e.ActorId == "m");
            Assert.IsNotNull(react.Detail, "the Marksman reacted to a snare");
            StringAssert.StartsWith(ClassSpells.SnareMark, react.Detail);

            BattleSimulation bonded = Fight("bonded", 3, ours, Side(U("x.s", UnitClass.Skirmisher, 4000)),
                bonds: new List<BondPair> { new BondPair("t", "m") });
            PlayOut(bonded);
            Assert.IsTrue(bonded.Events.Any(e => e.Kind == BattleEventKind.Chain && e.ActorId == "m" &&
                e.Detail == ClassSpells.SnareMark + "." + ChainRules.Bonded), "a bonded Marksman answers his Trapper's snare");
        }

        [Test]
        public void ABandsHeavySwingMeetsABraceOrASidestepAndTheBraceOpensTheSwinger()
        {
            var heavies = new SideRules(0, 0, false, HeavyRules.EveryTicks);
            BattleSimulation sim = Fight("brace", 5, Side(U("w", UnitClass.Warden, 4000), U("s", UnitClass.Skirmisher, 4000)),
                Side(U("x.w", UnitClass.Warden, 4000), U("x.s", UnitClass.Skirmisher, 4000)), heavies);
            PlayOut(sim);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Defence && (e.Detail == DefenceRules.Brace || e.Detail == DefenceRules.Sidestep)),
                "a heavy swing met a defence move");
            BattleEvent brace = sim.Events.FirstOrDefault(e => e.Kind == BattleEventKind.Defence && e.Detail == DefenceRules.Brace && e.ActorSide == 0);
            if (brace.Detail != null)
                Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Mark && e.Tick == brace.Tick && e.ActorId == brace.ActorId &&
                    e.Detail == ClassSpells.OpeningMark), "his Brace left the swinger an Opening");
        }

        [Test]
        public void FromRulesElevenALostFightCostsALifeAndYourSquadComesBack()
        {
            Assert.GreaterOrEqual(Campaign.RulesVersion, Campaign.RoadCombatRules);
            var hard = new CampaignTuning { EnemyScaleTier1 = 300 };
            for (ulong seed = 900; seed < 940; seed++)
            {
                Campaign c = CampaignSave.Read(CampaignSave.Write(CampaignForTest(seed)), hard);
                c.ChooseTribe(CampaignTribes.BloodridgeId);
                Assert.AreEqual(Campaign.RunLives, c.Lives);
                int squad = c.Reserve.Count;
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Passive, seed));
                if (r.Won) continue;
                Assert.AreEqual(Campaign.RunLives - 1, c.Lives, "a lost fight costs a life");
                Assert.AreEqual(Campaign.RunLives - 1, r.LivesLeft);
                Assert.AreEqual(squad, c.Reserve.Count, "nobody of yours is lost or captured");
                Assert.IsNull(r.Captured);
                CollectionAssert.Contains(c.Grudges, CampaignTribes.BloodridgeId, "your Grudge fighters hit the tribe that took it");
                Campaign back = CampaignSave.Read(CampaignSave.Write(c), hard);
                Assert.AreEqual(c.Lives, back.Lives, "the lives survive closing the game");
                for (int tries = 0; tries < 10 && c.Stage == CampaignStage.InRun; tries++)
                    r = CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Passive, seed + (ulong)tries));
                if (r.Won) continue;
                Assert.IsTrue(r.RunOver, "the run ends with no lives left");
                Assert.AreEqual(0, r.LivesLeft);
                return;
            }
            Assert.Fail("no fight was lost in forty campaigns");
        }

        [Test]
        public void ARulesElevenRoadFightSavesAndPlaysBackExactly()
        {
            Campaign c = CampaignForTest(4600);
            c.ChooseTribe(CampaignTribes.BloodridgeId);
            CampaignFight fight = c.StartEncounter(0);
            Assert.IsTrue(fight.Setup.Fight.RoadCombat);
            Assert.IsTrue(fight.Sim.RoadCombatOn);
            for (int i = 0; i < 200 && !fight.Sim.IsFinished; i++) fight.Session.Tick();
            string checksum = StateChecksum.Compute(fight.Sim.OrderedStateSnapshot);
            Campaign back = CampaignSave.Read(CampaignSave.Write(c));
            Assert.AreEqual(string.Empty, back.LoadNote);
            Assert.AreEqual(checksum, StateChecksum.Compute(back.Active.Sim.OrderedStateSnapshot), "it saves and plays back exactly mid fight");
            CampaignPilot.PlayFight(c, fight, new CampaignPilot(PilotStyle.Reference, 4600));
            Assert.IsTrue(c.Archive.TryReplay(fight.Setup.FightId, out _, out string failure), failure);
        }

        private static Campaign CampaignForTest(ulong seed)
        {
            Campaign c = Campaign.New(seed);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }
    }
}
