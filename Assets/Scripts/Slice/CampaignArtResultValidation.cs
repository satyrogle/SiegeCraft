using System;
using System.Collections;
using System.IO;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Fixed recorded fights, not a pilot search or balance sweep. All writes are isolated in _capture.
        IEnumerator ValidateResultScreens()
        {
            string fixtures=Argument("-ksResultValidate");
            var source=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"fixture-clue.txt")));
            _savePath=Path.Combine(_capture,"results-save.txt");
            _pilotOn=false;_paused=true;
            _campaign=Campaign.New(1);
            _fight=_campaign.StartTutorial();
            ReplayResultRecord(source.Archive.Find("camp.1.raiders.tutorial"));
            yield return null;Capture("01-training");
            Check(_result.TutorialWon,"recorded tutorial joins the Clarth");
            Check(Click("after-result"),"tutorial result continues");yield return null;
            _campaign.StartRun();
            int offer=-1;
            for(int i=0;i<_campaign.Offers.Count;i++)if(_campaign.Offers[i].Kind==EncounterKind.WarParty){offer=i;break;}
            Check(offer>=0,"fixed road offers recorded war party");
            _fight=_campaign.StartEncounter(offer);
            ReplayResultRecord(source.Archive.Find("camp.2.raiders.warparty"));
            yield return null;Capture("02-gear-choice");
            Check(_result.Won&&_result.SpoilsOffered!=null,"recorded path victory offers gear");
            int materials=_campaign.Materials,price=_result.MaterialsEarned;
            string gear=_result.SpoilsOffered.Id;
            Check(_pxButtonLabels["after-result"].key.StartsWith("KEEP MATERIALS|",StringComparison.Ordinal),"continue clearly keeps materials");
            string unchanged=CampaignSave.Write(_campaign);
            Check(Click("result-tactics"),"result opens tactics through UI");yield return null;Capture("03-tactics");
            Check(_modal!=null,"tactics popup is visible");
            Check(Click("modal-close"),"tactics closes through UI");yield return null;
            Check(Click("why"),"result still opens fight explanation");yield return null;
            Check(Click("modal-close"),"fight explanation closes");yield return null;
            Check(CampaignSave.Write(_campaign)==unchanged,"reading details changes no rewards or roster");
            Check(Click("take-spoils"),"take gear uses real result control");yield return null;
            Check(_modal!=null&&_campaign.Materials==materials&&_campaign.FindGear(gear)==null,"gear preview spends nothing");
            Check(Click("strip-0"),"strip first piece uses real preview control");yield return null;
            Check(_campaign.Materials==materials-price&&_campaign.FindGear(gear)!=null,"exchange spends materials once and adds the offered gear");
            Check(!_buttons.ContainsKey("take-spoils"),"taken gear cannot be claimed twice");
            string receipt=string.Join(" ",FightLines(_result,false).texts);
            Check(receipt.Contains("Materials exchanged: "+price)&&!receipt.Contains("Materials gained:"),"receipt does not still claim exchanged materials as a gain");
            var restored=CampaignSave.Read(File.ReadAllText(_savePath));
            Check(restored.Materials==_campaign.Materials&&restored.FindGear(gear)!=null,"exchange survives isolated save reload");
            Capture("04-gear-receipt");

            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"fixture-recipe.txt")));
            _fight=_campaign.StartEncounter(0);
            ReplayResultRecord(source.Archive.Find("camp.5.raiders.chief"));
            yield return null;
            Check(_result.ChiefCaptured&&_result.JoinedFighters.Count>0,"recorded chief capture recruits his band");
            string fighters=string.Join(" ",ResultFighterLines(_result,false).texts);
            foreach(var f in _result.JoinedFighters)Check(fighters.Contains(FighterName(f)),"recruit named: "+f.Name);
            Check(string.Join(" ",FightLines(_result,false).texts).Contains("Not gear yet"),"recipe is distinguished from awarded gear");
            Capture("05-chief-captured");

            // One fixed no-order loss, using the same original chief fixture and unchanged tuning.
            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"fixture-recipe.txt")));
            _fight=_campaign.StartEncounter(0);
            while(!_fight.Over)_fight.Tick();
            ShowResult();yield return null;
            Check(!_result.Won,"fixed no-order chief fight loses");
            fighters=string.Join(" ",ResultFighterLines(_result,false).texts);
            foreach(var f in _result.Fallen)Check(fighters.Contains(FighterName(f)),"fallen fighter named: "+f.Name);
            if(_result.Captured!=null)Check(fighters.Contains(FighterName(_result.Captured)),"captured fighter named separately");
            Check(_result.Fallen.Count>0||_result.Captured!=null,"loss fixture exercises a roster consequence");
            Capture("06-defeat");

            _campaign=CampaignSave.Read(File.ReadAllText(Path.Combine(fixtures,"fixture-hunt-win.txt")));
            _fight=_campaign.StartEncounter(0);
            ReplayResultRecord(source.Archive.Find("camp.11.alpha.alphahunt"));
            yield return null;
            Check(_result.RunWon&&_result.JoinedFighters.Count==0,"hunter victory wins run without recruits");
            Check(_pxButtonLabels["after-result"].key.StartsWith("RETURN TO CAMP|",StringComparison.Ordinal),"run-end action names camp");
            Capture("07-run-won");
            Check(_resultScrolls==0,"fixed result screens fit without scrolling");
            Check(Click("after-result"),"finished run returns through UI");yield return null;
            Check(_page==Page.ChooseTribe,"returned to camp after run");
        }

        void ReplayResultRecord(ArchivedFight recorded)
        {
            if(recorded==null)throw new InvalidOperationException("Missing fixed result record.");
            int next=0;
            while(true)
            {
                while(next<recorded.Log.Commands.Count&&recorded.Log.Commands[next].Tick==_fight.Sim.CurrentTick)
                {
                    var order=recorded.Log.Commands[next++];
                    if(!Directives.TryParse(order.TextArg,out Directive directive))throw new InvalidOperationException("Unknown recorded order.");
                    if(!_fight.TryOrder(directive,order.TargetId,out string why))throw new InvalidOperationException("Recorded order refused: "+why);
                }
                if(_fight.Over)break;
                if(_fight.Sim.CurrentTick>=recorded.Ticks)throw new InvalidOperationException("Recorded result did not finish on time.");
                _fight.Tick();
            }
            Check(next==recorded.Log.Commands.Count&&StateChecksum.Compute(_fight.Sim.OrderedStateSnapshot)==recorded.Record.Checksum,
                recorded.FightId+" matches its recorded final state");
            ShowResult();
        }
    }
}
