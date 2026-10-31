using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// C8 (11 Oct 2026): quitting and reopening at any point changes nothing. One scripted campaign is played
    /// straight through, then again once for every point in it, saved and reopened at that point: before a
    /// fight, twice in the middle of every fight, after it (so after a chief, and at camp after a run), and
    /// into the next run. Every replay must end in exactly the same save, so no reward, loss or lesson is
    /// applied twice or lost on the way. Camp purchases and tribe wars join the points once they exist (C2, C7).
    public class ReopenAnywhereTests
    {
        private static readonly long[] MidFightTicks = { 90, 600 };

        [Test]
        public void ReopeningAtAnyPointChangesNothing()
        {
            string expected = null;
            Script straight = null;
            ulong seed = 0;
            // Searched further since every road node forks (13 Oct 2026, "fix all 9"): the reading bot takes other roads.
            for (ulong s = 11; s < 400 && expected == null; s++)
            {
                var run = new Script(s, -1);
                string end = run.Play();
                if (run.ChiefsWon == 0 || run.RunsEnded == 0) continue;
                straight = run;
                expected = end;
                seed = s;
            }
            Assert.IsNotNull(expected, "no seed both won a chief and ended a run");

            int midFight = 0;
            for (int point = 0; point < straight.Points; point++)
            {
                var replay = new Script(seed, point);
                Assert.AreEqual(expected, replay.Play(), "reopened at point " + point + " of " + straight.Points);
                midFight += replay.MidFightReopens;
            }
            TestContext.WriteLine("seed " + seed + ": " + straight.Points + " points, " + midFight + " of them mid fight, " +
                straight.ChiefsWon + " chiefs won, " + straight.RunsEnded + " runs ended");
            Assert.Greater(midFight, 4, "too few reopens landed mid fight");
        }

        /// The same campaign every time: the tutorial, a run played until it ends, then two fights of the next
        /// run, all with the reading bot. reopenAt picks the one point to save and reopen at; -1 is none.
        private sealed class Script
        {
            private readonly ulong _seed;
            private readonly int _reopenAt;

            public Script(ulong seed, int reopenAt)
            {
                _seed = seed;
                _reopenAt = reopenAt;
            }

            public int Points { get; private set; }
            public int ChiefsWon { get; private set; }
            public int RunsEnded { get; private set; }
            public int MidFightReopens { get; private set; }

            public string Play()
            {
                Campaign c = Campaign.New(_seed);
                for (int t = 0; t < 5 && c.Stage == CampaignStage.Tutorial; t++)
                {
                    c = Point(c);
                    Fight(ref c, c.StartTutorial(), new CampaignPilot(PilotStyle.Reference, _seed + (ulong)t));
                    c = Point(c);
                }
                Assert.AreEqual(CampaignStage.ChooseTribe, c.Stage, "the tutorial was never won");
                for (int run = 0; run < 2 && c.Stage == CampaignStage.ChooseTribe; run++)
                {
                    c.StartRun();
                    c = Point(c);
                    int fights = run == 0 ? 8 : 2;
                    for (int n = 0; n < fights && c.Stage == CampaignStage.InRun; n++)
                    {
                        int pick = new CampaignPilot(PilotStyle.Reference, _seed).PickOffer(c);
                        bool chief = c.Offers[pick].Kind == EncounterKind.Chief;
                        c = Point(c);
                        EncounterResult r = Fight(ref c, c.StartEncounter(pick),
                            new CampaignPilot(PilotStyle.Reference, _seed * 31UL + (ulong)(run * 10 + n)));
                        if (chief && r.Won) ChiefsWon++;
                        if (r.RunOver) RunsEnded++;
                        c = Point(c);
                    }
                }
                return CampaignSave.Write(c);
            }

            /// Counts a point; at the chosen one, saves and carries on from the reopened copy.
            private Campaign Point(Campaign c)
            {
                if (Points++ != _reopenAt) return c;
                Assert.IsTrue(CampaignSave.TryRead(CampaignSave.Write(c), null, out Campaign copy, out string failure), failure);
                return copy;
            }

            private EncounterResult Fight(ref Campaign c, CampaignFight fight, CampaignPilot pilot)
            {
                int next = 0;
                while (!fight.Over)
                {
                    pilot.Step(fight);
                    fight.Tick();
                    if (next < MidFightTicks.Length && !fight.Over && fight.Sim.CurrentTick >= MidFightTicks[next])
                    {
                        next++;
                        bool reopening = Points == _reopenAt;
                        c = Point(c);
                        if (!reopening) continue;
                        MidFightReopens++;
                        fight = c.Active;
                        Assert.IsNotNull(fight, "reopened mid fight but no fight came back");
                    }
                }
                return c.Finish(fight);
            }
        }
    }
}
