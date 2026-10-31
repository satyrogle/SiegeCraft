using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The rule fixes in Jacob's "fix all 9" (13 Oct 2026), after Claude played four fights and reviewed them. The
    /// screen fixes are in the Slice and its validation.
    public class FixNineTests
    {
        private static Campaign Ready(ulong seed)
        {
            Campaign c = Campaign.New(seed);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }

        private static bool Blocked(BattleSimulation sim)
        {
            return sim.Events.Any(e => e.Kind == BattleEventKind.Directive && e.Detail == BattleSimulation.BlockedDetail);
        }

        /// In the playtest the coach asked for an Unleash and Bloodridge's war taught Hold blocked it. The tutorial is
        /// practice now: their doctrine comes, their habits do not, so the Unleash scares them. On the road their Hold
        /// still blocks a plain Unleash (RunRulesTests).
        [Test]
        public void TheTutorialUnleashScaresThem()
        {
            Campaign c = Campaign.New(900);
            Assert.IsTrue(c.Mind(CampaignTribes.BloodridgeId).HasHabit(Habit.HoldOnUnleash), "Bloodridge start out holding on Unleash");
            CampaignFight fight = c.StartTutorial();
            BattleSimulation sim = fight.Sim;
            for (int i = 0; i < 30; i++) fight.Tick();
            int before = sim.Nerve(1);
            Assert.IsTrue(fight.TryOrder(Directive.Unleash, null, out string why), why);
            Assert.Less(sim.Nerve(1), before, "the Unleash scares them");
            Assert.IsFalse(Blocked(sim), "nothing blocks it");
            Assert.AreEqual(0, sim.HoldTicksLeft(1), "and they do not Hold in answer");
        }

        /// The tutorial's orders are the coach's: nobody learns from them.
        [Test]
        public void NobodyLearnsFromTheTutorial()
        {
            for (ulong seed = 901; seed < 906; seed++)
            {
                Campaign c = Campaign.New(seed);
                int bloodridge = c.Mind(CampaignTribes.BloodridgeId).Lessons.Count, cult = c.CultMind.Lessons.Count;
                for (int i = 0; i < 10 && c.Stage == CampaignStage.Tutorial; i++)
                {
                    EncounterResult result = CampaignPilot.PlayFight(c, c.StartTutorial(), new CampaignPilot(PilotStyle.Reference, seed + (ulong)i));
                    Assert.IsEmpty(result.EnemyLessons, "Bloodridge learned nothing from the tutorial");
                    Assert.IsEmpty(result.CultLessons, "and neither did your fighters");
                }
                Assert.AreEqual(bloodridge, c.Mind(CampaignTribes.BloodridgeId).Lessons.Count);
                Assert.AreEqual(cult, c.CultMind.Lessons.Count);
            }
        }

        /// In the playtest a pick could leave one way on. Every node now leads to its own lane and one beside it.
        [Test]
        public void EveryRoadNodeForks()
        {
            for (ulong seed = 910; seed < 940; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                CampaignRoad road = c.Road;
                foreach (RoadNode n in road.All)
                {
                    if (n.Row + 1 >= road.Rows)
                    {
                        Assert.IsEmpty(n.Next, "the last row leads to the chief");
                        continue;
                    }
                    Assert.AreEqual(2, n.Next.Count, "row " + n.Row + " lane " + n.Lane + " forks, seed " + seed);
                    Assert.Contains(n.Lane, n.Next.ToList(), "one way is its own lane");
                    Assert.IsTrue(n.Next.Any(l => road.Node(n.Row + 1, l).Kind == RoadNodeKind.Fight), "and one leads to a fight");
                }
            }
        }
    }
}
