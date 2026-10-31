using System.Collections;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Isolated presenter regression. Native key injection is checked separately in the player.
        IEnumerator ValidateInputFlow()
        {
            yield return null;
            Check(Click("new"),"new campaign opens the tutorial");yield return null;
            Check(EventSystem.current.currentSelectedGameObject==_buttons["start-tutorial"].gameObject,"tutorial has a keyboard starting point");
            foreach(var submit in new[]{KeyCode.Return,KeyCode.Space})
            {
                SelectControl("title");HandleUiKey(submit);yield return null;
                Check(_page==Page.Title,"MENU submit stays on the title: "+submit);
                yield return null;
                Check(_page==Page.Title,"title does not continue without another press: "+submit);
                int selected=_titleSelected;
                HandleUiKey(KeyCode.Tab);HandleUiKey(KeyCode.Tab,true);
                Check(_titleSelected==selected,"title navigation returns to Continue: "+submit);
                HandleUiKey(submit);yield return null;
                Check(_page==Page.Tutorial,"a separate Continue submit advances exactly one page: "+submit);
            }
            HandleUiKey(KeyCode.Return);yield return null;
            Check(_page==Page.Fight,"Enter reaches combat from the tutorial");
            _pilotOn=false;_paused=true;RefreshFight();
            Check(!EventSystem.current.sendNavigationEvents,"Unity Submit cannot also dispatch Space to a selected button");
            SelectControl("pause");HandleUiKey(KeyCode.Space);
            Check(!_paused,"Space toggles pause once with the pause button selected");HandleUiKey(KeyCode.Space);
            SelectControl("order-3");int points=_fight.Sim.DirectivePoints(0);
            HandleUiKey(KeyCode.Space);
            Check(!_paused&&_fight.Sim.DirectivePoints(0)==points&&_fight.Sim.UnleashTicksLeft(0)==0,"Space does not activate the last selected order");HandleUiKey(KeyCode.Space);

            Check(Click("tactics"),"tactics opens through its pointer raycast");yield return null;
            Check(EventSystem.current.currentSelectedGameObject==_buttons["modal-close"].gameObject,"popup receives keyboard focus");
            foreach(var b in KeyboardButtons())Check(b.transform.IsChildOf(_modal),"popup navigation excludes background controls");
            EventSystem.current.SetSelectedGameObject(_buttons["order-3"].gameObject);HandleUiKey(KeyCode.Return);
            Check(_fight.Sim.DirectivePoints(0)==points&&_modal!=null,"popup rejects a stale background submit");
            Capture("01-tactics-focus");HandleUiKey(KeyCode.Escape);
            Check(_modal==null&&_paused,"Escape closes tactics without resuming the fight");
            Order(Directive.FocusFire);
            Check(_targeting&&KeyboardButtons().Count>0,"Focus Fire exposes keyboard targets");
            foreach(var b in KeyboardButtons())
            {
                var u=_fight.Sim.FindById(b.name.Substring(7));
                Check(u.Alive&&u.Side==1,"target navigation only includes living enemies");
            }
            HandleUiKey(KeyCode.Tab);string target=EventSystem.current.currentSelectedGameObject.name.Substring(7);
            HandleUiKey(KeyCode.Return);RefreshFight();
            Check(!_targeting&&_fight.Sim.FocusedId(0)==target,"Enter focuses the selected enemy through the order API");
            points=_fight.Sim.DirectivePoints(0);Order(Directive.FocusFire);HandleUiKey(KeyCode.Escape);
            Check(!_targeting&&_fight.Sim.DirectivePoints(0)==points,"Escape cancels targeting without spending points");Capture("02-keyboard-target");
            var pilot=new CampaignPilot(PilotStyle.Reference,21);
            while(!_fight.Over){pilot.Step(_fight);_fight.Tick();}
            _campaign=CampaignSave.Read(CampaignSave.Write(_campaign));ResumeFight();RefreshFight();
            Check(_paused&&_resultButton.gameObject.activeSelf,"a reopened finished fight exposes its result while paused");
            Check(EventSystem.current.currentSelectedGameObject==_buttons["see-result"].gameObject,"finished fight gives keyboard focus to its result");
            HandleUiKey(KeyCode.Return);yield return null;
            Check(_page==Page.Result,"Enter opens the resumed fight receipt");

            _fight=null;_campaign=Campaign.New(21);
            Check(CampaignPilot.PlayTutorial(_campaign,PilotStyle.Reference,21,10),"played tutorial creates the camp fixture");
            _hover="stale fighter tooltip";SetPage(Page.ChooseTribe);yield return null;
            Check(_hover!="stale fighter tooltip","page change clears old hover text");
            Check(Click("camp"),"Pals opens from camp");yield return null;
            Check(Click("pal-forge"),"physical camp opens the forge");yield return null;
            Check(!_campOpen&&_modal!=null&&!_buttons.ContainsKey("pal-forge"),"camp to forge cleans old modal controls");
            HandleUiKey(KeyCode.Escape);
            Check(_modal==null,"Escape closes the forge");
            HandleUiKey(KeyCode.Return);yield return null;
            Check(_page==Page.Offers,"Enter starts the next run from camp");
            HandleUiKey(KeyCode.Return);yield return null;
            Check(_page==Page.Squad,"Enter chooses the focused encounter");
            HandleUiKey(KeyCode.Tab);HandleUiKey(KeyCode.Tab,true);
            string squadControl=_squad.Count==0?"fighter-"+_campaign.Reserve[0].Id:"start-fight";
            Check(EventSystem.current.currentSelectedGameObject==_buttons[squadControl].gameObject,"Tab and Shift Tab navigate the squad controls");
            Capture("03-keyboard-squad");
            _savePath=System.IO.Path.Combine(_capture,"input-flow-save.txt");Save();
            Check(CampaignSave.TryReadFile(_savePath,null,out Campaign read,out _)&&read.Stage==_campaign.Stage,"input flow still saves through CampaignSave");
        }
    }
}
