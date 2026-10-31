using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// C6, the spam test (11 Oct 2026). Does one button spam beat playing the fight? Five bots play exactly
    /// the same fights: each fight is set up once, saved, and every bot plays its own copy from that save.
    /// No orders (Passive), Unleash spam, healer focus (FocusSpam: their Medic first, then the most hurt),
    /// reading the tells (Reference: dodges casts, punishes the worn out caster), and a plain sensible bot
    /// that only uses what the screen shows. Raw numbers, no verdicts and no rule changes. KS_N: fights per
    /// cell (default 100). KS_OUT: a folder for the report. Run it on purpose; it never runs alone.
    public class SpamSweepTests
    {
        private static readonly EncounterKind[] Kinds =
            { EncounterKind.WarParty, EncounterKind.HoldTheLine, EncounterKind.Duel, EncounterKind.Trial, EncounterKind.Chief, EncounterKind.AlphaHunt };

        private static readonly string[] BotNames = { "no orders", "Unleash spam", "healer focus", "reads tells", "plain sensible" };

        [Test, Explicit, Category("Bots")]
        public void SpamSweep()
        {
            int n = int.Parse(Env("KS_N", "100"), CultureInfo.InvariantCulture);
            var clock = Stopwatch.StartNew();
            var sb = new StringBuilder();
            sb.AppendLine("SPAM SWEEP " + DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + "  n=" + n +
                " per cell. Every bot plays the same fights, from the same save.");
            sb.AppendLine("won = fights won. s = average length in seconds. orders = orders given. nerve = your nerve left.");
            sb.AppendLine("ours, theirs = fighters standing at the end. recruits = fighters who joined after a chief win.");
            foreach (string tribe in new[] { CampaignTribes.BloodridgeId, CampaignTribes.SkullpeakId })
            {
                foreach (EncounterKind kind in Kinds)
                {
                    List<Situation> situations = Situations(tribe, kind, n);
                    Assert.Greater(situations.Count, 0, tribe + " " + kind + ": no fights found");
                    sb.AppendLine();
                    sb.AppendLine(Short(tribe) + " " + kind + "  (" + situations.Count + " fights)");
                    for (int b = 0; b < BotNames.Length; b++)
                    {
                        var total = new Tally();
                        foreach (Situation s in situations) total.Add(Play(s, b));
                        sb.AppendLine(total.Line(BotNames[b]));
                    }
                }
            }
            sb.AppendLine();
            sb.AppendLine("(" + clock.Elapsed.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " s)");
            string dir = Env("KS_OUT", string.Empty);
            if (dir.Length > 0) File.WriteAllText(Path.Combine(dir, "spam_sweep.txt"), sb.ToString());
            TestContext.WriteLine(sb.ToString());
        }

        /// A fight ready to start: the campaign saved just before it, and which offer it is.
        private sealed class Situation
        {
            public string Save;
            public int Offer;
            public ulong Seed;
        }

        /// The first n campaigns that offer this kind of fight for this tribe. Chiefs come after the set
        /// path, and the Sovereign's hunters after the last chief (11 Oct 2026), which the reading bot plays
        /// to get there; a lost path skips that campaign.
        private static List<Situation> Situations(string tribe, EncounterKind kind, int n)
        {
            var list = new List<Situation>();
            for (ulong i = 0; i < (ulong)n * 4 && list.Count < n; i++)
            {
                ulong seed = 7000UL + i * 7919UL;
                Campaign c = Campaign.New(seed);
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) continue;
                c.ChooseTribe(tribe);
                if (kind == EncounterKind.Chief || kind == EncounterKind.AlphaHunt)
                {
                    var path = new CampaignPilot(PilotStyle.Reference, seed);
                    for (int step = 0; step < 12 && c.Stage == CampaignStage.InRun && IndexOf(c, kind) < 0; step++)
                    {
                        EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(path.PickOffer(c)),
                            new CampaignPilot(PilotStyle.Reference, seed + (ulong)step));
                        if (!r.Won) break;
                    }
                    if (c.Stage != CampaignStage.InRun) continue;
                }
                int offer = IndexOf(c, kind);
                if (offer < 0) continue;
                list.Add(new Situation { Save = CampaignSave.Write(c), Offer = offer, Seed = seed });
            }
            return list;
        }

        private static int IndexOf(Campaign c, EncounterKind kind)
        {
            for (int i = 0; i < c.Offers.Count; i++) if (c.Offers[i].Kind == kind) return i;
            return -1;
        }

        private static Outcome Play(Situation s, int bot)
        {
            Assert.IsTrue(CampaignSave.TryRead(s.Save, null, out Campaign c, out string failure), failure);
            CampaignFight fight = c.StartEncounter(s.Offer);
            CampaignPilot pilot = bot switch
            {
                0 => new CampaignPilot(PilotStyle.Passive, s.Seed),
                1 => new CampaignPilot(PilotStyle.UnleashSpam, s.Seed),
                2 => new CampaignPilot(PilotStyle.FocusSpam, s.Seed),
                3 => new CampaignPilot(PilotStyle.Reference, s.Seed),
                _ => null
            };
            var plain = pilot == null ? new PlainPilot() : null;
            while (!fight.Over)
            {
                if (pilot != null) pilot.Step(fight);
                else plain.Step(fight);
                fight.Tick();
            }
            BattleSimulation sim = fight.Sim;
            int orders = 0;
            foreach (BattleEvent e in sim.Events)
                if (e.Kind == BattleEventKind.Directive && e.ActorSide == 0 && e.Detail != BattleSimulation.BlockedDetail) orders++;
            var outcome = new Outcome
            {
                Ticks = sim.EndTick,
                Orders = orders,
                Nerve = sim.Nerve(0),
                Ours = sim.AliveCount(0),
                Theirs = sim.AliveCount(1)
            };
            EncounterResult result = c.Finish(fight);
            outcome.Won = result.Won;
            outcome.Recruits = result.JoinedFighters == null ? 0 : result.JoinedFighters.Count;
            return outcome;
        }

        private struct Outcome
        {
            public bool Won;
            public long Ticks;
            public int Orders, Nerve, Ours, Theirs, Recruits;
        }

        private sealed class Tally
        {
            private int _fights, _won, _orders, _nerve, _ours, _theirs, _recruits;
            private long _ticks;

            public void Add(Outcome o)
            {
                _fights++;
                if (o.Won) _won++;
                _ticks += o.Ticks;
                _orders += o.Orders;
                _nerve += o.Nerve;
                _ours += o.Ours;
                _theirs += o.Theirs;
                _recruits += o.Recruits;
            }

            public string Line(string bot)
            {
                double f = Math.Max(1, _fights);
                return string.Format(CultureInfo.InvariantCulture,
                    "  {0,-15} won {1,3}%  s {2,5:0.0}  orders {3,4:0.0}  nerve {4,3:0}%  ours {5,3:0.0}  theirs {6,3:0.0}  recruits {7,3:0.0}",
                    bot, _won * 100 / Math.Max(1, _fights), _ticks / f / SimClock.TicksPerSecond, _orders / f, _nerve / f / 10.0,
                    _ours / f, _theirs / f, _recruits / f);
            }
        }

        /// A plain sensible player who only uses what the screen shows: dodge a cast when it is about to
        /// land (brace with Hold where falling back is not allowed), keep Hold up when holding the line,
        /// focus the most hurt enemy that most of the squad can reach, and Unleash with points to spare.
        /// It never reads the tribe's habits.
        private sealed class PlainPilot
        {
            private bool _answered;
            private int _lastWindup;
            private long _focusUntil = -1;
            private long _unleashUntil = -1;

            public void Step(CampaignFight fight)
            {
                BattleSimulation sim = fight.Sim;
                if (fight.Over || sim.CurrentTick < 20) return;
                long t = sim.CurrentTick;
                int w = sim.WindupTicks(1);
                if (w > _lastWindup) _answered = false;
                _lastWindup = w;
                bool holdLine = fight.Encounter.NoFallBack;
                int points = sim.DirectivePoints(0);

                if (w > 0)
                {
                    if (!_answered && w <= CampaignPilot.DodgeAtWindup)
                        _answered = holdLine ? fight.TryOrder(Directive.Hold, null, out _) : fight.TryOrder(Directive.FallBack, null, out _);
                    return;
                }
                if (holdLine)
                {
                    if (sim.HoldTicksLeft(0) == 0 && points >= Directives.Cost(Directive.Hold)) fight.TryOrder(Directive.Hold, null, out _);
                    return;
                }
                int spare = fight.Encounter.EnemyCasts ? Directives.Cost(Directive.FallBack) : 0;
                if (t >= _focusUntil && points >= Directives.Cost(Directive.FocusFire) + spare)
                {
                    BattleUnit best = null;
                    foreach (BattleUnit u in sim.Units)
                    {
                        if (!u.Alive || u.Side != 1) continue;
                        int reach = sim.StrikersOn(u, out int living);
                        if (reach * 2 < living) continue;
                        if (best == null || u.HpPermille < best.HpPermille) best = u;
                    }
                    if (best != null && fight.TryOrder(Directive.FocusFire, best.Id, out _))
                    {
                        _focusUntil = t + BattleSimulation.FocusTicks;
                        points -= Directives.Cost(Directive.FocusFire);
                    }
                }
                if (t >= _unleashUntil && points >= Directives.Cost(Directive.Unleash) + spare + 1 &&
                    fight.TryOrder(Directive.Unleash, null, out _))
                    _unleashUntil = t + BattleSimulation.UnleashTicks;
            }
        }

        private static string Short(string tribeId)
        {
            return tribeId == CampaignTribes.BloodridgeId ? "Bloodridge" : tribeId == CampaignTribes.SkullpeakId ? "Skullpeak" : tribeId;
        }

        private static string Env(string key, string fallback)
        {
            string value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
