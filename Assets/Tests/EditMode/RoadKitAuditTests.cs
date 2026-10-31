using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    public class RoadKitAuditTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void FallBackAlsoStopsAnAlreadySteadyPiercingShot(bool rules13)
        {
            UnitSpec U(string id,UnitClass c)=>new UnitSpec(id,c,SliceClasses.Stats(c).DefaultRow,UnitRole.Fighter,100,6000);
            var ours=new List<UnitSpec>{U("w",UnitClass.Warden),U("m",UnitClass.Marksman)};
            var theirs=new List<UnitSpec>{U("xw",UnitClass.Warden),U("xm",UnitClass.Marksman)};
            var sim=new BattleSimulation(new BattleSetup("steady-dodge-audit",9,ours,theirs,new TacticsProfile(),new TacticsProfile(),
                BattleSetup.DefaultTickLimit,BattleSetup.DefaultDirectivePoints,NerveRules.On(NerveRules.Full,NerveRules.Full),
                10,400,new SideRules(1,0,true),new SideRules(0,0,false),rules13?FightRules.Campaign.WithClearRoad():FightRules.Campaign.WithRoadKit()));
            var session=new SimSession(sim);
            var marksman=sim.Units.Single(u=>u.Id=="m");
            while(!sim.IsFinished&&sim.CurrentTick<1200&&!(marksman.Steady&&marksman.AimTicks>0))session.Tick();
            Assert.IsTrue(marksman.Steady&&marksman.AimTicks>0,"fixture reaches a real Steady aim");
            Assert.IsTrue(session.TryIssue(Directives.CommandKind,"0","",0,0,"FallBack",out var why),why);
            int from=sim.Events.Count;
            while(marksman.AimTicks>0&&sim.FallBackTicksLeft(0)>1&&!sim.IsFinished)session.Tick();
            Assert.AreEqual(0,marksman.AimTicks,"shot resolves during the dodge");
            var hits=sim.Events.Skip(from).Where(e=>e.Kind==BattleEventKind.Hit&&e.ActorId=="m").ToList();
            if(rules13) Assert.IsEmpty(hits,"Fall Back must suppress the whole shot, including Pierce: "+string.Join(",",hits.Select(e=>e.Detail)));
            else Assert.IsTrue(hits.Any(e=>e.Detail==BoostRules.PierceDetail),"rules 12 keeps historical Pierce for exact replay");
        }
    }
}
