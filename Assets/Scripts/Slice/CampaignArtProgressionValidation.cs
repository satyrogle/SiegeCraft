using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Public-API campaign fixtures; no owner saves, backend edits, or invented rewards.
        IEnumerator ValidateProgressionWiring()
        {
            _campaign=Campaign.New(21);
            Check(CampaignPilot.PlayTutorial(_campaign,PilotStyle.Reference,21,10),"progression tutorial fixture ready");
            SetPage(Page.ChooseTribe);yield return null;
            Check(Click("forge"),"camp opens the Butcher's forge");yield return null;
            Check(!_buttons["forge-buy"].interactable,"unknown recipe cannot be bought");Capture("01-forge-locked");CloseModal();

            var fixtures=new Dictionary<string,string>();var offerIndexes=new Dictionary<string,int>();var pilotSeeds=new Dictionary<string,ulong>();
            for(ulong seed=1;seed<=40&&fixtures.Count<5;seed++)
            {
                var c=Campaign.New(seed);if(!CampaignPilot.PlayTutorial(c,PilotStyle.Reference,seed,10))continue;
                var picker=new CampaignPilot(PilotStyle.Reference,seed);
                for(int run=0;run<3&&fixtures.Count<5;run++)
                {
                    c.StartRun();
                    for(int n=0;n<12&&c.Stage==CampaignStage.InRun;n++)
                    {
                        int offer=picker.PickOffer(c);ulong pilotSeed=seed*31UL+(ulong)n;
                        string before=CampaignSave.Write(c);
                        var f=c.StartEncounter(offer);var r=CampaignPilot.PlayFight(c,f,new CampaignPilot(PilotStyle.Reference,pilotSeed));
                        string key=r.RecipeLearned?"recipe":r.HuntComing?"hunters-next":r.RunWon?"hunt-win":null;
                        if(key!=null&&!fixtures.ContainsKey(key)){fixtures.Add(key,before);offerIndexes.Add(key,offer);pilotSeeds.Add(key,pilotSeed);}
                    }
                    if(c.Stage==CampaignStage.ChooseTribe&&c.CanForgeTabard(out _)&&!fixtures.ContainsKey("forge"))fixtures.Add("forge",CampaignSave.Write(c));
                    if(RematchClue.For(c,CampaignTribes.BloodridgeId)!=null&&!fixtures.ContainsKey("clue"))fixtures.Add("clue",CampaignSave.Write(c));
                }
            }
            Check(fixtures.Count==5,"real played campaigns supply recipe, forge, hunt and lesson fixtures");
            foreach(var entry in fixtures)File.WriteAllText(Path.Combine(_capture,"fixture-"+entry.Key+".txt"),entry.Value);

            _campaign=CampaignSave.Read(fixtures["forge"]);SetPage(Page.ChooseTribe);yield return null;
            Check(Click("forge"),"learned recipe opens at camp");yield return null;
            int materials=_campaign.Materials;
            Check(_buttons["forge-buy"].interactable==_campaign.CanForgeTabard(out _),"forge availability follows Campaign");Capture("02-forge-ready");
            Check(Click("forge-buy"),"forge purchase button raycast");yield return null;
            Check(_campaign.TabardForged&&_campaign.Materials==materials-CampaignForge.TabardPrice,"forge pays exactly once and unlocks the tabard");
            Check(!_buttons["forge-buy"].interactable,"forged item cannot be bought again");Capture("03-forge-receipt");
            Check(Click("forge-wear"),"forge opens wearer selection");yield return null;
            string wearer=_campaign.Reserve[_campaign.Reserve.Count-1].Id;
            Check(Click("wear-"+wearer),"wearer selection button raycast");yield return null;
            Check(_campaign.TabardWearer==wearer,"camp assignment follows Campaign");
            string mainSave=CampaignSave.Write(_campaign);
            Check(CampaignSave.Read(File.ReadAllText(_savePath)).TabardWearer==wearer,"purchase and wearer were saved to disk");
            File.WriteAllText(Path.Combine(_capture,"expected-save.txt"),mainSave);
            _campaign=null;SetPage(Page.Title);yield return null;
            Check(Click("continue"),"continue reopens the forged camp");yield return null;
            Check(_campaign.TabardForged&&_campaign.TabardWearer==wearer&&_campaign.Materials==materials-CampaignForge.TabardPrice,"continue preserves purchase, materials and assignment");
            Capture("04-camp-forged");
            _savePath=Path.Combine(_capture,"fixture-playing.txt");
            Check(Click("tribe-"+_campaign.Choosable()[0].Id),"start equipped run");yield return null;
            Check(Click("offer-0"),"open equipped squad");yield return null;
            Check(Click("tabard-picker"),"squad opens wearer selection");yield return null;
            wearer=_squad[0];Check(Click("wear-"+wearer),"equip a selected squad fighter");yield return null;
            Capture("05-equipped-squad");
            Check(Click("start-fight"),"equipped squad starts battle");_pilotOn=false;_paused=true;yield return null;
            var wearing=_fight.Sim.FindById(wearer);
            CampaignFighter worn=null;foreach(var f in _fight.Squad)if(f.Id==wearer)worn=f;
            Check(wearing!=null&&worn!=null&&_fight.Sim.FearPercent(wearing)==TribeTraits.FearPercent(worn.TribeId)+CampaignForge.TabardFear,"wearer receives the backend tabard effect");
            Check(FindFigure(wearer).tag.key.Contains("LAVA SUN TABARD"),"battle identifies the tabard wearer");Capture("06-tabard-battle");

            foreach(string key in new[]{"recipe","hunters-next","hunt-win"})
            {
                _fight=null;_campaign=CampaignSave.Read(fixtures[key]);SetPage(Page.Offers);yield return null;
                if(key=="hunt-win")
                {
                    Check(StepName(_campaign)=="final hunt - one fight","hunt path identifies one final fight");Capture("09-hunt-card");
                }
                Check(Click("offer-"+offerIndexes[key]),key+" card raycast");yield return null;
                Check(Click("start-fight"),key+" starts through squad UI");_pilotOn=false;_paused=true;yield return null;
                if(key=="hunt-win")
                {
                    foreach(var a in _figures)if(a.unit.Side==1)
                    {
                        Check(UnitName(a.unit).StartsWith("Hunt",StringComparison.Ordinal),"hunter has an accurate label");
                        foreach(var b in _figures)if(b.unit.Side==1&&a!=b)
                            Check(Mathf.Abs(a.feet.x-b.feet.x)>=96||Mathf.Abs(a.feet.y-b.feet.y)>=120,"hunter token targets do not overlap");
                    }
                    Capture("10-hunt-formation");
                }
                var pilot=new CampaignPilot(PilotStyle.Reference,pilotSeeds[key]);
                while(!_fight.Over){pilot.Step(_fight);_fight.Tick();}
                _endAge=3;DrawBattle();RefreshFight();yield return new WaitForSecondsRealtime(.15f);
                Check(Click("see-result"),key+" result button");yield return null;
                Check(_result.Won,key+" fixture wins");
                if(key=="recipe")Check(_result.RecipeLearned&&string.Join(" ",FightLines(_result,true).texts).Contains(CampaignForge.TabardName),"result announces Butcher's recipe");
                if(key=="hunters-next")Check(_result.HuntComing&&ResultLine(_result).Contains("One final fight"),"last chief result leads to the hunt");
                if(key=="hunt-win")Check(_result.RunWon&&_result.JoinedFighters.Count==0&&_result.MaterialsEarned==_campaign.Tuning.MaterialsPerChief,"hunter victory wins the run, pays correctly and recruits nobody");
                Check(_resultScrolls==0,"progression result text fits without scrolling");Capture("result-"+key);
            }
            _campaign=CampaignSave.Read(fixtures["clue"]);_fight=null;SetPage(Page.ChooseTribe);yield return null;
            var clue=RematchClue.For(_campaign,CampaignTribes.BloodridgeId);
            Check(Click("tribe-"+_campaign.Choosable()[0].Id),"start rematch run");yield return null;
            Check(Click("rematch"),"offers open their remembered lesson");yield return null;Capture("11-rematch-clue");
            string unchanged=CampaignSave.Write(_campaign);
            Check(Click("lesson-check"),"lesson verification button raycast");yield return null;
            Check(_campaign.Archive.TryVerifyLesson(clue.Lesson,out _),"lesson verification succeeds from archive");Capture("12-lesson-verified");
            Check(Click("lesson-source"),"clue opens its source fight");yield return null;
            Check(Click("replay-check"),"source fight replay check button");yield return null;
            Check(_campaign.Archive.TryReplay(clue.FightId,out _,out _),"source replay matches archived fight");Capture("13-source-fight");
            Check(CampaignSave.Write(_campaign)==unchanged,"reading and verifying history changes no campaign state");
            Check(Click("history-back"),"source opens fight history");yield return null;Capture("14-history");
            Check(Effect(Directive.Unleash).Contains("half Hold")&&ShortEffect(Directive.Hold).Contains("nerve"),"order wording exposes both nerve costs");
        }
    }
}
