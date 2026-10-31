using System;
using System.Collections.Generic;
using NUnit.Framework;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Sim.Tests
{
    /// The slice as a run (Jacob, 4 Oct 2026: "Kingdom Rush with Pokemon"): pick five plus the
    /// Butcher, break a tribe and its survivors join you, the raider chief escapes the first break
    /// and comes back smarter, his Eclipse is stolen, the Butcher forges gear, tiers set strength,
    /// and losing ends the run while the chiefs remember.
    public sealed class SliceRunTests
    {
        [Test]
        public void ARun_StartsWithTheClarthFive_AgainstTheRaiders()
        {
            SliceRun run = SliceRun.Start(new RunMemory(), 5);
            Assert.AreEqual(1, run.RunNumber);
            Assert.AreEqual(RunStage.Raiders, run.Stage);
            Assert.AreEqual(1, run.NextTier);
            Assert.AreSame(run.Raiders, run.NextEnemy);
            Assert.IsNull(run.Splinter);
            Assert.AreEqual(0, run.PlayerEclipseGrade);
            Assert.AreEqual(SliceRun.SquadSize, run.Reserve.Count);
            CollectionAssert.AreEqual(
                new[] { "clarth.warden", "clarth.marksman", "clarth.skirmisher", "clarth.trapper", "clarth.medic" },
                Ids(run.Reserve));
            CollectionAssert.AreEquivalent(Ids(run.Reserve), run.DefaultSquad());
        }

        [Test]
        public void TheSquad_IsFiveDifferentFightersFromTheReserve()
        {
            SliceRun run = SliceRun.Start(new RunMemory(), 5);
            Assert.Throws<ArgumentException>(() => run.NextFight(new[] { "clarth.warden" }));
            Assert.Throws<ArgumentException>(() => run.NextFight(new[]
                { "clarth.warden", "clarth.warden", "clarth.skirmisher", "clarth.trapper", "clarth.medic" }));
            Assert.Throws<ArgumentException>(() => run.NextFight(new[]
                { "clarth.warden", "nobody", "clarth.skirmisher", "clarth.trapper", "clarth.medic" }));
        }

        [Test]
        public void AFight_IsTheButcherAndFive_AgainstFiveAndTheirChief()
        {
            SliceRun run = SliceRun.Start(new RunMemory(), 5);
            RunTuning tuning = run.Tuning;
            BattleSetup setup = run.NextFight(run.DefaultSquad());
            IReadOnlyList<UnitSpec> us = setup.Side(0);
            IReadOnlyList<UnitSpec> them = setup.Side(1);
            Assert.AreEqual(SliceRun.SquadSize + 1, us.Count);
            Assert.AreEqual(SliceRun.ButcherId, us[0].Id);
            Assert.AreEqual(UnitRole.Pal, us[0].Role);
            Assert.AreEqual(tuning.ButcherPowerPercent, us[0].PowerPercent);

            Assert.AreEqual(tuning.EnemyDeploy + 1, them.Count);
            UnitSpec chief = them[them.Count - 1];
            Assert.AreEqual(UnitRole.Chief, chief.Role);
            Assert.AreEqual(SliceRun.RaiderChiefClass, chief.Class);
            Assert.AreEqual(Row.Back, chief.Row);
            Assert.AreEqual(tuning.Tier1Percent * tuning.ChiefPowerPercent / 100, chief.PowerPercent);
            Assert.AreEqual(tuning.Tier1Percent * tuning.ChiefHealthPercent / 100, chief.HealthPercent);
            for (int i = 0; i < them.Count - 1; i++) Assert.AreEqual(tuning.Tier1Percent, them[i].PowerPercent, "tier 1 strength");

            Assert.AreEqual(tuning.ChiefEclipseGrade, setup.Rules(1).EclipseGrade);
            Assert.AreEqual(tuning.AutoEclipseTicks, setup.Rules(1).AutoEclipseTicks);
            Assert.AreEqual(0, setup.Rules(0).EclipseGrade, "you have no Eclipse yet");
            Assert.IsTrue(setup.Rules(0).Combos);
            Assert.IsTrue(setup.Nerve.Enabled);
            Assert.AreEqual(tuning.UnleashNerve, setup.Nerve.UnleashCost);
            Assert.AreEqual(tuning.HoldDrainEveryTicks, setup.Nerve.HoldDrainEveryTicks);
            Assert.AreEqual(SliceRules.RefillTicks, setup.RefillTicks);
            Assert.AreEqual(SliceRules.HealthPercent, setup.HealthPercent);
        }

        [Test]
        public void BreakingTheRaiders_TheChiefEscapes_SurvivorsJoin_EclipseIsStolen_GearIsForged()
        {
            SliceRun run = SliceRun.Start(new RunMemory(), 5);
            BattleSetup setup = run.NextFight(run.DefaultSquad());
            SimSession session = Play(setup, SliceAutopilot.Style.Smart);
            var sim = (BattleSimulation)session.Simulation;
            int fieldSurvivors = 0;
            foreach (BattleUnit u in sim.Units)
            {
                if (u.Side == 1 && !u.IsLeader && u.Alive) fieldSurvivors++;
            }
            RunFightResult result = run.FinishFight(setup, session);

            Assert.IsTrue(result.Won, "precondition: the autopilot won this one");
            Assert.IsTrue(result.ChiefEscaped);
            Assert.IsFalse(result.ChiefCaught);
            Assert.AreEqual(fieldSurvivors, result.Joined.Count, "those who fought and lived join you");
            foreach (RunFighter f in result.Joined)
            {
                Assert.AreEqual(SliceRun.RaidersId, f.TribeId);
                Assert.AreEqual(1, f.Tier);
                Assert.IsFalse(f.Chief);
                Assert.IsNotNull(run.FindFighter(f.Id), "and they are in your reserve");
            }
            Assert.AreEqual(SliceRun.SquadSize + fieldSurvivors, run.Reserve.Count);

            Assert.IsNotNull(run.Splinter, "the chief got away");
            Assert.AreEqual(5, run.Splinter.Strength, "with the five who stayed home");
            Assert.AreSame(run.Memory.RaiderMind, run.Splinter.Mind, "it is still his mind");

            Assert.AreEqual(SliceRun.GradeFor(result.Record.EndTick, result.PlayerDead), result.StolenGrade);
            Assert.GreaterOrEqual(result.StolenGrade, 1);
            Assert.AreEqual(result.StolenGrade, run.PlayerEclipseGrade);
            CollectionAssert.Contains(run.Memory.Codex, "Move: Eclipse");

            Assert.AreEqual(GearKind.LavaSunTabard, result.LearnedRecipe, "as he runs, the Butcher learns his recipe");
            Assert.AreEqual(GearKind.LavaSunTabard, result.Forged, "and forges it on the spot");
            Assert.IsNotNull(result.GearWearer);
            Assert.AreEqual(GearKind.LavaSunTabard, result.GearWearer.Gear);
            Assert.AreEqual(Gear.FearBonus(GearKind.LavaSunTabard), result.GearWearer.FearPercent);
            CollectionAssert.Contains(run.Memory.Codex, "Recipe: Lava Sun Tabard");
            Assert.AreEqual(RunStage.Skullpeak, run.Stage);
            Assert.IsTrue(run.WarPending);
            Assert.Throws<InvalidOperationException>(() => run.NextFight(run.DefaultSquad()), "the war comes first");

            run.RunWar();
            BattleSetup next = run.NextFight(run.DefaultSquad());
            foreach (UnitSpec spec in next.Side(0))
            {
                if (spec.Id == result.GearWearer.Id)
                    Assert.AreEqual(Gear.FearBonus(GearKind.LavaSunTabard), spec.FearPercent, "the wearer takes it into the next fight");
            }
        }

        [Test]
        public void TheWar_TeachesTheChief_AndLeavesSkullpeakFitToFight()
        {
            SliceRun run = SliceRun.Start(new RunMemory(), 5);
            WinNext(run);
            RunWarResult war = run.RunWar();
            Assert.IsTrue(war.Fought);
            Assert.IsTrue(run.Memory.RaiderMind.HasLearnedFrom(war.FightId), "he learns from his war");
            Assert.GreaterOrEqual(run.Skullpeak.Nerve, run.Tuning.WarNerveFloor);
            Assert.GreaterOrEqual(run.Splinter.Nerve, run.Tuning.WarNerveFloor);
            Assert.IsFalse(run.WarPending);
            Assert.Throws<InvalidOperationException>(() => run.RunWar());

            BattleSetup skullpeak = run.NextFight(run.DefaultSquad());
            IReadOnlyList<UnitSpec> them = skullpeak.Side(1);
            UnitSpec chief = them[them.Count - 1];
            Assert.AreEqual(SliceRun.SkullpeakChiefClass, chief.Class);
            Assert.AreEqual(run.Tuning.Tier2Percent, them[0].PowerPercent, "tier 2 strength");
            Assert.AreEqual(0, skullpeak.Rules(1).EclipseGrade, "the Skullpeak chief has no Eclipse");
            Assert.AreEqual(run.PlayerEclipseGrade, skullpeak.Rules(0).EclipseGrade, "but you have it now");
        }

        [Test]
        public void AWholeRun_CanBeWon_CatchingBothChiefs_AndEveryFightReplays()
        {
            RunOutcome won = null;
            for (ulong seed = 1; seed <= 40 && won == null; seed++)
            {
                RunOutcome outcome = SliceAutopilot.PlayRun(new RunMemory(), seed, SliceAutopilot.Style.Smart);
                if (outcome.Won) won = outcome;
            }
            Assert.IsNotNull(won, "the smart autopilot should win some of 40 runs");
            SliceRun run = won.Run;
            Assert.IsTrue(run.Over);
            Assert.IsNull(run.NextEnemy);
            Assert.AreEqual(1, run.Memory.RunsWon);
            Assert.AreEqual(3, won.Fights.Count);

            RunFighter skullpeakChief = run.FindFighter("skullpeak.chief");
            RunFighter raiderChief = run.FindFighter("raiders.chief");
            Assert.IsNotNull(skullpeakChief);
            Assert.IsNotNull(raiderChief);
            Assert.IsTrue(skullpeakChief.Chief);
            Assert.AreEqual(2, skullpeakChief.Tier);
            Assert.AreEqual(3, raiderChief.Tier);
            Assert.IsTrue(won.Fights[2].ChiefCaught, "the second break catches him");
            Assert.AreEqual(run.Tuning.RematchEclipseGrade, run.Archive.Fights[run.Archive.Fights.Count - 1].Setup.Rules(1).EclipseGrade,
                "he comes back with a harder Eclipse");

            foreach (ArchivedFight fight in run.Archive.Fights)
                Assert.IsTrue(run.Archive.TryReplay(fight.FightId, out _, out string failure), fight.FightId + ": " + failure);
        }

        [Test]
        public void Losing_EndsTheRun_ButTheChiefRemembers()
        {
            var memory = new RunMemory();
            var brutal = new RunTuning { Tier1Percent = 400 };
            SliceRun run = SliceRun.Start(memory, 5, brutal);
            BattleSetup setup = run.NextFight(run.DefaultSquad());
            RunFightResult result = run.FinishFight(setup, Play(setup, SliceAutopilot.Style.Passive));
            Assert.IsFalse(result.Won, "precondition: nobody beats raiders four times as strong");
            Assert.IsTrue(result.RunOver);
            Assert.AreEqual(RunStage.Lost, run.Stage);
            Assert.IsTrue(run.Over);
            Assert.AreEqual(0, result.Joined.Count);
            Assert.Throws<InvalidOperationException>(() => run.NextFight(run.DefaultSquad()));
            Assert.Greater(memory.RaiderMind.FightsLearned.Count, 0, "he learned from beating you");

            SliceRun next = SliceRun.Start(memory, 6);
            Assert.AreEqual(2, next.RunNumber);
            Assert.AreEqual(SliceRun.SquadSize, next.Reserve.Count, "a fresh army");
            Assert.AreEqual(0, next.PlayerEclipseGrade);
            Assert.AreSame(memory.RaiderMind, next.Raiders.Mind, "against the same chief, who remembers");
        }

        [Test]
        public void TheStolenEclipse_IsGradedByHowCleanlyYouWon()
        {
            Assert.AreEqual(3, SliceRun.GradeFor(SliceRun.CleanWinTicks, 0));
            Assert.AreEqual(2, SliceRun.GradeFor(SliceRun.CleanWinTicks + 1, 0));
            Assert.AreEqual(2, SliceRun.GradeFor(SliceRun.CleanWinTicks, 1));
            Assert.AreEqual(2, SliceRun.GradeFor(SliceRun.QuickWinTicks, 1));
            Assert.AreEqual(1, SliceRun.GradeFor(SliceRun.QuickWinTicks + 1, 0));
            Assert.AreEqual(1, SliceRun.GradeFor(SliceRun.CleanWinTicks, 2));
        }

        [Test]
        public void ARun_IsDeterministic()
        {
            RunOutcome a = SliceAutopilot.PlayRun(new RunMemory(), 17, SliceAutopilot.Style.Smart);
            RunOutcome b = SliceAutopilot.PlayRun(new RunMemory(), 17, SliceAutopilot.Style.Smart);
            Assert.AreEqual(a.Run.Snapshot(), b.Run.Snapshot());
        }

        [Test]
        public void AnsweringTheChief_BeatsIgnoringHim()
        {
            const int runs = 60;
            int smartRematches = 0, smartRematchWins = 0, passiveRematches = 0, passiveRematchWins = 0;
            for (ulong seed = 1; seed <= runs; seed++)
            {
                Tally(SliceAutopilot.PlayRun(new RunMemory(), seed * 7919, SliceAutopilot.Style.Smart), ref smartRematches, ref smartRematchWins);
                Tally(SliceAutopilot.PlayRun(new RunMemory(), seed * 7919, SliceAutopilot.Style.Passive), ref passiveRematches, ref passiveRematchWins);
            }
            double smart = smartRematchWins / (double)Math.Max(1, smartRematches);
            double passive = passiveRematchWins / (double)Math.Max(1, passiveRematches);
            TestContext.WriteLine("Rematch wins over " + runs + " runs: answering every Eclipse " + (smart * 100).ToString("0") +
                "% of " + smartRematches + ", never answering " + (passive * 100).ToString("0") + "% of " + passiveRematches + ".");
            Assert.Greater(smart, passive + 0.3, "the chief's comeback punishes a player who never answers him");
            Assert.Greater(smart, 0.4, "and a player who does answer can beat him");
        }

        [Test]
        public void TheLegacyShowcaseKeepsItsCombosAndReplaysExactly()
        {
            // The old seed search promised all defences in a winning demo, which ceased to hold after
            // Jacob's 18 Oct spell-strength change. This fixed recording checks the retained prototype's
            // combo and replay contract; it makes no promise about demo coverage or balance.
            RunOutcome outcome = SliceAutopilot.PlayRun(new RunMemory(), 1, SliceAutopilot.Style.Showcase);
            Assert.IsTrue(outcome.Won);
            Assert.Greater(outcome.Ambushes, 0);
            Assert.Greater(outcome.Pyres, 0);
            Assert.Greater(outcome.Frenzies, 0);
            foreach (var fight in outcome.Run.Archive.Fights)
            {
                Assert.IsTrue(outcome.Run.Archive.TryReplay(fight.FightId, out _, out string why), why);
                Assert.IsTrue(FightPlayback.TryOpen(outcome.Run.Archive, fight.FightId, out var p, out why), why);
                Assert.IsTrue(p.Seek(p.Length), p.Failure);
                Assert.IsTrue(p.Matched);
            }
        }

        // ---------------------------------------------------------------- helpers

        private static List<string> Ids(IReadOnlyList<RunFighter> fighters)
        {
            var ids = new List<string>();
            foreach (RunFighter f in fighters) ids.Add(f.Id);
            return ids;
        }

        private static SimSession Play(BattleSetup setup, SliceAutopilot.Style style)
        {
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            var pilot = new SliceAutopilot(style);
            while (!sim.IsFinished && sim.CurrentTick < setup.TickLimit)
            {
                pilot.Step(session, sim);
                session.Tick();
            }
            return session;
        }

        private static RunFightResult WinNext(SliceRun run)
        {
            BattleSetup setup = run.NextFight(run.DefaultSquad());
            RunFightResult result = run.FinishFight(setup, Play(setup, SliceAutopilot.Style.Smart));
            Assert.IsTrue(result.Won, "precondition: the autopilot won " + setup.FightId);
            return result;
        }

        private static void Tally(RunOutcome outcome, ref int rematches, ref int wins)
        {
            foreach (RunFightResult f in outcome.Fights)
            {
                if (f.Fought != RunStage.Rematch) continue;
                rematches++;
                if (f.Won) wins++;
            }
        }
    }
}
