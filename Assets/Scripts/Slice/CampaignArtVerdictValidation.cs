using System;
using System.Collections;
using System.IO;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        IEnumerator ValidateChiefVerdict()
        {
            string fixtures=Argument("-ksVerdictValidate");
            _savePath=Path.Combine(_capture,"verdict-save.txt");
            for(int scenario=0;scenario<3;scenario++)
            {
                bool execute=scenario==1;
                string fixture=scenario==0?"pre-kneel.txt":scenario==1?"kneeling-near-limit.txt":"kneeling.txt";
                _playback=null;_fight=null;
                _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,fixture)));
                ResumeFight();_pilotOn=false;_speed=4;_paused=false;
                long started=_fight.Sim.CurrentTick;
                yield return null;
                Check(_fight.Sim.AwaitingVerdict,"chief waits for verdict scenario "+scenario);
                if(scenario==0)Check(_fight.Sim.CurrentTick==started+1,"4x update stops on the exact kneeling tick");
                string state=CampaignSave.Write(_campaign);
                long tick=_fight.Sim.CurrentTick;
                _targeting=true;_pilotOn=true; // Stale input/automation must not choose for the player.
                for(int frame=0;frame<120;frame++)yield return null;
                Check(_fight.Sim.CurrentTick==tick&&!_fight.Over,"unpaused 4x waiting has no time limit "+scenario);
                Check(!_pilotOn&&!_targeting,"verdict clears stale targeting and automatic orders "+scenario);
                Check(CampaignSave.Write(_campaign)==state,"waiting preserves full campaign "+scenario);
                Check(_pxStats.key.Contains("WITHOUT A TIME LIMIT"),"header replaces fight clock "+scenario);
                Check(_pxBanner.key.Contains("SPARE")&&_pxBanner2.key.Contains("EXECUTE"),"both outcomes explained "+scenario);
                Check(_orderTitle[0].key.Contains("SPARE")&&_orderTitle[2].key.Contains("EXECUTE"),"verdict button labels "+scenario);
                for(int i=0;i<Orders.Length;i++)
                    Check(_buttons["order-"+i].interactable==(i==0||i==2),"only legal verdict orders enabled "+scenario+" / "+i);
                Check(_orderCost[0].key.Contains("FREE")&&_orderCost[2].key.Contains("FREE"),"both verdict costs are free "+scenario);
                for(int i=0;i<Orders.Length;i++)
                    foreach(var image in _orderEffect[i].root.GetComponentsInChildren<RawImage>())
                        Check(image.rectTransform.anchoredPosition.x+image.rectTransform.rect.width<=128*Px,
                            "verdict explanation stays within its plate "+scenario+" / "+i);
                Check(ClassFighterHint(_fight.Sim.KneelingChief).Contains("No timer")&&!ClassFighterHint(_fight.Sim.KneelingChief).Contains("keep attacking"),"current chief hint matches verdict rules "+scenario);
                Order(Directive.Unleash);
                Check(CampaignSave.Write(_campaign)==state,"unavailable shortcut cannot change verdict "+scenario);
                _notice="Saving failed: validation probe";RefreshFight();
                Check(_pxBanner.key.Contains(_notice),"verdict never hides a save failure notice "+scenario);
                _notice="";RefreshFight();
                HandleUiKey(KeyCode.Space);yield return null;
                Check(_fight.Sim.CurrentTick==tick,"pause toggle cannot advance verdict "+scenario);
                Capture("verdict-"+scenario);
                if(scenario==0)
                {
                    // The transition saves automatically, before the user makes any choice.
                    var automatic=CampaignSave.Read(File.ReadAllText(_savePath));
                    Check(automatic.Active!=null&&automatic.Active.Sim.AwaitingVerdict&&automatic.Active.Sim.CurrentTick==tick,"kneeling transition autosaves the actual state");
                }
                Save();_campaign=CampaignSave.Read(File.ReadAllText(_savePath));ResumeFight();
                yield return null;
                Check(_paused&&_fight.Sim.AwaitingVerdict&&_fight.Sim.CurrentTick==tick,"saved verdict resumes paused and unchanged "+scenario);
                Check(_orderTitle[2].key.Contains("EXECUTE"),"resume restores verdict controls "+scenario);
                string id=_fight.Setup.FightId;
                int points=_fight.Sim.DirectivePoints(0);
                if(execute)Check(Click("order-2"),"execute clicked through real plate");
                else {SelectControl("order-0");HandleUiKey(KeyCode.Return);}
                yield return null;
                Check(_fight.Over&&!_fight.Sim.AwaitingVerdict,"choice ends fight exactly once "+scenario);
                Check(_fight.Sim.DirectivePoints(0)==points,"verdict spends no points "+scenario);
                Check(_fight.Sim.Events.Count(e=>e.Kind==BattleEventKind.Execute||e.Kind==BattleEventKind.Spare)==1,"one verdict event "+scenario);
                Check(_fight.Sim.Events.Any(e=>e.Kind==(execute?BattleEventKind.Execute:BattleEventKind.Spare)&&e.ActorId!=ChiefKits.TimeActor),"player choice is never a timeout "+scenario);
                state=CampaignSave.Write(_campaign);Order(execute?Directive.FocusFire:Directive.Hold);
                Check(CampaignSave.Write(_campaign)==state,"repeat verdict cannot change campaign "+scenario);
                Check(_resultButton.gameObject.activeSelf,"result accessible even after paused verdict "+scenario);
                Check(Click("see-result"),"verdict result opens "+scenario);yield return null;
                Check(_result.Won&&(execute?_result.ChiefExecuted&&_result.ChiefKilled&&!_result.ChiefCaptured:_result.ChiefCaptured&&!_result.ChiefKilled),"authoritative verdict receipt "+scenario);
                Capture("result-"+scenario);
                state=CampaignSave.Write(_campaign);
                File.WriteAllText(Path.Combine(_capture,"result-"+scenario+"-save.txt"),state);
                WatchFight(_campaign.Archive.Find(id));yield return null;
                Check(_playback!=null,"verdict recording opens "+scenario);
                SeekPlayback(_playback.Length);
                Check(_playback.Matched,"verdict replay checksum matches "+scenario);
                Check(CampaignSave.Write(_campaign)==state,"verdict replay leaves campaign unchanged "+scenario);
                _playback=null;SetPage(Page.ChooseTribe);
            }
        }
    }
}
