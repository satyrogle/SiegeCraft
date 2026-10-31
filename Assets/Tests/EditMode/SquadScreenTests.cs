using System;
using System.Linq;
using System.Reflection;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    public class SquadScreenTests
    {
        static BattleSimulation Fight(bool current)
        {
            var ours=new[]{new UnitSpec("leader",UnitClass.Marksman,Row.Back,UnitRole.Pal,20,100),
                new UnitSpec("w",UnitClass.Warden,Row.Front,UnitRole.Fighter,10,400),
                new UnitSpec("h",UnitClass.Medic,Row.Back,UnitRole.Fighter,10,100)};
            var enemy=new[]{new UnitSpec("s",UnitClass.Skirmisher,Row.Front,UnitRole.Fighter,300,9000).WithRoadKit(3,ChiefMove.Cleave|ChiefMove.Brand),
                new UnitSpec("m",UnitClass.Marksman,Row.Back,UnitRole.Fighter,300,9000)};
            var rules=FightRules.Campaign.WithClearRoad();
            return new BattleSimulation(new BattleSetup("leader-screen",9,ours,enemy,new TacticsProfile(),new TacticsProfile(),3000,8,
                NerveRules.On(),10,100,SideRules.None,new SideRules(0,0,false,150),current?rules.WithSquadScreen():rules));
        }
        [Test]
        public void LeaderTakesNoHitsWhileAnAllyStandsThenCanDie()
        {
            var sim=Fight(true); var leader=sim.Units[0]; bool exposed=false;
            while(!sim.IsFinished)
            {
                bool protectedBefore=sim.LeaderProtected(leader); sim.Tick();
                if(protectedBefore&&sim.LeaderProtected(leader)) Assert.AreEqual(leader.MaxHp,leader.Hp);
                if(!sim.LeaderProtected(leader)) exposed=true;
            }
            Assert.IsTrue(exposed);Assert.IsFalse(leader.Alive,"leader is vulnerable alone, not immortal");
            var deaths=sim.Events.Where(e=>e.Kind==BattleEventKind.Kill&&e.TargetId=="leader").Single();
            Assert.IsTrue(sim.Events.Any(e=>e.Kind==BattleEventKind.Kill&&e.TargetId=="w"&&e.Tick<=deaths.Tick));
            Assert.IsTrue(sim.Events.Any(e=>e.Kind==BattleEventKind.Kill&&e.TargetId=="h"&&e.Tick<=deaths.Tick));
        }
        [TestCase("burn")][TestCase("bleed")][TestCase("Eclipse")][TestCase("Cleave")][TestCase("Brand")]
        public void DamageBoundaryProtectsAgainstSplashAndPeriodicHits(string detail)
        {
            var sim=Fight(true);var leader=sim.Units[0];var enemy=sim.Units.Last();
            var damage=typeof(BattleSimulation).GetMethod("ApplyDamage",BindingFlags.Instance|BindingFlags.NonPublic);
            damage.Invoke(sim,new object[]{enemy,1,leader,leader.MaxHp*2,detail,false});
            Assert.AreEqual(leader.MaxHp,leader.Hp);
            Assert.AreEqual(StrikeBlock.SquadProtectsLeader,sim.CanStrike(enemy,leader));
            foreach(var u in sim.Units.Where(u=>u.Side==0&&!u.IsLeader))
                typeof(BattleUnit).GetProperty("Alive").SetValue(u,false);
            damage.Invoke(sim,new object[]{enemy,1,leader,leader.MaxHp*2,detail,false});
            Assert.IsFalse(leader.Alive);
        }
        [Test]
        public void HistoricalRulesThirteenRetainExposedLeader()
        {
            var sim=Fight(false);bool early=false;
            while(!sim.IsFinished)
            {
                sim.Tick(); if(sim.Units[0].Hp<sim.Units[0].MaxHp&&sim.Units.Any(u=>u.Side==0&&!u.IsLeader&&u.Alive))early=true;
            }
            Assert.IsTrue(early);Assert.IsFalse(sim.SquadScreenOn);
            Assert.IsFalse(sim.Events.Any(e=>e.Kind==BattleEventKind.ComboFinish));
        }
        [Test]
        public void FledOrBenchedAlliesCannotProvideProtection()
        {
            var sim=Fight(true);var leader=sim.Units[0];
            typeof(BattleUnit).GetProperty("Fled").SetValue(sim.Units[1],true);
            typeof(BattleUnit).GetProperty("Benched").SetValue(sim.Units[2],true);
            Assert.IsFalse(sim.LeaderProtected(leader));
            typeof(BattleUnit).GetProperty("Benched").SetValue(sim.Units[2],false);
            typeof(BattleUnit).GetProperty("PanicTicks").SetValue(sim.Units[2],90);
            Assert.IsTrue(sim.LeaderProtected(leader),"frozen ally is still standing");
        }
        [Test]
        public void NewCampaignHasFirstFightParryAndVersionedSaveReplay()
        {
            var c=Campaign.New(4600);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c,PilotStyle.Reference,4600,10));
            c.ChooseTribe(CampaignTribes.BloodridgeId);var f=c.StartEncounter(0);
            Assert.AreEqual(Campaign.RulesVersion,f.RulesVersion);Assert.IsTrue(f.Sim.SquadScreenOn);
            Assert.Greater(f.Setup.Rules(1).HeavyEveryTicks,0);
            for(int i=0;i<200&&!f.Over;i++)f.Tick();
            var back=CampaignSave.Read(CampaignSave.Write(c));
            Assert.AreEqual("",back.LoadNote);Assert.IsTrue(back.Active.Sim.SquadScreenOn);
            Assert.AreEqual(f.Sim.OrderedStateSnapshot,back.Active.Sim.OrderedStateSnapshot);
        }
        [Test]
        public void TeamworkNamesActualDistinctParticipantsAndSurvivesReplay()
        {
            var old=ClearRoadTests.Fixture(4).Setup;
            var setup=new BattleSetup(old.FightId,old.Seed,old.Side(0),old.Side(1),old.CopyTactics(0),old.CopyTactics(1),
                2400,8,NerveRules.On(),10,400,old.Rules(0),old.Rules(1),old.Fight.WithSquadScreen());
            var sim=new BattleSimulation(setup);var session=new SimSession(sim);
            while(!sim.IsFinished)session.Tick();
            var finishes=sim.Events.Where(e=>e.Kind==BattleEventKind.ComboFinish).ToArray();Assert.IsNotEmpty(finishes);
            Assert.AreEqual(BattleEventKind.End,sim.Events.Last().Kind,"finishing teamwork never follows the terminal event");
            foreach(var finish in finishes)
            {
                var origin=sim.Events.Single(e=>e.Kind==BattleEventKind.ComboSetup&&e.Tick==finish.Tick&&e.Amount==finish.Amount&&e.Detail==finish.Detail);
                Assert.AreEqual(origin.TargetId,finish.TargetId);Assert.AreNotEqual(origin.ActorId,finish.ActorId);
                Assert.IsTrue(sim.Events.Any(e=>e.Kind==BattleEventKind.Hit&&e.Tick==finish.Tick&&e.ActorId==finish.ActorId&&e.TargetId==finish.TargetId));
            }
            var archive=new FightArchive();var fight=archive.Add(setup,session.Log,sim);
            Assert.IsTrue(archive.TryReplay(fight.FightId,out var replay,out var why),why);
            Assert.AreEqual(sim.OrderedStateSnapshot,replay.OrderedStateSnapshot);
            Assert.AreEqual(finishes.Length,replay.Events.Count(e=>e.Kind==BattleEventKind.ComboFinish));
        }
    }
}
