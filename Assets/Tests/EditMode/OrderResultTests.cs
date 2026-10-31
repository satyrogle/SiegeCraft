using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The order results (Jacob, 23 Oct 2026: "go, build the order results in the sim and hand them to Codex";
    /// BattleSimulation.OrderResults). One quick test per piece, only to prove it runs. Nothing here is tuning.
    public class OrderResultTests
    {
        // A fighter at this percent of health, in its class's row: tough, so fights last.
        private static UnitSpec U(string id, UnitClass unitClass, int health = 1000)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, 100, health);
        }

        private static UnitSpec Chief(UnitClass chiefClass, int health = 1000)
        {
            return new UnitSpec("chief", chiefClass, SliceClasses.Stats(chiefClass).DefaultRow, UnitRole.Chief, 100, health);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        // Ours: one of each class.
        private static List<UnitSpec> Ours()
        {
            return Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman), U("h", UnitClass.Medic),
                U("t", UnitClass.Trapper));
        }

        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, SideRules rules,
            FightRules fight, TacticsProfile theirTactics = null)
        {
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), theirTactics ?? new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.Off, 10, 400, new SideRules(1, 0, true), rules,
                fight));
        }

        // A chief who casts his Eclipse every 10 s, behind his band.
        private static BattleSimulation Eclipse(string id, ulong seed)
        {
            return Fight(id, seed, Ours(), Side(U("x.s", UnitClass.Skirmisher), U("x.m", UnitClass.Marksman), Chief(UnitClass.Marksman)),
                new SideRules(2, 300, false), FightRules.Campaign);
        }

        // The Thundercoast chief with only these moves of his kit.
        private static BattleSimulation Thundercoast(string id, ulong seed, ChiefMove moves)
        {
            return Fight(id, seed, Side(U("a", UnitClass.Warden), U("b", UnitClass.Skirmisher), U("m", UnitClass.Marksman)),
                Side(U("y", UnitClass.Marksman), Chief(UnitClass.Warden)),
                new SideRules(2, 0, false, 0, ChiefSpell.Tide, flood: true, kit: ChiefKit.Thundercoast, moves: moves),
                FightRules.CampaignChiefTactics);
        }

        private static bool Order(SimSession session, string directive, string target = "", int arg0 = 0)
        {
            return session.TryIssue(Directives.CommandKind, "0", target, arg0, 0, directive, out _);
        }

        // Ticks until this holds, or the fight ends.
        private static void Until(SimSession session, BattleSimulation sim, Func<bool> done)
        {
            while (!done() && !sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit) session.Tick();
            Assert.IsTrue(done(), "it never came, tick " + sim.CurrentTick);
        }

        private static bool Seen(BattleSimulation sim, BattleEventKind kind)
        {
            return sim.Events.Any(e => e.Kind == kind);
        }

        // ------------------------------------------------------------ Hold

        [Test]
        public void AHoldTimedToHisCleaveParriesItAndOneGivenTooEarlyOnlySoaksIt()
        {
            // Only his Cleave, on our front row with no Warden to cover it.
            List<UnitSpec> Front() => Side(U("s", UnitClass.Skirmisher), U("s2", UnitClass.Skirmisher));
            List<UnitSpec> Theirs() => Side(U("x.m", UnitClass.Marksman), Chief(UnitClass.Marksman));
            var rules = new SideRules(2, 0, false, 0, ChiefSpell.Eclipse, kit: ChiefKit.Bloodridge, moves: ChiefMove.Cleave);

            // Parried: a Hold just before it falls.
            var sim = Fight("orders.parry", 101, Front(), Theirs(), rules, FightRules.CampaignChiefKit);
            var session = new SimSession(sim);
            Until(session, sim, () => sim.CleaveTicks(1) > 0 && sim.CleaveTicks(1) <= 2);
            Assert.IsTrue(Order(session, "Hold"));
            OrderResult up = sim.LastOrderResult(0);
            Assert.AreEqual(Directive.Hold, up.Order);
            Assert.AreEqual(sim.CurrentTick, up.Tick);
            Assert.IsTrue(up.Running);
            Assert.IsFalse(up.Final);
            Assert.IsEmpty(up.Effects);
            Until(session, sim, () => Seen(sim, BattleEventKind.Parry));
            OrderResult parried = sim.LastOrderResult(0);
            OrderEffect parry = parried.Effects.Single();
            Assert.AreEqual(OrderEffectKind.Parried, parry.Kind);
            Assert.AreEqual(ChiefKits.CleaveName, parry.Move);
            Assert.AreEqual("chief", parry.TargetId);
            Assert.AreEqual(parry.Tick + 1, parried.EndTick, "the parry spent it");
            Assert.IsTrue(parried.Final);
            Assert.IsTrue(parried.Helped);
            Assert.AreEqual(OrderMiss.None, parried.Miss);

            // Soaked: a Hold the moment it shows, a second before it falls, too early to parry.
            var early = Fight("orders.soak", 101, Front(), Theirs(), rules, FightRules.CampaignChiefKit);
            var es = new SimSession(early);
            Until(es, early, () => early.CleaveTicks(1) > 0);
            Assert.Greater(early.CleaveTicks(1), FightRules.PerfectHoldTicks);
            Assert.IsTrue(Order(es, "Hold"));
            Until(es, early, () => early.Events.Any(e => e.Kind == BattleEventKind.Land && e.Detail.StartsWith(ChiefKits.CleaveName + ".")));
            OrderEffect soaked = early.LastOrderResult(0).Effects.Single();
            Assert.AreEqual(OrderEffectKind.Soaked, soaked.Kind);
            Assert.AreEqual(ChiefKits.CleaveName, soaked.Move);
            Assert.IsTrue(soaked.CouldParry, "a Hold given just before would have parried it");
        }

        // ------------------------------------------------------------ Fall Back

        [Test]
        public void FallBackDodgesTheEclipseOneThatRanOutJustBeforeItCameTooEarlyAndOneWithNothingComingSaysSo()
        {
            // Dodged: Fall Back as it is about to go off. A Hold given with it has nothing left to meet.
            var sim = Eclipse("orders.dodge", 3);
            var session = new SimSession(sim);
            Until(session, sim, () => sim.WindupTicks(1) > 0 && sim.WindupTicks(1) <= BattleSimulation.FallBackTicks / 2);
            Assert.IsTrue(Order(session, "FallBack"));
            Assert.IsTrue(Order(session, "Hold"));
            Until(session, sim, () => Seen(sim, BattleEventKind.Release));
            OrderResult fallBack = sim.OrderResults(0)[0];
            Assert.AreEqual(Directive.FallBack, fallBack.Order);
            OrderEffect dodged = fallBack.Effects.Single();
            Assert.AreEqual(OrderEffectKind.Dodged, dodged.Kind);
            Assert.AreEqual(Directives.Name(Directive.Eclipse), dodged.Move);
            Assert.AreEqual("chief", dodged.TargetId);
            Until(session, sim, () => sim.LastOrderResult(0).Final);
            OrderResult hold = sim.LastOrderResult(0);
            Assert.AreEqual(Directive.Hold, hold.Order);
            Assert.IsEmpty(hold.Effects);
            Assert.AreEqual(OrderMiss.MetFirst, hold.Miss, "the Fall Back got clear first");
            Assert.AreEqual(Directives.Name(Directive.Eclipse), hold.MissMove);

            // Too early: Fall Back the moment it starts, which runs out half a second before it goes off.
            var early = Eclipse("orders.dodge.early", 3);
            var es = new SimSession(early);
            Until(es, early, () => early.WindupTicks(1) > 0);
            int left = early.WindupTicks(1);
            Assert.Greater(left, BattleSimulation.FallBackTicks);
            Assert.Less(left - BattleSimulation.FallBackTicks, SimClock.TicksPerSecond);
            Assert.IsTrue(Order(es, "FallBack"));
            Until(es, early, () => early.LastOrderResult(0).Final);
            OrderResult tooEarly = early.LastOrderResult(0);
            Assert.IsTrue(Seen(early, BattleEventKind.Release));
            Assert.IsEmpty(tooEarly.Effects);
            Assert.AreEqual(OrderMiss.TooEarly, tooEarly.Miss);
            Assert.AreEqual(Directives.Name(Directive.Eclipse), tooEarly.MissMove);

            // Nothing came: no spell, no moves.
            var calm = Fight("orders.calm", 4, Ours(), Side(U("x.s", UnitClass.Skirmisher), U("x.m", UnitClass.Marksman)),
                new SideRules(0, 0, false), FightRules.Campaign);
            var cs = new SimSession(calm);
            cs.Tick();
            Assert.IsTrue(Order(cs, "FallBack"));
            Until(cs, calm, () => !calm.LastOrderResult(0).Running);
            Assert.IsFalse(calm.LastOrderResult(0).Final, "it waits a second for a hit it came too early for");
            Until(cs, calm, () => calm.LastOrderResult(0).Final);
            OrderResult nothing = calm.LastOrderResult(0);
            Assert.AreEqual(nothing.Tick + BattleSimulation.FallBackTicks, nothing.EndTick);
            Assert.AreEqual(nothing.EndTick + SimClock.TicksPerSecond, calm.CurrentTick);
            Assert.AreEqual(OrderMiss.NothingCame, nothing.Miss);
        }

        [Test]
        public void FallingBackIntoHisRiptideIsTheWrongOrder()
        {
            var sim = Thundercoast("orders.riptide", 94, ChiefMove.Riptide);
            var session = new SimSession(sim);
            Until(session, sim, () => sim.RiptideTicksLeft(1) > 0);
            Assert.IsTrue(Order(session, "FallBack"));
            Until(session, sim, () => sim.LastOrderResult(0).Final);
            OrderResult pulled = sim.LastOrderResult(0);
            OrderEffect pull = pulled.Effects.Single();
            Assert.AreEqual(OrderEffectKind.Pulled, pull.Kind);
            Assert.AreEqual(ChiefKits.RiptideName, pull.Move);
            Assert.Greater(pull.Amount, 0);
            Assert.IsFalse(pulled.Helped);
            Assert.AreEqual(OrderMiss.WrongOrder, pulled.Miss);
            Assert.AreEqual(ChiefKits.RiptideName, pulled.MissMove);
        }

        // ------------------------------------------------------------ a move that runs a while

        [Test]
        public void AHoldDuringHisStormSurgeCountsWhenTheSurgeEndsAndFocusFireOnHimBreaksTheHorn()
        {
            // Braced: a Hold as the water rises runs 5 s of the surge's 8, and counts when it ends.
            var sim = Thundercoast("orders.surge", 96, ChiefMove.StormSurge);
            var session = new SimSession(sim);
            Until(session, sim, () => sim.SurgeTicksLeft > 0);
            Assert.IsTrue(Order(session, "Hold"));
            Until(session, sim, () => !sim.LastOrderResult(0).Running);
            OrderResult waiting = sim.LastOrderResult(0);
            Assert.Greater(sim.SurgeTicksLeft, 0, "the surge stands on past the Hold");
            Assert.IsFalse(waiting.Final, "what it did comes when the surge ends");
            Assert.AreEqual(OrderMiss.None, waiting.Miss);
            Until(session, sim, () => sim.SurgeTicksLeft == 0);
            OrderResult braced = sim.LastOrderResult(0);
            Assert.IsTrue(braced.Final);
            OrderEffect brace = braced.Effects.Single();
            Assert.AreEqual(OrderEffectKind.Braced, brace.Kind);
            Assert.AreEqual(ChiefKits.SurgeName, brace.Move);
            Assert.Greater(brace.Tick, braced.EndTick, "it landed after the Hold stopped");

            // Broken: Focus Fire on him while his horn sounds.
            var cut = Thundercoast("orders.horn", 96, ChiefMove.StormSurge);
            var cs = new SimSession(cut);
            Until(cs, cut, () => cut.HornTicksLeft(1) > 0);
            Assert.IsTrue(Order(cs, "FocusFire", "chief"));
            Until(cs, cut, () => cut.LastOrderResult(0).Effects.Any(e => e.Kind == OrderEffectKind.Broke));
            OrderEffect broke = cut.LastOrderResult(0).Effects.First(e => e.Kind == OrderEffectKind.Broke);
            Assert.AreEqual(ChiefKits.SurgeName, broke.Move);
            Assert.AreEqual("chief", broke.TargetId);
            Assert.AreEqual("chief", cut.LastOrderResult(0).TargetId);
        }

        [Test]
        public void AHoldDuringHerOverclockBracesItWhenTheFlurryEnds()
        {
            var sim = Fight("orders.overclock", 75, Side(U("a", UnitClass.Warden), U("b", UnitClass.Skirmisher), U("m", UnitClass.Marksman)),
                Side(U("w", UnitClass.Warden), U("med", UnitClass.Medic), Chief(UnitClass.Trapper)),
                new SideRules(2, 0, false, 0, ChiefSpell.SteelStorm, kit: ChiefKit.Skullpeak, moves: ChiefMove.Overclock),
                FightRules.CampaignChiefKit);
            var session = new SimSession(sim);
            Until(session, sim, () => sim.OverclockTicksLeft(1) > 0);
            Assert.IsTrue(Order(session, "Hold"));
            Until(session, sim, () => !sim.LastOrderResult(0).Running);
            Assert.Greater(sim.OverclockTicksLeft(1), 0, "her flurry runs on past the Hold");
            Assert.IsFalse(sim.LastOrderResult(0).Final);
            Until(session, sim, () => sim.LastOrderResult(0).Final);
            Assert.AreEqual(0, sim.OverclockTicksLeft(1));
            OrderEffect brace = sim.LastOrderResult(0).Effects.Single();
            Assert.AreEqual(OrderEffectKind.Braced, brace.Kind);
            Assert.AreEqual(ChiefKits.OverclockName, brace.Move);
            Assert.AreEqual("chief", brace.TargetId);
        }

        // ------------------------------------------------------------ Focus Fire

        [Test]
        public void FocusFireBreaksTheEclipseItIsOn()
        {
            // A frail chief, so our blows break his cast.
            var sim = new BattleSimulation(new BattleSetup("orders.break", 5, Ours(),
                Side(U("x.s", UnitClass.Skirmisher), U("x.m", UnitClass.Marksman),
                    new UnitSpec("chief", UnitClass.Marksman, Row.Back, UnitRole.Chief, 100, 25)),
                new TacticsProfile(), new TacticsProfile(), BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints,
                NerveRules.Off, 10, 400, new SideRules(1, 0, true), new SideRules(2, 60, false)));
            var session = new SimSession(sim);
            Until(session, sim, () => sim.WindupTicks(1) > 0);
            Assert.IsTrue(Order(session, "FocusFire", "chief"));
            Until(session, sim, () => Seen(sim, BattleEventKind.Interrupt));
            OrderResult focus = sim.LastOrderResult(0);
            Assert.AreEqual("chief", focus.TargetId);
            OrderEffect broke = focus.Effects.First(e => e.Kind == OrderEffectKind.Broke);
            Assert.AreEqual(Directives.Name(Directive.Eclipse), broke.Move);
            Assert.AreEqual("chief", broke.TargetId);
        }

        [Test]
        public void FocusFireOnAFighterTheirWardenCoversSaysCoveredAndByWhom()
        {
            // Their Warden steps in to cover whoever we focus.
            var tactics = new TacticsProfile();
            tactics.SetCoverFocused(TacticsProfile.MaxValue);
            var sim = Fight("orders.covered", 107, Ours(),
                Side(U("x.w", UnitClass.Warden), U("x.m", UnitClass.Marksman), U("x.s", UnitClass.Skirmisher)), new SideRules(0, 0, false),
                FightRules.Campaign, tactics);
            var session = new SimSession(sim);
            session.Tick();
            Assert.IsTrue(Order(session, "FocusFire", "x.m"));
            Until(session, sim, () => sim.LastOrderResult(0).Final);
            OrderResult focus = sim.LastOrderResult(0);
            OrderEffect cover = focus.Effects.Single(e => e.Kind == OrderEffectKind.Covered);
            Assert.AreEqual("x.w", cover.TargetId);
            Assert.Greater(cover.Amount, 0);
            Assert.IsFalse(focus.Effects.Any(e => e.Kind == OrderEffectKind.Damage), "every blow went to the Warden");
            Assert.AreEqual(OrderMiss.Covered, focus.Miss);
            Assert.AreEqual("x.w", focus.MissMove);
        }

        // ------------------------------------------------------------ Unleash

        [Test]
        public void UnleashSmashesHisSteelStormWallOnItsOwnTick()
        {
            var sim = Fight("orders.wall", 108, Ours(), Side(U("x.w", UnitClass.Warden), U("x.m", UnitClass.Marksman), Chief(UnitClass.Marksman)),
                new SideRules(2, 450, false, 0, ChiefSpell.SteelStorm), FightRules.Campaign);
            var session = new SimSession(sim);
            Until(session, sim, () => sim.Events.Any(e => e.Kind == BattleEventKind.Wall && e.ActorSide == 1 && e.Detail == ChiefSpells.WallUp));
            Assert.IsTrue(Order(session, "Unleash"));
            OrderResult unleash = sim.LastOrderResult(0);
            OrderEffect smash = unleash.Effects.Single();
            Assert.AreEqual(OrderEffectKind.Smashed, smash.Kind);
            Assert.AreEqual(ChiefSpells.Name(ChiefSpell.SteelStorm), smash.Move);
            Assert.AreEqual(unleash.Tick, smash.Tick);
            Assert.IsTrue(unleash.Running);
            Until(session, sim, () => sim.LastOrderResult(0).Final);
            Assert.IsTrue(sim.LastOrderResult(0).Helped);
            Assert.AreEqual(OrderMiss.None, sim.LastOrderResult(0).Miss);
        }

        // ------------------------------------------------------------ paused, and read only

        [Test]
        public void AnOrderGivenWhilePausedSaysSo()
        {
            var sim = Eclipse("orders.paused", 3);
            var session = new SimSession(sim);
            session.Tick();
            Assert.IsTrue(Order(session, "Hold", "", HeavyRules.PausedOrderArg));
            Assert.IsTrue(sim.LastOrderResult(0).Paused);
            Until(session, sim, () => !sim.LastOrderResult(0).Running);
            Assert.IsTrue(Order(session, "Hold"));
            Assert.IsFalse(sim.LastOrderResult(0).Paused);
            Assert.AreEqual(new[] { true, false }, sim.OrderResults(0).Select(r => r.Paused).ToArray());
        }

        [Test]
        public void ReadingTheOrderResultsEveryTickChangesNothingAndCountsOnlyTheOrdersYouGave()
        {
            // Their side holds whenever we Unleash and covers whoever we focus, on its own.
            TacticsProfile Reflexes()
            {
                var t = new TacticsProfile();
                t.SetHoldOnUnleash(TacticsProfile.MaxValue);
                t.SetCoverFocused(TacticsProfile.MaxValue);
                return t;
            }
            BattleSimulation Make() => Fight("orders.readonly", 111, Ours(),
                Side(U("x.w", UnitClass.Warden), U("x.s", UnitClass.Skirmisher), U("x.m", UnitClass.Marksman), Chief(UnitClass.Marksman)),
                new SideRules(2, 300, false), FightRules.Campaign, Reflexes());
            BattleSimulation quiet = Make(), asked = Make();
            var qs = new SimSession(quiet);
            var asks = new SimSession(asked);
            string[] orders = { "Hold", "FallBack", "Unleash", "FocusFire" };
            int given = 0, read = 0;
            while (!quiet.IsFinished && quiet.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                // Both give the same orders; one asks for both sides' results every tick.
                if (quiet.CurrentTick % 90 == 45)
                {
                    string order = orders[(int)(quiet.CurrentTick / 90 % orders.Length)];
                    string target = order == "FocusFire" ? "x.m" : string.Empty;
                    bool a = Order(qs, order, target), b = Order(asks, order, target);
                    Assert.AreEqual(a, b);
                    if (a) given++;
                }
                qs.Tick();
                asks.Tick();
                read += asked.OrderResults(0).Count + asked.OrderResults(1).Count;
                asked.LastOrderResult(0);
            }
            Assert.Greater(given, 0);
            Assert.Greater(read, 0);
            Assert.AreEqual(given, asked.OrderResults(0).Count, "only the orders we gave");
            Assert.AreEqual(0, asked.OrderResults(1).Count, "their reflexes are not orders");
            Assert.IsTrue(asked.Events.Any(e => e.Kind == BattleEventKind.Directive && e.ActorSide == 1), "they did react");
            Assert.AreEqual(quiet.EventHash, asked.EventHash);
            Assert.AreEqual(quiet.OrderedStateSnapshot, asked.OrderedStateSnapshot);
            TestContext.Progress.WriteLine(given + " orders, " + asked.OrderResults(0).Count(r => r.Helped) + " helped, misses: " +
                string.Join(", ", asked.OrderResults(0).Where(r => r.Miss != OrderMiss.None).Select(r => r.Order + " " + r.Miss + " " + r.MissMove)));
        }
    }
}
