using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The road kit (Jacob, 25 Oct 2026: "alright bro lets keep it as dodge and build these 5 lets go"; FightRules.RoadKit,
    /// BattleSimulation.Boosts.cs, Burn.cs and Carriers.cs; campaign rules version 12). One quick test per piece, only to prove
    /// it runs. Nothing here is tuning.
    public class RoadKitTests
    {
        private static UnitSpec U(string id, UnitClass unitClass, int health = 1000)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, 100, health);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        private static List<UnitSpec> Ours()
        {
            return Side(U("w", UnitClass.Warden, 3000), U("s", UnitClass.Skirmisher, 3000), U("t", UnitClass.Trapper, 3000),
                U("m", UnitClass.Marksman, 3000), U("h", UnitClass.Medic, 3000));
        }

        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs)
        {
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.On(NerveRules.Full, NerveRules.Full),
                10, 400, new SideRules(1, 0, true), new SideRules(0, 0, false), FightRules.Campaign.WithRoadKit()));
        }

        private static void PlayOut(BattleSimulation sim)
        {
            var session = new SimSession(sim);
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit) session.Tick();
        }

        [Test]
        public void WhileYouFallBackNobodyOfYoursAttacks()
        {
            BattleSimulation sim = Fight("dodge", 7, Ours(), Side(U("x.w", UnitClass.Warden, 3000), U("x.m", UnitClass.Marksman, 3000)));
            Assert.IsTrue(sim.RoadKitOn);
            var session = new SimSession(sim);
            for (int i = 0; i < 60; i++) session.Tick();
            Assert.IsTrue(session.TryIssue(Directives.CommandKind, "0", string.Empty, 0, 0, "FallBack", out string failure), failure);
            int from = sim.Events.Count;
            for (int i = 0; i < 30; i++) session.Tick();
            Assert.IsFalse(sim.Events.Skip(from).Any(e => e.Kind == BattleEventKind.Hit && e.ActorSide == 0 &&
                sim.Units.Any(u => u.Id == e.ActorId && u.Side == 0)), "nobody of yours struck while falling back");
        }

        [Test]
        public void EveryClassBoostsOnItsOwn()
        {
            BattleSimulation sim = Fight("boost", 9, Ours(),
                Side(U("x.w", UnitClass.Warden, 3000), U("x.s", UnitClass.Skirmisher, 3000), U("x.m", UnitClass.Marksman, 3000)));
            PlayOut(sim);
            var moves = sim.Events.Where(e => e.Kind == BattleEventKind.Boost && e.ActorSide == 0).Select(e => e.Detail).Distinct().ToList();
            TestContext.WriteLine(string.Join(", ", moves));
            Assert.Contains(BoostRules.ShieldBash, moves);
            Assert.Contains(BoostRules.TwinCut, moves);
            Assert.Contains(BoostRules.Steady, moves);
        }

        [Test]
        public void ABurningFightersBlowsBurnAndAnAimedShotFlaresIt()
        {
            List<UnitSpec> ours = Side(U("w", UnitClass.Warden, 3000).WithRoadKit(BurnRules.StackPercent, ChiefMove.None),
                U("m", UnitClass.Marksman, 3000));
            BattleSimulation sim = Fight("burn", 11, ours, Side(U("x.w", UnitClass.Warden, 6000)));
            PlayOut(sim);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Mark && e.ActorId == "w" && e.Detail == BurnRules.Mark), "his blows burn");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == BurnRules.Detail), "the burn ticks");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Cash && e.ActorId == "m" && e.Detail == BurnRules.FlareDetail),
                "an Aimed Shot flares it");
        }

        [Test]
        public void ARoadFighterCarriesHisChiefsMoves()
        {
            List<UnitSpec> theirs = Side(U("x.w", UnitClass.Warden, 3000).WithRoadKit(0, ChiefMove.Cleave | ChiefMove.Brand),
                U("x.s", UnitClass.Skirmisher, 3000).WithRoadKit(0, ChiefMove.BurningRing));
            BattleSimulation sim = Fight("carry", 13, Ours(), theirs);
            PlayOut(sim);
            foreach (string move in new[] { ChiefKits.CleaveName, ChiefKits.BrandName, ChiefKits.RingName })
                Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Tell && e.ActorSide == 1 && e.Detail == move), move + " was told");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Land && e.ActorSide == 1 && e.Detail.StartsWith(ChiefKits.CleaveName,
                StringComparison.Ordinal)), "a Cleave landed or was parried");
        }

        [Test]
        public void FromRulesTwelveFightsWonInARowMultiplyTheirMaterials()
        {
            Assert.AreEqual(12, Campaign.RoadKitRules);
            var easy = new CampaignTuning { EnemyScaleTier1 = 40, RulesCap = Campaign.RoadKitRules };
            for (ulong seed = 800; seed < 830; seed++)
            {
                Campaign c = Campaign.New(seed, easy);
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) continue;
                c.ChooseTribe(CampaignTribes.BloodridgeId);
                Assert.AreEqual(0, c.Streak);
                EncounterResult first = CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Reference, seed));
                if (!first.Won || c.Stage != CampaignStage.InRun) continue;
                Assert.AreEqual(1, first.Streak);
                EncounterResult second = CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Reference, seed + 1));
                if (!second.Won) continue;
                Assert.AreEqual(2, second.Streak, "a second win in a row");
                Assert.AreEqual(2, CampaignSave.Read(CampaignSave.Write(c), easy).Streak, "the streak survives closing the game");
                return;
            }
            Assert.Fail("no two fights were won in a row in thirty campaigns");
        }

        [Test]
        public void ABloodridgeRoadFightCarriesAndBurnsAndPlaysBack()
        {
            Campaign c = Campaign.New(4600,new CampaignTuning{RulesCap=12});
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, 4600, 10));
            c.ChooseTribe(CampaignTribes.BloodridgeId);
            CampaignFight fight = c.StartEncounter(0);
            Assert.IsTrue(fight.Setup.Fight.RoadKit);
            Assert.AreNotEqual(EncounterKind.Chief, fight.Encounter.Kind);
            Assert.IsTrue(fight.Setup.Side(1).Any(s => s.Carries != ChiefMove.None), "their road fighters carry");
            Assert.IsTrue(fight.Setup.Side(1).All(s => s.BurnPercent > 0), "Bloodridge's fighters burn");
            for (int i = 0; i < 200 && !fight.Sim.IsFinished; i++) fight.Session.Tick();
            string checksum = StateChecksum.Compute(fight.Sim.OrderedStateSnapshot);
            Campaign back = CampaignSave.Read(CampaignSave.Write(c));
            Assert.AreEqual(string.Empty, back.LoadNote);
            Assert.AreEqual(checksum, StateChecksum.Compute(back.Active.Sim.OrderedStateSnapshot), "it saves and plays back exactly mid fight");
            CampaignPilot.PlayFight(c, fight, new CampaignPilot(PilotStyle.Reference, 4600));
            Assert.IsTrue(c.Archive.TryReplay(fight.Setup.FightId, out _, out string failure), failure);
        }
    }
}
