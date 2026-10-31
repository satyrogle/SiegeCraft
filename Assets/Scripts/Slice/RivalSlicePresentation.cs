using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Slice
{
    // Presentation only: all orders, recruitment and gear still go through the existing run/sim APIs.
    public sealed partial class RivalSliceView
    {
        static readonly Color UiDark = new Color(.075f,.105f,.105f,1f);
        static readonly Color UiSurface = new Color(.12f,.16f,.155f,1);
        static readonly Color UiLine = new Color(.28f,.34f,.30f,1);
        static readonly Color UiPaper = new Color(.94f,.90f,.77f,1);
        static readonly Color UiMuted = new Color(.64f,.71f,.64f,1);
        static readonly Color UiGold = new Color(.87f,.70f,.40f,1);
        static readonly Color UiAlly = new Color(.42f,.75f,.65f,1);
        static readonly Color UiEnemy = new Color(.9f,.43f,.31f,1);

        Canvas _hudCanvas;
        RectTransform _hudRoot, _pageRoot, _tagRoot;
        GameObject _ownedEventSystem;
        Font _hudFont;
        string _presentationKey;
        bool _presentationDirty;
        ScrollRect _menuScroll;
        Text _clockText, _pointsText, _orderHint, _castText, _feedText, _bannerText, _bannerDetail;
        Image _castBackground;
        Button _speedButton, _pauseButton;
        readonly Image[] _courageFill = new Image[2];
        readonly Text[] _courageValue = new Text[2];
        readonly List<Image> _pointPips = new List<Image>();
        readonly List<OrderWidget> _orderWidgets = new List<OrderWidget>();
        readonly List<TagWidget> _tagWidgets = new List<TagWidget>();
        readonly List<Text> _floatWidgets = new List<Text>();
        readonly Dictionary<string,int> _hudArtTops = new Dictionary<string,int>();
        [Serializable] sealed class HudArtEntry { public string name; public int top; }
        [Serializable] sealed class HudArtFile { public HudArtEntry[] entries; }
        sealed class OrderWidget { public Directive Directive; public Button Button; public Text State; public Image Accent; }
        sealed class TagWidget { public RectTransform Root; public Image Health; public Text Name; }

        void BuildPresentation()
        {
            _hudFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var bounds=Resources.Load<TextAsset>("RivalSlice/HudArtBounds");
            if(bounds!=null)
                foreach(var entry in JsonUtility.FromJson<HudArtFile>(bounds.text).entries) _hudArtTops[entry.name]=entry.top;
            var go = new GameObject("Kindred Siege / HUD", typeof(RectTransform));
            _hudCanvas = go.AddComponent<Canvas>();
            _hudCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            _hudCanvas.worldCamera = _camera;
            _hudCanvas.planeDistance = 1f;
            _hudCanvas.sortingOrder = 100;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefWidth, RefHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            go.AddComponent<GraphicRaycaster>();
            if (EventSystem.current == null)
            {
                _ownedEventSystem = new GameObject("Slice UI input", typeof(EventSystem), typeof(StandaloneInputModule));
            }
            _hudRoot = UiRect(go.transform,"Reference frame",0,0,RefWidth,RefHeight);
            _hudRoot.anchorMin = _hudRoot.anchorMax = _hudRoot.pivot = new Vector2(.5f,.5f);
            _hudRoot.anchoredPosition = Vector2.zero;
            RebuildPresentation();
        }

        void LateUpdate()
        {
            if (_hudCanvas == null || _run == null) return;
            string key = CurrentStage + "/" + _fights.Count + "/" + _showCodex + "/" + _showGear + "/" + _showRival;
            if (_presentationDirty || key != _presentationKey) RebuildPresentation();
            if (CurrentStage == Stage.Fight && !_showCodex && !_showGear && !_showRival && _sim != null) UpdateBattleHud();
        }

        void RebuildPresentation()
        {
            float scroll = _menuScroll != null ? _menuScroll.verticalNormalizedPosition : 1f;
            bool sameScreen = _presentationKey == CurrentStage + "/" + _fights.Count + "/" + _showCodex + "/" + _showGear + "/" + _showRival;
            if (_pageRoot != null) { _pageRoot.gameObject.SetActive(false); Destroy(_pageRoot.gameObject); }
            _presentationKey = CurrentStage + "/" + _fights.Count + "/" + _showCodex + "/" + _showGear + "/" + _showRival;
            _presentationDirty = false; _menuScroll = null;
            _tagWidgets.Clear(); _floatWidgets.Clear(); _orderWidgets.Clear(); _pointPips.Clear(); _costRows.Clear();
            _pageRoot = UiRect(_hudRoot,"Screen",0,0,1280,720);
            // The battle gives the top of the screen to the fight; its codex button lives in the pause menu.
            bool battle = CurrentStage == Stage.Fight && !_showCodex && !_showGear && !_showRival;
            if (!battle)
            {
                UiPanel(_pageRoot,"Top strip",0,0,1280,48,UiDark);
                UiText(_pageRoot,"K I N D R E D   S I E G E",30,11,320,28,18,UiPaper,true);
                UiText(_pageRoot,"FALLEN STAR  /  THE BROKEN WASTES",388,14,550,23,12,UiMuted);
                UiButton(_pageRoot,"CODEX",1110,8,138,32,()=>{_showCodex=!_showCodex;_targeting=null;},false);
                UiPanel(_pageRoot,"Brass rule",30,47,1218,1,UiLine,false);
            }
            if (_showCodex) BuildCodexPage();
            else if (_showGear) BuildGearPage();
            else if (_showRival) BuildTacticsPage();
            else if (CurrentStage == Stage.Fight) BuildBattleHud();
            else if (CurrentStage == Stage.BeforeFight) BuildReservePage();
            else if (CurrentStage == Stage.Intro) BuildIntroPage();
            else BuildResultsPage();
            if (_menuScroll != null && sameScreen) _menuScroll.verticalNormalizedPosition=scroll;
        }

        RectTransform _pausePanel;
        readonly List<RectTransform> _costRows = new List<RectTransform>();

        void BuildBattleHud()
        {
            // Two fat courage bars, each draining toward its tribe's banner at the outer edge.
            for(int s=0;s<2;s++)
            {
                float banner=s==0?16:1150,track=s==0?132:668;Color tribe=s==0?UiAlly:UiEnemy;
                var flag=UiPanel(_pageRoot,"Tribe banner",banner,10,114,30,tribe,false);
                UiText(flag.transform,s==0?"CLARTH":EnemyName.ToUpperInvariant(),0,0,114,30,14,UiDark,true,TextAnchor.MiddleCenter);
                UiPanel(_pageRoot,"Courage track",track,14,480,22,new Color(.06f,.08f,.08f,.85f),false);
                _courageFill[s]=UiPanel(_pageRoot,"Courage fill",track,14,480,22,tribe,false);
                if(s==1){var fill=_courageFill[s].rectTransform;fill.pivot=new Vector2(1,1);fill.anchoredPosition=new Vector2(track+480,-14);}
            }
            _tagRoot=UiRect(_pageRoot,"Unit labels",0,0,1280,720);
            _castBackground=UiPanel(_pageRoot,"Telegraph",460,48,360,48,new Color(.08f,.11f,.105f,.92f),false);
            _castText=UiText(_castBackground.transform,"",10,4,340,40,13,UiPaper,true,TextAnchor.MiddleCenter);
            // Pausing opens the codex and the chief's tactics, so neither sits on the battlefield.
            _pausePanel=UiRect(_pageRoot,"Pause menu",462,104,356,36);
            UiButton(_pausePanel,"CODEX",0,0,170,36,()=>{_showCodex=true;_targeting=null;},false);
            UiButton(_pausePanel,"CHIEF TACTICS",186,0,170,36,()=>{_showRival=true;_targeting=null;},false);
            _pausePanel.gameObject.SetActive(false);
            // A persistent two-line event rail avoids covering the bottom formation with banners.
            UiPanel(_pageRoot,"Order console",0,596,1280,124,UiDark);
            UiPanel(_pageRoot,"Console trim",32,596,1216,2,UiGold,false);
            _bannerText=UiText(_pageRoot,"",32,569,760,26,19,UiGold,true);
            _bannerDetail=UiText(_pageRoot,"",32,547,850,20,12,UiPaper);
            for(int i=0;i<_setup.StartingDirectivePoints;i++) _pointPips.Add(UiPanel(_pageRoot,"Order point",32+i*20,611,14,14,UiGold,false));
            _feedText=UiText(_pageRoot,"",150,608,780,20,12,UiMuted);
            _speedButton=UiButton(_pageRoot,"1x SPEED",947,602,137,32,()=>fightSpeed=fightSpeed>1?1:2,false);
            _pauseButton=UiButton(_pageRoot,"PAUSE",1094,602,154,32,()=>_paused=!_paused,false);
            // Each order card shows its key, its name and what it costs. Nothing else.
            for(int i=0;i<OrderButtons.Length;i++)
            {
                int index=i;Directive order=OrderButtons[i];float x=32+i*174;
                var button=UiButton(_pageRoot,"",x,640,164,62,()=>PickOrder(order),false);
                button.gameObject.AddComponent<CanvasGroup>();
                UiText(button.transform,(index+1).ToString(),10,8,19,20,12,UiMuted,true);
                UiText(button.transform,Pretty(Directives.Name(order)).ToUpperInvariant(),28,7,130,24,15,UiPaper,true);
                var cost=UiRect(button.transform,"Cost",12,38,90,12);
                for(int c=0;c<Directives.Cost(order);c++)UiPanel(cost,"Cost dot",c*15,0,10,10,UiGold,false);
                var state=UiText(button.transform,"",12,35,146,16,11,UiMuted,true);
                var accent=UiPanel(button.transform,"Order state",0,59,164,3,UiLine,false);
                _orderWidgets.Add(new OrderWidget{Directive=order,Button=button,State=state,Accent=accent});
                _costRows.Add(cost);
            }
        }

        void UpdateBattleHud()
        {
            for(int s=0;s<2;s++)
                _courageFill[s].rectTransform.sizeDelta=new Vector2(480*Mathf.Clamp01(_sim.Nerve(s)/(float)NerveRules.Full),22);
            int points=_sim.DirectivePoints(0);
            for(int i=0;i<_pointPips.Count;i++)_pointPips[i].color=i<points?UiGold:UiLine;
            SetText(_speedButton.GetComponentInChildren<Text>(),fightSpeed>1?"2x SPEED":"1x SPEED");
            SetText(_pauseButton.GetComponentInChildren<Text>(),_paused?"RESUME":"PAUSE");
            _pausePanel.gameObject.SetActive(_paused);
            for(int i=0;i<_orderWidgets.Count;i++)
            {
                var widget=_orderWidgets[i];Directive d=widget.Directive;
                bool usable=!autopilot&&_sim.CanUse(0,d,out _);
                widget.Button.interactable=usable;
                bool selected=_targeting.HasValue&&_targeting.Value==d;
                widget.Accent.color=selected?UiAlly:usable?UiGold:UiLine;
                widget.Button.GetComponent<CanvasGroup>().alpha=usable||selected?1f:.45f;
                string state=d==Directive.Eclipse&&_run.PlayerEclipseGrade==0?"LOCKED":d==Directive.Frenzy&&_sim.FrenzyUsed(0)?"USED":"";
                SetText(widget.State,state);_costRows[i].gameObject.SetActive(state.Length==0);
            }
            int windup=_sim.WindupTicks(1);
            string move=_sim.WindupIsPyre(1)?"PYRE":"ECLIPSE";
            CastTell cast=EnemyCast();
            string telegraph=cast==CastTell.Dodging?"DODGING\nYour line falls back before it lands."
                :cast==CastTell.PressTwo?"PRESS 2 NOW\nFall Back dodges it."
                :cast==CastTell.NoPoints?"NO POINTS TO DODGE\nKeep one point for Fall Back."
                :cast==CastTell.Coming?"HIS "+move+" IN "+Seconds(windup)+"\n1 Hold halves it. 2 Fall Back dodges it."
                :_paused?"BATTLE PAUSED":_sim.FrenzyTicksLeft(0)>0?"FRENZY  "+Seconds(_sim.FrenzyTicksLeft(0)):_sim.WindupTicks(0)>0?"YOUR ECLIPSE  "+Seconds(_sim.WindupTicks(0)):"";
            _castBackground.gameObject.SetActive(telegraph.Length>0);SetText(_castText,telegraph);
            _castBackground.color=cast==CastTell.Dodging?new Color(.10f,.25f,.22f,.98f):windup>0?new Color(.35f,.16f,.11f,.98f):UiDark;
            string hint=autopilot?"AUTOPILOT":_targeting.HasValue?(_targeting.Value==Directive.Sacrifice?"Click your fighter to sacrifice. Esc cancels.":"Click an enemy to focus. Esc cancels."):_sim.IsEmpowered(0)?"Next order empowered.":"";
            SetText(_feedText,hint.Length>0?hint:_feed.Count>0?_feed[_feed.Count-1]:"Break their courage. Bring their survivors home.");
            _feedText.color=hint.Length>0?UiGold:UiMuted;
            Banner banner=_banners.Count>0?_banners[_banners.Count-1]:null;
            SetText(_bannerText,banner!=null?banner.Text:"");SetText(_bannerDetail,banner!=null?banner.Sub:"");
            if(banner!=null)_bannerText.color=banner.Color;
            UpdateUnitLabels();
            UpdateTells();
        }

        void UpdateUnitLabels()
        {
            // Units carry only a small health bar; their art says who they are.
            while(_tagWidgets.Count<_viewOrder.Count)
            {
                var root=UiRect(_tagRoot,"Unit health",0,0,84,6);
                UiPanel(root,"Health track",0,0,84,6,new Color(.06f,.08f,.08f,.85f),false);
                var hp=UiPanel(root,"Health",1,1,82,4,UiAlly,false);
                _tagWidgets.Add(new TagWidget{Root=root,Health=hp});
            }
            for(int i=0;i<_tagWidgets.Count;i++)
            {
                bool active=i<_viewOrder.Count&&_viewOrder[i].Unit.Alive&&!_viewOrder[i].Fleeing;
                var tag=_tagWidgets[i];tag.Root.gameObject.SetActive(active);if(!active)continue;
                var v=_viewOrder[i];var u=v.Unit;
                Vector2 p=HudPoint(HeadTop(v));
                tag.Root.anchoredPosition=new Vector2(p.x-42,-Mathf.Max(44,p.y-8));
                tag.Health.color=u.Side==0?UiAlly:UiEnemy;
                tag.Health.rectTransform.sizeDelta=new Vector2(82*Mathf.Clamp01(u.Hp/(float)u.MaxHp),4);
            }
            while(_floatWidgets.Count<_floats.Count)
            {
                var text=UiText(_tagRoot,"",0,0,160,30,15,UiPaper,true,TextAnchor.MiddleCenter);
                var outline=text.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.07f,.08f,.07f,.95f);outline.effectDistance=new Vector2(1,-1);
                _floatWidgets.Add(text);
            }
            for(int i=0;i<_floatWidgets.Count;i++)
            {
                Text text=_floatWidgets[i];
                // One small word per unit, just above its health bar. A newer one replaces the last, so
                // nothing climbs onto the fighter standing behind it in the column.
                bool newest=i<_floats.Count;
                for(int j=i+1;newest&&j<_floats.Count;j++)if(_floats[j].UnitId==_floats[i].UnitId)newest=false;
                text.gameObject.SetActive(newest);if(!newest)continue;
                FloatText f=_floats[i];float k=f.Age/f.Life;
                UnitView owner=View(f.UnitId);
                Vector2 p=HudPoint(owner!=null?HeadTop(owner):f.Pos);
                text.rectTransform.anchoredPosition=new Vector2(p.x-80,-(p.y-36-k*12));
                SetText(text,f.Text);text.fontSize=f.Big?21:15;text.color=new Color(f.Color.r,f.Color.g,f.Color.b,1-k);
            }
        }

        // The top of a unit's drawn art, from HudArtBounds when the sheet has an entry.
        Vector3 HeadTop(UnitView v)
        {
            if(v.Art==null)return v.Root.transform.position+Vector3.up*(v.Height+.3f);
            float artTop=_hudArtTops.TryGetValue(v.Art.sprite.texture.name,out int pixels)&&pixels>0?pixels/v.Art.sprite.pixelsPerUnit:v.Art.sprite.bounds.max.y;
            return v.Body.TransformPoint(new Vector3(0,artTop+.03f,0));
        }

        Vector2 HudPoint(Vector3 world)
        {
            // Use the same letterbox scale as the reference canvas. This also works before
            // Unity updates a camera-space canvas transform for a resized render target.
            float scale=Mathf.Min(Screen.width/RefWidth,Screen.height/RefHeight);
            Vector3 screen=_camera.WorldToScreenPoint(world);
            return new Vector2((screen.x-(Screen.width-RefWidth*scale)*.5f)/scale,
                (Screen.height-screen.y-(Screen.height-RefHeight*scale)*.5f)/scale);
        }

        void PageShell(string eyebrow,string title,string subtitle)
        {
            UiPanel(_pageRoot,"Page",20,65,1240,635,UiDark);
            UiText(_pageRoot,eyebrow,42,87,1160,20,12,UiGold,true);
            UiText(_pageRoot,title,40,112,1180,43,31,UiPaper,true);
            UiText(_pageRoot,subtitle,42,162,1160,31,14,UiMuted);
            UiPanel(_pageRoot,"Divider",42,200,1196,1,UiLine,false);
        }

        void BuildIntroPage()
        {
            PageShell("EXPEDITION 01  /  CLARTH TERRITORY","FROM THE WRECKAGE, A WAR PARTY.","Fallen Star settlement · The Broken Wastes");
            UiText(_pageRoot,"FALLEN\nSTAR",62,245,600,135,56,UiPaper,true);
            UiText(_pageRoot,"The Butcher leads. Five fighters follow.\nYou decide when the line holds — and when it breaks.",66,404,670,72,21,UiPaper);
            UiText(_pageRoot,"Break a tribe’s courage to recruit its survivors.\nTake the chief’s recipe. Face him again with what he learned.",66,498,650,67,17,UiMuted);
            UiButton(_pageRoot,"ASSEMBLE YOUR SQUAD  >",66,615,440,54,Continue,true);
            UiPanel(_pageRoot,"Field dossier",826,223,384,439,UiSurface);
            var sheet=LoadSheet("clarth","warden","se");
            if(sheet!=null)UiPortrait(_pageRoot,sheet[0],866,237,292,315);
            UiText(_pageRoot,"CLARTH  /  WARDEN",850,559,337,30,20,UiPaper,true,TextAnchor.MiddleCenter);
            UiText(_pageRoot,"ROCK + SAND DWELLERS",850,596,337,26,12,UiGold,true,TextAnchor.MiddleCenter);
        }

        void BuildReservePage()
        {
            int required=Mathf.Min(SliceRun.SquadSize,_run.Reserve.Count);
            PageShell("WAR PARTY  /  "+_squad.Count+" OF "+required+" SELECTED","CHOOSE YOUR FIVE.","The Butcher leads  ·  Next: "+_run.NextChiefName+"  ·  Tier "+_run.NextTier+"  ·  Enemy courage "+Percent(_run.NextEnemy.Nerve));
            int rows=(_run.Reserve.Count+2)/3;
            RectTransform content=UiScroll(_pageRoot,42,217,1196,370,rows*181);
            for(int i=0;i<_run.Reserve.Count;i++)
            {
                RunFighter f=_run.Reserve[i];bool selected=_squad.Contains(f.Id);float x=(i%3)*400,y=(i/3)*181;
                var card=UiButton(content,"",x,y,386,166,()=>{if(_squad.Contains(f.Id))_squad.Remove(f.Id);else if(_squad.Count<required)_squad.Add(f.Id);_presentationDirty=true;},false);
                card.GetComponent<Image>().color=selected?new Color(.20f,.28f,.24f):UiSurface;
                UiPanel(card.transform,"Selection",0,0,4,166,selected?UiGold:UiLine,false);
                var sheet=LoadSheet(TribePrefix(f.TribeId),f.Chief?"chief":SliceClasses.Name(f.Class).ToLowerInvariant(),"se");
                if(sheet!=null)UiPortrait(card.transform,sheet[0],10,9,126,149);
                UiText(card.transform,selected?"IN SQUAD":"RESERVE",148,15,222,20,11,selected?UiGold:UiMuted,true);
                UiText(card.transform,f.DisplayName,148,41,222,52,19,UiPaper,true);
                UiText(card.transform,"T"+f.Tier+"  /  "+SliceClasses.Stats(f.Class).DefaultRow.ToString().ToUpperInvariant()+" LINE",148,98,223,22,12,UiMuted);
                UiText(card.transform,f.Gear==GearKind.None?"No gear equipped":Gear.Name(f.Gear),148,129,221,25,12,f.Gear==GearKind.None?UiMuted:UiAlly);
            }
            UiText(_pageRoot,"Select a fighter to add or remove them. Recruits keep their tribe and class.",44,597,1180,25,13,UiMuted);
            UiButton(_pageRoot,"DEFAULT SQUAD",42,636,210,44,()=>{_squad.Clear();_squad.AddRange(_run.DefaultSquad());_presentationDirty=true;},false);
            var enter=UiButton(_pageRoot,"ENTER BATTLE  >",911,630,327,50,Continue,true);enter.interactable=_squad.Count==required;
        }

        void BuildResultsPage()
        {
            PageShell(CurrentStage==Stage.War?"TRIBAL RIVALRY":CurrentStage==Stage.Final?"EXPEDITION RECORD":"AFTER THE BATTLE",_cardTitle,"The world keeps moving. Your next choice carries forward.");
            RectTransform content=UiScroll(_pageRoot,44,224,1190,350,Mathf.Max(350,_card.Count*68));
            float y=0;
            foreach(var line in _card)
            {
                UiText(content,line.Text,0,y,1150,62,line.Heading?18:17,line.Color,line.Heading);y+=68;
            }
            if(CurrentStage==Stage.AfterFight&&_run.ForgedGear!=GearKind.None)
            {
                UiButton(_pageRoot,"ASSIGN FORGED GEAR",44,581,330,39,()=>{_showGear=true;_gearFailure="";},false);
                UiText(_pageRoot,"Worn by "+(_run.ForgedWearer!=null?_run.ForgedWearer.DisplayName:"no one"),394,589,790,30,13,UiAlly);
            }
            string label=CurrentStage==Stage.Final?"NEW RUN":CurrentStage==Stage.War?"CHOOSE SQUAD":_run.Over?"RUN RESULTS":_run.WarPending?"WORLD WAR":"CHOOSE SQUAD";
            UiButton(_pageRoot,label+"  >",911,636,327,44,Continue,true);
        }

        void BuildTacticsPage()
        {
            PageShell("RIVAL INTELLIGENCE  /  BATTLE PAUSED",EnemyName.ToUpperInvariant()+" REMEMBERS.","Learned reactions carried by this chief.");
            var content=UiScroll(_pageRoot,44,224,1190,376,TribeMind.HabitCount*65);
            int shown=0;
            for(int i=0;i<TribeMind.HabitCount;i++)
            {
                Habit habit=(Habit)i;if(!_battleEnemy.Mind.HasHabit(habit))continue;
                UiPanel(content,"Reaction marker",0,shown*65+5,4,35,UiEnemy,false);
                UiText(content,Meaning(habit),20,shown*65,1140,57,18,UiPaper);shown++;
            }
            if(shown==0)UiText(content,"No learned reactions yet. This is your first meeting.",0,10,1100,70,22,UiPaper);
            UiButton(_pageRoot,"RETURN TO BATTLE",911,636,327,44,()=>_showRival=false,true);
        }

        void BuildGearPage()
        {
            PageShell("THE BUTCHER’S FORGE",Gear.Name(_run.ForgedGear).ToUpperInvariant(),Gear.Describe(_run.ForgedGear));
            var candidates=_run.GearCandidates();var content=UiScroll(_pageRoot,44,220,1190,370,candidates.Count*74);
            for(int i=0;i<candidates.Count;i++)
            {
                RunFighter f=candidates[i];bool equipped=_run.ForgedWearer==f;float y=i*74;
                UiPanel(content,"Gear candidate",0,y,1165,65,UiSurface);
                UiText(content,f.DisplayName,18,y+12,700,38,19,UiPaper,true);
                UiButton(content,equipped?"EQUIPPED":"GIVE TO FIGHTER",874,y+10,272,44,()=>{ChooseGearWearer(f.Id);_presentationDirty=true;},equipped);
            }
            UiText(_pageRoot,_gearFailure,44,598,1150,30,15,UiEnemy);
            UiButton(_pageRoot,"DONE",911,636,327,44,()=>_showGear=false,true);
        }

        void BuildCodexPage()
        {
            PageShell("CODEX  /  DISCOVERIES THIS SESSION","WHAT YOU BROUGHT BACK.","Tribes, chiefs, moves and recipes discovered during this session.");
            var content=UiScroll(_pageRoot,44,220,1190,376,Mathf.Max(370,_memory.Codex.Count*57));
            int i=0;foreach(string entry in _memory.Codex)
            {
                UiText(content,(i+1).ToString("00"),0,i*57,50,40,15,UiGold,true);
                UiText(content,entry,62,i*57,1100,49,17,UiPaper);i++;
            }
            if(i==0)UiText(content,"Discoveries appear here as you fight, recruit and forge.",0,16,1140,60,22,UiPaper);
            UiButton(_pageRoot,"RETURN",911,636,327,44,()=>_showCodex=false,true);
        }

        RectTransform UiRect(Transform parent,string name,float x,float y,float width,float height)
        {
            var go=new GameObject(name,typeof(RectTransform));go.layer=5;
            var rect=go.GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(width,height);return rect;
        }

        Image UiPanel(Transform parent,string name,float x,float y,float w,float h,Color color,bool block=true)
        {
            var image=UiRect(parent,name,x,y,w,h).gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=block;return image;
        }

        Text UiText(Transform parent,string value,float x,float y,float w,float h,int size,Color color,bool bold=false,TextAnchor align=TextAnchor.UpperLeft)
        {
            var text=UiRect(parent,"Text",x,y,w,h).gameObject.AddComponent<Text>();text.font=_hudFont;
            text.text=value;text.fontSize=size;text.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;text.color=color;
            text.alignment=align;text.raycastTarget=false;text.supportRichText=false;text.verticalOverflow=VerticalWrapMode.Truncate;
            return text;
        }

        Button UiButton(Transform parent,string label,float x,float y,float w,float h,Action action,bool primary)
        {
            var image=UiPanel(parent,label.Length>0?label:"Order",x,y,w,h,primary?UiGold:UiSurface);
            var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.15f,1.15f,1.1f);colors.pressedColor=new Color(.75f,.80f,.75f);colors.disabledColor=new Color(.52f,.58f,.53f);colors.fadeDuration=.08f;button.colors=colors;
            var navigation=button.navigation;navigation.mode=Navigation.Mode.Automatic;button.navigation=navigation;
            UiText(button.transform,label,8,0,w-16,h,13,primary?UiDark:UiPaper,true,TextAnchor.MiddleCenter);
            button.onClick.AddListener(()=>action());return button;
        }

        void UiPortrait(Transform parent,Sprite sprite,float x,float y,float w,float h)
        {
            var image=UiRect(parent,"Portrait",x,y,w,h).gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
        }

        RectTransform UiScroll(Transform parent,float x,float y,float w,float h,float contentHeight)
        {
            var viewport=UiRect(parent,"Scroll viewport",x,y,w,h);
            var background=viewport.gameObject.AddComponent<Image>();background.color=new Color(1,1,1,.001f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content=UiRect(viewport,"Scroll content",0,0,w-12,Mathf.Max(h,contentHeight));
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;
            scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=34;
            _menuScroll=scroll;return content;
        }

        static void SetText(Text text,string value){if(text.text!=value)text.text=value;}
    }
}
