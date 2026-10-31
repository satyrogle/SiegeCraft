using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        static UnitSpec TacticsUnit(string id, UnitClass c, int power = 100, int health = 1000,
            UnitRole role = UnitRole.Fighter) => new UnitSpec(id, c, SliceClasses.Stats(c).DefaultRow, role, power, health);

        ArchivedFight TacticsFixture(int scenario, bool parryCenter = false)
        {
            bool skull = scenario == 2 || scenario == 3 || scenario == 5;
            ChiefMove moves = scenario == 0 ? ChiefMove.Cleave | ChiefMove.Brand | ChiefMove.Eclipse :
                scenario == 1 ? ChiefMove.BurningRing | ChiefMove.SunMantle | ChiefMove.Brand :
                scenario == 2 ? ChiefMove.SteelStorm | ChiefMove.Hook | ChiefMove.RifleLine :
                scenario == 3 ? ChiefMove.IronAdvance | ChiefMove.Overclock | ChiefMove.RifleLine :
                scenario == 5 ? ChiefMove.Hook : scenario == 6 ? ChiefMove.Cleave : ChiefKits.FirstWritten;
            var ours = new List<UnitSpec> { TacticsUnit("a",UnitClass.Skirmisher), TacticsUnit("b",UnitClass.Warden),
                TacticsUnit("c",UnitClass.Marksman), TacticsUnit("d",UnitClass.Medic) };
            var theirs = new List<UnitSpec> { TacticsUnit("x",UnitClass.Warden),TacticsUnit("y",UnitClass.Marksman),
                TacticsUnit("z",UnitClass.Medic), TacticsUnit("enemy.chief",skull?UnitClass.Trapper:UnitClass.Marksman,100,1000,UnitRole.Chief) };
            ulong seed = (ulong)(800 + scenario);
            if (scenario == 4 || scenario == 7)
            {
                seed=45; ours=new List<UnitSpec>{new UnitSpec("a",UnitClass.Skirmisher),new UnitSpec("b",UnitClass.Skirmisher),
                    new UnitSpec("c",UnitClass.Skirmisher),new UnitSpec("d",UnitClass.Marksman)};
                theirs=new List<UnitSpec>{TacticsUnit("enemy.chief",UnitClass.Marksman,100,300,UnitRole.Chief)};
                if(scenario==7)ours=new List<UnitSpec>{TacticsUnit("a",UnitClass.Skirmisher,100,500),TacticsUnit("b",UnitClass.Skirmisher),
                    TacticsUnit("c",UnitClass.Skirmisher,100,1500),TacticsUnit("d",UnitClass.Marksman)};
            }
            if (scenario == 5)
            {
                seed=76; ours=new List<UnitSpec>{TacticsUnit("a",UnitClass.Skirmisher,300),TacticsUnit("b",UnitClass.Skirmisher,300),
                    TacticsUnit("c",UnitClass.Skirmisher,300),TacticsUnit("d",UnitClass.Skirmisher,300)};
                theirs=new List<UnitSpec>{TacticsUnit("x",UnitClass.Skirmisher),TacticsUnit("r1",UnitClass.Warden).AsReserve(),
                    TacticsUnit("r2",UnitClass.Warden).AsReserve(),TacticsUnit("enemy.chief",UnitClass.Trapper,100,1000,UnitRole.Chief)};
            }
            if (scenario == 6)
            {
                seed=47; ours=new List<UnitSpec>{new UnitSpec("v",UnitClass.Warden),new UnitSpec("w",UnitClass.Warden),
                    new UnitSpec("p",UnitClass.Marksman,Row.Back,UnitRole.Pal,100,100)};
                theirs=new List<UnitSpec>{new UnitSpec("x",UnitClass.Skirmisher),TacticsUnit("enemy.chief",UnitClass.Marksman,100,600,UnitRole.Chief)};
            }
            ChiefKit kit=skull?ChiefKit.Skullpeak:ChiefKit.Bloodridge;
            var setup=new BattleSetup("camp."+(900+scenario+(parryCenter?100:0))+"."+(skull?CampaignTribes.SkullpeakId:CampaignTribes.BloodridgeId)+".chief",
                seed,ours,theirs,new TacticsProfile(),new TacticsProfile(),3600,5,NerveRules.Off,10,400,new SideRules(1,0,true),
                new SideRules(2,(moves&ChiefKits.SpellMove(kit))!=0?ChiefKits.SpellEveryTicks(kit):0,false,0,
                    skull?ChiefSpell.SteelStorm:ChiefSpell.Eclipse,kit:kit,moves:moves),parryCenter ? FightRules.CampaignChiefTactics.WithParryCenter() : FightRules.CampaignChiefTactics);
            var sim=new BattleSimulation(setup);var session=new SimSession(sim);bool smashed=false,empowered=false;
            while(!sim.IsFinished)
            {
                if(scenario==5)
                {
                    if(!sim.InLockdown(1)&&sim.FocusTicksLeft(0)==0)session.TryIssue(Directives.CommandKind,"0","enemy.chief",0,0,"FocusFire",out _);
                    if(sim.InLockdown(1)&&!smashed) smashed=session.TryIssue(Directives.CommandKind,"0","",0,0,"Unleash",out _);
                }
                if(scenario==6)
                {
                    if(sim.CleaveTicks(1)>0&&sim.CleaveTicks(1)<=10&&sim.HoldTicksLeft(0)==0)
                        session.TryIssue(Directives.CommandKind,"0","",0,0,"Hold",out _);
                    if(sim.GloryRankOf(0)==GloryRank.S&&!empowered)empowered=session.TryIssue(Directives.CommandKind,"0","",0,0,"Eclipse",out _);
                }
                // Keep the authored target alive through the second forecast window; no gameplay tuning.
                if(scenario==7&&sim.BloodSun(1)&&sim.CurrentTick<800&&sim.FallBackTicksLeft(0)==0)
                    session.TryIssue(Directives.CommandKind,"0","",0,0,"FallBack",out _);
                session.Tick();
            }
            File.WriteAllLines(Path.Combine(_capture,"events-"+scenario+".txt"),sim.Events.Select(e=>e.Tick+" "+e.Kind+" "+e.Detail+" "+e.Amount));
            return _campaign.Archive.Add(setup,session.Log,sim);
        }

        void CheckTacticsReadout(string tag)
        {
            var sim=ViewSim;string before=StateChecksum.Compute(sim.OrderedStateSnapshot);
            DrawBattle();DrawBattle();
            Check(StateChecksum.Compute(sim.OrderedStateSnapshot)==before,tag+": draw cannot change sim");
            Check(_gloryText.key.Contains("GLORY "+ChiefKits.Letter(sim.GloryRankOf(0))+"  "+sim.Glory(0)+"/"),tag+": authoritative Glory");
            Check(_gloryText.key.Contains("5TH: 2X")== (sim.GloryRankOf(0)==GloryRank.S),tag+": S empowerment cue");
            bool live=!ViewOver&&!sim.AwaitingVerdict;
            var forecasts=live?sim.Forecasts(1):Array.Empty<ChiefForecast>();
            for(int i=0;i<forecasts.Count;i++)
            {
                var f=forecasts[i];
                Check(_forecastText.key.Contains("F"+(i+1)+" "+ChiefKits.Name(f.Move)+" "+Seconds(f.TicksLeft)+"  "+ForecastAmount(f)),tag+": forecast "+i);
                foreach(var id in new[]{f.TargetId,f.SecondTargetId}.Where(id=>id.Length>0))
                    if(sim.FindById(id).Alive&&!ChiefCutInVisible)
                        Check(_tacticsMarks[id].key.Contains("F"+(i+1)),tag+": forecast linked target "+id);
            }
            foreach(var f in _figures)
                if(!live||!f.unit.Alive||f.unit.Benched||ChiefCutInVisible)
                    Check(_tacticsTargets[f.unit.Id].All(p=>!p.enabled)&&_tacticsMarks[f.unit.Id].key.StartsWith("|"),tag+": inactive mark cleared "+f.unit.Id);
            if(sim.Kit(1)==ChiefKit.Skullpeak&&live)
                Check(_movesText.key.Contains("WALL: HITS "+sim.WallPercentNow(1)+"%"),tag+": actual wall fraction");
            Canvas.ForceUpdateCanvases();
            foreach(var t in new[]{_forecastText,_movesText})
                foreach(var im in t.root.GetComponentsInChildren<RawImage>())
                    Check(im.rectTransform.anchoredPosition.x+im.rectTransform.rect.width<=(t==_movesText?698:360)*Px,tag+": text fits column");
        }

        IEnumerator ValidateChiefTactics()
        {
            string fixtures=Argument("-ksTacticsValidate");_savePath=Path.Combine(_capture,"tactics-save.txt");
            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"kneeling.txt")));
            _pilotOn=false;_paused=true;
            for(int scenario=0;scenario<8;scenario++)
            {
                var record=TacticsFixture(scenario);WatchFight(record);yield return null;
                var events=new List<BattleEvent>();
                // The archive checksum and timeline are exercised through the real playback controls below.
                SeekPlayback(_playback.Length);Check(_playback.Matched,"fixture replay matches "+scenario);
                events.AddRange(ViewSim.Events);
                if(scenario<4)
                    foreach(ChiefMove move in Enum.GetValues(typeof(ChiefMove)))
                        if(move!=ChiefMove.None&&(record.Setup.Rules(1).KitMoves&move)!=0)
                            Check(events.Any(e=>(e.Kind==BattleEventKind.Tell||e.Kind==BattleEventKind.Windup)&&e.Detail==ChiefKits.Name(move)),"fixture exercises "+move);
                if(scenario==4||scenario==5)Check(events.Any(e=>e.Kind==BattleEventKind.Phase),"fixture exercises phase "+scenario);
                if(scenario==6)Check(events.Any(e=>e.Kind==BattleEventKind.Empowered),"fixture earns and spends Glory S");
                var moments=events.Where(e=>e.Kind==BattleEventKind.Tell||e.Kind==BattleEventKind.Forecast||e.Kind==BattleEventKind.Phase||
                    e.Kind==BattleEventKind.Wall||e.Kind==BattleEventKind.Empowered||e.Kind==BattleEventKind.Land||e.Kind==BattleEventKind.Broken)
                    .GroupBy(e=>e.Kind+":"+e.Detail).Select(g=>g.First()).ToList();
                if(scenario==7)
                {
                    var dual=events.Where(e=>e.Kind==BattleEventKind.Forecast&&e.Detail=="Brand").GroupBy(e=>e.Tick).FirstOrDefault(g=>g.Count()==2);
                    Check(dual!=null,"Blood Sun fixture forecasts both Brand marks");moments.Add(dual.First());
                }
                int n=0;
                foreach(var e in moments)
                {
                    SeekPlayback(e.Tick+1);yield return null;CheckTacticsReadout(scenario+" "+e.Kind+" "+e.Detail);
                    if(e.Kind==BattleEventKind.Wall)
                    {
                        AnimateEvent(e);
                        string word=e.Detail=="lockdown"?"LOCKDOWN WALL":e.Detail=="rebuilt"?"WALL REBUILT":
                            e.Detail=="half"?"HALF WALL":e.Detail=="up"?"STEEL STORM":e.Detail=="smashed"?"WALL SMASHED":"WALL FELL";
                        Check(_flashText[e.ActorSide]==word,"actual wall event has correct callout: "+e.Detail);
                    }
                    if(e.Kind==BattleEventKind.Tell||e.Kind==BattleEventKind.Phase||e.Kind==BattleEventKind.Empowered)
                        Capture("chief-"+scenario+"-"+(n++));
                    if(scenario==7&&ViewSim.Forecasts(1).Any(f=>f.SecondTargetId.Length>0))
                    {
                        var f=ViewSim.Forecasts(1).First(f=>f.SecondTargetId.Length>0);
                        var receipt=ViewSim.Events.Last(x=>x.Kind==BattleEventKind.Forecast&&x.TargetId==f.SecondTargetId&&x.Detail=="Brand");
                        Check(_forecastText.key.Contains("/ B "+receipt.Amount+" HP"),"second Brand damage uses its own receipt");
                        Check(_tacticsMarks[f.TargetId].key.Contains("A")&&_tacticsMarks[f.SecondTargetId].key.Contains("B"),"two Brand forecast labels distinguished");
                        Capture("dual-brand");
                    }
                }
                SeekPlayback(0);yield return null;CheckTacticsReadout("restart "+scenario);
                Check(Click("chief-moves"),"moves guide opens through real control "+scenario);yield return null;
                Check(_paused&&_modal!=null,"guide pauses the fight "+scenario);
                Check(Click("modal-close"),"moves guide closes "+scenario);yield return null;
                SeekPlayback(_playback.Length);CheckTacticsReadout("end "+scenario);
                Check(!_tacticsPanel.gameObject.activeSelf,"finished replay hides tactical panel "+scenario);
            }
            File.WriteAllText(Path.Combine(_capture,"archive-save.txt"),CampaignSave.Write(_campaign));
            _playback=null;_campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"pre-kneel.txt")));ResumeFight();
            _paused=true;DrawBattle();CheckTacticsReadout("live resume");
            string saved=CampaignSave.Write(_campaign);Save();_campaign=CampaignSave.Read(File.ReadAllText(_savePath));ResumeFight();
            DrawBattle();Check(CampaignSave.Write(_campaign)==saved,"chief resume roundtrip preserves campaign");CheckTacticsReadout("live reload");
            _fight.Tick();DrainEvents();RefreshFight();DrawBattle();Order(Directive.Hold);ShowResult();yield return null;
            Check(_result.Graded,"campaign result carries backend grade");
            var lines=new PxLines();ChiefGradeLines(lines,_result);
            Check(lines.texts.Any(t=>t.Contains(ChiefKits.Letter(_result.Grade))),"grade card uses receipt");
            Check(_campaign.BestGrade(_result.Encounter.Tribe.Id,out var grade)&&grade==_result.Grade,"best grade persists");
            Capture("grade-result");ShowCodex();yield return null;Capture("chief-codex");CloseModal();
            _playback=null;
            var old=ClassFixture(9991,new[]{ClassFixtureUnit("a",UnitClass.Warden)},new[]{ClassFixtureUnit("b",UnitClass.Marksman)});
            WatchFight(old);yield return null;DrawBattle();Check(_tacticsPanel==null,"legacy replay has no chief tactics or Glory");
        }
    }
}
