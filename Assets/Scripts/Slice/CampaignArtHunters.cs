using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // The hunters on screen (Jacob, 18 Oct 2026: "you wire the hunters in game"; Claude, under a claim in the
        // handoff log): the bounty, the hunters' ladder, their ambushes on the road, each hunter's traits and weakness,
        // their band's moves in the fight and what a hunter fight did. Every rule stays in Campaign and
        // BattleSimulation (HunterCompany, BandTrait); this shows them and passes the clicks on. Plain 2x kit.

        // The move each fighter last wound up, by unit id, so its Release reads as what it was: a heavy swing, an
        // Eclipse, a Firebomb, a War Cry.
        readonly Dictionary<string,string> _windupName=new Dictionary<string,string>();

        /// The hunt after the last chief, or an ambush on the road: the hunters either way.
        static bool HunterFight(EncounterKind kind)=>kind==EncounterKind.AlphaHunt||kind==EncounterKind.Ambush;

        /// "Bounty 2 of 5".
        string BountyText()=>"Bounty "+_campaign.Bounty+" of "+BountyRules.Max;

        /// What a Release was, from the windup before it: "heavy swing", "Eclipse", "Firebomb", "War Cry", "Pyre".
        string CastName(BattleEvent e)
        {
            if(!string.IsNullOrEmpty(e.TargetId))return "heavy swing";
            // A chief can wind up a heavy during its spell. The newest windup for the actor alone is ambiguous.
            var events=ViewSim.Events;
            for(int i=events.Count-1;i>=0;i--)
            {
                var tell=events[i];
                if(tell.Tick<=e.Tick&&tell.Kind==BattleEventKind.Windup&&tell.ActorId==e.ActorId&&tell.Detail!=HeavyRules.Heavy)
                    return tell.Detail;
            }
            if(_windupName.TryGetValue(e.ActorId,out string move)&&move!=HeavyRules.Heavy)return move;
            return ChiefSpells.Name(ViewSim.Spell(e.ActorSide));
        }

        /// The materials beating this hunter pays (Campaign.Finish): a chief's for the hunt, plus the reward for the
        /// hunter's rank. withOmens counts the omens' bonus, as the result does.
        int HunterReward(Encounter e,bool withOmens)
        {
            if(e==null||e.Hunter==null)return 0;
            int materials=(e.Kind==EncounterKind.AlphaHunt?_campaign.Tuning.MaterialsPerChief:0)+HunterRules.Reward(e.HunterRank);
            return withOmens?materials*(100+OmenRules.MaterialsPercentEach*_campaign.Omens.Count)/100:materials;
        }

        /// A hunter in lines: who they are, each trait in the colour of the button that answers it, their weakness,
        /// and what they have done to you. shortly: a card's version, trait names and their buttons only.
        PxLines HunterLines(CampaignHunter h,int rank,bool shortly)
        {
            var lines=new PxLines();
            if(h==null)return lines;
            lines.Add(h.FullName,PxGold).Add("Rank "+rank+"   "+SliceClasses.Name(h.Class)+"   Level "+_campaign.HunterLevel(h),PxBone);
            foreach(var t in h.Traits)
                lines.Add(HunterTraits.Name(t)+": "+(shortly?OrderName(HunterTraits.Answer(t)):HunterTraits.Describe(t)),OrderColour(HunterTraits.Answer(t)));
            lines.Add(HunterWeaknesses.Name(h.Weakness)+(shortly?"":": "+HunterWeaknesses.Describe(h.Weakness)),PxMint);
            if(h.Wins>0)lines.Add("Beat you "+h.Wins+(h.Wins==1?" time":" times"),PxRed);
            if(_campaign.Company.Nemesis==h)lines.Add("Your nemesis: comes for you first",PxRed);
            return lines;
        }

        static void AddLines(PxLines into,PxLines lines)
        {
            for(int i=0;i<lines.texts.Count;i++)into.Add(lines.texts[i],lines.colours[i]);
        }

        /// The hunters' ladder (Campaign.Company) in a popup: top first, you among them.
        void ShowLadder()
        {
            CloseModal();
            var company=_campaign.Company;
            Modal("HUNTERS' LADDER","");
            var lines=new PxLines()
                .Add("You are rank "+company.PlayerRank+" of "+company.Ladder.Count+". Beat a hunter ranked above you and you take their place. Kill one and a recruit joins at the bottom.",PxBone)
                .Add("A hunter who beats you climbs a place, gains levels and learns a trait against the order you gave most. Company might "+
                    _campaign.Might+": every hunter fights that many levels above their own, and it grows every run.",PxMuted)
                .Gap();
            for(int i=0;i<company.Ladder.Count;i++)
            {
                string id=company.Ladder[i];
                if(id==HunterCompany.You){lines.Add((i+1)+"  YOU",PxGold);continue;}
                var h=company.Find(id);
                if(h==null)continue;
                var traits=new List<string>();foreach(var t in h.Traits)traits.Add(HunterTraits.Name(t));
                bool nemesis=company.Nemesis==h;
                lines.Add((i+1)+"  "+h.FullName+"   "+SliceClasses.Name(h.Class)+"   Lv "+_campaign.HunterLevel(h)+"   "+string.Join(", ",traits.ToArray())+
                    "   "+HunterWeaknesses.Name(h.Weakness)+(nemesis?"   NEMESIS":""),nemesis?PxRed:PxBone);
            }
            PxWrite(_modal,42,68,636,lines);
        }

        /// A road node the hunters hold, for the lines under the road.
        PxLines AmbushDetail(int state,Encounter e)
        {
            var h=_campaign.HunterOnRoad;int rank=_campaign.Company.Rank(h);
            string where=state==NodeLit?"":state==NodeHere?" You stand here.":state==NodeAhead?" Ahead on the road.":" Out of your reach now.";
            var lines=new PxLines().Add("Ambush: "+(h==null?"hunters wait here":h.FullName+", rank "+rank+", waits here")+"."+
                (e!=null?" Squad "+e.SquadSize+", enemies "+e.EnemyUnits+".":"")+where,PxRed);
            if(h!=null)
            {
                var traits=new List<string>();foreach(var t in h.Traits)traits.Add(HunterTraits.Name(t)+" ("+OrderName(HunterTraits.Answer(t))+")");
                lines.Add(string.Join(", ",traits.ToArray())+". "+HunterWeaknesses.Name(h.Weakness)+".",PxBone);
            }
            int reward=HunterRules.Reward(rank)*(100+OmenRules.MaterialsPercentEach*_campaign.Omens.Count)/100;
            lines.Add("Win: you take their place on the hunters' ladder, +"+reward+" materials. No gear. You can go round them.",PxMint)
                .Add(LivesRun?"Lose: one life; your squad returns. The hunter climbs and learns against your favourite order.":"Lose: one of yours captured, and the hunter climbs and learns against your favourite order.",PxRed);
            return lines;
        }

        /// What a hunter fight did, for the result's rewards panel. together: fewer words, so a long result fits.
        void HunterResultLines(PxLines lines,EncounterResult r,bool together)
        {
            if(r.Hunter==null)return;
            lines.Gap().Add("THE HUNTERS",PxGold);
            if(r.Won)lines.Add(r.HunterSlain?r.Hunter.FullName+" is dead. A recruit takes the bottom of the ladder.":r.Hunter.FullName+" got away.",r.HunterSlain?PxMint:PxBone);
            if(r.HunterPromoted)lines.Add(r.Hunter.Name+" climbed to rank "+_campaign.Company.Rank(r.Hunter)+", +"+HunterRules.PromotionLevels+" levels.",PxRed);
            if(r.TraitLearned)lines.Add(r.Hunter.Name+" learned "+HunterTraits.Name(r.NewTrait)+(together?"":": "+HunterTraits.Describe(r.NewTrait)),OrderColour(HunterTraits.Answer(r.NewTrait)));
            lines.Add("Your rank on the ladder: "+r.LadderRankAfter+(r.LadderRankAfter<r.LadderRankBefore?", up from "+r.LadderRankBefore:""),
                r.LadderRankAfter<r.LadderRankBefore?PxMint:PxBone);
        }

        /// The fight's tactics panel for a hunter fight: who leads them, their traits, and what rides on it.
        string HunterTracker(Encounter e)
        {
            var b=new StringBuilder("THE HUNTERS, "+BountyText().ToUpperInvariant()+", "+StepName(_campaign).ToUpperInvariant());
            var h=e.Hunter;
            if(h!=null)
            {
                b.Append('\n').Append(h.FullName).Append(", rank ").Append(e.HunterRank).Append(", level ").Append(_campaign.HunterLevel(h)).Append('.');
                foreach(var t in h.Traits)b.Append('\n').Append(HunterTraits.Name(t)).Append(": ").Append(HunterTraits.Describe(t));
                b.Append('\n').Append(HunterWeaknesses.Name(h.Weakness)).Append(": ").Append(HunterWeaknesses.Describe(h.Weakness));
            }
            b.Append("\nWin: +").Append(HunterReward(e,false)).Append(" materials and you climb the hunters' ladder.");
            if(e.Kind==EncounterKind.AlphaHunt)b.Append(" The run is won. The hunters never join.");
            b.Append(LivesRun?"\nLose: one life; your squad returns. The hunter climbs and learns against the order you gave most.":"\nLose: they capture one of yours, and the hunter climbs the ladder and learns against the order you gave most.");
            return b.ToString();
        }

        /// Their cast in the banner when it is not an Eclipse: what it is and the one button that answers it.
        string SpellBanner(BattleSimulation sim,out string second)
        {
            string caster=NameOf(sim.WindupCasterId(1)),left=Seconds(sim.WindupTicks(1));
            switch(sim.Spell(1))
            {
                case ChiefSpell.SteelStorm:
                    second="While their wall stands your hits barely land. Unleash smashes it open.";
                    return caster+" raises a Steel Storm in "+left+".";
                case ChiefSpell.Firebomb:
                    second=sim.ParryCenterOn ? "Time a fresh live Hold at the beat to parry." : _fight.Encounter.NoFallBack?"Fall Back is unavailable here. A Hold cannot stop fire.":"Fall Back to dodge it. A Hold cannot stop fire.";
                    return caster+" throws a Firebomb in "+left+".";
                case ChiefSpell.WarCry:
                    second="Focus Fire on "+caster+" to break it off, or their nerve comes back.";
                    return caster+" winds up a War Cry: "+left+".";
                default:
                    second="";
                    return caster+" is casting: "+left+".";
            }
        }

        /// A band's moves (BandTrait), for the fight feed; null for a reaction that is not a band's.
        string BandLine(BattleEvent e)
        {
            switch(e.Detail)
            {
                case BandRules.ShieldbreakDetail: return NameOf(e.ActorId)+"'s heavy swing broke your Hold: you lose "+e.Amount/10+"% nerve. Fall Back from them, or parry clean.";
                case BandRules.BerserkDetail: return NameOf(e.ActorId)+" is hurt: their band goes berserk for "+Seconds(e.Amount)+(ViewSim.ParryCenterOn ? ", twice as hard. Time Hold at their beats." : ", twice as hard and nothing to parry. Fall Back.");
                case BandRules.SecondWindDetail: return NameOf(e.ActorId)+" rallies them back from breaking. Focus Fire on the hunter: their fall still breaks them.";
                case BandRules.UnshakableDetail: return "Your Unleash cannot shake them. Focus Fire instead.";
                case BandRules.BannerTornDetail: return "Your Unleash tore down their banner: they lose "+e.Amount/10+"% nerve, and now they can break.";
                case BandRules.OffBalanceDetail: return NameOf(e.ActorId)+" swung at nothing and is off balance: their side loses nerve.";
                default: return null;
            }
        }

        /// The big word in the lane for a band's move, or null.
        static string BandWord(string detail)
        {
            switch(detail)
            {
                case BandRules.ShieldbreakDetail: return "SHIELD BROKEN!";
                case BandRules.BerserkDetail: return "BERSERK!";
                case BandRules.SecondWindDetail: return "SECOND WIND!";
                case BandRules.UnshakableDetail: return "UNSHAKABLE!";
                case BandRules.BannerTornDetail: return "BANNER DOWN!";
                case BandRules.OffBalanceDetail: return "OFF BALANCE!";
                default: return null;
            }
        }

        /// Tags over a band's leader: berserk while it lasts, and the banner while it flies.
        void BandTags(Figure f,BattleSimulation sim,List<string> texts,List<Color32> colours)
        {
            var u=f.unit;
            if(!u.IsLeader||u.Side!=1)return;
            if(sim.BerserkTicksLeft(1)>0){texts.Add("BERSERK "+Seconds(sim.BerserkTicksLeft(1)));colours.Add(PxRed);}
            if(sim.BannerUp(1)){texts.Add("BANNER UP");colours.Add(PxGold);}
        }

        // ------------------------------------------------------------ validation

        // Weaker tribes, so the reference pilot, which never parries a heavy swing, walks a run in a capture.
        static CampaignTuning EasyTribes()=>new CampaignTuning{EnemyScaleTier1=40,EnemyScaleTier2=40};

        // The reference pilot's pick, never an ambush: the hunters are met on purpose.
        int NonAmbushOffer(ulong seed)
        {
            int offer=new CampaignPilot(PilotStyle.Reference,seed).PickOffer(_campaign);
            if(_campaign.Offers[offer].Kind==EncounterKind.Ambush)
                for(int i=0;i<_campaign.Offers.Count;i++)if(_campaign.Offers[i].Kind!=EncounterKind.Ambush)offer=i;
            return offer;
        }

        // Plays on headless while this holds and the run is on, never taking an ambush.
        void WalkWhile(Func<bool> on,ulong seed)
        {
            for(int n=0;n<80&&_campaign.Stage==CampaignStage.InRun&&on();n++)
                CampaignPilot.PlayFight(_campaign,_campaign.StartEncounter(NonAmbushOffer(seed)),new CampaignPilot(PilotStyle.Reference,seed*31UL+(ulong)n));
        }

        // From a fight's squad page: starts the fight, checks its hunters, captures it running, plays it out and
        // opens its result. Stepped by hand from ValidateHunters, so a failed check still fails the run.
        IEnumerator HunterFightThrough(string name,PilotStyle style,ulong seed)
        {
            Check(Click("start-fight"),name+" starts");
            _pilot=new CampaignPilot(style,seed);_pilotOn=true;_speed=6;
            yield return null;
            Check(HunterFight(_fight.Encounter.Kind)&&_fight.Encounter.Hunter!=null,name+" is a hunter fight with a named hunter");
            foreach(var f in _figures)
                if(f.unit.Side==1)Check(f.artKey!=null&&f.artKey.StartsWith("hunter_",StringComparison.Ordinal),name+": hunter art on "+f.unit.Id);
            var leader=_fight.Sim.Leader(1);
            Check(leader!=null&&_fight.NameOf(leader.Id)==_fight.Encounter.Hunter.Name,name+": the hunter fights under their own name");
            CheckStagePresentation();
            int frames=0;while(!_fight.Over&&frames++<150)yield return null;
            Capture("h-"+name+"-fight");
            frames=0;while(!_fight.Over&&frames++<3600)yield return null;
            Check(_fight.Over,name+" ends");
            for(int i=0;i<76;i++)yield return null;
            Check(Click("see-result"),name+": the result opens");yield return null;
            Check(_result.Hunter!=null,name+": the result knows its hunter");
            Capture("h-"+name+"-result-"+(_result.Won?"won":"lost"));
        }

        /// The hunters through the real buttons (18 Oct 2026), each part on its own campaign: the ladder at camp, an
        /// ambush from its road node to its result, and the hunt after the last chief from its card to a lost result,
        /// so the hunter climbs.
        IEnumerator ValidateHunters()
        {
            _savePath=Path.Combine(_capture,"hunters-save.txt");
            yield return null;
            _campaign=Campaign.New(5);
            Check(CampaignPilot.PlayTutorial(_campaign,PilotStyle.Reference,5,10),"the tutorial is won for the ladder");
            SetPage(Page.ChooseTribe);yield return null;
            Capture("h-01-camp");
            Check(Click("ladder"),"the camp's LADDER button");yield return null;
            Check(_modal!=null,"the ladder opens");
            Capture("h-02-ladder");
            Check(Click("modal-close"),"the ladder closes");yield return null;

            bool ambush=false;
            for(ulong seed=1;seed<=40&&!ambush;seed++)
            {
                _campaign=Campaign.New(seed,EasyTribes());
                if(!CampaignPilot.PlayTutorial(_campaign,PilotStyle.Reference,seed,10))continue;
                _campaign.StartRun();
                WalkWhile(()=>_campaign.Target==CampaignTribes.Bloodridge,seed);
                if(_campaign.Stage!=CampaignStage.InRun||_campaign.Target!=CampaignTribes.Skullpeak)continue;
                int offer=-1;
                for(int i=0;i<_campaign.Offers.Count;i++)if(_campaign.Offers[i].Kind==EncounterKind.Ambush)offer=i;
                if(offer<0)continue;
                SetPage(Page.Offers);yield return null;
                Check(_buttons.ContainsKey("offer-"+offer),"the ambush has its node on the road");
                Capture("h-03-road-ambush");
                SelectControl("offer-"+offer);yield return null;yield return null;
                Capture("h-04-road-ambush-selected");
                Check(Click("offer-"+offer),"the ambush node opens its squad page");yield return null;
                Check(_page==Page.Squad&&_campaign.Offers[_offer].Kind==EncounterKind.Ambush,"the ambush squad page");
                Capture("h-05-ambush-squad");
                var through=HunterFightThrough("06-ambush",PilotStyle.Reference,seed);
                while(through.MoveNext())yield return through.Current;
                ambush=true;
            }
            Check(ambush,"an ambush played from its road node to its result");

            bool hunt=false;
            for(ulong seed=1;seed<=40&&!hunt;seed++)
            {
                _campaign=Campaign.New(seed,EasyTribes());
                if(!CampaignPilot.PlayTutorial(_campaign,PilotStyle.Reference,seed,10))continue;
                _campaign.StartRun();
                WalkWhile(()=>_campaign.Target!=AlphaHunt.Tribe,seed);
                if(_campaign.Stage!=CampaignStage.InRun||_campaign.Target!=AlphaHunt.Tribe)continue;
                SetPage(Page.Offers);yield return null;
                Check(_campaign.Offers[0].Hunter!=null&&_buttons.ContainsKey("ladder"),"a named hunter leads the hunt, and the ladder is a button away");
                Capture("h-07-hunt-card");
                Check(Click("offer-0"),"the hunt card");yield return null;
                Capture("h-08-hunt-squad");
                var through=HunterFightThrough("09-hunt",PilotStyle.Passive,seed);
                while(through.MoveNext())yield return through.Current;
                if(_result.Won)continue;
                Check(_result.HunterPromoted&&_result.LadderRankAfter>=_result.LadderRankBefore,"the hunter who won climbs the ladder");
                Check(Click("after-result"),"on from the lost hunt");yield return null;
                Check(_page==Page.Offers&&Click("ladder"),"the ladder from the hunt page");yield return null;
                Capture("h-10-ladder-after-loss");
                Check(Click("modal-close"),"the ladder closes again");yield return null;
                hunt=true;
            }
            Check(hunt,"the hunt played from its card to a lost result");
        }
    }
}
