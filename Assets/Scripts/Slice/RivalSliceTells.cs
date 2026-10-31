using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Slice
{
    // Presentation only: what the enemy is about to do, and what the last order did.
    public sealed partial class RivalSliceView
    {
        const float PopLife = 1.3f;

        sealed class Pop { public Vector3 World; public string Text; public Color Color; public float Age; }

        RectTransform _tellRoot;
        Text _chiefTell;
        readonly Text[] _shieldTell = new Text[2];
        readonly List<Image> _enemyPips = new List<Image>();
        readonly List<Pop> _pops = new List<Pop>();
        readonly List<Text> _popWidgets = new List<Text>();
        readonly bool[] _castWasPyre = new bool[2];

        /// A big word over the field: DODGED, BLOCKED, INTERRUPTED.
        void AddPop(Vector3 world, string text, Color color)
        {
            _pops.Add(new Pop { World = world, Text = text, Color = color });
        }

        /// The middle of a side's formation at head height, where a pop about the whole line goes.
        Vector3 LineTop(int side)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            for (int i = 0; i < _viewOrder.Count; i++)
            {
                UnitView v = _viewOrder[i];
                if (v.Unit.Side != side || !v.Unit.Alive) continue;
                sum += HeadTop(v);
                count++;
            }
            return count > 0 ? sum / count : Vector3.zero;
        }

        void UpdateTells()
        {
            EnsureTells();
            _tellRoot.SetAsLastSibling();
            for (int s = 0; s < 2; s++)
            {
                if (_sim.WindupTicks(s) > 0) _castWasPyre[s] = _sim.WindupIsPyre(s);
                int hold = _sim.HoldTicksLeft(s);
                SetText(_shieldTell[s], hold > 0 ? "SHIELDS UP  " + Seconds(hold) : string.Empty);
            }
            int theirPoints = _sim.DirectivePoints(1);
            for (int i = 0; i < _enemyPips.Count; i++) _enemyPips[i].color = i < theirPoints ? UiEnemy : UiLine;
            UpdateChiefTell();
            UpdatePops();
        }

        enum CastTell { None, Coming, PressTwo, NoPoints, Dodging }

        /// Where you stand against his cast right now. Every cast warning on screen reads from this.
        CastTell EnemyCast()
        {
            int windup = _sim.WindupTicks(1);
            if (windup <= 0) return CastTell.None;
            if (_fx[SliceRun.PlayerSide].FallBack >= windup) return CastTell.Dodging;
            if (windup > SliceAutopilot.DodgeAtWindup) return CastTell.Coming;
            return _sim.CanUse(SliceRun.PlayerSide, Directive.FallBack, out _) ? CastTell.PressTwo : CastTell.NoPoints;
        }

        /// Over the chief: his cast counting down, then press 2 (or why you can't), then his worn out window.
        void UpdateChiefTell()
        {
            int windup = _sim.WindupTicks(1);
            CastTell cast = EnemyCast();
            BattleUnit leader = _sim.Leader(1);
            UnitView chief = windup > 0 ? View(_sim.WindupCasterId(1))
                : _sim.WornOutTicksLeft(1) > 0 && leader != null ? View(leader.Id) : null;
            bool press = cast == CastTell.PressTwo;
            float pulse = .5f + .5f * Mathf.Sin(Time.unscaledTime * (press ? 14f : 6f));
            if (press)
            {
                for (int i = 0; i < _orderWidgets.Count; i++)
                {
                    if (_orderWidgets[i].Directive != Directive.FallBack) continue;
                    _orderWidgets[i].Accent.color = Color.Lerp(UiGold, Color.white, pulse);
                    _orderWidgets[i].Button.GetComponent<CanvasGroup>().alpha = 1f;
                }
            }
            _chiefTell.gameObject.SetActive(chief != null);
            if (chief == null) return;
            string move = _castWasPyre[1] ? "PYRE" : "ECLIPSE";
            int seconds = Mathf.CeilToInt(windup / (float)SimClock.TicksPerSecond);
            SetText(_chiefTell, cast == CastTell.Dodging ? "DODGING" : press ? "PRESS 2" : cast == CastTell.NoPoints ? "NO POINTS"
                : windup > 0 ? move + "  " + seconds : "WORN OUT  x2");
            _chiefTell.color = cast == CastTell.Dodging ? UiAlly : press ? Color.Lerp(UiGold, Color.white, pulse)
                : cast == CastTell.NoPoints ? UiMuted : windup > 0 ? UiEnemy : UiGold;
            _chiefTell.rectTransform.localScale = Vector3.one * (press ? 1f + .12f * pulse : 1f);
            Vector2 p = HudPoint(HeadTop(chief));
            _chiefTell.rectTransform.anchoredPosition = new Vector2(p.x, -(p.y - 58));
        }

        void UpdatePops()
        {
            for (int i = _pops.Count - 1; i >= 0; i--)
            {
                _pops[i].Age += Time.deltaTime;
                if (_pops[i].Age >= PopLife) _pops.RemoveAt(i);
            }
            while (_popWidgets.Count < _pops.Count)
            {
                Text text = UiText(_tellRoot, string.Empty, 0, 0, 420, 60, 34, UiPaper, true, TextAnchor.MiddleCenter);
                text.rectTransform.pivot = new Vector2(.5f, .5f);
                Outlined(text, 2f);
                _popWidgets.Add(text);
            }
            for (int i = 0; i < _popWidgets.Count; i++)
            {
                Text text = _popWidgets[i];
                bool on = i < _pops.Count;
                text.gameObject.SetActive(on);
                if (!on) continue;
                Pop pop = _pops[i];
                float k = pop.Age / PopLife;
                Vector2 p = HudPoint(pop.World);
                text.rectTransform.anchoredPosition = new Vector2(p.x, -(p.y - 60 - 18 * k));
                text.rectTransform.localScale = Vector3.one * (1f + .4f * Mathf.Clamp01(1f - pop.Age / .15f));
                SetText(text, pop.Text);
                text.color = new Color(pop.Color.r, pop.Color.g, pop.Color.b, k < .7f ? 1f : (1f - k) / .3f);
            }
        }

        /// The tells live on the battle HUD, which is rebuilt whenever a page opens or a fight starts.
        void EnsureTells()
        {
            if (_tellRoot != null && _tellRoot.parent == _tagRoot) return;
            _tellRoot = UiRect(_tagRoot, "Tells", 0, 0, 1280, 720);
            _chiefTell = UiText(_tellRoot, string.Empty, 0, 0, 240, 40, 26, UiPaper, true, TextAnchor.MiddleCenter);
            _chiefTell.rectTransform.pivot = new Vector2(.5f, .5f);
            Outlined(_chiefTell, 2f);
            _shieldTell[0] = UiText(_pageRoot, string.Empty, 132, 40, 300, 18, 12, UiAlly, true);
            _shieldTell[1] = UiText(_pageRoot, string.Empty, 748, 40, 300, 18, 12, UiEnemy, true, TextAnchor.UpperRight);
            Outlined(_shieldTell[0], 1f);
            Outlined(_shieldTell[1], 1f);
            // His order points, mirrored to yours: with his shields showing you can tell when he can answer.
            _enemyPips.Clear();
            for (int i = 0; i < _setup.StartingDirectivePoints; i++)
                _enemyPips.Add(UiPanel(_pageRoot, "Enemy order point", 1134 - i * 20, 42, 14, 14, UiLine, false));
            _popWidgets.Clear();
        }

        static void Outlined(Text text, float width)
        {
            var outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(.07f, .08f, .07f, .95f);
            outline.effectDistance = new Vector2(width, -width);
        }
    }
}
