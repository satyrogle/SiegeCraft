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
        // Isolated packaged scenarios: real campaign rewards and controls, then authored effect fixtures.
        // This is regression evidence, not a balance sweep or a claim of native hands-on play.
        IEnumerator ValidateButcher(bool layers=false)
        {
            string startupVolume=Argument("-ksExpectedReviewVolume");
            if(startupVolume!=null)
                Check(float.TryParse(startupVolume,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float expectedVolume)&&
                    Mathf.Approximately(_soundSettings.volume,expectedVolume)&&_soundSettings.muted,
                    "fresh process loads the isolated review volume and mute settings");
            _savePath=Path.Combine(_capture,"butcher15-save.txt");
            _layersReview=layers;
            _butcherReview=_squadReview=_clearReview=_kitReview=_roadReview=_gatesReview=true;
            NewReviewRoute();yield return null;
            var node=_campaign.Offers.First(e=>e.Node!=null&&e.Node.Pal==Pal.Butcher);
            int index=_campaign.Offers.ToList().IndexOf(node);
            Check(HasPalReward(node.Node)==_campaign.PalPickOffered(node.Node),"road reward agrees with campaign query");
            Check(Click("offer-"+index),"select Butcher's real road fight");yield return null;
            _auto=false;Check(Click("start-fight"),"start rules15 encounter through UI");_auto=true;
            _pilotOn=false;_paused=true;yield return null;
            Check(_fight.RulesVersion==(layers?Campaign.LayersRules:Campaign.ButcherComboRules)&&_fight.Sim.LayersOn==layers,
                "review starts the requested rules with the authoritative layer switch");
            Check(_speed==.5f,"manual learning pace remains half speed");
            Capture("01-butcher-fight");
            bool parried=false;
            for(int tick=0;tick<6000&&!_fight.Over;tick++)
            {
                var sim=_fight.Sim;
                if(sim.Threats(0).Any(t=>t.ParryWindow)&&sim.CanUse(0,Directive.Hold,out _))
                {
                    _paused=false;Order(Directive.Hold);_paused=true;
                }
                if(tick==0&&sim.CanUse(0,Directive.FocusFire,out _))
                    Focus(sim.Units.First(u=>u.Side==1&&u.Alive));
                HudTicks(1);
                if(!parried&&sim.Events.Any(e=>e.Kind==BattleEventKind.Parry&&e.ActorSide==0))
                {parried=true;Capture("02-real-parry");}
                if(tick%120==0)yield return null;
            }
            Check(parried,"real road fight includes a successfully timed parry");
            Check(_fight.Over,"real Butcher fight reaches outcome");
            ShowResult();yield return null;
            Check(_result.Won&&_campaign.FavourOffer!=null&&_campaign.FavourOffer.Pal==Pal.Butcher,"win genuinely earns Butcher offer");
            File.WriteAllText(Path.Combine(_capture,"earned-offer.txt"),CampaignSave.Write(_campaign));
            yield return ValidateButcherChoiceUi();
            var piece=_campaign.Gear.First(g=>ClassSpells.IsCombo(g.Bend));
            var wearer=_campaign.Reserve.First(f=>f.GearId==piece.Id);
            _fight=null;CloseModal();SetPage(Page.Offers);yield return null;
            Check(Click("offer-0"),"choose next encounter after combo");yield return null;
            Check(Click("start-fight"),"next encounter starts through UI");_pilotOn=false;_paused=true;yield return null;
            Check(_fight.Setup.Side(0).Any(u=>u.Id==wearer.Id&&u.Bend==piece.Bend),"equipped combo enters authoritative next battle setup");
            HudTicks(180);
            string active=CampaignSave.Write(_campaign);
            var resumed=CampaignSave.Read(active);
            Check(resumed.LoadNote.Length==0&&resumed.Active.Sim.OrderedStateSnapshot==_fight.Sim.OrderedStateSnapshot,
                "rules15 active save restores exact combo state");
            File.WriteAllText(Path.Combine(_capture,layers?"active17.txt":"active15.txt"),active);
            Capture("06-next-fight-combo");
            for(int i=0;i<6;i++){HudTicks(8);Capture("motion-"+i.ToString("D2"));yield return null;}
            for(int i=0;i<6000&&!_fight.Over;i++){HudTicks(1);if(i%180==0)yield return null;}
            Check(_fight.Over,"combo fight reaches outcome");
            string final=_fight.Sim.OrderedStateSnapshot;ShowResult();yield return null;
            var record=_campaign.Archive.Fights.Last();
            Check(_campaign.Archive.TryReplay(record.FightId,out var replay,out string why)&&replay.OrderedStateSnapshot==final,
                "earned combo fight archive replays exactly: "+why);

            // Each combo is presented from an actual simulated, archived effect; no hand-built event receipts.
            _fight=null;_result=null;SetPage(Page.Offers);
            foreach(ClassBend bend in Enum.GetValues(typeof(ClassBend)))
            {
                if(!ClassSpells.IsCombo(bend))continue;
                BattleSetup setup=ButcherEffectFixture(bend,layers);
                var sim=new BattleSimulation(setup);var session=new SimSession(sim);
                bool focusedSlow=false;
                while(!sim.IsFinished)
                {
                    if(bend==ClassBend.TimeSnare&&!focusedSlow)
                    {
                        var slowed=sim.Units.FirstOrDefault(u=>u.Side==1&&u.SlowTicks>0);
                        if(slowed!=null)
                        {
                            Check(session.TryIssue(Directives.CommandKind,"0",slowed.Id,0,0,Directives.Name(Directive.FocusFire),out _),
                                "non-ClearRoad fixture focuses the genuinely slowed enemy");
                            focusedSlow=true;
                        }
                    }
                    session.Tick();
                }
                var effect=sim.Events.FirstOrDefault(e=>e.Detail==ClassSpells.Name(bend)&&
                    (e.Kind==BattleEventKind.Pull||e.Kind==BattleEventKind.Portal||e.Kind==BattleEventKind.Freeze));
                Check(effect.Detail==ClassSpells.Name(bend),"actual effect emitted: "+bend);
                var archived=_campaign.Archive.Add(setup,session.Log,sim);
                WatchFight(archived,0,archived.Ticks);yield return null;
                SeekPlayback(effect.Tick+(bend==ClassBend.TimeSnare?2:1));yield return null;
                var receipt=_butcherEffects.FirstOrDefault(r=>r!=null&&r.receipt.Detail==effect.Detail);
                bool commandPriority=bend==ClassBend.TimeSnare;
                if(commandPriority)
                {
                    Log("TimeSnare replay receipt: clean="+CleanCombatHud+" footer="+_butcherFooterShown+
                        " banner="+_pxBanner.key+" legacyReceipt="+_orderReceipt.key);
                    Capture("time-snare-priority");
                }
                Check(receipt!=null&&receipt.corners.Any(c=>c.enabled)&&(commandPriority?
                    !_butcherFooterShown&&(CleanCombatHud?_pxBanner.key.Contains("FOCUS FIRE"):
                        _orderReceipt.root.gameObject.activeInHierarchy&&_orderReceipt.key.Contains("FOCUS FIRE")):
                    _butcherFooterShown&&_butcherFooterReceipt.Detail==effect.Detail&&_butcherFooterTitle.key.Contains(effect.Detail.ToUpperInvariant())),
                    "visible named effect receipt and actual target bracket: "+bend);
                Check(commandPriority?_pxBanner.root.gameObject.activeSelf&&_pxBanner2.root.gameObject.activeSelf:
                    _butcherFooterTitle.root.GetComponentsInChildren<UnityEngine.UI.RawImage>().Any(g=>g.enabled)&&
                    _butcherFooterDetail.root.GetComponentsInChildren<UnityEngine.UI.RawImage>().Any(g=>g.enabled)&&
                    _butcherFooterTitle.x==_pxBanner.x&&_butcherFooterTitle.y==_pxBanner.y&&
                    _butcherFooterDetail.x==_pxBanner2.x&&_butcherFooterDetail.y==_pxBanner2.y&&
                    !_pxBanner.root.gameObject.activeSelf&&!_pxBanner2.root.gameObject.activeSelf,
                    "receipt reuses existing feedback positions without stacking old labels: "+bend);
                string words=ButcherReceiptText(ViewSim,effect);
                if(bend==ClassBend.StoppedBullets)
                    Check(words.EndsWith(" / SHOTS HELD "+effect.Amount),"Stopped Bullets counts bullets instead of displaying seconds");
                if(bend==ClassBend.TimeSnare)
                {
                    Check(words.EndsWith(" / SLOW "+Seconds(effect.Amount)),"Time Snare reports the slow duration");
                    var figure=FindFigure(effect.TargetId);var read=_butcherUnits[effect.TargetId];
                    Check(focusedSlow&&figure.tag.key.Contains("YOUR FOCUS"),"non-ClearRoad target shows actual Focus and slow state together");
                    Check(read.held.enabled&&ButcherFighterHint(ViewSim,figure.unit).Contains("SLOW"),
                        "slowed fighter has an HP-bar marker and truthful full hover timing");
                    Check(!read.state.root.GetComponentsInChildren<UnityEngine.UI.RawImage>().Any(g=>g.enabled),
                        "no multiline status extends into neighbouring fighter rows");
                }
                if(bend==ClassBend.Rewind)
                {
                    Check(words==ClassSpells.Name(bend),"Rewind primary receipt never interprets restored HP as seconds");
                    var heal=sim.Events.First(e=>e.Tick==effect.Tick&&e.Kind==BattleEventKind.Heal&&e.Detail==ClassSpells.RewindDetail);
                    Check(ButcherReceiptText(ViewSim,heal).EndsWith("+"+heal.Amount+" HP"),"Rewind outcome reports actual healed HP");
                }
                Capture("effect-"+bend);
                _hover="fixture fighter hint";DrawBattle();
                Check(!_butcherFooterShown&&_pxBanner.root.gameObject.activeSelf&&_pxBanner2.root.gameObject.activeSelf,
                    "fighter hover restores original feedback priority: "+bend);
                _hover="";DrawBattle();
                string state=ViewSim.OrderedStateSnapshot;
                for(int n=0;n<4;n++){DrawBattle();yield return null;}
                Check(ViewSim.OrderedStateSnapshot==state,"paused effect drawing leaves sim unchanged: "+bend);
                SeekPlayback(0);yield return null;
                Check(_butcherEffects.All(r=>string.IsNullOrEmpty(r.receipt.Detail)&&r.corners.All(c=>!c.enabled)),"rewind clears effect receipts: "+bend);
                SeekPlayback(archived.Ticks);yield return null;
                Check(_playback.Matched,"combo fixture verifies full replay: "+bend);
                LeavePlayback();CloseModal();yield return null;
            }
            Log("Butcher15 packaged scenarios complete. Scripted UI and rendered moving fights; native input, listening and owner feel remain separate.");
        }

        static BattleSetup ButcherEffectFixture(ClassBend bend,bool layers=false)
        {
            UnitSpec U(string id,UnitClass cls,Row? row=null)=>row.HasValue?new UnitSpec(id,cls,row.Value):new UnitSpec(id,cls);
            var ours=new List<UnitSpec>{U("clarth.combo",ClassSpells.ClassOf(bend)).WithBend(bend)};
            var theirs=new List<UnitSpec>{U("raiders.h1",UnitClass.Medic,Row.Front),U("raiders.h2",UnitClass.Medic,Row.Front),U("raiders.h3",UnitClass.Medic,Row.Front)};
            SideRules rules=null;
            if(bend==ClassBend.SingularityNet||bend==ClassBend.TimeSnare)
            {ours.Insert(0,U("clarth.w",UnitClass.Warden));theirs=new List<UnitSpec>{U("raiders.w1",UnitClass.Warden),U("raiders.w2",UnitClass.Warden),U("raiders.w3",UnitClass.Warden)};}
            if(bend==ClassBend.MercyWell)
            {
                ours.InsertRange(0,new[]{U("clarth.w1",UnitClass.Warden,Row.Front),U("clarth.w2",UnitClass.Warden,Row.Front),U("clarth.w3",UnitClass.Warden,Row.Front)});
                theirs=new List<UnitSpec>{U("raiders.w1",UnitClass.Warden),U("raiders.w2",UnitClass.Warden),U("raiders.w3",UnitClass.Warden)};rules=new SideRules(1,0,false);
            }
            if(bend==ClassBend.ExecutionCorridor||bend==ClassBend.FrozenFlurry)ours.Insert(0,U("clarth.m",UnitClass.Marksman));
            if(bend==ClassBend.WrongDoor)
            {ours.Add(U("clarth.s1",UnitClass.Skirmisher,Row.Front));ours.Add(U("clarth.s2",UnitClass.Skirmisher,Row.Front));theirs=new List<UnitSpec>{U("raiders.w1",UnitClass.Warden),U("raiders.w2",UnitClass.Warden),U("raiders.w3",UnitClass.Warden)};rules=new SideRules(1,0,false);}
            if(bend==ClassBend.Doorway||bend==ClassBend.Rewind)
            {ours.Insert(0,U("clarth.w",UnitClass.Warden));theirs=new List<UnitSpec>{U("raiders.m1",UnitClass.Marksman),U("raiders.m2",UnitClass.Marksman)};}
            if(bend==ClassBend.StillWall)theirs=new List<UnitSpec>{U("raiders.w1",UnitClass.Warden),U("raiders.w2",UnitClass.Warden)};
            if(bend==ClassBend.Evac)
            {ours.Insert(0,new UnitSpec("clarth.w",UnitClass.Warden,Row.Front,UnitRole.Fighter,100,20));theirs=new List<UnitSpec>{new UnitSpec("raiders.w1",UnitClass.Warden,Row.Front,UnitRole.Fighter,300,100),new UnitSpec("raiders.w2",UnitClass.Warden,Row.Front,UnitRole.Fighter,300,100),new UnitSpec("raiders.w3",UnitClass.Warden,Row.Front,UnitRole.Fighter,300,100)};}
            return new BattleSetup("camp."+(900+(int)bend)+".raiders.warparty",4,ours,theirs,new TacticsProfile(),new TacticsProfile(),
                BattleSetup.DefaultTickLimit,BattleSetup.DefaultDirectivePoints,NerveRules.Off,0,400,null,rules,layers?FightRules.Campaign.WithLayers():FightRules.Campaign);
        }
    }
}
