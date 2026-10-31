using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The Skullpeak chief's kit (Jacob, 22 Oct 2026: "go, build the Skullpeak chief and the Bloodridge extras in the sim";
    /// DESIGN_LOCKS "The Bloodridge chief finished and the Skullpeak chief's kit"). One quick test per piece, only to prove it
    /// runs. Nothing here is tuning.
    public class SkullpeakKitTests
    {
        // A fighter at these percents of power and health, in its class's row. Tough by default, so fights last.
        private static UnitSpec U(string id, UnitClass unitClass, int power = 100, int health = 1000)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, power, health);
        }

        // She leads from the back, a Trapper, as in the campaign.
        private static UnitSpec Chief(int health = 1000)
        {
            return new UnitSpec("chief", UnitClass.Trapper, Row.Back, UnitRole.Chief, 100, health);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        // Her side plays her kit with these moves and no heavy swings; ours has a grade 1 Eclipse on its 5th button, and combos.
        // No nerve unless asked, so nothing breaks; with tactics, her band's jobs too.
        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, ChiefMove moves,
            bool nerve = false, bool tactics = false)
        {
            var kit = new SideRules(2, (moves & ChiefMove.SteelStorm) != 0 ? ChiefKits.SteelStormEveryTicks : 0, false, 0,
                ChiefSpell.SteelStorm, kit: ChiefKit.Skullpeak, moves: moves);
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, nerve ? NerveRules.On() : NerveRules.Off, 10, 400,
                new SideRules(1, 0, true), kit, tactics ? FightRules.CampaignChiefTactics : FightRules.CampaignChiefKit));
        }

        private static bool Order(SimSession session, string directive, string target = "")
        {
            return session.TryIssue(Directives.CommandKind, "0", target, 0, 0, directive, out _);
        }

        // Ticks until an event matches, calling before ahead of each tick; events from before the call are not looked at.
        private static BattleEvent? Next(BattleSimulation sim, SimSession session, Func<BattleEvent, bool> match, Action before = null)
        {
            int seen = sim.Events.Count;
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                before?.Invoke();
                session.Tick();
                for (; seen < sim.Events.Count; seen++)
                    if (match(sim.Events[seen])) return sim.Events[seen];
            }
            return null;
        }

        private static string Landed(string move, string how)
        {
            return move + "." + how;
        }

        private static int Count(ChiefMove moves)
        {
            int n = 0;
            for (int bit = 0; bit < 30; bit++)
                if (((int)moves & (1 << bit)) != 0) n++;
            return n;
        }

        // ------------------------------------------------------------ Steel Storm

        [Test]
        public void HerWardensLockShieldsThenTheWallMakesYourBlowsDoATenthUntilUnleashSmashesIt()
        {
            var sim = Fight("sp.storm", 71, Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("c", UnitClass.Skirmisher)),
                Side(U("w", UnitClass.Warden), U("v", UnitClass.Warden), Chief()), ChiefMove.SteelStorm);
            var session = new SimSession(sim);
            Assert.AreEqual(ChiefKit.Skullpeak, sim.Kit(1));
            Assert.AreEqual(ChiefMove.SteelStorm, sim.KitMoves(1));

            BattleEvent windup = Next(sim, session, e => e.Kind == BattleEventKind.Windup && e.ActorId == "chief").Value;
            Assert.AreEqual(ChiefKits.SteelStormEveryTicks, windup.Tick);
            Assert.AreEqual(ChiefKits.LockTicks, windup.Amount, "her Wardens lock shields over two seconds");
            BattleEvent up = Next(sim, session, e => e.Kind == BattleEventKind.Wall).Value;
            Assert.AreEqual(ChiefSpells.WallUp, up.Detail);
            Assert.AreEqual(windup.Tick + ChiefKits.LockTicks - 1, up.Tick);
            Assert.AreEqual(ChiefSpells.WallDamagePercent, sim.WallPercentNow(1));
            Assert.IsTrue(sim.WallSmashable(1));
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Line && e.Detail == ChiefKits.EclipseLine && e.Tick == up.Tick),
                "she speaks as it goes up whole");
            Assert.AreEqual(string.Empty, ChiefKits.Line(ChiefKit.Skullpeak, ChiefKits.EclipseLine, false), "her words are Jacob's to write");

            // Unleash smashes it: a Smash for our Glory, and our blows land in full again.
            Assert.IsTrue(Order(session, "Unleash"));
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Wall && e.ActorSide == 1 && e.Detail == ChiefSpells.WallSmashed));
            Assert.AreEqual(ChiefKits.SmashGlory, sim.Glory(0));
            Assert.AreEqual(100, sim.WallPercentNow(1));
            Assert.IsFalse(sim.WallSmashable(1));
        }

        [Test]
        public void YourTrapperSnaringOneOfHerWardensWhileTheyLockLeavesTheWallHalfBuilt()
        {
            // Her Wardens swing into our armed trap now and then; we look for a fight where one is held while they lock.
            for (ulong seed = 1; seed < 60; seed++)
            {
                var sim = Fight("sp.half", seed, Side(U("a", UnitClass.Warden), U("b", UnitClass.Skirmisher), U("t", UnitClass.Trapper)),
                    Side(U("w", UnitClass.Warden), U("v", UnitClass.Warden), Chief()), ChiefMove.SteelStorm);
                var session = new SimSession(sim);
                BattleEvent windup = Next(sim, session, e => e.Kind == BattleEventKind.Windup && e.ActorId == "chief").Value;
                BattleEvent up = Next(sim, session, e => e.Kind == BattleEventKind.Wall).Value;
                bool snared = sim.Events.Any(e => e.Kind == BattleEventKind.Mark && e.Detail == ClassSpells.SnareMark &&
                    (e.TargetId == "w" || e.TargetId == "v") && e.Tick > windup.Tick - ClassSpells.SnareTicks && e.Tick <= up.Tick);
                Assert.AreEqual(snared ? ChiefKits.WallHalf : ChiefSpells.WallUp, up.Detail, "seed " + seed);
                if (!snared) continue;
                Assert.AreEqual(ChiefKits.HalfWallPercent, sim.WallPercentNow(1));
                Assert.AreEqual(ChiefKits.SnareGlory, sim.Glory(0), "the snare answered it");
                Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Line && e.Detail == ChiefKits.EclipseLine), "no line for a half wall");
                return;
            }
            Assert.Fail("in sixty fights our trap never caught one of her Wardens while they locked");
        }

        // ------------------------------------------------------------ Hook

        [Test]
        public void HerChainGlintsAtYourBackRowThenDragsThemUnlessAHoldSnapsItOrAWardenAnchorsThem()
        {
            List<UnitSpec> Theirs() => Side(U("w", UnitClass.Warden), Chief());

            // Dragged: her Warden, who never reaches our back row, strikes the one it dragged, and every blow on them lands.
            var sim = Fight("sp.hook", 72, Side(U("a", UnitClass.Skirmisher), U("m", UnitClass.Marksman)), Theirs(), ChiefMove.Hook);
            var session = new SimSession(sim);
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.HookName, tell.Detail);
            Assert.GreaterOrEqual(tell.Tick, ChiefKits.HookEveryTicks);
            Assert.AreEqual(ChiefKits.HookTellTicks, tell.Amount);
            Assert.AreEqual("m", tell.TargetId, "one of our back row");
            BattleEvent land = Next(sim, session, e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual(Landed(ChiefKits.HookName, ChiefKits.Full), land.Detail);
            BattleUnit m = sim.FindById("m");
            Assert.Greater(m.HookTicks, 0);
            Assert.AreEqual(StrikeBlock.None, sim.CanStrike(sim.FindById("w"), m), "in front of her shields");
            BattleEvent struck = Next(sim, session, e => e.Kind == BattleEventKind.Hit && e.ActorId == "w").Value;
            Assert.AreEqual("m", struck.TargetId, "her side goes for them");
            Assert.LessOrEqual(struck.Tick, land.Tick + ChiefKits.HookTicks);
            Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Miss && e.TargetId == "m" && e.Tick > land.Tick &&
                e.Tick < land.Tick + ChiefKits.HookTicks), "every blow on them lands");

            // Snapped: a Hold as it bites. She staggers.
            var parry = Fight("sp.hook.parry", 72, Side(U("a", UnitClass.Skirmisher), U("m", UnitClass.Marksman)), Theirs(), ChiefMove.Hook);
            var ps = new SimSession(parry);
            BattleEvent snapped = Next(parry, ps, e => e.Kind == BattleEventKind.Land, () =>
            {
                if (parry.HookParryable(1) && parry.HookTellTicks(1) <= 10 && parry.HoldTicksLeft(0) == 0) Order(ps, "Hold");
            }).Value;
            Assert.AreEqual(Landed(ChiefKits.HookName, ChiefKits.Parried), snapped.Detail);
            Assert.IsTrue(parry.Events.Any(e => e.Kind == BattleEventKind.Parry && e.TargetId == "chief" && e.Tick == snapped.Tick), "she staggers");
            Assert.AreEqual(0, parry.FindById("m").HookTicks);
            Assert.AreEqual(ChiefKits.ParryGlory, parry.Glory(0));
            Assert.IsTrue(parry.Events.Any(e => e.Kind == BattleEventKind.Line && e.Detail == ChiefKits.ParryLine && e.Tick == snapped.Tick));

            // Anchored: our Warden covers them as it bites.
            var anchor = Fight("sp.hook.anchor", 72, Side(U("g", UnitClass.Warden), U("m", UnitClass.Marksman)), Theirs(), ChiefMove.Hook);
            BattleEvent held = Next(anchor, new SimSession(anchor), e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual(Landed(ChiefKits.HookName, ChiefKits.Guarded), held.Detail);
            Assert.IsTrue(anchor.Events.Any(e => e.Kind == BattleEventKind.Guard && e.ActorId == "g" && e.TargetId == "m" && e.Tick == held.Tick));
            Assert.AreEqual(0, anchor.FindById("m").HookTicks);
            Assert.AreEqual(ChiefKits.GuardGlory, anchor.Glory(0));
        }

        // ------------------------------------------------------------ Rifle Line

        [Test]
        public void HerShootersLockOntoYourMostHurtThenAVolleyUnlessYouBreakTheirAimOrFallBack()
        {
            // Her shooters: her Marksman, her Trapper and herself. Our Medic stands.
            List<UnitSpec> Theirs() => Side(U("w", UnitClass.Warden), U("s1", UnitClass.Marksman), U("s2", UnitClass.Trapper), Chief());
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Warden), U("b", UnitClass.Skirmisher), U("m", UnitClass.Marksman), U("h", UnitClass.Medic));

            var sim = Fight("sp.rifle", 73, Ours(), Theirs(), ChiefMove.RifleLine);
            var session = new SimSession(sim);
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.RifleName, tell.Detail);
            Assert.AreEqual(ChiefKits.RifleAimTicks, tell.Amount);
            List<BattleEvent> aims = sim.Events.Where(e => e.Kind == BattleEventKind.Aim && e.Detail == ChiefKits.RifleDetail && e.Tick == tell.Tick).ToList();
            Assert.That(aims.Count, Is.InRange(1, ChiefKits.RifleShots));
            Assert.IsTrue(aims.All(e => e.ActorSide == 1 && e.TargetId == tell.TargetId && sim.FindById(e.ActorId).Class != UnitClass.Medic));
            BattleEvent land = Next(sim, session, e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual(tell.Tick + ChiefKits.RifleAimTicks - 1, land.Tick);
            Assert.AreEqual(tell.TargetId, land.TargetId);
            Assert.AreEqual(aims.Count, sim.Events.Count(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.RifleDetail && e.Tick == land.Tick),
                "one shot from each shooter");
            Assert.AreEqual(Landed(ChiefKits.RifleName, ChiefKits.Healed), land.Detail, "our Medic stands and the mark lives");

            // Dodged: Fall Back as it fires takes everyone out of sight.
            var dodge = Fight("sp.rifle.dodge", 73, Ours(), Theirs(), ChiefMove.RifleLine);
            var ds = new SimSession(dodge);
            BattleEvent dodged = Next(dodge, ds, e => e.Kind == BattleEventKind.Land, () =>
            {
                if (dodge.RifleLineTicks(1) > 0 && dodge.RifleLineTicks(1) <= 20 && dodge.FallBackTicksLeft(0) == 0) Order(ds, "FallBack");
            }).Value;
            Assert.AreEqual(Landed(ChiefKits.RifleName, ChiefKits.Dodged), dodged.Detail);
            Assert.IsFalse(dodge.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.RifleDetail));
            Assert.AreEqual(ChiefKits.DodgeGlory, dodge.Glory(0));

            // Broken: Focus Fire one of her band's shooters, and hitting him loses him his shot.
            var focus = Fight("sp.rifle.focus", 73, Ours(), Theirs(), ChiefMove.RifleLine);
            var fs = new SimSession(focus);
            BattleEvent lost = Next(focus, fs, e => e.Kind == BattleEventKind.Broken && e.Detail == ChiefKits.RifleDetail, () =>
            {
                if (focus.RifleLineTicks(1) <= 0 || focus.FocusTicksLeft(0) > 0) return;
                BattleUnit shooter = focus.Units.FirstOrDefault(u => u.Side == 1 && u.RifleTicks > 0 && u.Role == UnitRole.Fighter);
                if (shooter != null) Order(fs, "FocusFire", shooter.Id);
            }).Value;
            Assert.AreEqual(0, focus.FindById(lost.TargetId).RifleTicks, "he lost his shot");
            Assert.AreEqual(0, lost.ActorSide);
            BattleEvent volley = Next(focus, fs, e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual(Landed(ChiefKits.RifleName, ChiefKits.Broken), volley.Detail);
            Assert.IsTrue(focus.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.Name(ChiefAnswer.Break) && e.Tick == volley.Tick));
        }

        // ------------------------------------------------------------ Iron Advance

        [Test]
        public void HerShieldLineMarchesAndCrushesYourFrontRowEachSecondUnlessYouBraceGiveGroundOrStaggerIt()
        {
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Warden), U("b", UnitClass.Warden), U("m", UnitClass.Marksman));
            List<UnitSpec> Theirs() => Side(U("w", UnitClass.Warden), U("v", UnitClass.Warden), Chief());
            string full = Landed(ChiefKits.AdvanceName, ChiefKits.Full);

            // Eaten: four crushes on our front row, one a second.
            var sim = Fight("sp.advance", 74, Ours(), Theirs(), ChiefMove.IronAdvance);
            var session = new SimSession(sim);
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.AdvanceName, tell.Detail);
            Assert.AreEqual(ChiefKits.AdvanceEveryTicks, tell.Tick);
            Assert.AreEqual(ChiefKits.AdvanceTicks, tell.Amount);
            while (sim.AdvanceTicksLeft(1) > 0) session.Tick();
            List<BattleEvent> crushes = sim.Events.Where(e => e.Kind == BattleEventKind.Land && e.Detail == full).ToList();
            CollectionAssert.AreEqual(new long[] { 29, 59, 89, 119 }.Select(d => tell.Tick + d).ToList(), crushes.Select(e => e.Tick).ToList());
            foreach (BattleEvent crush in crushes)
                CollectionAssert.AreEquivalent(new[] { "a", "b" }, sim.Events.Where(e => e.Kind == BattleEventKind.Hit &&
                    e.Detail == ChiefKits.AdvanceDetail && e.Tick == crush.Tick).Select(e => e.TargetId).ToList());
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.EatenDetail && e.Tick == crushes[3].Tick));

            // Braced: a Hold through the whole march. No damage, and it answers it.
            var brace = Fight("sp.advance.brace", 74, Ours(), Theirs(), ChiefMove.IronAdvance);
            var bs = new SimSession(brace);
            Next(brace, bs, e => e.Kind == BattleEventKind.Tell);
            Assert.IsTrue(Order(bs, "Hold"));
            while (brace.AdvanceTicksLeft(1) > 0) bs.Tick();
            Assert.AreEqual(4, brace.Events.Count(e => e.Kind == BattleEventKind.Land && e.Detail == Landed(ChiefKits.AdvanceName, ChiefKits.Braced)));
            Assert.IsFalse(brace.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.AdvanceDetail));
            Assert.IsTrue(brace.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.Name(ChiefAnswer.Brace)));

            // Given ground: Fall Back through the march. No damage, but her side gets nerve back. Our blades are all melee here,
            // so nothing of ours lands on her meanwhile, and an Unleash before it shook her.
            var give = Fight("sp.advance.give", 74, Side(U("a", UnitClass.Warden), U("b", UnitClass.Warden)), Theirs(), ChiefMove.IronAdvance,
                nerve: true);
            var gs = new SimSession(give);
            while (give.CurrentTick < ChiefKits.AdvanceEveryTicks - 60) gs.Tick();
            Assert.IsTrue(Order(gs, "Unleash"));
            Next(give, gs, e => e.Kind == BattleEventKind.Tell);
            while (give.AdvanceTicksLeft(1) > 0)
            {
                if (give.FallBackTicksLeft(0) == 0) Order(gs, "FallBack");
                gs.Tick();
            }
            List<BattleEvent> gave = give.Events.Where(e => e.Kind == BattleEventKind.Land &&
                e.Detail == Landed(ChiefKits.AdvanceName, ChiefKits.Dodged)).ToList();
            Assert.AreEqual(4, gave.Count);
            Assert.Greater(gave.Sum(e => e.Amount), 0, "her side got nerve back");
            Assert.IsFalse(give.Events.Any(e => e.Kind == BattleEventKind.Hit && e.Detail == ChiefKits.AdvanceDetail));
            Assert.IsTrue(give.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.Name(ChiefAnswer.Dodge)));

            // Staggered: Unleash into the march stops it, and her front row loses time.
            var stagger = Fight("sp.advance.stagger", 74, Ours(), Theirs(), ChiefMove.IronAdvance);
            var ss = new SimSession(stagger);
            BattleEvent march = Next(stagger, ss, e => e.Kind == BattleEventKind.Tell).Value;
            while (stagger.CurrentTick < march.Tick + 40) ss.Tick();
            int cooldown = stagger.FindById("w").Cooldown;
            Assert.IsTrue(Order(ss, "Unleash"));
            Assert.IsTrue(stagger.Events.Any(e => e.Kind == BattleEventKind.Land && e.Detail == Landed(ChiefKits.AdvanceName, ChiefKits.Staggered)));
            Assert.AreEqual(0, stagger.AdvanceTicksLeft(1));
            Assert.AreEqual(cooldown + FightRules.StaggerTicks, stagger.FindById("w").Cooldown, "her line staggers");
            Assert.IsTrue(stagger.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.Name(ChiefAnswer.Smash)));
            for (int i = 0; i < ChiefKits.AdvanceTicks; i++) ss.Tick();
            Assert.AreEqual(1, stagger.Events.Count(e => e.Kind == BattleEventKind.Land && e.Detail == full), "no more crushes");
        }

        // ------------------------------------------------------------ Overclock

        [Test]
        public void HerMedicJabsHerBandThenTheyActTwiceAsFastUnlessYouFocusHerMedic()
        {
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Warden), U("b", UnitClass.Skirmisher), U("m", UnitClass.Marksman));
            List<UnitSpec> Theirs() => Side(U("w", UnitClass.Warden), U("med", UnitClass.Medic), Chief());

            var sim = Fight("sp.overclock", 75, Ours(), Theirs(), ChiefMove.Overclock);
            var session = new SimSession(sim);
            BattleEvent tell = Next(sim, session, e => e.Kind == BattleEventKind.Tell).Value;
            Assert.AreEqual(ChiefKits.OverclockName, tell.Detail);
            Assert.AreEqual("med", tell.ActorId, "her Medic jabs");
            Assert.AreEqual(ChiefKits.OverclockEveryTicks, tell.Tick);
            Assert.AreEqual(ChiefKits.JabTicks, tell.Amount);
            BattleEvent land = Next(sim, session, e => e.Kind == BattleEventKind.Land).Value;
            Assert.AreEqual(Landed(ChiefKits.OverclockName, ChiefKits.Full), land.Detail);
            Assert.AreEqual(tell.Tick + ChiefKits.JabTicks - 1, land.Tick);
            Assert.AreEqual(ChiefKits.OverclockTicks, land.Amount);
            Assert.Greater(sim.OverclockTicksLeft(1), 0);
            while (sim.OverclockTicksLeft(1) > 0) session.Tick();
            int Swings(long after, long until) => sim.Events.Count(e => e.ActorId == "w" &&
                (e.Kind == BattleEventKind.Hit || e.Kind == BattleEventKind.Miss) && e.Tick > after && e.Tick <= until);
            Assert.Greater(Swings(land.Tick, land.Tick + ChiefKits.OverclockTicks),
                Swings(land.Tick - ChiefKits.OverclockTicks, land.Tick), "her Warden swings faster");

            // Broken: Focus Fire on her Medic while she jabs.
            var cut = Fight("sp.overclock.cut", 75, Ours(), Theirs(), ChiefMove.Overclock);
            var cs = new SimSession(cut);
            BattleEvent broken = Next(cut, cs, e => e.Kind == BattleEventKind.Land, () =>
            {
                if (cut.JabTicksLeft(1) > 0 && cut.FocusTicksLeft(0) == 0) Order(cs, "FocusFire", "med");
            }).Value;
            Assert.AreEqual(Landed(ChiefKits.OverclockName, ChiefKits.Broken), broken.Detail);
            Assert.AreEqual(0, cut.OverclockTicksLeft(1));
            Assert.AreEqual(ChiefKits.BreakGlory, cut.Glory(0));
        }

        // ------------------------------------------------------------ Lockdown

        [Test]
        public void AtHalfHealthSheStepsBehindAWallThatNeverDropsAndOnlyAnUnleashOpensIt()
        {
            // Her band's Skirmisher does not guard her, so our heavy blades bring her to half; two Wardens wait in reserve.
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Skirmisher, 300), U("b", UnitClass.Skirmisher, 300), U("c", UnitClass.Skirmisher, 300),
                U("d", UnitClass.Skirmisher, 300));
            var sim = Fight("sp.lockdown", 76, Ours(), Side(U("x", UnitClass.Skirmisher), U("r1", UnitClass.Warden).AsReserve(),
                U("r2", UnitClass.Warden).AsReserve(), Chief()), ChiefMove.Hook);
            var session = new SimSession(sim);
            BattleEvent phase = Next(sim, session, e => e.Kind == BattleEventKind.Phase, () =>
            {
                if (sim.FocusTicksLeft(0) == 0) Order(session, "FocusFire", "chief");
            }).Value;
            Assert.AreEqual(ChiefKits.PhaseName(ChiefKit.Skullpeak), phase.Detail);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Line && e.Detail == ChiefKits.LockdownLine && e.Tick == phase.Tick));
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Wall && e.ActorSide == 1 && e.Detail == ChiefKits.WallLockdown &&
                e.Tick == phase.Tick));
            Assert.IsTrue(sim.InLockdown(1));
            Assert.AreEqual(ChiefSpells.WallDamagePercent, sim.WallPercentNow(1), "her reserve Wardens joined, so it holds");
            Assert.IsTrue(sim.WallSmashable(1));

            // Unleash smashes it open: six seconds of full blows, and nothing more to smash until it is whole again, twelve
            // seconds after the smash.
            while (sim.DirectivePoints(0) < Directives.Cost(Directive.Unleash)) session.Tick();
            Assert.IsTrue(Order(session, "Unleash"));
            BattleEvent smashed = sim.Events.Last(e => e.Kind == BattleEventKind.Wall);
            Assert.AreEqual(ChiefSpells.WallSmashed, smashed.Detail);
            long at = smashed.Tick;
            Assert.AreEqual(ChiefKits.LockdownOpenTicks, sim.WallOpenTicks(1));
            Assert.AreEqual(100, sim.WallPercentNow(1));
            Assert.IsFalse(sim.WallSmashable(1));
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.Name(ChiefAnswer.Smash)));
            while (sim.CurrentTick <= at + ChiefKits.LockdownOpenTicks) session.Tick();
            Assert.AreEqual(ChiefSpells.WallDamagePercent, sim.WallPercentNow(1), "the window closed");
            Assert.IsFalse(sim.WallSmashable(1), "not whole yet");
            BattleEvent rebuilt = Next(sim, session, e => e.Kind == BattleEventKind.Wall).Value;
            Assert.AreEqual(ChiefKits.WallRebuilt, rebuilt.Detail);
            Assert.AreEqual(at + ChiefKits.LockdownRebuildTicks - 1, rebuilt.Tick);
            Assert.IsTrue(sim.WallSmashable(1));

            // With no Warden of hers standing it is down from the start.
            var bare = Fight("sp.lockdown.bare", 76, Ours(), Side(U("x", UnitClass.Skirmisher), Chief()), ChiefMove.Hook);
            var bs = new SimSession(bare);
            BattleEvent p = Next(bare, bs, e => e.Kind == BattleEventKind.Phase, () =>
            {
                if (bare.FocusTicksLeft(0) == 0) Order(bs, "FocusFire", "chief");
            }).Value;
            Assert.IsTrue(bare.Events.Any(e => e.Kind == BattleEventKind.Wall && e.Detail == ChiefKits.WallLockdown && e.Tick == p.Tick));
            Assert.IsTrue(bare.Events.Any(e => e.Kind == BattleEventKind.Wall && e.Detail == ChiefSpells.WallFell && e.Tick == p.Tick));
            Assert.AreEqual(100, bare.WallPercentNow(1));
        }

        // ------------------------------------------------------------ Breach, her hidden chain

        [Test]
        public void SmashTwoWallsInTheirFirstSecondThenHoldAsAHookBitesAndYourSkirmisherTakesOutHerSteelLeg()
        {
            var sim = Fight("sp.breach", 77, Side(U("a", UnitClass.Warden), U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman)),
                Side(U("w", UnitClass.Warden), U("v", UnitClass.Warden), Chief()), ChiefMove.SteelStorm | ChiefMove.Hook);
            var session = new SimSession(sim);
            BattleEvent breach = Next(sim, session, e => e.Kind == BattleEventKind.Combo && e.Detail == ChiefKits.Breach, () =>
            {
                int smashes = sim.Events.Count(e => e.Kind == BattleEventKind.Wall && e.Detail == ChiefSpells.WallSmashed);
                if (smashes < ChiefKits.BreachSmashes && sim.WallSmashable(1)) Order(session, "Unleash");
                else if (smashes >= ChiefKits.BreachSmashes && sim.HookParryable(1) && sim.HookTellTicks(1) <= 10 && sim.HoldTicksLeft(0) == 0)
                    Order(session, "Hold");
            }).Value;
            Assert.AreEqual("s", breach.ActorId, "our Skirmisher runs the chain back");
            Assert.AreEqual("chief", breach.TargetId);
            Assert.AreEqual(ChiefKits.BreachStunTicks, breach.Amount);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Land && e.Detail == Landed(ChiefKits.HookName, ChiefKits.Parried) &&
                e.Tick == breach.Tick));
            Assert.Greater(sim.FindById("chief").StunTicks, 0, "she is down");
            Assert.IsTrue(sim.HookGone(1));
            for (int i = 0; i < 2 * ChiefKits.HookEveryTicks && !sim.IsFinished; i++) session.Tick();
            Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Tell && e.Detail == ChiefKits.HookName && e.Tick > breach.Tick),
                "she cannot Hook again this fight");
        }

        // ------------------------------------------------------------ her band's jobs

        [Test]
        public void KillHerShieldbearerAndHerWallsComeUpSlowerKillHerSpotterAndHerRifleLineAimsLonger()
        {
            // Her Shieldbearer (her first Warden) and her Spotter (her first Marksman) barely stand.
            List<UnitSpec> Theirs() => Side(U("w", UnitClass.Warden, 100, 1), U("v", UnitClass.Warden), U("s1", UnitClass.Marksman, 100, 1),
                U("s2", UnitClass.Trapper), Chief());
            List<UnitSpec> Ours() => Side(U("a", UnitClass.Skirmisher), U("b", UnitClass.Skirmisher), U("m", UnitClass.Marksman));
            var sim = Fight("sp.jobs", 78, Ours(), Theirs(), ChiefMove.SteelStorm | ChiefMove.RifleLine, tactics: true);
            var session = new SimSession(sim);
            Assert.AreEqual(BandJob.Shieldbearer, sim.JobOf(sim.FindById("w")));
            Assert.AreEqual(BandJob.Spotter, sim.JobOf(sim.FindById("s1")));
            Assert.AreEqual(BandJob.None, sim.JobOf(sim.FindById("v")));
            Assert.AreEqual(BandJob.None, sim.JobOf(sim.FindById("chief")));
            Assert.AreEqual("w", sim.JobHolderId(1, BandJob.Shieldbearer));
            Assert.AreEqual("s1", sim.JobHolderId(1, BandJob.Spotter));

            while (!(sim.JobDown(1, BandJob.Shieldbearer) && sim.JobDown(1, BandJob.Spotter)) && sim.CurrentTick < ChiefKits.RifleEveryTicks)
            {
                string next = !sim.JobDown(1, BandJob.Shieldbearer) ? "w" : "s1";
                if (sim.FocusedId(0) != next) Order(session, "FocusFire", next);
                session.Tick();
            }
            Assert.IsTrue(sim.JobDown(1, BandJob.Shieldbearer) && sim.JobDown(1, BandJob.Spotter), "both fell before her first Rifle Line");
            BattleEvent rifle = Next(sim, session, e => e.Kind == BattleEventKind.Tell && e.Detail == ChiefKits.RifleName).Value;
            Assert.AreEqual(ChiefKits.RifleAimTicks * ChiefKits.JobSlowPercent / 100, rifle.Amount, "her Rifle Line aims longer");
            BattleEvent storm = Next(sim, session, e => e.Kind == BattleEventKind.Windup && e.ActorId == "chief").Value;
            Assert.AreEqual(ChiefKits.LockTicks * ChiefKits.JobSlowPercent / 100, storm.Amount, "her walls come up slower");

            // Without the tactics her band has no jobs.
            var plain = Fight("sp.jobs.off", 78, Ours(), Theirs(), ChiefMove.SteelStorm);
            Assert.AreEqual(BandJob.None, plain.JobOf(plain.FindById("w")));
            Assert.AreEqual(string.Empty, plain.JobHolderId(1, BandJob.Shieldbearer));
        }

        // ------------------------------------------------------------ the campaign

        [Test]
        public void TheSkullpeakChiefFightsWithHerKitAndHerFightSavesAndPlaysBackExactly()
        {
            // Her band at 60%: at 40% the reference pilot breaks her before her first Rifle Line in every one of these forty
            // under rules 9, where your points come back faster and marks crack her nerve (24 Oct 2026).
            var t = new CampaignTuning { PathFights = 0, EnemyScaleTier2 = 60 };
            for (ulong seed = 4500; seed < 4540; seed++)
            {
                Campaign c = Campaign.New(seed, t);
                Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
                c.ChooseTribe(CampaignTribes.SkullpeakId);
                Assert.AreEqual(EncounterKind.Chief, c.Offers[0].Kind);
                Assert.IsFalse(c.MetChief(CampaignTribes.SkullpeakId));
                CampaignFight fight = c.StartEncounter(0);
                Assert.AreEqual(Campaign.RulesVersion, fight.RulesVersion);
                Assert.IsTrue(fight.Setup.Fight.ChiefTactics);
                Assert.AreEqual(ChiefKit.Skullpeak, fight.Sim.Kit(1));
                ChiefMove moves = fight.Sim.KitMoves(1);
                Assert.AreEqual(c.ChiefMovesThisRun(CampaignTribes.Skullpeak), moves);
                Assert.AreEqual(ChiefKits.Takes, Count(moves), "three of her five");
                Assert.AreEqual(moves, moves & ChiefKits.Pool(ChiefKit.Skullpeak));
                Assert.AreEqual((moves & ChiefMove.SteelStorm) != 0 ? ChiefKits.SteelStormEveryTicks : 0, fight.Setup.Rules(1).AutoEclipseTicks);
                Assert.AreEqual(string.Empty, fight.ChiefLine(ChiefKits.KneelLine), "her words are Jacob's to write");

                // Halfway the save holds the fight exactly.
                var pilot = new CampaignPilot(PilotStyle.Reference, seed);
                while (!fight.Over && fight.Sim.CurrentTick < ChiefKits.RifleEveryTicks + 30)
                {
                    pilot.Step(fight);
                    fight.Tick();
                }
                if (fight.Over) continue;
                Campaign half = CampaignSave.Read(CampaignSave.Write(c), t);
                Assert.IsEmpty(half.LoadNote);
                Assert.AreEqual(fight.Session.Checksum, half.Active.Session.Checksum);

                // Played out, you have met her, and the recorded fight plays back exactly.
                CampaignPilot.PlayFight(c, fight, pilot);
                Assert.IsTrue(c.MetChief(CampaignTribes.SkullpeakId), "you have met her now, win or lose");
                Assert.IsTrue(c.Archive.TryReplay(fight.Setup.FightId, out _, out string failure), failure);
                Assert.AreEqual(ChiefKits.Breach, ChiefKits.ChainName(ChiefKit.Skullpeak));
                return;
            }
            Assert.Fail("no Skullpeak chief fight lasted past her first Rifle Line in forty campaigns");
        }
    }
}
