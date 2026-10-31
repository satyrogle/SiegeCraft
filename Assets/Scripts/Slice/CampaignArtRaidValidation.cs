using System;
using System.Collections;
using System.IO;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        IEnumerator ValidateRaidHud()
        {
            yield return null;
            _savePath=Path.Combine(_capture,"raid-save.txt");
            _campaign=FreshRaidPreview();_fight=_campaign.Active;_playback=null;
            BeginFight();_pilotOn=false;_paused=true;yield return null;
            Check(RaidHud&&CleanCombatHud,"fresh preview uses rules 10 raid HUD");
            Check(ViewSim.Units.Count(u=>u.Side==1)==1,"chief fights alone with no reserves");
            Check(EnemyPreview(_fight.Encounter).StartsWith("SOLO CHIEF"),"encounter preview names solo chief");
            Check(_orderCost[0].key.Contains("FREE"),"Hold button shows FREE");
            Check(_nerveLabel[1].key.Contains("POSTURE"),"chief bar labelled posture");
            Check(RaidGuide.Contains("Eclipse cannot be interrupted"),"guide excludes obsolete Eclipse interrupt");
            string fresh=CampaignSave.Write(_campaign);
            File.WriteAllText(Path.Combine(_capture,"preview-start.txt"),fresh);
            int points=ViewSim.DirectivePoints(0);
            Check(Click("order-0"),"free Hold receives actual UI raycast");
            Check(ViewSim.DirectivePoints(0)==points,"paused Hold spends no points");
            Check(Click("chief-moves"),"raid move guide opens");yield return null;Capture("01-guide");
            Check(Click("modal-close"),"guide closes");yield return null;
            GateRestore(fresh);yield return null;_paused=true;
            string before=ViewSim.OrderedStateSnapshot;
            for(int i=0;i<4;i++){RefreshFight();DrawBattle();yield return null;}
            Check(before==ViewSim.OrderedStateSnapshot,"paused HUD does not mutate simulation");
            Capture("02-solo-start");CheckCleanTextBounds();

            // One fixed encounter, twice: inspect the untouched tells, then exercise actual commands.
            bool castSeen=false,beatSeen=false,ultimateSeen=false,flashSeen=false,cutSeen=false;
            for(int i=0;i<2100&&!ViewOver&&!ViewSim.AwaitingVerdict;i++)
            {
                HudTicks(1);var t=CurrentThreat;
                if(t.CastBar&&!castSeen)
                {castSeen=true;Check(_beatTrack.enabled&&_beatWindow.enabled&&!_beatCursor.enabled,"interrupt uses cast bar only");Capture("03-cast");}
                if(!t.IsNone&&!t.CastBar&&t.BeatTicks>=0&&!beatSeen)
                {beatSeen=true;Check(!_beatTrack.enabled&&!_beatWindow.enabled&&!_beatCursor.enabled,"parry move has no timing bar");Capture("04-parry-tell");}
                if(t.Tier==ThreatTier.Spell&&!t.IsNone)
                {
                    ultimateSeen=true;
                    Check((t.Answers&ThreatAnswer.FocusFire)==0,"Eclipse cannot advertise Focus interrupt");
                    if(t.ParryWindow&&!flashSeen)
                    {_paused=false;DrawBattle();flashSeen=_raidFlash.enabled;
                        Check(!ChiefCutInVisible&&(ViewSim.SquadScreenOn?_parryOuter.enabled:_raidBeatRing.enabled),"ultimate cut-in clears before actionable parry window");
                        if(ViewSim.SquadScreenOn)Check(_squadNames.key.StartsWith("|"),"combo caption does not cover chief posture bar");
                        Capture("06-eclipse-window");_paused=true;}
                }
                if(ViewSim.WindupTicks(1)>20&&!cutSeen)
                {_paused=false;DrawBattle();cutSeen=ChiefCutInVisible;if(cutSeen)Capture("05-eclipse-cut-in");_paused=true;}
                if(i%60==0)yield return null;
            }
            Check(castSeen&&beatSeen&&ultimateSeen,"actual encounter exposes cast, parry and ultimate phases");
            Check(flashSeen,"Eclipse live parry window produces a single flash");
            Check(cutSeen,"Eclipse windup uses existing chief cut-in");

            // Mantle belongs to the untouched seed-21 repertoire. Exercise its real interrupt via UI.
            _campaign=FreshRaidPreview(21);_fight=_campaign.Active;BeginFight();_pilotOn=false;_paused=true;
            yield return null;
            for(int n=0;n<1800&&!ViewOver&&!_threatReads.Any(t=>t.CastBar&&t.Name==ChiefKits.MantleName);n++)HudTicks(1);
            Check(_threatReads.Any(t=>t.CastBar&&t.Name==ChiefKits.MantleName),"alternate fixed repertoire exposes Mantle cast");
            Check(_beatTrack.enabled&&_beatWindow.enabled,"Mantle cast bar survives a higher-ranked simultaneous threat");
            if(!CurrentThreat.CastBar)Check(_threatTarget.key.Contains(ChiefKits.MantleName.ToUpperInvariant()),"secondary Mantle interrupt is named on screen");
            Check(Click("order-2"),"Focus button enters target selection");
            HandleUiKey(KeyCode.Return);RefreshFight();
            Check(ViewSim.FocusedId(0)==ViewSim.Leader(1).Id,"keyboard confirms the solo chief target");
            for(int n=0;n<150&&!ViewOver;n++)HudTicks(1);
            Check(ViewSim.Events.Any(e=>e.Kind==BattleEventKind.Land&&e.Detail==ChiefKits.MantleName+"."+RaidRules.Broken),"actual Focus breaks Mantle");
            Capture("06b-mantle-interrupt");

            GateRestore(fresh);yield return null;_paused=true;
            // Exhaust points through genuine accepted orders, then check zero-point Hold.
            int spend=0;
            while(ViewSim.DirectivePoints(0)>0&&spend++<12)
                Order(ViewSim.DirectivePoints(0)>=2?Directive.Unleash:Directive.FallBack);
            Check(ViewSim.DirectivePoints(0)==0,"fixture exhausts points through commands");
            Check(Click("order-0")&&ViewSim.DirectivePoints(0)==0,"zero-point Hold remains clickable and free");
            GateRestore(fresh);yield return null;_paused=true;
            _speed=.5f;long moving=ViewSim.CurrentTick;_paused=false;
            for(int n=0;n<18;n++)yield return null;
            _paused=true;Check(ViewSim.CurrentTick>moving,"half-speed player Update advances live combat");
            Capture("06c-moving-half-speed");
            _canvas.scaleFactor=.75f;CheckCleanTextBounds();_canvas.scaleFactor=1;
            GateRestore(fresh);yield return null;_paused=true;
            var pilot=new CampaignPilot(PilotStyle.Reference,21);
            bool saved=false,interrupt=false;
            for(int i=0;i<6000&&!ViewOver;i++)
            {
                if(ViewSim.AwaitingVerdict){Order(Directive.Hold);break;}
                pilot.Step(_fight);HudTicks(1);
                interrupt|=ViewSim.Events.Any(e=>e.Kind==BattleEventKind.Land&&e.Detail==ChiefKits.MantleName+"."+RaidRules.Broken);
                if(i==180)
                {
                    string snapshot=ViewSim.OrderedStateSnapshot,save=CampaignSave.Write(_campaign);
                    GateRestore(save);yield return null;_paused=true;
                    Check(snapshot==ViewSim.OrderedStateSnapshot,"mid-fight save restores posture and Hold timing exactly");saved=true;
                }
                if(i%90==0)yield return null;
            }
            Check(saved&&ViewOver,"fixed raid encounter settles after save/reload");
            Log("Mantle interrupted by reference pilot: "+interrupt);
            Capture("07-chief-end");CheckCleanTextBounds();
            string final=ViewSim.OrderedStateSnapshot;
            var result=_campaign.Finish(_fight);
            Log("Raid outcome won="+result.Won+" joined="+result.Joined);
            var archive=_campaign.Archive.Fights.Last();
            WatchFight(archive,0,archive.Ticks);yield return null;SeekPlayback(archive.Ticks);yield return null;
            Check(ViewSim.OrderedStateSnapshot==final,"raid archive replays exact final state");
            Check(RaidHud&&_nerveLabel[1].key.Contains("POSTURE"),"raid replay retains posture label");
            SeekPlayback(0);yield return null;Check(!_raidFlash.enabled,"backward seek clears stale flash");
            LeavePlayback();CloseModal();yield return null;

            // Real historical packaged checkpoint: retain old rules and their point cost.
            GateRestore(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"ReviewRoute","chief.txt")));
            yield return null;_paused=true;
            Check(!ViewSim.RaidChiefOn&&!RaidHud,"historical active chief is not silently upgraded");
            Check(DisplayOrderCost(ViewSim,Directive.Hold)==1&&!_orderCost[0].key.Contains("FREE"),"historical Hold still costs a point");
            Capture("08-legacy-chief");
            Log("Raid HUD integration checks complete; scripted player evidence is not human feel approval.");
        }
    }
}
