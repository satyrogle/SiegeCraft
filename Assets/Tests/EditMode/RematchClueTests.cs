using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// C7 (Jacob, 11 Oct 2026: the tribe war is cut, so C7 is the card clue only). Before a fight the card
    /// can say what that tribe learned from one of your fights and point at the fight that taught it.
    public class RematchClueTests
    {
        private const string B = CampaignTribes.BloodridgeId;
        private const string S = CampaignTribes.SkullpeakId;

        private static Campaign Ready(ulong seed)
        {
            Campaign c = Campaign.New(seed);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }

        [Test]
        public void ATribeThatLearnedNothingFromYouHasNoClue()
        {
            Campaign c = Campaign.New(30);
            Assert.IsNull(RematchClue.For(c, B), "habits from their wars are not lessons from you");
            Assert.IsNull(RematchClue.For(c, S));
            Assert.IsNull(RematchClue.For(c, AlphaHunt.Id), "the hunters never learn");
            Assert.IsNull(RematchClue.For(c, CampaignTribes.CultId), "your own cult is not a rematch");
        }

        [Test]
        public void WhatTheyLearnedFromYouShowsBeforeTheNextFightAndPlaysBack()
        {
            for (ulong seed = 160; seed < 220; seed++)
            {
                Campaign c = Ready(seed);
                CampaignPilot.PlayRun(c, B, PilotStyle.FocusSpam, seed);
                RematchClue clue = RematchClue.For(c, B);
                if (clue == null) continue;
                Assert.IsTrue(c.Mind(B).HasHabit(clue.Habit), "the clue claims a habit they do not use");
                Assert.Greater(clue.Lesson.ValueAfter, clue.Lesson.ValueBefore, "a clue is something they picked up");
                Assert.AreEqual(clue.Lesson.Why, clue.Text);
                Assert.AreEqual(CampaignTribes.Bloodridge.ChiefName, clue.ChiefName);
                Assert.IsNotNull(clue.Source, "the fight that taught it is in the archive");
                Assert.AreEqual(clue.FightId, clue.Source.FightId);
                Assert.IsTrue(c.Archive.TryVerifyLesson(clue.Lesson, out string failure), failure);

                // Closing the game keeps it, and the fight still plays back.
                Campaign read = CampaignSave.Read(CampaignSave.Write(c));
                RematchClue again = RematchClue.For(read, B);
                Assert.IsNotNull(again);
                Assert.AreEqual(clue.FightId, again.FightId);
                Assert.AreEqual(clue.Habit, again.Habit);
                Assert.AreEqual(clue.Text, again.Text);
                Assert.IsTrue(read.Archive.TryVerifyLesson(again.Lesson, out failure), failure);
                return;
            }
            Assert.Fail("Bloodridge never picked anything up from focus spam in sixty campaigns");
        }

        [Test]
        public void TheClueIsTheNewestThingTheyLearnedThatTheyStillDo()
        {
            int clues = 0, none = 0;
            for (ulong seed = 220; seed < 260; seed++)
            {
                Campaign c = Ready(seed);
                for (int run = 0; run < 3; run++)
                {
                    CampaignPilot.PlayRun(c, B, run % 2 == 0 ? PilotStyle.Random : PilotStyle.UnleashSpam, seed * 7UL + (ulong)run);
                    foreach (string tribe in new[] { B, S })
                    {
                        TribeMind mind = c.Mind(tribe);
                        Lesson expected = null;
                        for (int i = mind.Lessons.Count - 1; i >= 0 && expected == null; i--)
                        {
                            Lesson l = mind.Lessons[i];
                            if (RematchClue.Raises(l.Rule, out Habit habit) && l.ValueAfter > l.ValueBefore && mind.HasHabit(habit)) expected = l;
                        }
                        RematchClue clue = RematchClue.For(c, tribe);
                        if (expected == null)
                        {
                            Assert.IsNull(clue, tribe + ": a clue with nothing behind it");
                            none++;
                            continue;
                        }
                        Assert.IsNotNull(clue, tribe);
                        Assert.AreSame(expected, clue.Lesson, tribe);
                        Assert.IsTrue(mind.HasHabit(clue.Habit), tribe);
                        clues++;
                    }
                }
            }
            TestContext.WriteLine(clues + " clues checked, " + none + " times a tribe had none.");
            Assert.Greater(clues, 10, "too few clues to check");
        }
    }
}
