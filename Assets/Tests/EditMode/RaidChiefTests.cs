using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The raid chief (Jacob, 25 Oct 2026: "show me in game those boss fights ... do 1 pass"; FightRules.RaidChief,
    /// BattleSimulation.Raid.cs, campaign rules version 10). One quick test per piece, only to prove it runs. Nothing here is
    /// tuning.
    public class RaidChiefTests
    {
        private static UnitSpec U(string id, UnitClass unitClass, int health = 1000)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, 100, health);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        // The Bloodridge chief alone with only his Cleave, under the raid chief.
        private static BattleSimulation Alone(string id, ulong seed)
        {
            var theirs = Side(new UnitSpec("chief", UnitClass.Warden, Row.Front, UnitRole.Chief, 100, 4000));
            var ours = Side(U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman), U("h", UnitClass.Medic));
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.On(NerveRules.Full, NerveRules.Full),
                10, 400, new SideRules(1, 0, true),
                new SideRules(2, 0, false, 0, ChiefSpell.Eclipse, kit: ChiefKit.Bloodridge, moves: ChiefMove.Cleave),
                FightRules.CampaignChiefTactics.WithRaidChief()));
        }

        private static bool Order(SimSession session, string directive)
        {
            return session.TryIssue(Directives.CommandKind, "0", string.Empty, 0, 0, directive, out _);
        }

        // Ticks on until a threat of this name on side 0 has its beat this many ticks off, and returns it.
        private static Threat Beat(SimSession session, BattleSimulation sim, string name, int at = 1)
        {
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                foreach (Threat t in sim.Threats(0))
                    if (t.Name == name && t.BeatTicks == at) return t;
                session.Tick();
            }
            Assert.Fail(name + "'s beat never came");
            return Threat.None;
        }

        [Test]
        public void YourHoldIsFreeAndAParryPaysNoPointsButCracksHisBar()
        {
            BattleSimulation sim = Alone("free", 11);
            var session = new SimSession(sim);
            Assert.IsTrue(sim.RaidChiefOn);
            Beat(session, sim, ChiefKits.CleaveName);
            int points = sim.DirectivePoints(0);
            int nerve = sim.Nerve(1);
            Assert.IsTrue(Order(session, "Hold"));
            Assert.AreEqual(points, sim.DirectivePoints(0), "a Hold costs nothing");
            session.Tick();
            Assert.AreEqual(ChiefKits.CleaveName, sim.Events.Last(e => e.Kind == BattleEventKind.Parry).Detail);
            Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Earn && e.ActorSide == 0 && e.Detail == FightRules.ParryReason),
                "a parry pays no points");
            Assert.Less(sim.Nerve(1), nerve, "it cracks his bar");
        }

        [Test]
        public void MashingHoldNeverRestartsTheParryWindow()
        {
            BattleSimulation sim = Alone("mash", 11);
            var session = new SimSession(sim);
            Beat(session, sim, ChiefKits.CleaveName, FightRules.PerfectHoldTicks + 10);
            Assert.AreEqual(0, sim.HoldRefreshTicksLeft(0), "the first live Hold can start fresh timing");
            Assert.IsTrue(Order(session, "Hold"));
            Assert.AreEqual(RaidRules.MashTicks, sim.HoldRefreshTicksLeft(0), "a second press cannot restart timing immediately");
            // Pressed again 20 ticks later, under a second after the first: the window still runs from the first press.
            for (int i = 0; i < 20; i++) session.Tick();
            Assert.IsTrue(sim.Threats(0).Any(t => t.ParryWindow), "the raw threat window alone would mislead the HUD");
            Assert.IsTrue(sim.CanUse(0, Directive.Hold, out _), "guard is still a valid command");
            Assert.Greater(sim.HoldRefreshTicksLeft(0), 0, "the authoritative timing query distinguishes guard from a fresh parry");
            Assert.IsTrue(Order(session, "Hold"));
            Assert.AreEqual(RaidRules.MashTicks, sim.HoldRefreshTicksLeft(0), "another press restarts the recovery wait");
            Next(session, sim, e => e.Kind == BattleEventKind.Land && e.Detail.StartsWith(ChiefKits.CleaveName, StringComparison.Ordinal) ||
                e.Kind == BattleEventKind.Parry);
            Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Parry), "the mashed Hold did not parry");
        }

        [Test]
        public void HoldRecoveryQueryIsReadOnlyAndExpiresAtTheExistingBoundary()
        {
            var sim = Alone("hold-recovery-readout", 11);
            var session = new SimSession(sim);
            Assert.IsTrue(Order(session, "Hold"));
            string snapshot = sim.OrderedStateSnapshot;
            int events = sim.Events.Count;
            for (int i = 0; i < 100; i++) Assert.AreEqual(RaidRules.MashTicks, sim.HoldRefreshTicksLeft(0));
            Assert.AreEqual(snapshot, sim.OrderedStateSnapshot);
            Assert.AreEqual(events, sim.Events.Count);
            Assert.AreEqual(0, sim.HoldRefreshTicksLeft(1), "only the player's raid Hold has the anti-mash wait");
            for (int i = 0; i < RaidRules.MashTicks - 1; i++) session.Tick();
            Assert.AreEqual(1, sim.HoldRefreshTicksLeft(0));
            session.Tick();
            Assert.AreEqual(0, sim.HoldRefreshTicksLeft(0));
            Assert.AreEqual(0, ClearRoadTests.Fixture(1).HoldRefreshTicksLeft(0), "ordinary road Hold retains its existing timing");
        }

        [Test]
        public void ActiveSaveRestoresHoldRecoveryFromItsRecordedCommands()
        {
            var campaign = Campaign.New(4600, new CampaignTuning { PathFights = 0 });
            Assert.IsTrue(CampaignPilot.PlayTutorial(campaign, PilotStyle.Reference, 4600, 10));
            campaign.ChooseTribe(CampaignTribes.BloodridgeId);
            var fight = campaign.StartEncounter(0);
            for (int i = 0; i < 10; i++) fight.Tick();
            Assert.IsTrue(fight.TryOrder(Directive.Hold, string.Empty, out string why), why);
            for (int i = 0; i < 7; i++) fight.Tick();
            int recovery = fight.Sim.HoldRefreshTicksLeft(0);
            Assert.Greater(recovery, 0);
            var restored = CampaignSave.Read(CampaignSave.Write(campaign), new CampaignTuning { PathFights = 0 });
            Assert.AreEqual(string.Empty, restored.LoadNote);
            Assert.AreEqual(fight.Sim.OrderedStateSnapshot, restored.Active.Sim.OrderedStateSnapshot);
            Assert.AreEqual(recovery, restored.Active.Sim.HoldRefreshTicksLeft(0));
        }

        [Test]
        public void HisBarRefillsLikePostureAndPlainHitsNeverCrackIt()
        {
            BattleSimulation sim = Alone("posture", 11);
            var session = new SimSession(sim);
            Beat(session, sim, ChiefKits.CleaveName);
            Assert.IsTrue(Order(session, "Hold"));
            session.Tick();
            int cracked = sim.Nerve(1);
            Assert.Less(cracked, NerveRules.Full);
            int hp = sim.Units.First(u => u.Role == UnitRole.Chief).Hp;
            for (int i = 0; i < 30; i++) session.Tick();
            Assert.Less(sim.Units.First(u => u.Role == UnitRole.Chief).Hp, hp, "your squad hit him meanwhile");
            int gain = sim.Nerve(1) - cracked;
            Assert.Greater(gain, 0, "his bar refills while nothing answers him");
            Assert.LessOrEqual(gain, RaidRules.PostureRegenPerSecond, "at most PostureRegenPerSecond a second, at full health");
        }

        [Test]
        public void FromRulesTenBloodridgesChiefFightsAloneAndHisFightPlaysBack()
        {
            Assert.GreaterOrEqual(Campaign.RulesVersion, Campaign.RaidChiefRules);
            Campaign c = Campaign.New(4600, new CampaignTuning { PathFights = 0 });
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, 4600, 10));
            c.ChooseTribe(CampaignTribes.BloodridgeId);
            CampaignFight fight = c.StartEncounter(0);
            Assert.AreEqual(EncounterKind.Chief, fight.Encounter.Kind);
            Assert.IsTrue(fight.Setup.Fight.RaidChief);
            Assert.IsTrue(fight.Sim.RaidChiefOn);
            Assert.AreEqual(1, fight.Setup.Side(1).Count, "no band and no reserves");
            Assert.AreEqual(UnitRole.Chief, fight.Setup.Side(1)[0].Role);
            for (int i = 0; i < 200 && !fight.Sim.IsFinished; i++) fight.Session.Tick();
            string checksum = StateChecksum.Compute(fight.Sim.OrderedStateSnapshot);
            Campaign back = CampaignSave.Read(CampaignSave.Write(c), new CampaignTuning { PathFights = 0 });
            Assert.AreEqual(string.Empty, back.LoadNote);
            Assert.AreEqual(checksum, StateChecksum.Compute(back.Active.Sim.OrderedStateSnapshot), "it saves and plays back exactly mid fight");
            CampaignPilot.PlayFight(c, fight, new CampaignPilot(PilotStyle.Reference, 4600));
            Assert.IsTrue(c.Archive.TryReplay(fight.Setup.FightId, out _, out string failure), failure);
        }

        // Ticks on until this happens.
        private static void Next(SimSession session, BattleSimulation sim, Func<BattleEvent, bool> match)
        {
            int seen = sim.Events.Count;
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                session.Tick();
                for (int i = seen; i < sim.Events.Count; i++) if (match(sim.Events[i])) return;
                seen = sim.Events.Count;
            }
            Assert.Fail("it never happened, tick " + sim.CurrentTick);
        }
    }
}
