using System;
using System.Collections;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // These saved fixtures test the historical button-favour UI, not rules 15's Butcher inventory.
        Campaign LegacyFavourCampaign(string data)=>CampaignSave.Read(data,new CampaignTuning{RulesCap=Campaign.SquadScreenRules});

        void CheckRoadDecisionSlots(string tag)
        {
            Check(_roadDetailSlots.Length==10&&_roadDetailSlots[0].key.StartsWith("RISK|")&&_roadDetailSlots[5].key.StartsWith("REWARD|"),tag+" concise risk and reward are rendered");
            foreach(var slot in _roadDetailSlots)
            {
                string text=(slot.key??"").Split('|')[0];
                Check(_tinyFont.Measure(text,1)<=330&&slot.y+10<=394,tag+" concise road line fits above Details");
            }
        }

        // Full enemy composition moved into DETAILS. Verify the actual pixel text made by that UI,
        // rather than accepting only the backing PxLines or expecting it in the concise road columns.
        string[] FavourRoadPreview(Encounter encounter)
        {
            if(!Campaign.IsRaidChief(encounter.Tribe,encounter.Kind,Math.Min(Campaign.RulesVersion,_campaign.Tuning.RulesCap)))
                return new[]{EnemyPreview(encounter)};
            var chief=_campaign.ChiefOf(encounter.Tribe.Id);
            return new[]{(chief==null?encounter.Tribe.ChiefName:chief.FullName)+" / SOLO RAID CHIEF",
                "Your squad wears down his health. Answer his moves to empty his posture.",
                "At zero posture he kneels. Spare recruits his tribe; Execute takes his piece."};
        }

        void CheckRoadPreviewPixels(Encounter encounter,string tag)
        {
            Check(_modal!=null&&_modal.name=="ROAD DETAILS",tag+" actual road Details modal opens");
            Canvas.ForceUpdateCanvases();
            int width=_modal.GetComponentInChildren<ScrollRect>()==null?636:628;
            var images=_modal.GetComponentsInChildren<RawImage>().Where(i=>i.name=="part"&&i.enabled&&i.texture is Texture2D).ToArray();
            foreach(string preview in FavourRoadPreview(encounter))
            foreach(string line in _tinyFont.Wrap(preview.ToUpperInvariant(),width,1))
            {
                Color32 colour=preview==preview.ToUpperInvariant()?PxGold:PxBone;
                var expected=_tinyFont.Render(line,colour,colour,1,true);
                var pixels=expected.GetPixels32();
                var image=images.FirstOrDefault(i=>i.texture.width==expected.width&&i.texture.height==expected.height&&((Texture2D)i.texture).GetPixels32().SequenceEqual(pixels));
                Destroy(expected);
                Check(image!=null,tag+" Details renders enemy preview: "+line);
                var corners=new Vector3[4];image.rectTransform.GetWorldCorners(corners);
                var lo=_modal.InverseTransformPoint(corners[0]);var hi=_modal.InverseTransformPoint(corners[2]);
                Check(lo.x>=0&&hi.x<=W&&lo.y>=-H&&hi.y<=0,tag+" enemy preview is inside the modal");
                var mask=image.GetComponentInParent<RectMask2D>();
                if(mask!=null)foreach(var corner in corners)
                    Check(RectTransformUtility.RectangleContainsScreenPoint(mask.rectTransform,RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera,corner),_canvas.worldCamera),tag+" enemy preview is inside its visible scroll viewport");
            }
        }

        // Fixed saved-state fixtures and actual UI commands. No tuning, pilots or balance sweeps.
        IEnumerator ValidateFavours()
        {
            string fixtures=Argument("-ksFavourValidate");
            string source=File.ReadAllText(Path.Combine(fixtures,"skullpeak-duel.txt"));
            _savePath=Path.Combine(_capture,"favour-save.txt");
            foreach(string tribe in new[]{"raiders","skullpeak","stoneveil","thundercoast"})
            {
                for(int row=0;row<=3;row++)
                {
                    string data=source.Replace("run\tskullpeak\t0\t-1","run\t"+tribe+"\t"+row+"\t-1");
                    var baseline=LegacyFavourCampaign(data);
                    int count=baseline.Offers.Count;
                    for(int i=0;i<count;i++)
                    {
                        _fight=null;_playback=null;_campaign=LegacyFavourCampaign(data);
                        var e=_campaign.Offers[i];string tag=tribe+"-"+row+"-"+i;
                        bool raid=Campaign.IsRaidChief(e.Tribe,e.Kind,Math.Min(Campaign.RulesVersion,_campaign.Tuning.RulesCap));
                        var previewLines=FavourRoadPreview(e);
                        var preview=e.Node==null?_campaign.ChiefSquad:_campaign.SquadOn(e.Node);
                        Check(preview.SequenceEqual(e.EnemyClasses),tag+" road squad matches encounter");
                        string saved=CampaignSave.Write(_campaign);
                        SetPage(Page.Offers);yield return null;
                        ExecuteEvents.Execute(_buttons["offer-"+i].gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerEnterHandler);
                        yield return null;
                        Check(_roadHover!=null&&previewLines.All(s=>_roadHover.texts.Contains(s)),tag+" pointer hover delivers actual enemy preview");
                        CheckRoadDecisionSlots(tag);
                        if(row==0&&i==0)
                        {
                            Capture(tag+"-road");
                        }
                        ExecuteEvents.Execute(_buttons["offer-"+i].gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerExitHandler);
                        SelectControl("codex");SelectControl("offer-"+i);yield return null;
                        Check(_roadHover==null&&_roadSelected!=null&&previewLines.All(s=>_roadSelected.texts.Contains(s)),tag+" keyboard focus delivers same preview");
                        Check(Click("road-details"),tag+" road Details raycast");yield return null;
                        CheckRoadPreviewPixels(e,tag);
                        if(row==0&&i==0)Capture(tag+"-road-details");
                        Check(Click("modal-close"),tag+" road Details closes through UI");yield return null;
                        Check(_modal==null&&_page==Page.Offers&&CampaignSave.Write(_campaign)==saved,tag+" road Details returns without changing the campaign");
                        if(row==0&&i==0)
                        {
                            Check(Click("codex"),tag+" road codex raycast");yield return null;
                            Check(Click("fight-history"),tag+" fight history accessible");yield return null;
                            Check(_modal!=null&&_modal.name=="FIGHT HISTORY",tag+" history visible");
                            HandleUiKey(KeyCode.Escape);yield return null;
                            Check(_modal==null&&_page==Page.Offers&&_buttons.ContainsKey("favours"),tag+" history returns to road with favours");
                            SelectControl("codex");HandleUiKey(KeyCode.Return);yield return null;
                            Check(_modal!=null&&_modal.name=="CODEX",tag+" Enter opens road codex");
                            HandleUiKey(KeyCode.Escape);yield return null;
                        }
                        Check(CampaignSave.Write(_campaign)==saved,tag+" road drawing leaves campaign unchanged");
                        Check(Click("offer-"+i),tag+" select node");yield return null;
                        if(row==0&&i==0)Capture(tag+"-squad");
                        QaChooseDuelist();yield return null;
                        Check(Click("start-fight"),tag+" start previewed fight");_pilotOn=false;_paused=true;_speed=1;
                        var actual=_fight.Setup.Side(1);
                        if(raid)
                            Check(_fight.Sim.RaidChiefOn&&preview.Count==0&&actual.Count==1&&actual[0].Role==UnitRole.Chief&&
                                actual[0].Class==e.Tribe.ChiefClass&&!actual[0].Reserve,tag+" solo preview spawns only the actual raid chief");
                        else
                        {
                            var band=e.Kind==EncounterKind.Duel?actual:actual.Where(u=>u.Role==UnitRole.Fighter&&!u.Reserve).ToArray();
                            Check(band.Select(u=>u.Class).SequenceEqual(preview)&&actual.Count(u=>!u.Reserve)==e.EnemyUnits,tag+" preview equals spawned starting fighters and total");
                            if(e.Kind==EncounterKind.Chief)
                                Check(actual.Count(u=>u.Role==UnitRole.Chief)==1&&actual.Count(u=>u.Reserve)==ChiefFight.Reserves,
                                    tag+" band chief and separate second-phase reserves are present");
                        }
                    }
                }
            }

            // Camp previews and the live start must use the same deterministic next-run selection.
            _fight=null;
            string camp=string.Join("\n",source.Replace("\r","").Split('\n').Where(l=>!l.StartsWith("run\t")&&!l.StartsWith("road\t")&&!l.StartsWith("offer\t")));
            for(int run=0;run<3;run++)
            {
                _campaign=LegacyFavourCampaign(camp.Replace("seed\t1\n","seed\t"+(run+1)+"\n"));
                var route=_campaign.RunRoute.Select(t=>t.Id).ToArray();
                Check(Route().Select(t=>t.Id).SequenceEqual(route)&&NextRun().Contains(_campaign.NextRunTribe.Name),"camp shows authoritative next route "+run);
                SetPage(Page.ChooseTribe);yield return null;
                if(run==0){Check(Click("camp-run"),"next route popup opens");yield return null;Capture("next-run-route");CloseModal();yield return null;}
                string next=_campaign.NextRunTribe.Id;
                Check(Click("tribe-"+next),"start predicted run "+run);yield return null;
                Check(_campaign.Target.Id==next&&Route().Select(t=>t.Id).SequenceEqual(route),"started route matches preview "+run);
                Save();_campaign=LegacyFavourCampaign(File.ReadAllText(_savePath));
                Check(Route().Select(t=>t.Id).SequenceEqual(route),"route survives save/reload "+run);
            }

            string clean=CampaignSave.Write(LegacyFavourCampaign(source));
            var navigation=ValidateFavourNavigation(clean,camp);
            while(navigation.MoveNext())yield return navigation.Current;
            // An ambush replaces the underlying node's squad and reward.
            string hunted=clean.Replace("end\n","bounty\t100\nend\n");
            _fight=null;_campaign=LegacyFavourCampaign(hunted);
            var ambushNode=_campaign.Road.All.First(n=>_campaign.IsAmbush(n));
            hunted=hunted.Replace("run\tskullpeak\t0\t-1","run\tskullpeak\t"+ambushNode.Row+"\t-1");
            _campaign=LegacyFavourCampaign(hunted);
            int ambush=_campaign.Offers.ToList().FindIndex(e=>e.Kind==EncounterKind.Ambush);
            Check(ambush>=0,"fixed bounty fixture offers ambush");
            var ambushEncounter=_campaign.Offers[ambush];var ambushBand=_campaign.SquadOn(ambushEncounter.Node).ToArray();
            SetPage(Page.Offers);yield return null;
            ExecuteEvents.Execute(_buttons["offer-"+ambush].gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerEnterHandler);
            yield return null;Capture("ambush-road");
            Check(!HasPalReward(ambushEncounter.Node)&&EnemyPreview(ambushEncounter).Contains("captain"),"ambush replaces Pal reward and names captain");
            Check(Click("offer-"+ambush),"ambush selected");yield return null;
            Check(Click("start-fight"),"ambush starts");_pilotOn=false;_paused=true;
            Check(_fight.Setup.Side(1).Where(u=>u.Role==UnitRole.Fighter).Select(u=>u.Class).SequenceEqual(ambushBand)&&
                _fight.Setup.Side(1).Last().Class==ambushEncounter.Hunter.Class,"ambush preview matches band and named captain");
            _fight=null;
            string capped=string.Join("",Enumerable.Range(1,FavourRules.Count).Select(f=>"favour\t"+f+"\t3\n"));
            _campaign=LegacyFavourCampaign(clean.Replace("end\n",capped+"end\n"));
            Check(_campaign.Road.All.Where(n=>n.Pal!=Pal.None).All(n=>!HasPalReward(n)),"all max-rank favours show fallback gear rewards");
            foreach(var pal in FavourRules.Pals)
            {
                var all=FavourRules.Of(pal);
                for(int rank=0;rank<3;rank++)for(int choice=0;choice<all.Count;choice++)
                {
                    string held=rank==0?"":"favour\t"+(int)all[choice]+"\t"+rank+"\n";
                    string offer="favouroffer\t"+(int)pal+"\t20\t"+string.Join("\t",all.Select(f=>((int)f).ToString()))+"\n";
                    _fight=null;_playback=null;_result=null;
                    _campaign=LegacyFavourCampaign(clean.Replace("end\n",held+offer+"end\n"));
                    string tag=pal+"-rank"+rank+"-choice"+choice;
                    Save();ShowTitle();yield return null;
                    Check(Click("continue"),tag+" continue saved offer");yield return null;
                    // Continue uses the product's default tuning; restore this historical fixture's explicit cap.
                    _campaign=LegacyFavourCampaign(CampaignSave.Write(_campaign));SetPage(_page);yield return null;
                    Check(_campaign.FavourOffer!=null&&Click("favours"),tag+" saved offer accessible from road");yield return null;
                    if(choice==0&&rank!=1)Capture(tag+"-offer");
                    int materials=_campaign.Materials,bounty=_campaign.Bounty;
                    Check(Click("modal-close"),tag+" keep materials");yield return null;
                    Check(_campaign.Materials==materials&&_campaign.FavourRank(all[choice])==rank,tag+" keep changes neither rank nor bank");
                    Check(Click("favours"),tag+" reopen before next encounter");yield return null;
                    Check(Click("favour-"+choice),tag+" choose favour");yield return null;
                    Check(_campaign.Materials==materials-20&&_campaign.FavourRank(all[choice])==rank+1&&_campaign.FavourOffer==null&&_campaign.Bounty==bounty,tag+" exchanges once at correct rank without bounty");
                    Check(!_campaign.TryTakeFavour(choice,out _),tag+" duplicate exchange rejected");
                    _campaign=LegacyFavourCampaign(File.ReadAllText(_savePath));
                    Check(_campaign.Materials==materials-20&&_campaign.FavourRank(all[choice])==rank+1&&_campaign.FavourOffer==null,tag+" saved purchase round-trips");
                    if(choice==0&&rank==2){ShowFavours();yield return null;Capture(tag+"-held");CloseModal();yield return null;}
                    var expected=_campaign.RunFavours.ToArray();
                    var fight=_campaign.StartEncounter(0);
                    Check(fight.Setup.Rules(0).Favours.ToArray().SequenceEqual(expected),tag+" next fight receives authoritative favour effects");
                    if(choice==0&&rank==2)
                    {
                        for(int tick=0;tick<12&&!fight.Over;tick++)fight.Tick();
                        var reloaded=LegacyFavourCampaign(CampaignSave.Write(_campaign));
                        Check(reloaded.Active!=null&&reloaded.Active.Sim.OrderedStateSnapshot==fight.Sim.OrderedStateSnapshot&&
                            reloaded.Active.Setup.Rules(0).Favours.ToArray().SequenceEqual(expected),tag+" active fight reload preserves favour state");
                    }
                }
            }

            // A pending offer is not affordable after its materials were spent; UI cannot debit twice or hide why.
            _fight=null;_campaign=LegacyFavourCampaign(clean.Replace("end\n","favouroffer\t1\t99999\t1\t2\t3\nend\n"));
            SetPage(Page.Offers);yield return null;Check(Click("favours"),"unaffordable offer opens");yield return null;
            Check(!_buttons["favour-0"].interactable&&!Click("favour-0"),"unaffordable choice disabled");Capture("unaffordable-offer");
            CloseModal();yield return null;

            // Keeping the materials until the next fight expires the choice, including across a save/reload.
            _campaign=LegacyFavourCampaign(clean.Replace("end\n","favouroffer\t1\t20\t1\t2\t3\nend\n"));
            int bank=_campaign.Materials;
            SetPage(Page.Offers);yield return null;Check(Click("favours"),"decline opens");yield return null;
            Check(Click("modal-close"),"decline keeps materials");yield return null;
            Save();_campaign=LegacyFavourCampaign(File.ReadAllText(_savePath));SetPage(Page.Offers);yield return null;
            Check(Click("offer-0"),"decline selects next encounter");yield return null;
            Check(Click("start-fight"),"decline starts next encounter");_pilotOn=false;_paused=true;
            Check(_campaign.FavourOffer==null&&_campaign.Materials==bank&&_campaign.Favours.Count==0,"next fight expires declined offer without charging");

            // Taking an event is also an authoritative encounter transition.
            _fight=null;_campaign=LegacyFavourCampaign(clean.Replace("end\n","favouroffer\t1\t20\t1\t2\t3\nend\n"));
            var eventNode=_campaign.Road.All.First(n=>n.Kind==RoadNodeKind.Event);
            string eventSave=clean.Replace("run\tskullpeak\t0\t-1","run\tskullpeak\t"+eventNode.Row+"\t-1")
                .Replace("end\n","favouroffer\t1\t20\t1\t2\t3\nend\n");
            _campaign=LegacyFavourCampaign(eventSave);
            eventNode=_campaign.NextNodes.First(n=>n.Kind==RoadNodeKind.Event);
            int free=eventNode.Event.Choices.ToList().FindIndex(c=>c.Cost==0);
            Check(free>=0&&_campaign.TryTakeEvent(eventNode.Lane,free,out _,out _),"free event choice taken through Campaign");
            Check(_campaign.FavourOffer==null,"event expires previous favour offer");

            // One actual earned offer and its result receipt, using an archived fixed command schedule.
            _campaign=LegacyFavourCampaign(source.Replace("run\tskullpeak\t0\t-1","run\traiders\t0\t-1"));
            int pick=_campaign.Offers.ToList().FindIndex(e=>e.Node!=null&&e.Node.Pal!=Pal.None);
            Check(pick>=0,"fixed Bloodridge road contains favour encounter");
            SetPage(Page.Offers);yield return null;Check(Click("offer-"+pick),"earned favour node selected");yield return null;
            Check(Click("start-fight"),"earned favour fight starts");_pilotOn=false;_paused=true;
            var archive=LegacyFavourCampaign(File.ReadAllText(Path.Combine(fixtures,"fixture-clue.txt")));
            var record=archive.Archive.Find("camp.2.raiders.warparty");int cursor=0;
            while(!_fight.Over)
            {
                while(cursor<record.Log.Commands.Count&&record.Log.Commands[cursor].Tick<=_fight.Sim.CurrentTick)
                {
                    var order=record.Log.Commands[cursor++];
                    if(Directives.TryParse(order.TextArg,out Directive directive))_fight.TryOrder(directive,order.TargetId,out _);
                }
                _fight.Tick();
            }
            ShowResult();yield return null;
            Check(_result.Won&&_result.FavourOffered!=null&&_result.SpoilsOffered==null,"real favour victory offers favour instead of gear");
            Capture("earned-favour-result");
            Save();File.Copy(_savePath,Path.Combine(_capture,"review-offer.txt"),true);
            Check(Click("choose-favour"),"result opens earned choices");yield return null;
            Check(Click("favour-0"),"result takes earned favour");yield return null;
            Check(FavourExchanged(_result)&&_screen.Find("MATERIALS EXCHANGED value")!=null,"result receipt identifies actual exchange");
            Capture("earned-favour-receipt");
            string hunt=CampaignSave.Write(LegacyFavourCampaign(File.ReadAllText(Path.Combine(fixtures,"fixture-hunt-win.txt"))));
            int roster=0;
            hunt=string.Join("\n",hunt.Split('\n').Where(l=>!l.StartsWith("lives\t")&&(!l.StartsWith("fighter\t")||++roster<=Campaign.MinSquad)));
            // Authored last-life rules14 fixture: this historical command schedule may lose under
            // current road rules. A real loss on the last life must clear run-only favours too.
            _campaign=LegacyFavourCampaign(hunt.Replace("end\n","lives\t1\nfavour\t1\t3\nend\n"));
            Check(_campaign.Lives==1&&_campaign.FavourRank((Favour)1)==3,"authored final-life hunt holds the legacy favour before combat");
            _fight=_campaign.StartEncounter(0);record=archive.Archive.Find("camp.11.alpha.alphahunt");cursor=0;
            Check(_fight.RulesVersion==Campaign.SquadScreenRules,"last-life hunt retains rules14");
            while(!_fight.Over)
            {
                while(cursor<record.Log.Commands.Count&&record.Log.Commands[cursor].Tick<=_fight.Sim.CurrentTick)
                {
                    var order=record.Log.Commands[cursor++];
                    if(Directives.TryParse(order.TextArg,out Directive directive))_fight.TryOrder(directive,order.TargetId,out _);
                }
                _fight.Tick();
            }
            string fightId=_fight.Setup.FightId;
            var ended=_campaign.Finish(_fight);_fight=null;
            Check(ended.RunOver&&_campaign.Favours.Count==0&&_campaign.FavourOffer==null,"actual run end removes favours and offers");
            WatchFight(_campaign.Archive.Find(fightId));SeekPlayback(_playback.Length);yield return null;
            Check(_playback.AtEnd&&_playback.Matched,"new fight with favour replays to matching checksum");
        }

        // The caller first earns this offer through a real rules-15 fight. Initial cases use that actual
        // offer; the separately labelled occupied-slot case below supplies authored inventory only.
        IEnumerator ValidateButcherChoiceUi()
        {
            var offer=_campaign.FavourOffer;
            Check(offer!=null&&offer.Pal==Pal.Butcher&&offer.Choices.Count>0&&offer.Choices.All(FavourRules.IsCombo),
                "earned Butcher offer contains real combo choices");
            if(offer==null||offer.Choices.Count==0)yield break;
            foreach(var favour in offer.Choices)
            {
                var bend=FavourRules.ComboBend(favour);
                string card=string.Join("\n",FavourChoiceLines(favour).texts);
                Check(!card.ToLowerInvariant().Contains("rank")&&card.Contains(SliceClasses.Name(ClassSpells.ClassOf(bend)))&&
                    card.Contains(ComboAugmentName(bend))&&card.Contains(ComboPactName(bend))&&card.Contains("One gear slot"),
                    "combo card teaches its class, augment, pact and slot without invented ranks");
            }
            string pending=CampaignSave.Write(_campaign);
            ShowFavours();yield return null;
            Check(_modal!=null&&_buttons.ContainsKey("favour-0"),"real combo choice modal opens");
            Check(Click("modal-close"),"combo offer can keep materials");yield return null;
            Check(CampaignSave.Write(_campaign)==pending,"keeping materials preserves the real combo offer");
            var tuning=_campaign.Tuning;
            _campaign=CampaignSave.Read(pending,tuning);
            Check(_campaign.FavourOffer.Choices.SequenceEqual(offer.Choices),"seeded combo offer survives save and reload");
            ShowFavours();yield return null;Capture("butcher-choice");
            Favour chosen=_campaign.FavourOffer.Choices[0];
            ClassBend chosenBend=FavourRules.ComboBend(chosen);
            int bank=_campaign.Materials,cost=_campaign.SpoilsMaterials,bounty=_campaign.Bounty;
            Check(Click("favour-0"),"actual choice button takes a Butcher combo");yield return null;
            var piece=_campaign.FindGear(Campaign.ComboGearId(chosenBend));
            Check(piece!=null&&piece.Bend==chosenBend&&piece.Bonus==0,"chosen combo exists as class gear with no stat bonus");
            Check(_campaign.FavourOffer==null&&_campaign.Materials==bank-cost&&_campaign.Bounty==bounty,
                "real combo pick spends its banked materials exactly once without bounty");
            Check(!_campaign.TryTakeFavour(0,out _),"spent combo offer cannot be taken twice");
            Check(_favourReceiptPlacement==ComboPlacement(chosenBend)&&!_notice.ToLowerInvariant().Contains("rank"),
                "combo receipt reports the actual equipped recipient or unworn inventory");
            Check(_campaign.Favours.All(f=>!FavourRules.IsCombo(f.Favour))&&HeldFavourText().Contains(piece.Name)&&
                HeldFavourText().Contains(ComboPlacement(chosenBend)),"held picks include the combo from gear, with its actual placement");
            string purchased=CampaignSave.Write(_campaign);
            _campaign=CampaignSave.Read(purchased,tuning);
            Check(CampaignSave.Write(_campaign)==purchased&&_campaign.FindGear(piece.Id)!=null,
                "combo ownership and automatic equipment round-trip through the authoritative save");
            _fight=null;CloseModal();SetPage(Page.Offers);yield return null;
            Check(Click("offer-0"),"open a real next-encounter squad page for gear changes");yield return null;
            var fighter=_campaign.Reserve.First(f=>f.GearId==piece.Id);
            ShowGearPicker(fighter.Id);yield return null;
            Check(_buttons.ContainsKey("equip-"+piece.Id)&&!_pxButtonLabels["equip-"+piece.Id].key.Contains("+0%"),
                "gear picker shows a combo instead of a fabricated zero-percent damage reward");
            Capture("butcher-inventory");
            Check(Click("unequip"),"actual gear control unequips the earned combo");yield return null;
            var off=CampaignSave.Read(File.ReadAllText(_savePath),tuning);
            Check(off.FindGear(piece.Id)!=null&&off.Reserve.All(f=>f.GearId!=piece.Id),"unequip retains the combo in the saved inventory");
            ShowGearPicker(fighter.Id);yield return null;
            Check(Click("equip-"+piece.Id),"actual gear control re-equips the earned combo");yield return null;
            var on=CampaignSave.Read(File.ReadAllText(_savePath),tuning);
            Check(on.Reserve.Single(f=>f.Id==fighter.Id).GearId==piece.Id,"re-equipped combo is saved on the chosen fighter");

            // Authored save-state case, separate from the genuine earned-offer coverage above. The choice
            // remains one the real fight offered; only the already-equipped inventory is supplied here.
            _campaign=CampaignSave.Read(pending,tuning);_fight=null;_result=null;
            var matching=_campaign.Reserve.Where(f=>piece.Fits(f.Class)).ToArray();
            var alternatives=FavourRules.Combos(Pal.Butcher).Select(FavourRules.ComboBend)
                .Where(b=>b!=chosenBend&&ClassSpells.ClassOf(b)==ClassSpells.ClassOf(chosenBend)).ToArray();
            Check(matching.Length<=alternatives.Length,"authored occupied-slot fixture has distinct legal class combos");
            string occupied=pending;
            for(int i=0;i<matching.Length;i++)
            {
                var bend=alternatives[i];
                string record="piece\t"+Campaign.ComboGearId(bend)+"\t"+matching[i].TribeId+"\t0\t0\t0\t"+ClassSpells.Name(bend)+
                    "\t"+(int)matching[i].Class+"\t"+(int)bend+"\n";
                occupied=occupied.Replace("end\n",record+"end\n");
            }
            _campaign=CampaignSave.Read(occupied,tuning);
            for(int i=0;i<matching.Length;i++)Check(_campaign.TryEquip(matching[i].Id,Campaign.ComboGearId(alternatives[i]),out _),
                "authored fixture fills the matching class slot through Campaign");
            File.WriteAllText(Path.Combine(_capture,"authored-unworn-offer.txt"),CampaignSave.Write(_campaign));
            SetPage(Page.Offers);yield return null;ShowFavours();yield return null;
            Check(Click("favour-0"),"real combo choice control accepts the offered combo with every class slot occupied");yield return null;
            Check(_campaign.FindGear(piece.Id)!=null&&_campaign.Reserve.All(f=>f.GearId!=piece.Id)&&
                _notice.Contains("not equipped"),"occupied class slots produce an honest unworn combo receipt");
            var unworn=CampaignSave.Read(File.ReadAllText(_savePath),tuning);
            Check(unworn.FindGear(piece.Id)!=null&&unworn.Reserve.All(f=>f.GearId!=piece.Id),"unworn combo survives the saved purchase");
            Check(Click("offer-0"),"choose next encounter to assign the unworn combo");yield return null;
            ShowGearPicker(matching[0].Id);yield return null;Capture("butcher-unworn");
            Check(Click("equip-"+piece.Id),"actual gear picker swaps the unworn combo onto a matching fighter");yield return null;
            var swapped=CampaignSave.Read(File.ReadAllText(_savePath),tuning);
            Check(swapped.Reserve.Single(f=>f.Id==matching[0].Id).GearId==piece.Id&&
                swapped.FindGear(Campaign.ComboGearId(alternatives[0]))!=null,"swap saves the new wearer and retains the displaced combo");

            // Root's next-fight checks continue from the genuine first purchase, not the authored inventory.
            _campaign=CampaignSave.Read(purchased,tuning);_fight=null;_result=null;
            _favourReceiptPlacement=ComboPlacement(chosenBend);_notice=FavourRules.Name(chosen)+". "+_favourReceiptPlacement;
            Save();CloseModal();SetPage(Page.Offers);yield return null;
        }

        IEnumerator ValidateFavourNavigation(string clean,string camp)
        {
            foreach(var pal in FavourRules.Pals)for(int count=1;count<=3;count++)
            {
                _fight=null;_playback=null;_result=null;
                var favours=FavourRules.Of(pal);
                string held=string.Join("",favours.Skip(count).Select(f=>"favour\t"+(int)f+"\t3\n"));
                string offer="favouroffer\t"+(int)pal+"\t20\t"+string.Join("\t",favours.Take(count).Select(f=>(int)f))+"\n";
                _campaign=LegacyFavourCampaign(clean.Replace("end\n",held+offer+"end\n"));
                SetPage(Page.Offers);yield return null;
                string before=CampaignSave.Write(_campaign),tag=pal+"-"+count+"-choices";
                SelectControl("favours");HandleUiKey(KeyCode.Return);yield return null;
                Check(_modal!=null&&_buttons.ContainsKey("favour-"+(count-1))&&!_buttons.ContainsKey("favour-"+count),tag+" Enter opens exact eligible choices");
                var controls=KeyboardButtons();var visited=new System.Collections.Generic.HashSet<string>();
                for(int step=0;step<controls.Count;step++)
                {
                    HandleUiKey(KeyCode.Tab);
                    var selected=EventSystem.current.currentSelectedGameObject;
                    Check(selected!=null&&selected.transform.IsChildOf(_modal),tag+" Tab stays inside modal");
                    visited.Add(selected.name);
                }
                Check(visited.Count==controls.Count,tag+" Tab reaches every enabled control");
                HandleUiKey(KeyCode.Tab,true);HandleUiKey(KeyCode.Tab);
                Check(Click("held-favours"),tag+" held list opens");yield return null;
                Check(_modal.name=="HELD FAVOURS"&&!_buttons.ContainsKey("favour-0"),tag+" old choice controls removed");
                Check(Click("favour-offer"),tag+" back to choice");yield return null;
                Check(_buttons.ContainsKey("favour-0")&&!_buttons.ContainsKey("favour-offer"),tag+" choice restored without stale controls");
                if(pal==Pal.Butcher&&count<3)Capture(tag);
                SelectControl("modal-close");HandleUiKey(KeyCode.Space);yield return null;
                Check(_modal==null&&CampaignSave.Write(_campaign)==before,tag+" Space keeps materials without state change");
                Check(Click("favours"),tag+" pointer reopens choice");yield return null;
                HandleUiKey(KeyCode.Escape);yield return null;
                Check(_modal==null&&CampaignSave.Write(_campaign)==before,tag+" Escape preserves offer and bank");
                SelectControl("favours");HandleUiKey(KeyCode.Return);yield return null;
                SelectControl("favour-"+(count-1));int bank=_campaign.Materials;
                HandleUiKey(KeyCode.Return);yield return null;
                Check(_modal==null&&_campaign.FavourRank(favours[count-1])==1&&_campaign.Materials==bank-20&&_campaign.FavourOffer==null,tag+" Enter purchases exactly once");
            }

            _fight=null;_campaign=LegacyFavourCampaign(camp);
            // Rules14's staged Bloodridge road already has four rows. Compare with the same
            // campaign's unmodified road, so Long Road must add one rather than total four.
            var ordinaryRoad=LegacyFavourCampaign(camp);
            Check(ordinaryRoad.TrySetOmen(Omen.LongRoad,false,out _),"comparison road disables Long Road through Campaign");
            ordinaryRoad.ChooseTribe(_campaign.NextRunTribe.Id);
            int expectedRows=ordinaryRoad.Road.Rows+1;
            Check(_campaign.TrySetOmen(Omen.LongRoad,true,out _),"Long Road enabled through Campaign");
            SetPage(Page.ChooseTribe);yield return null;
            Check(Click("tribe-"+_campaign.NextRunTribe.Id),"Long Road starts from camp");yield return null;
            Check(_campaign.Road.Rows==expectedRows&&_campaign.PathLength==expectedRows,"Long Road adds one row to the authoritative base road");
            Check(_buttons.ContainsKey("codex")&&_buttons.ContainsKey("favours"),"Long Road retains codex and favour controls");
            string state=CampaignSave.Write(_campaign);
            var nodes=_screen.GetComponentsInChildren<EventTrigger>();
            foreach(var node in nodes)
            {
                ExecuteEvents.Execute(node.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerEnterHandler);
                yield return null;
                Check(_roadHover!=null&&_roadDetailSlots.Any(s=>!string.IsNullOrEmpty(s.key)&&!s.key.StartsWith("|")),"Long Road node hover displays detail");
                CheckRoadDecisionSlots("Long Road");
                ExecuteEvents.Execute(node.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerExitHandler);
            }
            Capture("long-road");
            Check(Click("codex"),"Long Road codex clickable");yield return null;
            Check(Click("fight-history"),"Long Road history clickable");yield return null;
            HandleUiKey(KeyCode.Escape);yield return null;
            Check(CampaignSave.Write(_campaign)==state&&_page==Page.Offers,"Long Road navigation preserves run");
        }
    }
}
