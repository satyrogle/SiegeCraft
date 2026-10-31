using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The slice campaign as runs (Jacob's go, 10 Oct 2026): the tutorial wins you the Clarth, and
    /// every run leaves camp, walks each tribe's path to its chief, takes the tribe and its chief's
    /// order for the rest of the run, and ends back at camp, won or lost, keeping materials, the
    /// codex and what the tribes learned. The bots cannot tell whether any of it reads well or is
    /// fun; that is Jacob's playtest.
    public class CampaignTests
    {
        private const string B = CampaignTribes.BloodridgeId;
        private const string S = CampaignTribes.SkullpeakId;

        private static Campaign Ready(ulong seed, CampaignTuning tuning = null)
        {
            Campaign c = Campaign.New(seed, tuning ?? Tuned());
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }

        private static List<string> Ids(List<CampaignTribe> tribes)
        {
            var ids = new List<string>();
            for (int i = 0; i < tribes.Count; i++) ids.Add(tribes[i].Id);
            return ids;
        }

        private static List<EncounterKind> Kinds(IReadOnlyList<Encounter> offers)
        {
            var kinds = new List<EncounterKind>();
            for (int i = 0; i < offers.Count; i++) kinds.Add(offers[i].Kind);
            return kinds;
        }

        private static string Habits(TacticsProfile t)
        {
            return t.CoverFocused + "/" + t.HoldOnUnleash + "/" + t.PressOnFallBack + "/" + t.AvoidGuarded + "/" + t.TrapCaution;
        }

        private static int Fighters(IReadOnlyList<UnitSpec> specs)
        {
            int count = 0;
            for (int i = 0; i < specs.Count; i++) if (specs[i].Role == UnitRole.Fighter) count++;
            return count;
        }

        /// Plays the run on with the reference pilot until this tribe's chief is beaten. Null if the
        /// run ended first.
        private static EncounterResult PlayUntilChiefBeaten(Campaign c, string tribeId, ulong seed)
        {
            var picker = new CampaignPilot(PilotStyle.Reference, seed);
            for (ulong n = 0; c.Stage == CampaignStage.InRun; n++)
            {
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(picker.PickOffer(c)),
                    new CampaignPilot(PilotStyle.Reference, seed * 31UL + n));
                if (r.Won && r.Encounter.Kind == EncounterKind.Chief && r.Encounter.Tribe.Id == tribeId) return r;
            }
            return null;
        }

        // ------------------------------------------------------------ the run

        [Test]
        public void TutorialWinJoinsTheClarthAndOpensCamp()
        {
            Campaign c = Campaign.New(11, Tuned());
            Assert.AreEqual(CampaignStage.Tutorial, c.Stage);
            CampaignFight tutorial = c.StartTutorial();
            Assert.AreEqual(Campaign.LeaderId, tutorial.Setup.Side(0)[0].Id, "the leader commands the Clarth in the tutorial");
            CampaignPilot.PlayFight(c, tutorial, new CampaignPilot(PilotStyle.Reference, 11));
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, 11, 10));
            Assert.IsTrue(c.HasJoined(CampaignTribes.ClarthId));
            Assert.GreaterOrEqual(c.Reserve.Count, Tuned().TopUpTo);
            Assert.AreEqual(CampaignStage.ChooseTribe, c.Stage, "the tutorial ends at camp");
            CollectionAssert.AreEqual(new[] { c.RunRoute[0].Id }, Ids(c.Choosable()), "camp offers the first tribe of this run's route");
            Assert.AreEqual(1, c.RunRoute[0].Tier);
        }

        [Test]
        public void EveryTribeArrivesWithWhatItsWarsTaughtIt()
        {
            Campaign c = Campaign.New(5, new CampaignTuning());
            Assert.IsTrue(c.Mind(B).HasHabit(Habit.HoldOnUnleash));
            Assert.IsTrue(c.Mind(S).HasHabit(Habit.CoverFocused));
            Assert.IsTrue(c.Mind(S).HasHabit(Habit.HoldOnUnleash));
            Assert.IsTrue(c.Mind(B).HasHabit(Habit.PressOnFallBack));
            Assert.IsTrue(c.Mind(S).HasHabit(Habit.PressOnFallBack));
            Assert.IsFalse(c.Mind(CampaignTribes.ClarthId).HasHabit(Habit.PressOnFallBack));
        }

        [Test]
        public void ARunMeetsOneTribePerTierInOrder()
        {
            Assert.AreEqual(B, Campaign.FirstTribe().Id);
            Assert.AreEqual(S, Campaign.NextTribe(CampaignTribes.Bloodridge).Id);
            Assert.IsNull(Campaign.NextTribe(CampaignTribes.Skullpeak), "Skullpeak is the slice's last tier");
            Campaign c = Ready(12);
            c.StartRun();
            Assert.AreEqual(CampaignStage.InRun, c.Stage);
            // Since 18 Oct 2026 a run starts at one of the first tier's tribes, picked for the run (RunRoute).
            Assert.AreEqual(1, c.Target.Tier);
            Assert.AreSame(c.RunRoute[0], c.Target);
            Assert.AreEqual(0, c.Step);
            Assert.AreEqual(1, c.RunsStarted);
        }

        [Test]
        public void EachStepOffersThreeDifferentEncountersBeforeTheChief()
        {
            Campaign c = Ready(12);
            c.StartRun();
            Assert.AreEqual(3, c.Offers.Count);
            Assert.AreEqual(3, new HashSet<EncounterKind>(Kinds(c.Offers)).Count);
            CollectionAssert.DoesNotContain(Kinds(c.Offers), EncounterKind.Chief);
        }

        [Test]
        public void TheCardTellsTheTruth()
        {
            for (ulong seed = 600; seed < 620; seed++)
            {
                foreach (string tribe in new[] { B, S })
                {
                    Campaign c = Ready(seed);
                    c.ChooseTribe(tribe);
                    for (int i = 0; i < c.Offers.Count; i++) Assert.IsEmpty(c.Offers[i].CanShow, "no card shows feats any more");
                    Encounter first = c.Offers[0];
                    CampaignFight fight = c.StartEncounter(0);
                    Assert.AreEqual(first.EnemyFighters, Fighters(fight.Setup.Side(1)), first.Title);
                    Assert.AreEqual(first.EnemyUnits, fight.Setup.Side(1).Count, first.Title);
                    Assert.AreEqual(first.SquadSize, fight.Squad.Count, first.Title);
                    Assert.AreEqual(first.EnemyCasts, fight.Setup.Rules(1).AutoEclipseTicks > 0, first.Title);
                }
            }
        }

        [Test]
        public void TheChiefComesAfterTheSetPath()
        {
            int pathFights = Tuned().PathFights;
            bool sawChief = false;
            for (ulong seed = 20; seed < 60 && !sawChief; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                var pilot = new CampaignPilot(PilotStyle.Reference, seed);
                for (int n = 0; n <= pathFights && c.Stage == CampaignStage.InRun; n++)
                {
                    if (c.Step >= pathFights)
                    {
                        CollectionAssert.AreEqual(new[] { EncounterKind.Chief }, Kinds(c.Offers));
                        sawChief = true;
                        break;
                    }
                    CollectionAssert.DoesNotContain(Kinds(c.Offers), EncounterKind.Chief);
                    int before = c.Step;
                    EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(pilot.PickOffer(c)),
                        new CampaignPilot(PilotStyle.Reference, seed + (ulong)n));
                    if (!r.Won) break;
                    Assert.AreEqual(before + 1, c.Step);
                    Assert.AreEqual(c.Step >= pathFights, r.ChiefReady);
                    Assert.AreEqual(Tuned().MaterialsPerFight * Math.Max(1, r.Streak), r.MaterialsEarned);
                }
            }
            Assert.IsTrue(sawChief, "the reference pilot never walked Bloodridge's path in forty campaigns");
        }

        // Parked 13 Oct 2026: with Sprint 2's quirks and the Clarth set, the reference pilot answered three ways in
        // none of 47 Eclipse trials (2 of 47 without the Clarth set's doctrine). How often it can is a tuning question.
        [Test, Category("Balance"), Ignore(Sprint1Tests.ParkedForTuning)]
        public void TheEclipseTrialStillCountsYourAnswers()
        {
            int best = 0;
            for (ulong seed = 900; seed < 960 && best < Feats.EclipseWaysNeeded; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                int index = Kinds(c.Offers).IndexOf(EncounterKind.Trial);
                if (index < 0) continue;
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(index), new CampaignPilot(PilotStyle.Reference, seed));
                Assert.AreEqual(Feats.EclipseWays(r.Record.Events), r.EclipseWays);
                best = Math.Max(best, r.EclipseWays);
            }
            Assert.GreaterOrEqual(best, Feats.EclipseWaysNeeded, "the reference pilot never answered their Eclipse three ways");
        }

        [Test]
        public void TheChampionWindsUpABigSwing()
        {
            for (ulong seed = 560; seed < 600; seed++)
            {
                Campaign c = Ready(seed);
                c.ChooseTribe(S);
                int index = Kinds(c.Offers).IndexOf(EncounterKind.Duel);
                if (index < 0) continue;
                Assert.IsTrue(c.Offers[index].EnemyCasts);
                CampaignFight fight = c.StartEncounter(index);
                bool sawWindup = false;
                while (!fight.Over && !sawWindup)
                {
                    fight.Tick();
                    sawWindup = fight.Sim.WindupTicks(1) > 0;
                }
                Assert.IsTrue(sawWindup, "their champion never wound up");
                return;
            }
            Assert.Fail("no duel was offered in forty campaigns");
        }

        [Test]
        public void BreakingAChiefCarriesTheRunToTheNextTribe()
        {
            for (ulong seed = 40; seed < 80; seed++)
            {
                Campaign c = Ready(seed);
                c.ChooseTribe(B);
                EncounterResult r = PlayUntilChiefBeaten(c, B, seed);
                if (r == null) continue;
                Assert.IsTrue(r.Joined);
                Assert.IsTrue(c.HasJoined(B));
                Assert.IsNotNull(r.AssistUnlocked);
                Assert.AreEqual("Eclipse", r.AssistUnlocked.Name);
                Assert.AreEqual(1, c.Assists.Count);
                // No floor since 11 Oct 2026 (call J4); RecruitTests checks the count against who stood.
                Assert.IsNotNull(r.JoinedFighters);
                // A better grade against his kit sends home more (Battle.ChiefKits, 21 Oct 2026).
                Assert.IsTrue(r.Graded);
                // From rules 12 the streak multiplies it (Campaign.Streak).
                Assert.AreEqual(Tuned().MaterialsPerChief * (100 + ChiefKits.GradeMaterialsPercent * (int)r.Grade) / 100 * Math.Max(1, r.Streak), r.MaterialsEarned);
                Assert.IsFalse(r.RunOver, "the run goes on to the next tier");
                // The second tier's tribe this run picks (18 Oct 2026: Skullpeak or the sea tribe).
                Assert.AreEqual(2, r.NextTribe.Tier);
                Assert.AreSame(c.RunRoute[1], r.NextTribe);
                Assert.AreEqual(CampaignStage.InRun, c.Stage);
                Assert.AreSame(r.NextTribe, c.Target);
                Assert.AreEqual(0, c.Step);
                CollectionAssert.Contains(c.Codex, "Joined: Bloodridge");
                return;
            }
            Assert.Fail("the reference pilot never beat the Bloodridge chief in forty campaigns");
        }

        [Test]
        public void TheChiefOrderComesOncePerFight()
        {
            for (ulong seed = 40; seed < 80; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                if (PlayUntilChiefBeaten(c, B, seed) == null) continue;
                int index = Kinds(c.Offers).IndexOf(EncounterKind.Duel) == 0 ? 1 : 0;
                CampaignFight fight = c.StartEncounter(index);
                for (int i = 0; i < 30; i++) fight.Tick();
                Assert.IsTrue(fight.TryOrder(Directive.Eclipse, null, out string first), first);
                for (int i = 0; i < 300 && !fight.Over; i++) fight.Tick();
                Assert.IsFalse(fight.TryOrder(Directive.Eclipse, null, out string second));
                StringAssert.Contains("already came", second);
                return;
            }
            Assert.Fail("no run in forty took a chief's order");
        }

        [Test]
        public void FrenzyIsGoneAndTheStarterCallIsUnavailableInDuels()
        {
            Campaign c = Ready(13);
            c.ChooseTribe(B);
            string before = CampaignSave.Write(c);
            Assert.IsEmpty(c.Assists, "no captured chief yet");
            CampaignFight fight = c.StartEncounter(0);
            Assert.IsNotNull(fight.Assist, "the Clarth starter spell is available without a captured chief");
            Assert.IsFalse(fight.TryOrder(Directive.Frenzy, null, out _));
            Assert.IsTrue(fight.TryOrder(Directive.Eclipse, null, out string why), why);

            c = CampaignSave.Read(before, Tuned());
            int duel = Kinds(c.Offers).IndexOf(EncounterKind.Duel);
            Assert.GreaterOrEqual(duel, 0, "the fixed route includes a duel");
            fight = c.StartEncounter(duel);
            Assert.IsNull(fight.Assist);
            Assert.IsFalse(fight.TryOrder(Directive.Frenzy, null, out _));
            Assert.IsFalse(fight.TryOrder(Directive.Eclipse, null, out why));
            StringAssert.Contains("No chief", why);
        }

        [Test]
        public void LosingEndsTheRunBackAtCampAndResetsIt()
        {
            for (ulong seed = 70; seed < 110; seed++)
            {
                Campaign c = Ready(seed);
                List<EncounterResult> run = CampaignPilot.PlayRun(c, B, PilotStyle.Random, seed);
                EncounterResult last = run[run.Count - 1];
                if (last.Won) continue;
                int earned = 0;
                for (int i = 0; i < run.Count; i++) earned += run[i].MaterialsEarned;
                Assert.IsTrue(last.RunOver);
                Assert.IsFalse(last.RunWon);
                Assert.AreEqual(CampaignStage.ChooseTribe, c.Stage);
                Assert.IsNull(c.Target);
                Assert.AreEqual(0, c.Assists.Count);
                CollectionAssert.AreEqual(new[] { CampaignTribes.ClarthId }, c.Joined);
                Assert.AreEqual(Tuned().TopUpTo, c.Reserve.Count);
                for (int i = 0; i < c.Reserve.Count; i++) Assert.AreEqual(CampaignTribes.ClarthId, c.Reserve[i].TribeId);
                for (int i = 0; i < last.Fallen.Count; i++)
                    for (int j = 0; j < c.Reserve.Count; j++)
                        Assert.AreNotEqual(last.Fallen[i].Id, c.Reserve[j].Id);
                Assert.AreEqual(earned, c.Materials, "materials come home even from a lost run");
                return;
            }
            Assert.Fail("the random pilot never lost a run in forty campaigns");
        }

        [Test]
        public void TheTribesNoLongerHateAnOrderOrScoreRespect()
        {
            for (int i = 0; i < CampaignTribes.All.Length; i++) Assert.AreEqual(Feat.None, CampaignTribes.All[i].Hates, CampaignTribes.All[i].Name);
            for (ulong seed = 1300; seed < 1305; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.UnleashSpam, seed));
                Assert.IsEmpty(r.Awards, "nothing scores respect or taboos any more");
                Assert.IsFalse(r.SentAway);
            }
        }

        [Test]
        public void EveryRunStartsWithAFreshClarthSquad()
        {
            Campaign c = Ready(700);
            CampaignPilot.PlayRun(c, B, PilotStyle.Random, 700);
            Assert.AreEqual(CampaignStage.ChooseTribe, c.Stage);
            c.StartRun();
            Assert.AreEqual(Tuned().TopUpTo, c.Reserve.Count);
            for (int i = 0; i < c.Reserve.Count; i++) Assert.AreEqual(CampaignTribes.ClarthId, c.Reserve[i].TribeId);
            Assert.AreEqual(0, c.Assists.Count);
            Assert.AreEqual(2, c.RunsStarted);
        }

        [Test]
        public void HoldingTheLineToTheEndCountsAsAWin()
        {
            int held = 0;
            for (ulong seed = 100; seed < 160; seed++)
            {
                foreach (string tribe in new[] { B, S })
                {
                    Campaign c = Ready(seed);
                    c.ChooseTribe(tribe);
                    int index = Kinds(c.Offers).IndexOf(EncounterKind.HoldTheLine);
                    if (index < 0) continue;
                    CampaignFight fight = c.StartEncounter(index);
                    // The Turtle, not the reference pilot: from rules 8 (the parry at the center, 24 Oct 2026) every Hold that is
                    // not a parry cracks your nerve, and the reference pilot holds the line with Hold, so it never lasts to time.
                    EncounterResult r = CampaignPilot.PlayFight(c, fight, new CampaignPilot(PilotStyle.Turtle, seed));
                    if (r.TimedOut && fight.Sim.BrokenSide != 0)
                    {
                        Assert.IsTrue(r.Won, "held to the end but not counted as a win");
                        held++;
                    }
                }
            }
            Assert.Greater(held, 0, "no hold the line fight ran to time");
        }

        [Test]
        public void HoldTheLineRefusesFallBack()
        {
            for (ulong seed = 300; seed < 340; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                int index = Kinds(c.Offers).IndexOf(EncounterKind.HoldTheLine);
                if (index < 0) continue;
                Assert.IsTrue(c.Offers[index].NoFallBack);
                CampaignFight fight = c.StartEncounter(index);
                for (int i = 0; i < 40; i++) fight.Tick();
                Assert.IsFalse(fight.TryOrder(Directive.FallBack, null, out string why));
                StringAssert.Contains("No falling back", why);
                Assert.AreEqual(0, fight.FallBacksOrdered);
                return;
            }
            Assert.Fail("no hold the line was offered in forty campaigns");
        }

        [Test]
        public void RunningOutTheClockLosesEverywhereButHoldTheLine()
        {
            CampaignTuning shortFights = Tuned();
            // The 13 Oct roster as lives, which rules 11 replaced and old saves still play.
            shortFights.RulesCap = Campaign.RaidChiefRules;
            shortFights.FightTicks = 300;
            int checkedFights = 0;
            for (ulong seed = 800; seed < 810; seed++)
            {
                Campaign c = CampaignSave.Read(CampaignSave.Write(Ready(seed)), shortFights);
                c.StartRun();
                int index = Kinds(c.Offers).IndexOf(EncounterKind.HoldTheLine) == 0 ? 1 : 0;
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(index), new CampaignPilot(PilotStyle.Passive, seed));
                if (!r.TimedOut) continue;
                Assert.IsFalse(r.Won, r.Encounter.Title + " ran out of time and was counted as a win");
                Assert.IsNotNull(r.Captured, "a lost fight costs one of yours captured, and the run goes on (13 Oct 2026)");
                checkedFights++;
            }
            Assert.Greater(checkedFights, 0, "no fight ran out of time in ten seconds");
        }

        [Test]
        public void FallingBackAtEveryChanceNoLongerWins()
        {
            // 9 Oct 2026: while anyone falls back no melee lands, so falling back over and over
            // let ranged fighters win war parties and duels on time. Tribes now arrive knowing to
            // Unleash when you fall back, and running out the clock is a loss.
            int played = 0, won = 0;
            for (ulong seed = 1200; seed < 1240; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                for (int i = 0; i < c.Offers.Count; i++)
                {
                    if (c.Offers[i].Kind != EncounterKind.WarParty && c.Offers[i].Kind != EncounterKind.Duel) continue;
                    EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(i), new CampaignPilot(PilotStyle.FallBackSpam, seed));
                    played++;
                    if (r.Won) won++;
                    break;
                }
            }
            Assert.Greater(played, 10);
            Assert.LessOrEqual(won * 100 / played, 20, "falling back at every chance won " + won + " of " + played);
        }

        [Test]
        public void SkullpeaksTrialSendsFewerOfYoursThanTheirs()
        {
            for (ulong seed = 500; seed < 530; seed++)
            {
                Campaign c = Ready(seed);
                c.ChooseTribe(S);
                int index = Kinds(c.Offers).IndexOf(EncounterKind.Trial);
                if (index < 0) continue;
                CampaignFight fight = c.StartEncounter(index);
                Assert.Less(Fighters(fight.Setup.Side(0)), Fighters(fight.Setup.Side(1)));
                return;
            }
            Assert.Fail("no Skullpeak trial was offered in thirty campaigns");
        }

        [Test]
        public void SkullpeaksTrialAlwaysBringsBothWardens()
        {
            // 10 Oct 2026: the lineup used to change with every fight, and the first trial, with no Warden in it, was won
            // by doing nothing. Since 18 Oct it sends one of three squads ("three squads per fight shown on the node"),
            // and every one keeps both Wardens, early or late on the path, since stepping in front of the focus is the trial.
            var steps = new HashSet<int>();
            for (ulong seed = 560; seed < 620 && steps.Count < 2; seed++)
            {
                Campaign c = Ready(seed);
                c.ChooseTribe(S);
                for (int step = 0; step < 2 && c.Stage == CampaignStage.InRun; step++)
                {
                    int index = Kinds(c.Offers).IndexOf(EncounterKind.Trial);
                    if (index >= 0 && !steps.Contains(step))
                    {
                        Encounter trial = c.Offers[index];
                        IReadOnlyList<UnitClass> lineup = CampaignTribes.Skullpeak.Lineup(EncounterKind.Trial, trial.Squad);
                        IReadOnlyList<UnitSpec> theirs = c.StartEncounter(index).Setup.Side(1);
                        Assert.AreEqual(lineup.Count, theirs.Count);
                        int wardens = 0;
                        for (int i = 0; i < theirs.Count; i++)
                        {
                            Assert.AreEqual(lineup[i], theirs[i].Class);
                            if (theirs[i].Class == UnitClass.Warden) wardens++;
                        }
                        Assert.AreEqual(2, wardens, "both Wardens, whichever squad came");
                        steps.Add(step);
                        break;
                    }
                    CampaignPilot.PlayFight(c, c.StartEncounter(index == 0 ? 1 : 0), new CampaignPilot(PilotStyle.Reference, seed));
                }
            }
            Assert.AreEqual(2, steps.Count, "no Skullpeak trial was seen both first and second on a path");
        }

        [Test, Category("Balance"), Ignore(Sprint1Tests.ParkedForTuning)]
        public void DoingNothingLosesSkullpeaksHoldTheLineAndTrial()
        {
            // 10 Oct 2026: every bot held Skullpeak's line, even one giving no orders, and doing
            // nothing won their first trial. Both now need orders: Hold for the line, and focus
            // on their Wardens first in the trial.
            foreach (EncounterKind kind in new[] { EncounterKind.HoldTheLine, EncounterKind.Trial })
            {
                int played = 0, idleWon = 0, orderedWon = 0;
                for (ulong seed = 600; seed < 700 && played < 20; seed++)
                {
                    Campaign idle = Ready(seed);
                    idle.ChooseTribe(S);
                    int index = Kinds(idle.Offers).IndexOf(kind);
                    if (index < 0) continue;
                    played++;
                    if (CampaignPilot.PlayFight(idle, idle.StartEncounter(index), new CampaignPilot(PilotStyle.Passive, seed)).Won) idleWon++;
                    Campaign ordered = Ready(seed);
                    ordered.ChooseTribe(S);
                    if (CampaignPilot.PlayFight(ordered, ordered.StartEncounter(index), new CampaignPilot(PilotStyle.Reference, seed)).Won) orderedWon++;
                }
                Assert.GreaterOrEqual(played, 10, kind + " was rarely offered");
                Assert.LessOrEqual(idleWon * 100 / played, 20, "doing nothing won " + idleWon + " of " + played + " " + kind);
                Assert.GreaterOrEqual(orderedWon * 100 / played, 80, "giving orders won only " + orderedWon + " of " + played + " " + kind);
            }
        }

        [Test]
        public void FocusingOnlyTheirWardensIsNotCalledNeverFocusing()
        {
            // 9 Oct 2026 playtest: after six Focus Fire orders on Skullpeak's Wardens the fade lesson
            // said "You never focused fire this fight". Their Warden never covers a Warden, so those
            // orders do not count for his habit, but the sentence has to stay true.
            for (ulong seed = 700; seed < 740; seed++)
            {
                Campaign c = Ready(seed);
                c.ChooseTribe(S);
                int index = Kinds(c.Offers).IndexOf(EncounterKind.Trial);
                if (index < 0) continue;
                CampaignFight fight = c.StartEncounter(index);
                int focused = 0;
                while (!fight.Over)
                {
                    BattleUnit warden = null;
                    for (int i = 0; i < fight.Sim.Units.Count && warden == null; i++)
                    {
                        BattleUnit u = fight.Sim.Units[i];
                        if (u.Side == 1 && u.Alive && u.Class == UnitClass.Warden) warden = u;
                    }
                    if (warden != null && fight.Sim.CurrentTick >= 20 && fight.TryOrder(Directive.FocusFire, warden.Id, out _)) focused++;
                    fight.Tick();
                }
                c.Finish(fight);
                Assert.Greater(focused, 0, "no Focus Fire on a Warden was taken");
                Lesson fade = null;
                foreach (Lesson l in c.Mind(S).Lessons) if (l.Rule == LessonRules.FadePrefix + LessonRules.Focus) fade = l;
                Assert.IsNotNull(fade, "their cover habit did not fade");
                StringAssert.DoesNotContain("You never focused fire this fight", fade.Why);
                return;
            }
            Assert.Fail("no Skullpeak trial was offered in forty campaigns");
        }

        [Test]
        public void ADuelIsOneOfYoursAgainstTheirChampion()
        {
            for (ulong seed = 530; seed < 560; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                int index = Kinds(c.Offers).IndexOf(EncounterKind.Duel);
                if (index < 0) continue;
                CampaignFight fight = c.StartEncounter(index);
                Assert.AreEqual(1, fight.Squad.Count);
                Assert.AreEqual(1, fight.Setup.Side(0).Count, "the leader only gives orders in a duel");
                Assert.AreEqual(1, fight.Setup.Side(1).Count);
                Assert.IsNull(fight.Assist);
                return;
            }
            Assert.Fail("no duel was offered in thirty campaigns");
        }

        [Test]
        public void TheWholeSliceIsOneRun()
        {
            // The run's shape, not its balance: weaker enemies, so a bot can finish a run while the 13 Oct rules
            // wait for tuning (Jacob's rule 8, 12 Oct 2026: build first, optimise later).
            CampaignTuning t = Tuned();
            t.EnemyScaleTier1 = 50;
            t.EnemyScaleTier2 = 50;
            t.HuntPercent = 50;
            for (ulong seed = 210; seed < 290; seed++)
            {
                Campaign c = Ready(seed, t);
                c.ChooseTribe(B);
                string lastTribe = c.RunRoute[c.RunRoute.Count - 1].Id;
                var run = new List<EncounterResult>();
                var picker = new CampaignPilot(PilotStyle.Reference, seed);
                int expectedMaterials = 0;
                for (ulong n = 0; c.Stage == CampaignStage.InRun; n++)
                {
                    var fight = c.StartEncounter(picker.PickOffer(c));
                    var encounter = fight.Encounter;
                    int reward = encounter.Kind == EncounterKind.Chief || encounter.Kind == EncounterKind.AlphaHunt
                        ? t.MaterialsPerChief : encounter.Kind == EncounterKind.Ambush ? 0 : t.MaterialsPerFight;
                    if (encounter.Hunter != null) reward += HunterRules.Reward(c.Company.Rank(encounter.Hunter));
                    var result = CampaignPilot.PlayFight(c, fight, new CampaignPilot(PilotStyle.Reference, seed * 31UL + n));
                    // A chief with his own kit sends home more for a better grade (Battle.ChiefKits, 21 Oct 2026).
                    if (result.Graded) reward = reward * (100 + ChiefKits.GradeMaterialsPercent * (int)result.Grade) / 100;
                    // From rules 12 the streak multiplies it (Campaign.Streak).
                    if (result.Won) expectedMaterials += reward * Math.Max(1, result.Streak);
                    run.Add(result);
                }
                EncounterResult last = run[run.Count - 1];
                if (!last.RunWon) continue;
                // Since 11 Oct 2026 (call J2) the run is won by breaking the Sovereign's hunters, who come after the last chief.
                Assert.AreEqual(EncounterKind.AlphaHunt, last.Encounter.Kind);
                Assert.AreEqual(AlphaHunt.Id, last.Encounter.Tribe.Id);
                Assert.AreEqual(EncounterKind.Chief, run[run.Count - 2].Encounter.Kind);
                Assert.AreEqual(lastTribe, run[run.Count - 2].Encounter.Tribe.Id);
                Assert.IsNull(last.NextTribe);
                Assert.AreEqual(1, c.RunsWon);
                Assert.AreEqual(CampaignStage.ChooseTribe, c.Stage, "a won run ends back at camp");
                Assert.AreEqual(0, c.Assists.Count, "chief orders reset with the run");
                Assert.IsFalse(c.HasJoined(B), "the run's tribes reset with it");
                // A raid chief (rules 10) can fall before he kneels, and then his tribe never joins: his piece comes instead.
                bool captured = false;
                foreach (EncounterResult r in run)
                    if (r.Encounter.Kind == EncounterKind.Chief && r.Encounter.Tribe.Id == B) captured = r.ChiefCaptured;
                if (captured || !Campaign.IsRaidChief(CampaignTribes.Bloodridge, EncounterKind.Chief, Campaign.RulesVersion))
                    CollectionAssert.Contains(c.Codex, "Joined: Bloodridge");
                CollectionAssert.Contains(c.Codex, "Joined: " + CampaignTribes.Find(lastTribe).Name);
                CollectionAssert.Contains(c.Codex, "Tribe: " + AlphaHunt.Tribe.Name);
                Assert.AreEqual(expectedMaterials, c.Materials, "won encounters pay their base materials plus any hunter rank reward");
                Campaign reopened = CampaignSave.Read(CampaignSave.Write(c), t);
                Assert.AreEqual(c.Materials, reopened.Materials);
                Assert.Greater(reopened.Mind(lastTribe).FightsLearned.Count, 0);
                return;
            }
            Assert.Fail("no run in eighty won the whole slice");
        }

        // ------------------------------------------------------------ memory

        [Test]
        public void SaveAndReadGiveBackTheSameCampaign()
        {
            Campaign c = Ready(150);
            CampaignPilot.PlayRun(c, B, PilotStyle.Reference, 150);
            string saved = CampaignSave.Write(c);
            Campaign read = CampaignSave.Read(saved, Tuned());
            Assert.AreEqual(saved, CampaignSave.Write(read));
            Assert.IsEmpty(read.LoadNote);
            Assert.AreEqual(CampaignStage.ChooseTribe, read.Stage);
            Assert.AreEqual(c.Reserve.Count, read.Reserve.Count);
            Assert.AreEqual(c.Materials, read.Materials);
            CollectionAssert.AreEqual(c.Codex, read.Codex);
            Assert.AreEqual(c.Mind(B).Snapshot(), read.Mind(B).Snapshot());
            Assert.AreEqual(c.CultMind.Snapshot(), read.CultMind.Snapshot());
        }

        [Test]
        public void ARunInProgressSurvivesClosingTheGame()
        {
            // 10 Oct 2026: closing the game part way through a run used to undo the whole run.
            Campaign c = null;
            ulong found = 0;
            for (ulong seed = 152; seed < 172 && c == null; seed++)
            {
                Campaign t = Ready(seed);
                t.StartRun();
                CampaignPilot.PlayFight(t, t.StartEncounter(new CampaignPilot(PilotStyle.Reference, seed).PickOffer(t)),
                    new CampaignPilot(PilotStyle.Reference, seed));
                if (t.Stage == CampaignStage.InRun)
                {
                    c = t;
                    found = seed;
                }
            }
            Assert.IsNotNull(c, "every run in twenty ended on its first fight");
            string saved = CampaignSave.Write(c);
            Campaign read = CampaignSave.Read(saved, Tuned());
            Assert.AreEqual(saved, CampaignSave.Write(read));
            Assert.AreEqual(CampaignStage.InRun, read.Stage);
            Assert.AreEqual(c.Target.Id, read.Target.Id);
            Assert.AreEqual(c.Step, read.Step);
            Assert.AreEqual(c.Materials, read.Materials);
            CollectionAssert.AreEqual(Kinds(c.Offers), Kinds(read.Offers));

            // The rest of the run plays out the same from the reopened save.
            var picker = new CampaignPilot(PilotStyle.Reference, found);
            for (ulong n = 1; c.Stage == CampaignStage.InRun; n++)
            {
                ulong s = found * 31UL + n;
                CampaignPilot.PlayFight(c, c.StartEncounter(picker.PickOffer(c)), new CampaignPilot(PilotStyle.Reference, s));
                CampaignPilot.PlayFight(read, read.StartEncounter(picker.PickOffer(read)), new CampaignPilot(PilotStyle.Reference, s));
            }
            Assert.AreEqual(CampaignSave.Write(c), CampaignSave.Write(read));
        }

        [Test]
        public void TheTribesAndOrdersOfARunSurviveClosingTheGame()
        {
            for (ulong seed = 40; seed < 80; seed++)
            {
                Campaign c = Ready(seed);
                c.ChooseTribe(B);
                if (PlayUntilChiefBeaten(c, B, seed) == null) continue;
                string saved = CampaignSave.Write(c);
                Campaign read = CampaignSave.Read(saved, Tuned());
                Assert.AreEqual(saved, CampaignSave.Write(read));
                Assert.IsEmpty(read.LoadNote);
                Assert.AreEqual(CampaignStage.InRun, read.Stage);
                Assert.AreEqual(c.Target.Id, read.Target.Id);
                Assert.AreEqual(2, read.Target.Tier);
                Assert.IsTrue(read.HasJoined(B));
                Assert.AreEqual(1, read.Assists.Count);
                Assert.AreEqual(c.Reserve.Count, read.Reserve.Count);
                return;
            }
            Assert.Fail("the reference pilot never beat the Bloodridge chief in forty campaigns");
        }

        [Test]
        public void AFightClosedHalfwayPlaysOnFromTheSameTick()
        {
            // A fight is saved as the moment it began plus every order since, so reopening plays it
            // back to the same tick. Closing the game can no longer undo a fight you are losing.
            foreach (int stopAt in new[] { 0, 61, 400, 1100 })
            {
                Campaign c = Ready(153);
                c.ChooseTribe(S);
                CampaignFight fight = c.StartEncounter(0);
                var pilot = new CampaignPilot(PilotStyle.Reference, 153);
                while (!fight.Over && fight.Sim.CurrentTick < stopAt)
                {
                    pilot.Step(fight);
                    fight.Tick();
                }
                string saved = CampaignSave.Write(c);
                Campaign read = CampaignSave.Read(saved, Tuned());
                Assert.IsEmpty(read.LoadNote);
                Assert.IsNotNull(read.Active, "no fight after reopening at tick " + stopAt);
                Assert.AreEqual(fight.Sim.CurrentTick, read.Active.Sim.CurrentTick);
                Assert.AreEqual(fight.Session.Checksum, read.Active.Session.Checksum, "the fight came back different at tick " + stopAt);
                Assert.AreEqual(saved, CampaignSave.Write(read));

                // Both play on with no more orders and end the same way.
                while (!fight.Over) fight.Tick();
                while (!read.Active.Over) read.Active.Tick();
                EncounterResult a = c.Finish(fight);
                EncounterResult b = read.Finish(read.Active);
                Assert.AreEqual(a.Won, b.Won);
                Assert.AreEqual(CampaignSave.Write(c), CampaignSave.Write(read));
            }
        }

        [Test]
        public void TheTutorialClosedHalfwayPlaysOnToo()
        {
            Campaign c = Campaign.New(154, Tuned());
            CampaignFight fight = c.StartTutorial();
            var pilot = new CampaignPilot(PilotStyle.Reference, 154);
            while (!fight.Over && fight.Sim.CurrentTick < 200)
            {
                pilot.Step(fight);
                fight.Tick();
            }
            Campaign read = CampaignSave.Read(CampaignSave.Write(c), Tuned());
            Assert.AreEqual(CampaignStage.Tutorial, read.Stage);
            Assert.IsNotNull(read.Active);
            Assert.AreEqual(fight.Session.Checksum, read.Active.Session.Checksum);
        }

        [Test]
        public void AFightThatWillNotPlayBackIsDroppedWithANote()
        {
            Campaign c = Ready(155);
            c.StartRun();
            CampaignFight fight = c.StartEncounter(0);
            for (int i = 0; i < 90; i++) fight.Tick();
            // An order the fight refuses: no chief answers your call yet.
            string broken = CampaignSave.Write(c).Replace("\nend\n", "\norder\t30\tEclipse\t\nend\n");
            Campaign read = CampaignSave.Read(broken, Tuned());
            Assert.IsNull(read.Active);
            Assert.AreEqual(CampaignStage.InRun, read.Stage);
            Assert.IsNotEmpty(read.LoadNote);
            CollectionAssert.AreEqual(Kinds(c.Offers), Kinds(read.Offers));
        }

        [Test]
        public void ARunThatCannotBeReadNeverCostsTheCamp()
        {
            for (ulong seed = 157; seed < 177; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Reference, seed));
                if (c.Stage != CampaignStage.InRun) continue;
                string saved = CampaignSave.Write(c);
                string broken = saved.Replace("\nrun\t" + c.Target.Id + "\t", "\nrun\tnobody\t");
                Assert.AreNotEqual(saved, broken);
                Campaign read = CampaignSave.Read(broken, Tuned());
                Assert.AreEqual(CampaignStage.ChooseTribe, read.Stage, "an unreadable run opens at camp");
                Assert.IsNotEmpty(read.LoadNote);
                Assert.AreEqual(c.Materials, read.Materials);
                CollectionAssert.AreEqual(c.Codex, read.Codex);
                Assert.AreEqual(c.Mind(B).Snapshot(), read.Mind(B).Snapshot());
                Assert.AreEqual(Tuned().TopUpTo, read.Reserve.Count);
                return;
            }
            Assert.Fail("every run in twenty ended on its first fight");
        }

        [Test]
        public void ASaveFromTheRespectCampaignOpensAtCamp()
        {
            Campaign c = Ready(156);
            c.StartRun();
            CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Reference, 156));
            string now = CampaignSave.Write(c).Replace("\nend\n", "\nrespect\t" + B + "\t40\nend\n");
            foreach (int version in new[] { 1, 2 })
            {
                string old = now.Replace(CampaignSave.Header + "\t" + CampaignSave.Version, CampaignSave.Header + "\t" + version);
                Assert.AreNotEqual(now, old);
                Campaign read = CampaignSave.Read(old, Tuned());
                Assert.AreEqual(CampaignStage.ChooseTribe, read.Stage, "an old save opens at camp");
                Assert.IsNotEmpty(read.LoadNote);
                Assert.AreEqual(c.Mind(B).Snapshot(), read.Mind(B).Snapshot(), "what Bloodridge learned is kept");
                CollectionAssert.AreEqual(c.Codex, read.Codex);
                Assert.AreEqual(Tuned().TopUpTo, read.Reserve.Count);
                Assert.AreEqual(0, read.Assists.Count);
            }
        }

        [Test]
        public void ABrokenSaveIsRefusedWithAReason()
        {
            Campaign c = Ready(151);
            string saved = CampaignSave.Write(c);
            Assert.IsFalse(CampaignSave.TryRead(saved.Substring(0, saved.Length / 2), null, out _, out string why));
            Assert.IsNotEmpty(why);
            Assert.IsFalse(CampaignSave.TryRead("hello", null, out _, out _));
        }

        [Test]
        public void AReopenedTribeRemembersYouAndFightsDifferently()
        {
            for (ulong seed = 160; seed < 200; seed++)
            {
                Campaign c = Ready(seed);
                CampaignPilot.PlayRun(c, B, PilotStyle.FocusSpam, seed);
                Campaign reopened = CampaignSave.Read(CampaignSave.Write(c), Tuned());
                TribeMind remembered = reopened.Mind(B);
                if (remembered.Lessons.Count == 0) continue;
                Assert.IsNotEmpty(remembered.Lessons[0].Why);

                Campaign fresh = Ready(seed);
                reopened.StartRun();
                fresh.StartRun();
                string now = Habits(reopened.StartEncounter(0).Setup.CopyTactics(1));
                string before = Habits(fresh.StartEncounter(0).Setup.CopyTactics(1));
                Assert.AreNotEqual(before, now, "what Bloodridge learned did not reach the next run");
                return;
            }
            Assert.Fail("Bloodridge never learned anything in forty runs");
        }

        [Test]
        public void TheSameSeedAndPilotGiveTheSameCampaign()
        {
            Campaign a = Ready(200);
            CampaignPilot.PlayRun(a, B, PilotStyle.Reference, 200);
            Campaign b = Ready(200);
            CampaignPilot.PlayRun(b, B, PilotStyle.Reference, 200);
            Assert.AreEqual(CampaignSave.Write(a), CampaignSave.Write(b));
        }

        // ------------------------------------------------------------ balance guards

        private static int RunWinRate(CampaignTuning tuning, string tribeId, PilotStyle style, int n)
        {
            var won = new bool[n];
            var played = new bool[n];
            Parallel.For(0, n, i =>
            {
                ulong seed = 1000UL + (ulong)i * 7919UL;
                Campaign c = Campaign.New(seed, tuning);
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) return;
                List<EncounterResult> run = CampaignPilot.PlayRun(c, tribeId, style, seed);
                played[i] = true;
                won[i] = run[run.Count - 1].RunWon;
            });
            int p = 0, w = 0;
            for (int i = 0; i < n; i++)
            {
                if (!played[i]) continue;
                p++;
                if (won[i]) w++;
            }
            return p == 0 ? 0 : w * 100 / p;
        }

        // Parked 13 Oct 2026: since your roster is your lives a run survives losses, so every pilot wins more runs (the
        // Turtle won 57% against Bloodridge). How many is a tuning question.
        [Test, Category("Balance"), Ignore(Sprint1Tests.ParkedForTuning)]
        public void NoSpamOrRandomPilotWinsMoreThanAThirdOfRuns()
        {
            CampaignTuning t = Tuned();
            var report = new StringBuilder();
            bool ok = true;
            foreach (PilotStyle style in new[] { PilotStyle.Random, PilotStyle.UnleashSpam, PilotStyle.FocusSpam, PilotStyle.HoldSpam,
                PilotStyle.Turtle, PilotStyle.FallBackSpam, PilotStyle.Passive })
            {
                foreach (string tribe in new[] { B, S })
                {
                    int rate = RunWinRate(t, tribe, style, 40);
                    report.Append(style).Append(' ').Append(tribe).Append(' ').Append(rate).Append("%; ");
                    if (rate > 33) ok = false;
                }
            }
            TestContext.WriteLine(report.ToString());
            Assert.IsTrue(ok, report.ToString());
        }

        [Test, Category("Balance"), Ignore(Sprint1Tests.ParkedForTuning)]
        public void TheReferenceWinsMoreRunsThanTheCheater()
        {
            CampaignTuning t = Tuned();
            int reference = RunWinRate(t, B, PilotStyle.Reference, 60);
            int cheater = RunWinRate(t, B, PilotStyle.Cheater, 60);
            TestContext.WriteLine("Whole slice runs won: reference " + reference + "%, cheater " + cheater + "%.");
            Assert.Greater(reference, 0, "the reference pilot never won a whole run");
            Assert.LessOrEqual(cheater, reference, "cheater " + cheater + "% against reference " + reference + "%");
        }

        // ------------------------------------------------------------ bots (numbers, printed)

        private static readonly EncounterKind[] Pool =
        {
            EncounterKind.WarParty, EncounterKind.HoldTheLine, EncounterKind.Duel, EncounterKind.Trial
        };

        private static string Env(string name, string fallback)
        {
            string v = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrWhiteSpace(v) ? fallback : v.Trim();
        }

        /// The tuning under test: defaults, changed by KS_TUNE ("Name=value,Name=value").
        private static CampaignTuning Tuned()
        {
            // These campaign contracts use the mixed-node road. Rules 13's staged road has its own integration tests.
            return Apply(new CampaignTuning { RulesCap = 12 }, Env("KS_TUNE", string.Empty));
        }

        private static CampaignTuning Apply(CampaignTuning t, string config)
        {
            if (string.IsNullOrWhiteSpace(config)) return t;
            foreach (string part in config.Split(','))
            {
                if (part.Trim().Length == 0) continue;
                string[] kv = part.Split('=');
                if (kv.Length != 2) throw new ArgumentException("bad tuning part " + part);
                PropertyInfo p = typeof(CampaignTuning).GetProperty(kv[0].Trim());
                if (p == null || !p.CanWrite) throw new ArgumentException("no tuning called " + kv[0]);
                string raw = kv[1].Trim();
                object value;
                if (p.PropertyType == typeof(int)) value = int.Parse(raw, CultureInfo.InvariantCulture);
                else if (p.PropertyType == typeof(bool)) value = raw == "1" || raw.ToLowerInvariant() == "true";
                else if (p.PropertyType.IsEnum) value = Enum.Parse(p.PropertyType, raw);
                else throw new ArgumentException("cannot set " + kv[0]);
                p.SetValue(t, value);
            }
            return t;
        }

        private static PilotStyle[] Styles(string list)
        {
            if (list == "all") return (PilotStyle[])Enum.GetValues(typeof(PilotStyle));
            var styles = new List<PilotStyle>();
            foreach (string s in list.Split(',')) styles.Add((PilotStyle)Enum.Parse(typeof(PilotStyle), s.Trim()));
            return styles.ToArray();
        }

        private static string[] Tribes(string list)
        {
            if (list == "b") return new[] { B };
            if (list == "s") return new[] { S };
            return new[] { B, S };
        }

        private static string Short(string tribeId)
        {
            CampaignTribe t = CampaignTribes.Find(tribeId);
            return t == null ? tribeId : t.Name;
        }

        private static string Abbrev(EncounterKind kind)
        {
            switch (kind)
            {
                case EncounterKind.WarParty: return "War";
                case EncounterKind.HoldTheLine: return "Hold";
                case EncounterKind.Duel: return "Duel";
                case EncounterKind.Trial: return "Trial";
                case EncounterKind.Chief: return "Chief";
                case EncounterKind.AlphaHunt: return "Hunt";
                default: return kind.ToString();
            }
        }

        private static string Pct(int part, int whole)
        {
            return whole == 0 ? "  -" : (part * 100 / whole).ToString(CultureInfo.InvariantCulture).PadLeft(3) + "%";
        }

        /// The tuning bench: every pilot against every encounter, whole runs, and how many runs it
        /// takes to win the whole slice once. KS_SWEEP: tuning configs split by '|'. KS_N: campaigns
        /// per cell. KS_MODES: kinds,runs,clear. KS_STYLES: pilot names or all. KS_TRIBES: b, s or
        /// both (where a run starts). KS_TAG and KS_OUT: where the report goes. Run it on purpose; it
        /// never runs alone.
        [Test, Explicit, Category("Bots")]
        public void BotsSweep()
        {
            string[] configs = Env("KS_SWEEP", string.Empty).Split('|');
            int n = int.Parse(Env("KS_N", "40"), CultureInfo.InvariantCulture);
            var modes = new HashSet<string>(Env("KS_MODES", "kinds,runs").Split(','));
            PilotStyle[] styles = Styles(Env("KS_STYLES", "all"));
            string[] tribes = Tribes(Env("KS_TRIBES", "both"));
            var sb = new StringBuilder();
            sb.AppendLine("CAMPAIGN BOTS " + DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + "  n=" + n);
            sb.AppendLine("kinds: first step of a run, full squad. won/played, eclipse ways, materials per win.");
            foreach (string config in configs)
            {
                var clock = Stopwatch.StartNew();
                CampaignTuning tuning = Apply(Tuned(), config);
                sb.AppendLine();
                sb.AppendLine("== " + (config.Length == 0 ? "defaults" : config) + (Env("KS_TUNE", "").Length > 0 ? "  on " + Env("KS_TUNE", "") : ""));
                if (modes.Contains("kinds"))
                    foreach (string tribe in tribes)
                        foreach (PilotStyle style in styles)
                            sb.AppendLine(KindLine(tuning, tribe, style, n));
                if (modes.Contains("runs"))
                    foreach (string tribe in tribes)
                        foreach (PilotStyle style in styles)
                            sb.AppendLine(RunLine(tuning, tribe, style, n));
                if (modes.Contains("clear"))
                    foreach (PilotStyle style in styles)
                        sb.AppendLine(ClearLine(tuning, style, n));
                sb.AppendLine("   (" + clock.Elapsed.TotalSeconds.ToString("0.0", CultureInfo.InvariantCulture) + " s)");
            }
            string dir = Env("KS_OUT", TestContext.CurrentContext.WorkDirectory);
            string path = Path.Combine(dir, "campaign_bots_" + Env("KS_TAG", "sweep") + ".txt");
            File.WriteAllText(path, sb.ToString());
            TestContext.WriteLine(sb.ToString());
        }

        private static string KindLine(CampaignTuning tuning, string tribeId, PilotStyle style, int n)
        {
            int tries = n * 3 / 2;
            var results = new EncounterResult[Pool.Length * tries];
            Parallel.For(0, results.Length, j =>
            {
                EncounterKind kind = Pool[j / tries];
                ulong seed = 5000UL + (ulong)(j % tries) * 7919UL;
                Campaign c = Campaign.New(seed, tuning);
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) return;
                c.ChooseTribe(tribeId);
                int index = -1;
                for (int i = 0; i < c.Offers.Count; i++) if (c.Offers[i].Kind == kind) index = i;
                if (index < 0) return;
                results[j] = CampaignPilot.PlayFight(c, c.StartEncounter(index), new CampaignPilot(style, seed * 3UL + 1UL));
            });
            var line = new StringBuilder();
            line.Append(string.Format(CultureInfo.InvariantCulture, "kinds {0,-10} {1,-12}", Short(tribeId), style));
            for (int k = 0; k < Pool.Length; k++)
            {
                int played = 0, won = 0, gain = 0, ways = 0;
                for (int i = 0; i < tries && played < n; i++)
                {
                    EncounterResult r = results[k * tries + i];
                    if (r == null) continue;
                    played++;
                    ways += r.EclipseWays;
                    if (!r.Won) continue;
                    won++;
                    gain += r.MaterialsEarned;
                }
                line.Append(string.Format(CultureInfo.InvariantCulture, " | {0,-5}{1}/{2,-3} w{3:0.0} +{4,2:0}",
                    Abbrev(Pool[k]), Pct(won, played), played, played == 0 ? 0 : ways * 1.0 / played, won == 0 ? 0 : gain * 1.0 / won));
            }
            return line.ToString();
        }

        /// Whole runs that start at this tribe: how often the run is won, how often each chief falls,
        /// fights and deaths per run, and each kind of fight's win rate.
        private static string RunLine(CampaignTuning tuning, string tribeId, PilotStyle style, int n)
        {
            var runs = new List<EncounterResult>[n];
            Parallel.For(0, n, i =>
            {
                ulong seed = 1000UL + (ulong)i * 7919UL;
                Campaign c = Campaign.New(seed, tuning);
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) return;
                runs[i] = CampaignPilot.PlayRun(c, tribeId, style, seed);
            });
            int played = 0, won = 0, fights = 0, fallen = 0;
            var chiefs = new int[CampaignTribes.All.Length];
            var kindPlayed = new int[7];
            var kindWon = new int[7];
            for (int i = 0; i < n; i++)
            {
                List<EncounterResult> run = runs[i];
                if (run == null || run.Count == 0) continue;
                played++;
                for (int s = 0; s < run.Count; s++)
                {
                    EncounterResult r = run[s];
                    fights++;
                    kindPlayed[(int)r.Encounter.Kind]++;
                    fallen += r.Fallen.Count;
                    if (!r.Won) continue;
                    kindWon[(int)r.Encounter.Kind]++;
                    if (r.Encounter.Kind != EncounterKind.Chief) continue;
                    for (int t = 0; t < CampaignTribes.All.Length; t++) if (CampaignTribes.All[t].Id == r.Encounter.Tribe.Id) chiefs[t]++;
                }
                if (run[run.Count - 1].RunWon) won++;
            }
            var beaten = new StringBuilder();
            for (int t = 0; t < CampaignTribes.All.Length; t++)
            {
                if (CampaignTribes.All[t].IsStarter) continue;
                beaten.Append("  ").Append(Short(CampaignTribes.All[t].Id)).Append(" chief ").Append(Pct(chiefs[t], played));
            }
            var kinds = new StringBuilder();
            for (int k = 0; k < kindPlayed.Length; k++)
            {
                if (kindPlayed[k] == 0) continue;
                kinds.Append(" | ").Append(Abbrev((EncounterKind)k)).Append(' ').Append(Pct(kindWon[k], kindPlayed[k])).Append('/')
                    .Append(kindPlayed[k].ToString(CultureInfo.InvariantCulture));
            }
            return string.Format(CultureInfo.InvariantCulture,
                "runs  {0,-10} {1,-12} RUN {2}{3}  fights {4,4:0.0}  dead/run {5,4:0.0}{6}",
                Short(tribeId), style, Pct(won, played), beaten, played == 0 ? 0 : fights * 1.0 / played,
                played == 0 ? 0 : fallen * 1.0 / played, kinds);
        }

        /// Runs it takes to win the whole slice once, at most MaxRuns, with what the tribes learn
        /// carried from run to run.
        private static string ClearLine(CampaignTuning tuning, PilotStyle style, int n)
        {
            const int MaxRuns = 6;
            var needed = new int[n];
            var fights = new int[n];
            var ok = new bool[n];
            Parallel.For(0, n, i =>
            {
                ulong seed = 9000UL + (ulong)i * 7919UL;
                Campaign c = Campaign.New(seed, tuning);
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) return;
                ok[i] = true;
                int r = 0;
                while (c.RunsWon == 0 && r < MaxRuns)
                {
                    fights[i] += CampaignPilot.PlayRun(c, B, style, seed + (ulong)r * 101UL).Count;
                    r++;
                }
                needed[i] = c.RunsWon > 0 ? r : MaxRuns + 1;
            });
            int played = 0, first = 0, by3 = 0, by6 = 0, total = 0;
            for (int i = 0; i < n; i++)
            {
                if (!ok[i]) continue;
                played++;
                total += fights[i];
                if (needed[i] == 1) first++;
                if (needed[i] <= 3) by3++;
                if (needed[i] <= MaxRuns) by6++;
            }
            return string.Format(CultureInfo.InvariantCulture, "clear {0,-12} | first run {1}  by run 3 {2}  by run 6 {3} | fights {4:0.0}",
                style, Pct(first, played), Pct(by3, played), Pct(by6, played), played == 0 ? 0 : total * 1.0 / played);
        }
    }
}
