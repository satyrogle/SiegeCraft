using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Fixed authored QA fixtures, never owner saves or gameplay tuning.
        Campaign BeastCampFixture(CampaignTuning tuning=null)
        {
            string text=File.ReadAllText(Path.Combine(Argument("-ksBeastValidate"),"fixture-recipe.txt"));
            var runKeys=new HashSet<string>{"joined","fighter","assist","gear","piece","spoils","grudge","run","road","feat","offer","fight","proof","squad","order","bounty","beaten","promoted","favour","favouroffer","routepick","beast"};
            text=string.Join("\n",text.Split('\n').Where(l=>!runKeys.Contains(l.Split('\t')[0])));
            var c=CampaignSave.Read(text,tuning);
            Check(c.Stage==CampaignStage.ChooseTribe&&c.LoadNote.Length==0,"authored camp fixture reads cleanly");
            return c;
        }

        void CheckBeastDisplay(string tag)
        {
            var sim=ViewSim;string before=StateChecksum.Compute(sim.OrderedStateSnapshot);
            DrawBattle();DrawNerve(sim);
            Check(before==StateChecksum.Compute(sim.OrderedStateSnapshot),tag+": display leaves state untouched");
            Check(_beastPanel!=null&&_nerveLabel[1].key.Contains("NO NERVE")&&!_nerveFill[1].enabled,tag+": no false beast nerve");
            foreach(var part in BeastKits.Parts(sim.Beast(1)))
            {
                var unit=sim.Part(part);var f=FindFigure(unit.Id);
                Check(f.artKey==null&&f.body==null,tag+": honest part marker "+part);
                foreach(string line in BeastPartLine(sim,part).Split('\n'))Check(_tinyFont.Measure(line.ToUpperInvariant(),1)<=150,tag+": part line fits "+part+" "+line);
                Check(_beastParts[part].key.Contains(BeastKits.Name(part).ToUpperInvariant()),tag+": named part "+part);
                Check(_beastParts[part].key.Contains(unit.Alive?unit.Hp+"/"+unit.MaxHp:BeastSevered(sim,part)?"BROKEN":"DOWN / NO TROPHY"),tag+": actual part health or death "+part);
            }
            if(!ViewOver&&sim.BeastHeldId.Length>0&&sim.FindById(sim.BeastHeldId).Alive)Check(_beastMarks[sim.BeastHeldId].key.Contains("HELD"),tag+": held fighter marked");
            if(!ViewOver&&sim.BeastCaged)Check(_beastParts[BeastPart.Heart].key.Contains("SEALED"),tag+": heart sealed");
            if(sim.BeastEscapeTicks>0&&!ViewOver)Check(_beastState.key.Contains(Seconds(sim.BeastEscapeTicks)),tag+": authoritative escape countdown");
        }

        ArchivedFight BeastReplayFixture(int scenario)
        {
            bool bear=scenario>=2;var kit=bear?BeastKit.CageBear:BeastKit.SandShark;
            var ours=new List<UnitSpec>{TacticsUnit("clarth.skirmisher.a",UnitClass.Skirmisher,100,2000),TacticsUnit("clarth.marksman.b",UnitClass.Marksman,100,2000),TacticsUnit("clarth.medic.c",UnitClass.Medic,100,2000)};
            var theirs=new List<UnitSpec>();
            string id="camp."+(9100+scenario)+"."+(bear?CampaignTribes.SkullpeakId:CampaignTribes.BloodridgeId)+".beast";
            foreach(var part in BeastKits.Parts(kit))
            {
                int hp=scenario==4?(part==BeastPart.Ribcage||part==BeastPart.Heart?1:2000):scenario==0||scenario==2?2000:part==BeastPart.Tail?5000:part==BeastPart.Heart?1000:1;
                theirs.Add(new UnitSpec(id+"."+BeastKits.Id(kit)+"."+BeastKits.Name(part),UnitClass.Warden,BeastKits.RowOf(part),UnitRole.Fighter,100,hp));
            }
            var setup=new BattleSetup(id,(ulong)(9100+scenario),ours,theirs,new TacticsProfile(),new TacticsProfile(),1800,5,NerveRules.Off,
                10,400,new SideRules(1,0,true),new SideRules(0,0,false,beast:kit),FightRules.CampaignBeast);
            var sim=new BattleSimulation(setup);var session=new SimSession(sim);
            while(!sim.IsFinished)
            {
                var target=scenario==4?sim.Part(sim.BeastCaged?BeastPart.Ribcage:BeastPart.Heart):scenario==1?sim.Part(BeastPart.Fin):scenario==3?BeastKits.Parts(kit).Select(sim.Part).FirstOrDefault(u=>u.Alive):null;
                if(target!=null&&target.Alive&&sim.FocusedId(0)!=target.Id)session.TryIssue(Directives.CommandKind,"0",target.Id,0,0,"FocusFire",out _);
                session.Tick();
            }
            File.WriteAllLines(Path.Combine(_capture,"beast-events-"+scenario+".txt"),sim.Events.Select(e=>e.Tick+" "+e.Kind+" "+e.Detail+" "+e.Amount));
            return _campaign.Archive.Add(setup,session.Log,sim);
        }

        IEnumerator ValidateBeasts()
        {
            _savePath=Path.Combine(_capture,"beast-save.txt");_pilotOn=false;_paused=true;
            for(int scenario=0;scenario<6;scenario++)
            {
                bool bear=scenario>=2&&scenario<=4;
                var tuning=new CampaignTuning();
                if(scenario==1)tuning.FightTicks=1200;
                if(scenario==3)tuning.EnemyScaleTier2=25;
                if(scenario==4)tuning.FightTicks=60;
                if(scenario==5)tuning.EnemyScaleTier1=400;
                _playback=null;_campaign=BeastCampFixture(tuning);
                if(bear)_campaign.ChooseTribe(CampaignTribes.SkullpeakId);else _campaign.StartRun();
                SetPage(Page.Offers);yield return null;Capture("road-"+scenario);
                Check(_roadDetailSlots.Length==8&&_roadDetailSlots.Last().y+8<420,"full road details fit above beast button "+scenario);
                int step=_campaign.Step;
                Check(Click("beast-offer"),"optional road button "+scenario);yield return null;
                Check(_page==Page.Squad&&SelectedEncounter.Beast==(bear?BeastKit.CageBear:BeastKit.SandShark),"correct beast briefing "+scenario);
                if(scenario==5)foreach(var f in _campaign.Reserve.Where(f=>f.Class==UnitClass.Warden||f.Class==UnitClass.Medic)){Check(Click("fighter-"+f.Id),"authored loss squad removes cover/healing");yield return null;}
                Capture("road-squad-"+scenario);
                Check(Click("start-fight"),"beast starts through UI "+scenario);_pilotOn=false;_paused=true;yield return null;
                Check(_campaign.Step==step,"starting beast costs no step "+scenario);CheckBeastDisplay("live "+scenario);
                if(bear)
                {
                    Order(Directive.FocusFire);yield return null;
                    Check(Click("beast-part-Heart"),"heart target click "+scenario);yield return null;
                    Check(_fight.Sim.FocusedId(0)!=_fight.Sim.Part(BeastPart.Heart).Id&&_notice.Contains("ribcage"),"sealed heart refusal comes from sim "+scenario);
                    _targeting=false;
                    if(scenario==4)
                    {
                        int points=_fight.Sim.DirectivePoints(0);Order(Directive.FocusFire);yield return null;
                        Check(Click("beast-part-Ribcage"),"legal part focus button");yield return null;
                        Check(_fight.Sim.FocusedId(0)==_fight.Sim.Part(BeastPart.Ribcage).Id&&_fight.Sim.DirectivePoints(0)==points-Directives.Cost(Directive.FocusFire),"part focus uses authoritative command and cost");
                    }
                }
                Check(Tracker(_fight.Sim,_fight.Encounter).Contains("nobody captured")&&!Tracker(_fight.Sim,_fight.Encounter).Contains("gear in their place"),"beast tactics use correct loss and reward policy "+scenario);
                for(int t=0;t<30&&!_fight.Over;t++)_fight.Tick();
                Save();string snapshot=StateChecksum.Compute(_fight.Sim.OrderedStateSnapshot);
                _campaign=CampaignSave.Read(File.ReadAllText(_savePath),tuning);ResumeFight();_pilotOn=false;_paused=true;
                Check(_campaign.LoadNote.Length==0&&StateChecksum.Compute(_fight.Sim.OrderedStateSnapshot)==snapshot,"active beast resumes exactly "+scenario);
                CheckBeastDisplay("resume "+scenario);yield return null;Capture("fight-"+scenario);
                while(!_fight.Over)_fight.Tick();
                DrainEvents();RefreshFight();DrawBattle();_endAge=3;RefreshFight();yield return null;
                Check(Click("see-result"),"beast result opens "+scenario);yield return null;
                Check(_notice.Length==0,"finished battle clears stale combat warnings "+scenario);
                Check(_result.Encounter.Kind==EncounterKind.Beast&&_result.Captured==null,"beast result has no captured fighter "+scenario);
                Check(_result.Trophies.SequenceEqual(_result.Won?_result.PartsBroken:Array.Empty<BeastPart>()),"only won broken parts become trophies "+scenario);
                Check(string.Join(" ",FightLines(_result,false).texts).Contains("TROPHIES BANKED: "+_result.Trophies.Count),"result shows actual trophy receipt "+scenario);
                Check(_result.RunOver||_campaign.Step==step,"finish costs no road step "+scenario);
                Check(!_result.Won||_campaign.BeastOffer==null,"won beast removed from road "+scenario);
                if(scenario==0||scenario==3)Check(_result.Won,"fixed winning beast fixture "+scenario);
                if(scenario==1)Check(_result.BeastFled&&!_result.BeastKilled,"fixed shark partial escape fixture");
                if(scenario==2||scenario==4||scenario==5)Check(!_result.Won,"fixed defeat fixture "+scenario);
                if(scenario==4)Check(_campaign.BeastOffer!=null,"bear timeout leaves a retry beside road");
                File.WriteAllText(Path.Combine(_capture,"result-"+scenario+".txt"),CampaignSave.Write(_campaign));
                Capture("result-"+scenario);
                var archive=_campaign.Archive.Fights.Last();WatchFight(archive);yield return null;
                SeekPlayback(_playback.Length);yield return null;
                Check(_playback.Matched,"campaign beast archive verifies "+scenario);CheckBeastDisplay("archive end "+scenario);
            }
            _playback=null;_campaign=BeastCampFixture();SetPage(Page.ChooseTribe);
            for(int scenario=0;scenario<5;scenario++)
            {
                var record=BeastReplayFixture(scenario);WatchFight(record);yield return null;
                SeekPlayback(_playback.Length);Check(_playback.Matched,"authored beast archive verifies "+scenario);
                if(scenario==4){Check(ViewSim.Events.Any(e=>e.Kind==BattleEventKind.Kill&&e.Detail=="heart"),"heart kill fixture drops unbroken parts");yield return null;Capture("heart-kill-no-trophies");}
                var moments=ViewSim.Events.Where(e=>e.Kind==BattleEventKind.Tell||e.Kind==BattleEventKind.Phase||e.Kind==BattleEventKind.Sever||e.Kind==BattleEventKind.Flee||e.Kind==BattleEventKind.Land)
                    .GroupBy(e=>e.Kind+":"+e.Detail).Select(g=>g.First()).ToList();
                int n=0;
                foreach(var e in moments)
                {
                    SeekPlayback(e.Tick+1);yield return null;CheckBeastDisplay(scenario+" "+e.Kind+" "+e.Detail);
                    if(e.Kind==BattleEventKind.Tell||e.Kind==BattleEventKind.Phase)Capture("moment-"+scenario+"-"+n++);
                }
                SeekPlayback(0);yield return null;CheckBeastDisplay("restart "+scenario);
                Check(Click("beast-moves"),"beast guide button "+scenario);yield return null;
                Check(_paused&&_modal!=null,"guide pauses "+scenario);Capture("moves-"+scenario);CloseModal();
            }
            string archived=CampaignSave.Write(_campaign);
            Check(CampaignSave.Write(CampaignSave.Read(archived))==archived,"authored beast archive survives save decoding");
            File.WriteAllText(Path.Combine(_capture,"archive-save.txt"),archived);
            _playback=null;_campaign=BeastCampFixture();
            string camp=CampaignSave.Write(_campaign);
            string trophies=string.Concat(Enumerable.Range(0,8).Select(i=>"\ntrophy\t"+i));
            _campaign=CampaignSave.Read(camp.Replace("\nforge\t",trophies+"\nforge\t"));SetPage(Page.ChooseTribe);yield return null;
            ShowForge();yield return null;Check(Click("beast-trophies"),"Butcher opens trophy inventory");yield return null;Capture("trophies");
            for(int p=0;p<8;p++)
            {
                var part=(BeastPart)p;ShowTrophyRecipes(part);yield return null;
                foreach(var piece in BeastPieces.From(part))Check(_buttons["forge-beast-"+piece].interactable==_campaign.CanForgeTrophy(part,piece,out _),"recipe eligibility "+part+" "+piece);
                var pick=BeastPieces.From(part).FirstOrDefault(x=>_campaign.CanForgeTrophy(part,x,out _));
                if(!_campaign.CanForgeTrophy(part,pick,out _))continue;
                int materials=_campaign.Materials,count=_campaign.TrophyCount(part);
                Capture("recipes-"+part);
                Check(Click("forge-beast-"+pick),"forge real recipe "+part);yield return null;
                Check(_campaign.BeastPieceForged(pick)&&_campaign.TrophyCount(part)==count-1&&_campaign.Materials==materials,"one trophy only, permanent piece "+pick);
                Check(!_buttons["forge-beast-"+pick].interactable,"cannot buy twice "+pick);
            }
            CloseModal();Save();string saved=CampaignSave.Write(_campaign);_campaign=CampaignSave.Read(File.ReadAllText(_savePath));
            Check(saved==CampaignSave.Write(_campaign),"trophies and forged pieces survive disk reload");
            _campaign.StartRun();foreach(var piece in _campaign.BeastPiecesForged)Check(_campaign.Gear.Any(g=>g.Id==BeastPieces.GearId(piece)),"future run carries "+piece);
            ShowTrophyRecipes(BeastPart.Heart);yield return null;
            Check(BeastPieces.From(BeastPart.Heart).All(p=>!_buttons["forge-beast-"+p].interactable),"forge unavailable away from camp");CloseModal();
            var old=ClassFixture(9992,new[]{ClassFixtureUnit("a",UnitClass.Warden)},new[]{ClassFixtureUnit("b",UnitClass.Marksman)});
            WatchFight(old);yield return null;Check(_beastPanel==null,"ordinary replay has no beast overlay");
        }
    }
}
