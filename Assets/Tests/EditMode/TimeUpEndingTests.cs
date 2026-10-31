using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Fights that run out of time (C5, 11 Oct 2026). A sample of real fights never timed out, so these force
    /// it with short fights: the answers the screen reads (Ending and FightWon) must match Finish. Running out
    /// of time loses, except Hold the line, which is won by still standing.
    public class TimeUpEndingTests
    {
        [Test]
        public void ATutorialThatRunsOutOfTimeIsALossOnBothCounts()
        {
            Campaign c = Campaign.New(5UL, new CampaignTuning { FightTicks = 30 });
            CampaignFight fight = c.StartTutorial();
            while (!fight.Over) fight.Tick();
            Assert.AreEqual(FightEnd.TimeUp, fight.Sim.Ending.How);
            Assert.AreEqual(-1, fight.Sim.Ending.Side);
            bool predicted = c.FightWon(fight);
            EncounterResult result = c.Finish(fight);
            Assert.IsTrue(result.TimedOut);
            Assert.IsFalse(result.Won);
            Assert.IsFalse(predicted);
        }

        [Test]
        public void HoldTheLineThatRunsOutOfTimeIsAWinOnBothCounts()
        {
            for (ulong seed = 300; seed < 340; seed++)
            {
                Campaign c = Campaign.New(seed, new CampaignTuning { HoldTicks = 40 });
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) continue;
                c.StartRun();
                int index = -1;
                for (int i = 0; i < c.Offers.Count && index < 0; i++) if (c.Offers[i].Kind == EncounterKind.HoldTheLine) index = i;
                if (index < 0) continue;
                CampaignFight fight = c.StartEncounter(index);
                while (!fight.Over) fight.Tick();
                Assert.AreEqual(FightEnd.TimeUp, fight.Sim.Ending.How);
                bool predicted = c.FightWon(fight);
                EncounterResult result = c.Finish(fight);
                Assert.IsTrue(result.TimedOut);
                Assert.IsTrue(result.Won);
                Assert.IsTrue(predicted);
                return;
            }
            Assert.Fail("no hold the line was offered in forty campaigns");
        }
    }
}
