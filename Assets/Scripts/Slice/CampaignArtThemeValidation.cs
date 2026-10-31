using System;
using System.Collections;
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
        // Fixed UI scenarios, public campaign commands and real simulated receipts. No balance sweep.
        IEnumerator ValidateTheme()
        {
            yield return null;
            if(Argument("-ksThemeReopen")!=null)
            {
                string expected=File.ReadAllText(Path.Combine(_capture,"restart-expected.txt"));
                Check(Click("continue"),"theme second-process Continue");yield return null;
                Check(CampaignSave.Write(_campaign)==expected&&_campaign.LoadNote.Length==0,"paired ranked offer survives actual process restart exactly");
                ShowFavours();yield return null;
                Check(CombinedRewardPending&&_buttons.ContainsKey("reward-pal-0")&&_buttons.ContainsKey("reward-gear-0"),"restarted offer shows both reward types");
                Capture("restart-rewards");yield break;
            }
            IntegrationCap(19);NewReviewRoute();yield return null;
            Check(_campaign.PalRoadRun&&_campaign.Offers.Where(e=>e.Node!=null&&e.Node.Kind==RoadNodeKind.Fight).All(e=>e.Node.Pal!=Pal.None),"new review road uses rules19 Pal assignments");
            int index=_campaign.Offers.ToList().FindIndex(e=>e.Node!=null&&e.Node.Pal==Pal.Butcher);
            Check(index>=0&&Click("offer-"+index),"open actual Butcher road fight");yield return null;
            QaChooseDuelist();yield return null;
            Check(Click("start-fight"),"start rules19 fight through UI");yield return null;
            Check(_fight.RulesVersion==19,"new review launches19");
            var finish=QaFinishFight(21);while(finish.MoveNext())yield return finish.Current;
            Check(_result.Won&&CombinedRewardPending,"real victory earns Pal picks and gear together");
            Check(_modal!=null&&_buttons.ContainsKey("reward-pal-0"),"earned Pal reward opens automatically after victory");
            Check(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==_buttons["reward-pal-0"].gameObject,"earned reward selects the first Pal pick instead of banking");
            CloseModal();yield return null;
            var result=_result;string earned=CampaignSave.Write(_campaign);
            File.WriteAllText(Path.Combine(_capture,"earned19.txt"),earned);
            Check(_buttons.ContainsKey("choose-reward")&&!_buttons.ContainsKey("choose-favour")&&!_buttons.ContainsKey("take-spoils"),"result has one combined control with no overlapping legacy buttons");
            Check(Click("choose-reward"),"combined result control raycasts");yield return null;
            Check(_modal.GetComponentsInChildren<UnityEngine.UI.ScrollRect>().Length==0,"all reward summaries fit without hiding a stat behind scrolling");
            Capture("reward-overview");
            int picks=_campaign.FavourOffer.Choices.Count,gears=_campaign.SpoilsChoices.Count;
            for(int i=0;i<picks;i++)
            {
                int before=_campaign.Materials;
                Check(Click("reward-pal-"+i),"Pal summary raycasts "+i);yield return null;
                Check(_campaign.Materials==before&&CombinedRewardPending,"viewing Pal details spends nothing "+i);
                if(i==0)
                {
                    Check(Click("reward-details"),"full reward explanation opens behind Details");yield return null;
                    Check(Click("reward-detail-back"),"full explanation returns to the same unspent pick");yield return null;
                    Check(_campaign.Materials==before&&CombinedRewardPending,"nested reward explanation cannot spend or discard the offer");
                }
                Check(Click("reward-back"),"Pal details return to all rewards "+i);yield return null;
            }
            for(int i=0;i<gears;i++)
            {
                int before=_campaign.Materials;
                Check(Click("reward-gear-"+i),"gear summary raycasts "+i);yield return null;
                Check(_campaign.Materials==before&&CombinedRewardPending,"viewing gear details spends nothing "+i);
                Check(Click("reward-back"),"gear details return to all rewards "+i);yield return null;
            }
            Check(CampaignSave.Write(_campaign)==earned,"all previews leave full authoritative state unchanged");
            Check(Click("bank-reward"),"keep materials control raycasts");yield return null;
            Check(_page==Page.Offers&&CampaignSave.Write(_campaign)==earned,"bank navigates to road without spending or erasing its still-valid offer");
            ShowFavours();yield return null;
            Check(_buttons.ContainsKey("reward-gear-0"),"road Pal entry also shows remaining gear");CloseModal();
            _result=null;SetPage(Page.Offers);ShowFavours();yield return null;
            Check(Click("reward-gear-0"),"road entry previews gear without a retained result");yield return null;
            Check(Click("strip-0"),"road entry takes its still-valid gear");yield return null;
            Check(_page==Page.Offers&&_modal==null&&_campaign.FavourOffer==null,"road gear claim stays on road without a null result page");

            for(int i=0;i<gears+picks;i++)
            {
                bool pal=i>=gears;int choice=pal?i-gears:i;
                _fight=null;_result=result;_campaign=CampaignSave.Read(earned,ReviewTuning);SetPage(Page.Result);yield return null;
                int bank=_campaign.Materials,heat=_campaign.Heat,cost=_campaign.SpoilsMaterials;
                var favour=pal?_campaign.FavourOffer.Choices[choice]:Favour.None;
                string piece=pal?"":_campaign.SpoilsChoices[choice].Id;
                Check(Click("choose-reward"),"reopen combined choice "+i);yield return null;
                Check(Click((pal?"reward-pal-":"reward-gear-")+choice),"preview chosen reward "+i);yield return null;
                Capture("detail-"+i);
                Check(Click((pal?"favour-":"strip-")+choice),"confirm reward through command "+i);yield return null;
                Check(_campaign.FavourOffer==null&&_campaign.SpoilsChoices.Count==0&&_campaign.Materials==bank-cost,"taking one clears both lists and spends once "+i);
                Check(pal?_campaign.FavourRank(favour)==1&&_campaign.Heat==heat:
                    _campaign.FindGear(piece)!=null&&_campaign.Heat==heat+BountyRules.HeatPerStrip,"correct reward and bounty "+i);
                Check(!ClickIfPresent(pal?"favour-"+choice:"strip-"+choice),"spent control cannot be reused "+i);
                string written=CampaignSave.Write(_campaign);
                Check(CampaignSave.Write(CampaignSave.Read(written,ReviewTuning))==written,"taken reward round-trips "+i);
            }

            // Authored QA checkpoint: take a real offered combo, then restore that same offer for rank-up tests.
            // No authoritative state is assigned; the fixture is read by the public save reader.
            var held=CampaignSave.Read(earned,ReviewTuning);var first=held.FavourOffer.Choices[0];
            string offerLines=string.Join("\n",earned.Split('\n').Where(l=>l.StartsWith("spoils\t")||l.StartsWith("favouroffer\t")))+"\n";
            Check(held.TryTakeFavour(0,out _),"authored rank fixture takes a genuinely offered combo");
            string rankSave=CampaignSave.Write(held).Replace("end\n",offerLines+"end\n");
            // The first real win banked only its own price. This authored checkpoint supplies camp funds
            // for two additional, independently purchased ranks; production rewards remain unchanged.
            rankSave=System.Text.RegularExpressions.Regex.Replace(rankSave,@"(?m)^camp\t\d+","camp\t100");
            for(int rank=2;rank<=3;rank++)
            {
                _result=null;_fight=null;_campaign=CampaignSave.Read(rankSave,ReviewTuning);SetPage(Page.Offers);yield return null;
                Check(_campaign.LoadNote.Length==0&&_campaign.FavourRank(first)==rank-1,"rank fixture reads without rollback "+rank);
                string wearer=_campaign.Reserve.First(f=>f.GearId==Campaign.ComboGearId(FavourRules.ComboBend(first))).Id;
                ShowFavours();yield return null;Check(Click("reward-pal-0"),"preview existing combo "+rank);yield return null;
                Check(string.Join(" ",FavourChoiceLines(first).texts).Contains("Upgrade: rank "+(rank-1)+" to "+rank),"upgrade describes actual next rank "+rank);
                Capture("rank-"+rank+"-preview");
                Check(Click("favour-0"),"upgrade existing combo through real button "+rank);yield return null;
                var piece=_campaign.FindGear(Campaign.ComboGearId(FavourRules.ComboBend(first)));
                Check(piece.Rank==rank&&_campaign.Reserve.First(f=>f.Id==wearer).GearId==piece.Id&&_campaign.Gear.Count(g=>g.Id==piece.Id)==1,"rank-up preserves one piece and wearer "+rank);
                rankSave=CampaignSave.Write(_campaign).Replace("end\n",offerLines+"end\n");
                if(rank==2)
                {
                    string canonical=CampaignSave.Write(CampaignSave.Read(rankSave,ReviewTuning));
                    File.WriteAllText(Path.Combine(_capture,"restart-expected.txt"),canonical);
                    File.WriteAllText(Path.Combine(_capture,"restart-save.txt"),canonical);
                }
            }
            _campaign=CampaignSave.Read(earned,ReviewTuning);_result=null;_fight=null;_notice="";SetPage(Page.Offers);
            foreach(var bend in new[]{ClassBend.BlackHoleRound,ClassBend.AroundTheCorner,ClassBend.StoppedBullets})
            {
                for(int rank=1;rank<=3;rank+=2)
                {
                    var ours=new[]{new UnitSpec("clarth.combo",ClassSpells.ClassOf(bend)).WithBend(bend).WithComboRank(rank)};
                    var theirs=bend==ClassBend.BlackHoleRound?
                        new[]{new UnitSpec("raiders.h1",UnitClass.Medic,Row.Front,UnitRole.Fighter,4,5000),
                            new UnitSpec("raiders.h2",UnitClass.Medic,Row.Front,UnitRole.Fighter,4,5000),
                            new UnitSpec("raiders.h3",UnitClass.Medic,Row.Front,UnitRole.Fighter,4,5000)}:
                        new[]{new UnitSpec("raiders.h1",UnitClass.Medic,Row.Front,UnitRole.Fighter,4,5000)};
                    var setup=new BattleSetup("camp."+(4000+(int)bend*10+rank)+".raiders.warparty",4,ours,theirs,
                        new TacticsProfile(),new TacticsProfile(),1200,BattleSetup.DefaultDirectivePoints,NerveRules.Off,0,400,null,null,
                        FightRules.Campaign.WithLayers().WithMarks());
                    var sim=new BattleSimulation(setup);var session=new SimSession(sim);
                    while(!sim.IsFinished)session.Tick();
                    var marks=sim.Events.Where(e=>e.Kind==BattleEventKind.Mark&&e.Detail==MarkRules.Name(MarkRules.Of(bend))).ToArray();
                    var burst=sim.Events.FirstOrDefault(e=>e.Kind==BattleEventKind.Burst);
                    Check(marks.Length>0&&(rank==1||burst.Kind==BattleEventKind.Burst),"real theme mark, plus immediate burst at rank3 "+bend+" rank"+rank);
                    var record=_campaign.Archive.Add(setup,session.Log,sim);WatchFight(record,0,record.Ticks);yield return null;
                    if(rank==1)
                    {
                        SeekPlayback(marks[0].Tick+1);yield return null;
                        var target=ViewSim.Units.First(u=>u.Id==marks[0].TargetId);var read=_butcherUnits[target.Id];
                        int row=MarkRules.Of(bend)==StatusKind.Crush?0:MarkRules.Of(bend)==StatusKind.Exposed?1:2;
                        Check(target.Statuses[MarkRules.Of(bend)].Stacks>0&&read.marks.Skip(row*3).Take(3).All(p=>p.enabled),"actual target displays mark pips "+bend);
                        Check(ButcherFighterHint(ViewSim,target).Contains("1/3"),"hover gives stacks and timer "+bend);
                        Capture("mark-"+bend);
                    }
                    if(burst.Kind==BattleEventKind.Burst)
                    {
                        SeekPlayback(burst.Tick+1);yield return null;
                        Check(_butcherEffects.Any(r=>r.receipt.Kind==BattleEventKind.Burst&&r.corners.Any(c=>c.enabled)),"burst brackets actual target "+bend+" rank"+rank);
                        Check(_butcherFooterShown&&_butcherFooterTitle.key.Contains("BURST"),"named burst receipt "+bend+" rank"+rank);
                        Capture("burst-"+bend+"-"+rank);
                    }
                    SeekPlayback(0);yield return null;
                    Check(_butcherUnits.Values.All(r=>r.marks.All(p=>!p.enabled)),"seek before marks clears all pips "+bend+" rank"+rank);
                    SeekPlayback(record.Ticks);yield return null;Check(_playback.Matched,"theme replay verifies "+bend+" rank"+rank);
                    LeavePlayback();CloseModal();yield return null;
                }
            }
            // Leave an unspent paired ranked offer at this route's actual save path for a new player process.
            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(_capture,"restart-save.txt")),ReviewTuning);
            _result=null;_fight=null;SetPage(Page.Offers);Save();
            Check(CampaignSave.Write(_campaign)==File.ReadAllText(Path.Combine(_capture,"restart-expected.txt")),"saved ranked offer ready for separate process");
            Log("Theme19 scripted UI and rendered replay scenarios complete. No native feel or balance approval claimed.");
        }

        bool ClickIfPresent(string id)=>_buttons.ContainsKey(id)&&Click(id);
    }
}
