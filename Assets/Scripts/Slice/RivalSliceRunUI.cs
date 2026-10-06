using System;
using System.Collections.Generic;
using UnityEngine;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Slice
{
    // Frontend state only. SliceRun owns rosters, rewards, war results and progression.
    public sealed partial class RivalSliceView
    {
        readonly List<string> _squad = new List<string>();
        Vector2 _reserveScroll, _cardScroll, _codexScroll;
        bool _showCodex, _showRival, _showGear;
        Vector2 _gearScroll;
        string _gearFailure = string.Empty;
        static readonly Directive[] OrderButtons =
        {
            Directive.Hold, Directive.FallBack, Directive.FocusFire, Directive.Unleash,
            Directive.Sacrifice, Directive.Eclipse, Directive.Frenzy
        };

        string EnemyName => _battleEnemy != null && _battleEnemy.Id == SliceRun.SkullpeakId ? "Skullpeak" : "Bloodridge";

        void StartRun()
        {
            ClearFight();
            _sim = null;
            _session = null;
            _setup = null;
            _battleEnemy = null;
            _fights.Clear();
            _feed.Clear();
            _showCodex = _showRival = _showGear = _paused = false;
            Finished = false;
            Summary = string.Empty;
            _run = SliceRun.Start(_memory, (ulong)Math.Max(1, worldSeed));
            _squad.Clear();
            _squad.AddRange(_run.DefaultSquad());
            _cardScroll = Vector2.zero;
            ShowIntro(Math.Max(1, worldSeed));
        }

        string ArtPiece(BattleUnit unit)
        {
            RunFighter fighter = unit.Side == 0 ? _run.FindFighter(unit.Id) : null;
            return unit.Role == UnitRole.Chief || (fighter != null && fighter.Chief)
                ? "chief" : SliceClasses.Name(unit.Class).ToLowerInvariant();
        }

        static string TribePrefix(string tribeId)
        {
            return tribeId == SliceRun.ClarthId ? "clarth" : tribeId == SliceRun.SkullpeakId ? "skull" : "blood_rot";
        }

        bool PointerInBattle()
        {
            float scale = Mathf.Min(Screen.width / RefWidth, Screen.height / RefHeight);
            float x = (Input.mousePosition.x - (Screen.width - RefWidth * scale) * 0.5f) / scale;
            float y = (Screen.height - Input.mousePosition.y - (Screen.height - RefHeight * scale) * 0.5f) / scale;
            return x >= 20f && x <= 1260f && y >= 145f && y <= 550f;
        }

        static string LessonSummary(Lesson lesson)
        {
            // Describe the actual tactic change. Do not repeat the backend's unverified causal prose.
            if (lesson.Rule == LessonRules.Focus) return "Learned to cover a focused fighter.";
            if (lesson.Rule == LessonRules.Unleash) return "Learned to answer Unleash with Hold.";
            if (lesson.Rule == LessonRules.FallBack) return "Learned to answer Fall Back with Unleash.";
            if (lesson.Rule == LessonRules.Guard) return "Learned to aim ranged attacks away from a guarded fighter.";
            if (lesson.Rule == LessonRules.Threat) return "Remembered the threat of your " + SliceClasses.Name(lesson.Subject) + ".";
            if (lesson.Rule.StartsWith(LessonRules.FadePrefix, StringComparison.Ordinal)) return "An unused learned reaction eased off.";
            return "A tactic changed after this fight.";
        }

        bool PresentRunEvent(BattleEvent e)
        {
            switch (e.Kind)
            {
                case BattleEventKind.Windup:
                    Feed((e.ActorSide == 0 ? "The Butcher" : EnemyName + " chief") + " is winding up " + e.Detail + ".");
                    if (View(e.ActorId) is UnitView caster) Flash(caster, Amber);
                    return true;
                case BattleEventKind.Release:
                    AddBanner((e.ActorSide == 1 ? "ENEMY " : "YOUR ") + "ECLIPSE: " + e.Detail.ToUpperInvariant(),
                        e.Detail == Moves.Dodged ? "The cast missed." : e.Detail == Moves.Soaked ? "Hold softened the impact." : "The cast landed.",
                        e.ActorSide == 0 ? Amber : Rival, 2f);
                    return true;
                case BattleEventKind.Interrupt:
                    AddBanner("CAST INTERRUPTED", e.ActorSide == 0 ? "Your next order is empowered." : "The chief's next order is empowered.", Teal, 2f);
                    return true;
                case BattleEventKind.Transform:
                    AddBanner("THE BUTCHER: FRENZY", "Your Pal and allies strike harder.", Amber, 2f);
                    if (View(e.ActorId) is UnitView pal) Flash(pal, Rival);
                    return true;
                case BattleEventKind.Combo:
                    AddBanner(e.Detail.ToUpperInvariant(), "Orders combined.", Teal, 2f);
                    return true;
                case BattleEventKind.Empowered:
                    SideFx fx = _fx[e.ActorSide];
                    if (e.Detail == Directives.Name(Directive.Hold)) fx.Hold *= 2;
                    if (e.Detail == Directives.Name(Directive.FallBack)) fx.FallBack *= 2;
                    if (e.Detail == Directives.Name(Directive.FocusFire)) fx.Focus *= 2;
                    if (e.Detail == Directives.Name(Directive.Unleash)) fx.Unleash *= 2;
                    Feed(Pretty(e.Detail) + " is empowered.");
                    return true;
                default: return false;
            }
        }

        void DrawOrders()
        {
            var bar = new Rect(20f, 610f, 1240f, 84f);
            Fill(bar, new Color(0.04f, 0.045f, 0.05f, 0.96f));
            int points = _sim.DirectivePoints(0);
            GUI.Label(new Rect(32f, 614f, 620f, 22f), "ORDERS  " + Dots(points) + (_sim.IsEmpowered(0) ? "   NEXT ORDER EMPOWERED" : ""), _notice);
            int refill = _setup.RefillTicks;
            string refillText = points >= _setup.StartingDirectivePoints ? "Points full" : refill > 0 ? "+1 in " + Seconds(refill - _sim.CurrentTick % refill) : "";
            GUI.Label(new Rect(690f, 614f, 210f, 22f), refillText, _small);
            GUI.enabled = !_showRival;
            if (GUI.Button(new Rect(938f, 612f, 142f, 24f), fightSpeed > 1f ? "SPEED 2x" : "SPEED 1x", _button)) fightSpeed = fightSpeed > 1f ? 1f : 2f;
            if (GUI.Button(new Rect(1090f, 612f, 152f, 24f), _paused ? "RESUME" : "PAUSE", _button)) _paused = !_paused;
            GUI.enabled = true;
            for (int i = 0; i < OrderButtons.Length; i++)
            {
                Directive d = OrderButtons[i];
                string text = (i + 1) + " " + Pretty(Directives.Name(d)) + "  [" + Directives.Cost(d) + "]";
                if (d == Directive.Eclipse && _run.PlayerEclipseGrade == 0) text = "6 Eclipse / locked";
                if (d == Directive.Frenzy && _sim.FrenzyUsed(0)) text = "7 Frenzy / used";
                GUI.enabled = !autopilot && !_showCodex && !_showRival && _sim.CanUse(0, d, out _);
                if (GUI.Button(new Rect(32f + i * 174f, 643f, 164f, 39f), text, _button)) PickOrder(d);
                GUI.enabled = true;
            }
            string hint = autopilot ? "AUTOPILOT / Smart policy" : _targeting.HasValue
                ? "Choose " + (_targeting.Value == Directive.Sacrifice ? "your fighter to sacrifice" : "an enemy to focus") + ". Esc cancels."
                : "Your fighters act automatically. Spend orders when they matter.";
            GUI.Label(new Rect(24f, 587f, 1150f, 21f), hint, _notice);
        }

        void DrawRivalPanel()
        {
            int windup = _sim.WindupTicks(1);
            if (windup > 0)
            {
                Fill(new Rect(20f, 106f, 800f, 30f), new Color(0.25f, 0.07f, 0.06f, 0.95f));
                GUI.Label(new Rect(32f, 110f, 780f, 24f), "CHIEF CASTING " + (_sim.WindupIsPyre(1) ? "PYRE" : "ECLIPSE") + "  " + Seconds(windup) + "   /   Interrupt, Hold or Fall Back", _notice);
            }
            else if (_sim.FrenzyTicksLeft(0) > 0)
                GUI.Label(new Rect(28f, 111f, 650f, 24f), "FRENZY  " + Seconds(_sim.FrenzyTicksLeft(0)), _notice);
            else if (_sim.WindupTicks(0) > 0)
                GUI.Label(new Rect(28f, 111f, 650f, 24f), "YOUR CAST  " + Seconds(_sim.WindupTicks(0)), _notice);
            if (GUI.Button(new Rect(1038f, 105f, 216f, 29f), _showRival ? "CLOSE TACTICS" : "CHIEF TACTICS", _button))
            { _showRival = !_showRival; _targeting = null; }
            if (!_showRival) return;
            Fill(new Rect(345f, 170f, 590f, 330f), new Color(0.04f, 0.045f, 0.05f, 0.98f));
            GUI.Label(new Rect(365f, 190f, 550f, 30f), EnemyName.ToUpperInvariant() + " / LEARNED REACTIONS", _title);
            float y = 232f;
            int shown = 0;
            for (int i = 0; i < TribeMind.HabitCount; i++)
            {
                Habit habit = (Habit)i;
                if (!_battleEnemy.Mind.HasHabit(habit)) continue;
                string text = Meaning(habit);
                GUI.Label(new Rect(365f, y, 550f, 40f), text, _body);
                y += 43f;
                shown++;
            }
            if (shown == 0) GUI.Label(new Rect(365f, y, 550f, 50f), "No learned reactions yet.", _body);
            GUI.Label(new Rect(365f, 468f, 550f, 22f), "Battle paused while this panel is open.", _small);
        }

        void DrawFeed()
        {
            if (_feed.Count > 0) GUI.Label(new Rect(28f, 560f, 1220f, 22f), _feed[_feed.Count - 1], _small);
        }

        void DrawBanners()
        {
            if (_banners.Count == 0 || _showRival || _showCodex) return;
            Banner b = _banners[_banners.Count - 1];
            Fill(new Rect(200f, 505f, 880f, 48f), new Color(0f, 0f, 0f, 0.8f));
            Color before = _banner.normal.textColor;
            _banner.normal.textColor = b.Color;
            GUI.Label(new Rect(210f, 508f, 860f, 26f), b.Text, _banner);
            GUI.Label(new Rect(210f, 536f, 860f, 18f), b.Sub, _bannerSub);
            _banner.normal.textColor = before;
        }

        void DrawReserve()
        {
            Fill(new Rect(24f, 76f, 1232f, 608f), new Color(0.04f, 0.045f, 0.05f, 0.98f));
            GUI.Label(new Rect(44f, 90f, 800f, 34f), "PICK YOUR FIVE  /  THE BUTCHER LEADS", _title);
            GUI.Label(new Rect(44f, 125f, 980f, 28f), "Next: " + _run.NextChiefName + "  /  Tier " + _run.NextTier + "  /  Enemy courage " + Percent(_run.NextEnemy.Nerve), _body);
            int required = Mathf.Min(SliceRun.SquadSize, _run.Reserve.Count);
            GUI.Label(new Rect(44f, 156f, 800f, 26f), _squad.Count + " / " + required + " selected. Select a card to add or remove that fighter.", _notice);
            var viewport = new Rect(44f, 192f, 1190f, 416f);
            int rows = (_run.Reserve.Count + 2) / 3;
            _reserveScroll = GUI.BeginScrollView(viewport, _reserveScroll, new Rect(0f, 0f, 1168f, rows * 155f));
            for (int i = 0; i < _run.Reserve.Count; i++)
            {
                RunFighter f = _run.Reserve[i];
                bool selected = _squad.Contains(f.Id);
                Rect cell = new Rect((i % 3) * 389f, (i / 3) * 155f, 375f, 143f);
                Fill(cell, selected ? new Color(0.28f, 0.20f, 0.11f) : new Color(0.095f, 0.10f, 0.11f));
                if (selected) Fill(new Rect(cell.x, cell.y, 4f, cell.height), Amber);
                if (!autopilot && !_showCodex && GUI.Button(cell, GUIContent.none, GUIStyle.none))
                {
                    if (selected) _squad.Remove(f.Id);
                    else if (_squad.Count < required) _squad.Add(f.Id);
                }
                Sprite[] frames = LoadSheet(TribePrefix(f.TribeId), f.Chief ? "chief" : SliceClasses.Name(f.Class).ToLowerInvariant(), "se");
                if (frames != null) DrawPortrait(frames[0], new Rect(cell.x + 6f, cell.y + 5f, 100f, 132f));
                GUI.Label(new Rect(cell.x + 110f, cell.y + 10f, 252f, 44f), f.DisplayName, _body);
                GUI.Label(new Rect(cell.x + 110f, cell.y + 54f, 252f, 22f), "Tier " + f.Tier + "  /  " + SliceClasses.Stats(f.Class).DefaultRow + " row", _small);
                GUI.Label(new Rect(cell.x + 110f, cell.y + 77f, 252f, 22f), "Power " + f.PowerPercent + "%   Health " + f.HealthPercent + "%", _small);
                GUI.Label(new Rect(cell.x + 110f, cell.y + 102f, 252f, 36f), f.Gear == GearKind.None ? "No gear" : Gear.Name(f.Gear), _notice);
            }
            GUI.EndScrollView();
            GUI.enabled = !autopilot && !_showCodex;
            if (GUI.Button(new Rect(44f, 634f, 190f, 34f), "DEFAULT SQUAD", _button)) { _squad.Clear(); _squad.AddRange(_run.DefaultSquad()); }
            GUI.enabled = !autopilot && !_showCodex && _squad.Count == required;
            if (GUI.Button(new Rect(1000f, 630f, 232f, 38f), "ENTER BATTLE", _button)) Continue();
            GUI.enabled = true;
        }

        static void DrawPortrait(Sprite sprite, Rect box)
        {
            Rect region = sprite.textureRect;
            float size = Mathf.Min(box.width / region.width, box.height / region.height);
            Rect dest = new Rect(box.x + (box.width - region.width * size) * 0.5f, box.yMax - region.height * size,
                region.width * size, region.height * size);
            Rect uv = new Rect(region.x / sprite.texture.width, region.y / sprite.texture.height,
                region.width / sprite.texture.width, region.height / sprite.texture.height);
            GUI.DrawTextureWithTexCoords(dest, sprite.texture, uv);
        }

        void DrawCard()
        {
            var box = new Rect(170f, 80f, 940f, 604f);
            Fill(box, new Color(0.04f, 0.045f, 0.05f, 0.98f));
            GUI.Label(new Rect(box.x + 24f, box.y + 20f, box.width - 48f, 64f), _cardTitle, _cardTitleStyle);
            float width = box.width - 76f, height = 12f;
            foreach (CardLine line in _card) height += _body.CalcHeight(new GUIContent(line.Text.Length == 0 ? " " : line.Text), width) + 16f;
            _cardScroll = GUI.BeginScrollView(new Rect(box.x + 24f, box.y + 90f, box.width - 48f, box.height - 216f), _cardScroll,
                new Rect(0f, 0f, width, height));
            float y = 0f;
            foreach (CardLine line in _card)
            {
                GUIStyle style = line.Heading ? _notice : _body;
                Color before = style.normal.textColor;
                style.normal.textColor = line.Color;
                float h = _body.CalcHeight(new GUIContent(line.Text.Length == 0 ? " " : line.Text), width);
                GUI.Label(new Rect(0f, y, width, h), line.Text, style);
                style.normal.textColor = before;
                y += h + 16f;
            }
            GUI.EndScrollView();
            if (CurrentStage == Stage.AfterFight && _run.ForgedWearer != null)
            {
                GUI.Label(new Rect(box.x + 24f, box.yMax - 120f, 635f, 44f),
                    Gear.Name(_run.ForgedGear) + " / " + _run.ForgedWearer.DisplayName, _notice);
                GUI.enabled = !autopilot;
                if (GUI.Button(new Rect(box.xMax - 240f, box.yMax - 112f, 216f, 34f), "CHOOSE GEAR WEARER", _button))
                { _showGear = true; _gearScroll = Vector2.zero; _gearFailure = string.Empty; }
                GUI.enabled = true;
            }
            if (autopilot)
            {
                float hold = CurrentStage == Stage.AfterFight ? CardSeconds + 2f : CardSeconds;
                Fill(new Rect(box.x, box.yMax - 4f, box.width * Mathf.Clamp01(_stageClock / hold), 4f), Amber);
            }
            else
            {
                GUI.enabled = !_showCodex;
                string label = CurrentStage == Stage.Final ? "NEW RUN" : CurrentStage == Stage.Intro || CurrentStage == Stage.War ? "CHOOSE SQUAD" : _run.Over ? "RUN RESULTS" : _run.WarPending ? "WORLD WAR" : "CHOOSE SQUAD";
                if (GUI.Button(new Rect(box.xMax - 240f, box.yMax - 52f, 216f, 36f), label, _button)) Continue();
                GUI.enabled = true;
            }
        }

        void DrawGearChooser()
        {
            Fill(new Rect(24f, 76f, 1232f, 608f), new Color(0.04f, 0.045f, 0.05f, 1f));
            GUI.Label(new Rect(44f, 92f, 1100f, 36f), "THE BUTCHER'S FORGE / " + Gear.Name(_run.ForgedGear).ToUpperInvariant(), _title);
            GUI.Label(new Rect(44f, 138f, 1100f, 32f), Gear.Describe(_run.ForgedGear) + ". Pick an eligible fighter before continuing.", _body);
            List<RunFighter> candidates = _run.GearCandidates();
            _gearScroll = GUI.BeginScrollView(new Rect(44f, 192f, 1190f, 400f), _gearScroll,
                new Rect(0f, 0f, 1154f, Math.Max(400f, candidates.Count * 58f)));
            for (int i = 0; i < candidates.Count; i++)
            {
                RunFighter f = candidates[i];
                string label = (_run.ForgedWearer == f ? "EQUIPPED / " : "GIVE TO / ") + f.DisplayName + " / Tier " + f.Tier;
                if (GUI.Button(new Rect(0f, i * 58f, 1140f, 48f), label, _button))
                    ChooseGearWearer(f.Id);
            }
            GUI.EndScrollView();
            GUI.Label(new Rect(44f, 607f, 910f, 40f), _gearFailure, _body);
            if (GUI.Button(new Rect(1010f, 638f, 220f, 36f), "DONE", _button)) _showGear = false;
        }

        void ChooseGearWearer(string fighterId)
        {
            if (_run.GiveForgedGear(fighterId, out string failure)) _gearFailure = string.Empty;
            else _gearFailure = failure;
        }

        void DrawCodex()
        {
            Fill(new Rect(0f, 54f, RefWidth, RefHeight - 54f), new Color(0.04f, 0.045f, 0.05f, 1f));
            GUI.Label(new Rect(54f, 84f, 1000f, 36f), "CODEX / DISCOVERIES THIS SESSION", _title);
            GUI.Label(new Rect(54f, 128f, 1060f, 42f), "Tribes, caught chiefs, stolen moves and the Butcher's recipes appear here as you discover them.", _body);
            _codexScroll = GUI.BeginScrollView(new Rect(54f, 188f, 1172f, 440f), _codexScroll,
                new Rect(0f, 0f, 1140f, Math.Max(440f, _memory.Codex.Count * 44f)));
            for (int i = 0; i < _memory.Codex.Count; i++)
            {
                Fill(new Rect(0f, i * 44f, 1140f, 38f), new Color(0.10f, 0.105f, 0.11f));
                GUI.Label(new Rect(14f, i * 44f + 7f, 1100f, 26f), _memory.Codex[i], _body);
            }
            GUI.EndScrollView();
            if (GUI.Button(new Rect(1010f, 646f, 210f, 36f), "RETURN", _button)) _showCodex = false;
        }
    }
}
