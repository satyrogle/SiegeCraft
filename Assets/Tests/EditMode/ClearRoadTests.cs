using System;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    public class ClearRoadTests
    {
        static UnitSpec U(string id,UnitClass c)=>new UnitSpec(id,c,SliceClasses.Stats(c).DefaultRow,UnitRole.Fighter,100,9000);
        internal static BattleSimulation Fixture(int stage)
        {
            var ours=new[]{U("w",UnitClass.Warden),U("s",UnitClass.Skirmisher),U("t",UnitClass.Trapper),U("m",UnitClass.Marksman),U("h",UnitClass.Medic)};
            var theirs=new[]{U("xw",UnitClass.Warden).WithRoadKit(1,ChiefMove.Cleave|ChiefMove.Brand),U("xs",UnitClass.Skirmisher),U("xt",UnitClass.Trapper),U("xm",UnitClass.Marksman),U("xh",UnitClass.Medic)};
            return new BattleSimulation(new BattleSetup("clear-road",9,ours,theirs,new TacticsProfile(),new TacticsProfile(),
                2400,8,NerveRules.On(NerveRules.Full,NerveRules.Full),10,400,new SideRules(1,0,true),new SideRules(0,0,false,150),FightRules.Campaign.WithClearRoad(stage)));
        }
        [TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)]
        public void SystemsArriveInOrderAndEnemiesKeepTheSimpleKit(int stage)
        {
            var sim=Fixture(stage);var session=new SimSession(sim);
            while(!sim.IsFinished)session.Tick();
            Assert.IsTrue(sim.Events.Any(e=>e.ActorSide==0&&e.Kind==BattleEventKind.Cash),"player combos still hit");
            Assert.IsFalse(sim.Events.Any(e=>e.ActorSide==1&&(e.Kind==BattleEventKind.Boost||e.Kind==BattleEventKind.Chain||e.Kind==BattleEventKind.Defence)),"enemy does not run the player's reactive kit");
            Assert.IsTrue(sim.Events.Any(e=>e.ActorSide==1&&e.Kind==BattleEventKind.Hit),"enemy still attacks");
            Assert.IsTrue(sim.Events.Any(e=>e.Kind==BattleEventKind.Mark&&e.Detail==BurnRules.Mark),"Bloodridge Burn remains");
            Assert.AreEqual(stage>=2,sim.Events.Any(e=>e.Kind==BattleEventKind.Defence),"defence progression");
            Assert.AreEqual(stage>=3,sim.Events.Any(e=>e.Kind==BattleEventKind.Boost),"boost progression including Steady's separate path");
            Assert.AreEqual(stage>=4,sim.Events.Any(e=>e.Kind==BattleEventKind.Tell&&e.Detail==ChiefKits.CleaveName),"carrier progression");
            var replay=new BattleSimulation(sim.Setup);var again=new SimSession(replay);
            while(!replay.IsFinished)again.Tick();
            Assert.AreEqual(sim.OrderedStateSnapshot,replay.OrderedStateSnapshot);
        }
        [Test]
        public void NewRoadHasFourSquadFightsAndLayoutSurvivesSave()
        {
            var c=Campaign.New(4600,new CampaignTuning{RulesCap=13});
            Assert.IsTrue(CampaignPilot.PlayTutorial(c,PilotStyle.Reference,4600,10));
            c.ChooseTribe(CampaignTribes.BloodridgeId);
            Assert.AreEqual(4,c.Road.Rows);
            Assert.IsTrue(c.Road.All.All(n=>n.Kind==RoadNodeKind.Fight&&n.Fight==EncounterKind.WarParty));
            var f=c.StartEncounter(0);Assert.AreEqual(1,f.Sim.RoadStage);
            Assert.AreEqual(0,f.Setup.Rules(1).HeavyEveryTicks,"first step leaves attention on squad combos");
            for(int i=0;i<200&&!f.Over;i++)f.Tick();
            var back=CampaignSave.Read(CampaignSave.Write(c));
            Assert.AreEqual("",back.LoadNote);Assert.AreEqual(4,back.Road.Rows);
            Assert.AreEqual(f.Sim.OrderedStateSnapshot,back.Active.Sim.OrderedStateSnapshot);
            CampaignPilot.PlayFight(c,f,new CampaignPilot(PilotStyle.Reference,4600));
            Assert.IsTrue(c.Archive.TryReplay(f.Setup.FightId,out var replay,out var why),why);
            Assert.AreEqual(f.Sim.OrderedStateSnapshot,replay.OrderedStateSnapshot);
        }
        [TestCase(true,0,400,false)]
        [TestCase(true,0,200,true)]
        [TestCase(true,1,400,true)]
        [TestCase(false,0,400,true)]
        public void FrenzyRequiresQuarterHealthForThePlayerOnly(bool rules13,int side,int hpPermille,bool expected)
        {
            var a=new[]{new UnitSpec("a",UnitClass.Warden,Row.Front,UnitRole.Fighter,10,9000)};
            var b=new[]{new UnitSpec("b",UnitClass.Warden,Row.Front,UnitRole.Fighter,10,9000)};
            var sim=new BattleSimulation(new BattleSetup("frenzy-threshold",7,a,b,new TacticsProfile(),new TacticsProfile(),600,8,
                NerveRules.On(NerveRules.Full,NerveRules.Full),10,400,SideRules.None,SideRules.None,rules13?FightRules.Campaign.WithClearRoad():FightRules.Campaign.WithRoadKit()));
            var unit=sim.Units.Single(u=>u.Side==side);
            // Arrange a precise exhausted-nerve boundary; the next real hit, through Tick, must decide the response.
            typeof(BattleUnit).GetProperty("Hp").SetValue(unit,unit.MaxHp*hpPermille/1000);
            typeof(BattleUnit).GetProperty("UnitNerve").SetValue(unit,1);
            var session=new SimSession(sim);for(int i=0;i<180&&!sim.IsFinished;i++)session.Tick();
            Assert.IsTrue(sim.Events.Any(e=>e.Kind==BattleEventKind.Hit&&e.TargetId==unit.Id),"fixture received real hits");
            Assert.AreEqual(expected,unit.Frenzied);
        }
        [Test]
        public void RulesTwelveActiveRoadKeepsItsLayoutAndSnapshot()
        {
            var tuning=new CampaignTuning{RulesCap=12};var c=Campaign.New(4600,tuning);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c,PilotStyle.Reference,4600,10));c.ChooseTribe(CampaignTribes.BloodridgeId);
            var f=c.StartEncounter(0);for(int i=0;i<200&&!f.Over;i++)f.Tick();
            var old=CampaignSave.Write(c).Replace(CampaignSave.Header+"\t"+CampaignSave.Version,CampaignSave.Header+"\t21").Replace("clearroad\t0\n","");
            var back=CampaignSave.Read(old);
            Assert.AreEqual("",back.LoadNote);Assert.AreEqual(3,back.Road.Rows);
            Assert.AreEqual(12,back.Active.RulesVersion);Assert.IsFalse(back.Active.Sim.ClearRoadOn);
            Assert.AreEqual(f.Sim.OrderedStateSnapshot,back.Active.Sim.OrderedStateSnapshot);
        }
        [Test]
        public void LongRoadStillAddsOneStepToTheNewRoad()
        {
            var c=Campaign.New(4600);Assert.IsTrue(CampaignPilot.PlayTutorial(c,PilotStyle.Reference,4600,10));
            Assert.IsTrue(c.TrySetOmen(Omen.LongRoad,true,out var why),why);c.ChooseTribe(CampaignTribes.BloodridgeId);
            Assert.AreEqual(5,c.PathLength);Assert.AreEqual(5,CampaignSave.Read(CampaignSave.Write(c)).Road.Rows);
        }
    }
}
