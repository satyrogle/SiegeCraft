using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Sim.Tests
{
    /// Sprint 3 of the design Jacob chose on 12 and 13 Oct 2026 (his go, 13 Oct: "go start sprint 3"; DESIGN_LOCKS,
    /// "Sprint 3 calls"): the road you see whole, events on it, bonus goals and omens. Jacob's rule 8 is build first,
    /// tune later, so each rule gets one quick test that proves it runs. None of these is balance.
    public sealed class Sprint3Tests
    {
        private static Campaign Ready(ulong seed, CampaignTuning tuning = null)
        {
            tuning = tuning ?? new CampaignTuning();
            tuning.RulesCap = Math.Min(tuning.RulesCap, 12); // Historical events/goals road, retained alongside rules 13.
            Campaign c = Campaign.New(seed, tuning);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }

        private static List<string> Describe(CampaignRoad road)
        {
            return road.All.Select(n => n.Row + "." + n.Lane + ":" + n.Kind + ":" + n.Fight + ":" + n.Objective + ":" +
                (n.Event == null ? "" : n.Event.Id) + ":" + string.Join(",", n.Next)).ToList();
        }

        [Test]
        public void TheRoadIsAMapYouSeeWholeBeforeTheFirstFight()
        {
            for (ulong seed = 4000; seed < 4010; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                CampaignRoad road = c.Road;
                Assert.IsNotNull(road);
                Assert.AreEqual(new CampaignTuning().PathFights, road.Rows);
                Assert.AreEqual(road.Rows * CampaignRoad.Lanes, road.All.Count, "every node is there from the start");
                for (int row = 0; row < road.Rows; row++)
                {
                    var fights = new List<EncounterKind>();
                    int events = 0, goals = 0;
                    for (int lane = 0; lane < CampaignRoad.Lanes; lane++)
                    {
                        RoadNode n = road.Node(row, lane);
                        if (n.Kind == RoadNodeKind.Event) events++;
                        else
                        {
                            fights.Add(n.Fight);
                            if (n.Objective != FightObjective.None) goals++;
                        }
                        if (row + 1 < road.Rows)
                        {
                            Assert.IsNotEmpty(n.Next, "every node leads on");
                            Assert.IsTrue(n.Next.Any(l => road.Node(row + 1, l).Kind == RoadNodeKind.Fight), "every node leads to a fight");
                        }
                        else Assert.IsEmpty(n.Next, "the last row leads to the chief");
                    }
                    Assert.AreEqual(row == 0 ? 0 : 1, events, "the first row is all fights, every later row has one event");
                    Assert.AreEqual(fights.Count, fights.Distinct().Count(), "a row's fights are all different kinds");
                    Assert.AreEqual(1, goals, "one fight in each row carries a bonus goal");
                }
                for (int row = 1; row < road.Rows; row++)
                    for (int lane = 0; lane < CampaignRoad.Lanes; lane++)
                        Assert.IsTrue(Enumerable.Range(0, CampaignRoad.Lanes).Any(l => road.Node(row - 1, l).Next.Contains(lane)),
                            "a way into every node");
                CollectionAssert.AreEqual(road.All.Take(CampaignRoad.Lanes).ToList(), c.NextNodes.ToList(), "the whole first row is open");
                Assert.AreEqual(CampaignRoad.Lanes, c.Offers.Count);
                Assert.IsTrue(c.Offers.All(o => o.Node != null && o.Node.Row == 0));

                Campaign again = Ready(seed);
                again.StartRun();
                CollectionAssert.AreEqual(Describe(road), Describe(again.Road), "the same campaign, run and tribe make the same road");
            }
        }

        [Test]
        public void WinningMovesYouAlongTheRoadAndLosingLeavesYouWhereYouStood()
        {
            var easy = new CampaignTuning { EnemyScaleTier1 = 40 };
            bool won = false;
            for (ulong seed = 4100; seed < 4120 && !won; seed++)
            {
                Campaign c = Ready(seed, easy);
                c.StartRun();
                Encounter picked = c.Offers[1];
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(1), new CampaignPilot(PilotStyle.Reference, seed));
                if (!r.Won) continue;
                won = true;
                Assert.AreEqual(1, c.Step);
                Assert.AreEqual(picked.Node.Lane, c.RoadLane, "you stand on the node you won");
                CollectionAssert.AreEqual(picked.Node.Next.ToList(), c.NextNodes.Select(n => n.Lane).ToList(), "the lanes it leads to");
                CollectionAssert.AreEqual(c.NextNodes.Where(n => n.Kind == RoadNodeKind.Fight).Select(n => n.Lane).ToList(),
                    c.Offers.Select(o => o.Node.Lane).ToList(), "the fights among them are on offer");
            }
            Assert.IsTrue(won, "the reference pilot never won an easy first fight");

            var hard = new CampaignTuning { EnemyScaleTier1 = 300 };
            for (ulong seed = 4120; seed < 4140; seed++)
            {
                Campaign c = CampaignSave.Read(CampaignSave.Write(Ready(seed)), hard);
                c.StartRun();
                List<string> before = c.NextNodes.Select(n => n.Row + "." + n.Lane).ToList();
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Passive, seed));
                if (r.Won || r.RunOver) continue;
                Assert.AreEqual(0, c.Step);
                Assert.AreEqual(-1, c.RoadLane);
                CollectionAssert.AreEqual(before, c.NextNodes.Select(n => n.Row + "." + n.Lane).ToList(), "the same forks again");
                return;
            }
            Assert.Fail("giving no orders against a tribe three times as strong never lost a first fight");
        }

        [Test]
        public void AnEventIsANodeYouTakeWithAChoice()
        {
            var easy = new CampaignTuning { EnemyScaleTier1 = 40 };
            var seen = new HashSet<string>();
            for (ulong seed = 4200; seed < 4280 && seen.Count < RoadEvents.All.Length; seed++)
            {
                Campaign c = Ready(seed, easy);
                c.StartRun();
                int offer = -1;
                RoadNode target = null;
                for (int i = 0; i < c.Offers.Count && offer < 0; i++)
                    foreach (int lane in c.Offers[i].Node.Next)
                        if (c.Road.Node(1, lane).Kind == RoadNodeKind.Event && !seen.Contains(c.Road.Node(1, lane).Event.Id))
                        {
                            offer = i;
                            target = c.Road.Node(1, lane);
                        }
                if (offer < 0) continue;
                if (!CampaignPilot.PlayFight(c, c.StartEncounter(offer), new CampaignPilot(PilotStyle.Reference, seed)).Won) continue;
                RoadEvent ev = target.Event;
                seen.Add(ev.Id);
                CollectionAssert.Contains(c.NextNodes, target);
                CollectionAssert.DoesNotContain(c.Offers.Select(o => o.Node).ToList(), target, "an event is no fight on offer");

                int materials = c.Materials;
                for (int choice = 0; choice < ev.Choices.Count; choice++)
                    if (ev.Choices[choice].Cost > materials)
                    {
                        Assert.IsFalse(c.TryTakeEvent(target.Lane, choice, out _, out string refused), "a choice you cannot pay for");
                        StringAssert.Contains("materials", refused);
                    }
                int pick = Enumerable.Range(0, ev.Choices.Count).First(i => ev.Choices[i].Cost <= materials);
                RoadChoice taken = ev.Choices[pick];
                int fighters = c.Reserve.Count, gear = c.Gear.Count;
                Assert.IsTrue(c.TryTakeEvent(target.Lane, pick, out EventOutcome outcome, out string why), why);
                Assert.AreEqual(2, c.Step, "the run moves on");
                Assert.AreEqual(target.Lane, c.RoadLane, "you stand on the event's node");
                Assert.IsNull(c.Spoils, "the spoils offer goes once the run moves on");
                Assert.AreEqual(materials + outcome.Materials, c.Materials);
                Assert.IsNotEmpty(outcome.Text);
                switch (taken.Effect)
                {
                    case EventEffect.TakeInStraggler:
                        Assert.AreEqual(fighters + 1, c.Reserve.Count);
                        CollectionAssert.Contains(c.Reserve, outcome.Fighter);
                        Assert.AreEqual(CampaignTribes.ClarthId, outcome.Fighter.TribeId);
                        break;
                    case EventEffect.BuyPauper:
                    case EventEffect.BuyMid:
                        Assert.AreEqual(gear + 1, c.Gear.Count);
                        Assert.AreEqual(taken.Effect == EventEffect.BuyMid ? GearTier.Mid : GearTier.Pauper, outcome.Gear.Tier);
                        break;
                    case EventEffect.Pray:
                        if (outcome.Gear != null) CollectionAssert.Contains(c.Gear, outcome.Gear);
                        else Assert.LessOrEqual(outcome.Materials, 0);
                        break;
                    default:
                        Assert.AreEqual(fighters, c.Reserve.Count);
                        Assert.AreEqual(gear, c.Gear.Count);
                        break;
                }
                Assert.IsFalse(c.TryTakeEvent(target.Lane, pick, out _, out _), "an event is taken once");
            }
            Assert.AreEqual(RoadEvents.All.Length, seen.Count, "every event was reached and taken");
        }

        [Test]
        public void AMetBonusGoalRaisesTheDropATier()
        {
            int met = 0, missed = 0;
            for (ulong seed = 4300; seed < 4400 && (met < 2 || missed < 2); seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                int i = c.Offers.ToList().FindIndex(o => o.Objective != FightObjective.None);
                Assert.GreaterOrEqual(i, 0, "the first row's goal is on offer");
                Encounter e = c.Offers[i];
                Assert.IsNotEmpty(FightObjectives.Text(e.Objective));
                CampaignFight fight = c.StartEncounter(i);
                EncounterResult r = CampaignPilot.PlayFight(c, fight, new CampaignPilot(PilotStyle.Reference, seed));
                if (!r.Won) continue;
                Assert.AreEqual(e.Objective, r.Objective);
                FightEnding end = fight.Sim.Ending;
                bool expected = e.Objective == FightObjective.NobodyFalls ? r.Fallen.Count == 0
                    : e.Objective == FightObjective.Quick ? fight.Sim.CurrentTick <= FightObjectives.QuickTicks
                    : end.How == FightEnd.NerveBreak && end.Side == 1 && end.Standing >= FightObjectives.BreakStanding;
                Assert.AreEqual(expected, r.ObjectiveMet, e.Objective + " judged from the fight itself");
                GearTier plain = c.DropOf(e.Node);
                Assert.AreEqual(CampaignGearRules.DropTier(e.Kind), plain, "no omens: the fight's own drop");
                Assert.AreEqual(r.ObjectiveMet ? (GearTier)Math.Min((int)GearTier.Princess, (int)plain + 1) : plain, r.SpoilsOffered.Tier);
                if (r.ObjectiveMet) met++;
                else missed++;
            }
            Assert.GreaterOrEqual(met, 2, "too few goals met to check");
            Assert.GreaterOrEqual(missed, 2, "too few goals missed to check");
        }

        [Test]
        public void BloodMoonKeepsTheCaveTribesBleedingBlades()
        {
            // Until 21 Oct 2026 the omen's harder blows lost Stoneveil's bleeding blades; campaign rules 3 keep them.
            Campaign dark = Ready(4402), light = Ready(4402);
            Assert.IsTrue(dark.TrySetOmen(Omen.BloodMoon, true, out string why), why);
            dark.ChooseTribe(CampaignTribes.StoneveilId);
            light.ChooseTribe(CampaignTribes.StoneveilId);
            IReadOnlyList<UnitSpec> them = dark.StartEncounter(0).Setup.Side(1), plain = light.StartEncounter(0).Setup.Side(1);
            Assert.AreEqual(plain.Count, them.Count);
            Assert.IsTrue(plain.Any(u => u.BleedPercent > 0), "their blades bleed");
            for (int i = 0; i < plain.Count; i++)
            {
                Assert.AreEqual(plain[i].BleedPercent, them[i].BleedPercent, them[i].Id + " keeps its bleed");
                Assert.Greater(them[i].PowerPercent, plain[i].PowerPercent, them[i].Id + " hits harder");
            }
        }

        [Test]
        public void OmensArePickedAtCampAndMakeTheRunHarderForBetterDrops()
        {
            Campaign c = Ready(4400);
            Assert.IsTrue(c.TrySetOmen(Omen.BloodMoon, true, out string why), why);
            Assert.IsTrue(c.TrySetOmen(Omen.LongRoad, true, out why), why);
            Assert.IsTrue(c.TrySetOmen(Omen.ThinRanks, true, out why), why);
            Assert.IsTrue(c.TrySetOmen(Omen.ThinRanks, false, out why), why);
            CollectionAssert.AreEqual(new[] { Omen.BloodMoon, Omen.LongRoad }, c.Omens);
            int squad = c.Reserve.Count;
            c.StartRun();
            Assert.IsFalse(c.TrySetOmen(Omen.HungryHunters, true, out why), "omens are picked at camp, not in a run");
            Assert.AreEqual(squad, c.Reserve.Count, "Thin Ranks was dropped again");
            Assert.AreEqual(new CampaignTuning().PathFights + 1, c.Road.Rows, "Long Road: one more row");
            Assert.AreEqual(c.Road.Rows, c.PathLength);
            foreach (Encounter e in c.Offers)
                Assert.AreEqual((GearTier)Math.Min((int)GearTier.Princess, (int)CampaignGearRules.DropTier(e.Kind) + 1), c.DropOf(e.Node),
                    "two omens raise every road drop a tier");

            // Blood Moon: their fighters hit harder than in the same fight without it.
            Campaign plain = Ready(4400);
            plain.StartRun();
            CampaignFight dark = c.StartEncounter(0), light = plain.StartEncounter(0);
            Assert.AreEqual(light.Encounter.Kind, dark.Encounter.Kind);
            // Fighters wearing a piece (20 Oct 2026) are left out: two omens also raise the piece's tier, so its bonus differs.
            int compared = 0;
            for (int i = 0; i < light.Setup.Side(1).Count; i++)
            {
                if (light.Setup.Side(1)[i].Bend != ClassBend.None || dark.Setup.Side(1)[i].Bend != ClassBend.None) continue;
                Assert.AreEqual(light.Setup.Side(1)[i].PowerPercent * (100 + OmenRules.BloodMoonPercent) / 100, dark.Setup.Side(1)[i].PowerPercent);
                compared++;
            }
            Assert.Greater(compared, 0, "someone of theirs wears no piece");

            // Thin Ranks: the run starts one fighter short.
            Campaign thin = Ready(4401);
            int full = thin.Reserve.Count;
            Assert.IsTrue(thin.TrySetOmen(Omen.ThinRanks, true, out why), why);
            thin.StartRun();
            Assert.AreEqual(full - 1, thin.Reserve.Count);

            // Two omens: a won fight's materials are worth half again.
            var easy = new CampaignTuning { EnemyScaleTier1 = 40 };
            for (ulong seed = 4402; seed < 4420; seed++)
            {
                Campaign rich = Ready(seed, easy);
                Assert.IsTrue(rich.TrySetOmen(Omen.HungryHunters, true, out why), why);
                Assert.IsTrue(rich.TrySetOmen(Omen.LongRoad, true, out why), why);
                rich.StartRun();
                EncounterResult r = CampaignPilot.PlayFight(rich, rich.StartEncounter(0), new CampaignPilot(PilotStyle.Reference, seed));
                if (!r.Won) continue;
                Assert.AreEqual(easy.MaterialsPerFight * (100 + 2 * OmenRules.MaterialsPercentEach) / 100, r.MaterialsEarned);
                return;
            }
            Assert.Fail("the reference pilot never won an easy first fight");
        }

        [Test]
        public void HungryHuntersBringMore()
        {
            var t = new CampaignTuning { PathFights = 0, ChiefHealthPercent = 1, EnemyScaleTier1 = 40, EnemyScaleTier2 = 40 };
            for (ulong seed = 4450; seed < 4480; seed++)
            {
                Campaign c = Ready(seed, t);
                Assert.IsTrue(c.TrySetOmen(Omen.HungryHunters, true, out string why), why);
                c.StartRun();
                for (int n = 0; n < 4 && c.Stage == CampaignStage.InRun && c.Target != AlphaHunt.Tribe; n++)
                    CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Reference, seed + (ulong)n));
                if (c.Stage != CampaignStage.InRun || c.Target != AlphaHunt.Tribe) continue;
                int joined = c.Joined.Count(id => id != CampaignTribes.ClarthId);
                Assert.AreEqual(AlphaHunt.Fighters + AlphaHunt.PerTribeJoined * joined + OmenRules.HungryHuntersExtra, c.Offers[0].EnemyFighters);
                Assert.IsNull(c.Road, "the hunters have no road");
                return;
            }
            Assert.Fail("no run reached the hunters in thirty campaigns");
        }

        [Test]
        public void TheSaveKeepsWhereYouStandOnTheRoadAndTheOmens()
        {
            var easy = new CampaignTuning { EnemyScaleTier1 = 40 };
            for (ulong seed = 4500; seed < 4520; seed++)
            {
                Campaign c = Ready(seed, easy);
                Assert.IsTrue(c.TrySetOmen(Omen.HungryHunters, true, out string why), why);
                c.StartRun();
                if (!CampaignPilot.PlayFight(c, c.StartEncounter(c.Offers.Count - 1), new CampaignPilot(PilotStyle.Reference, seed)).Won) continue;
                string saved = CampaignSave.Write(c);
                Campaign read = CampaignSave.Read(saved, easy);
                Assert.IsEmpty(read.LoadNote);
                Assert.AreEqual(saved, CampaignSave.Write(read), "every omen and the place on the road come back");
                Assert.AreEqual(c.Step, read.Step);
                Assert.AreEqual(c.RoadLane, read.RoadLane);
                CollectionAssert.AreEqual(c.Omens, read.Omens);
                CollectionAssert.AreEqual(Describe(c.Road), Describe(read.Road));
                CollectionAssert.AreEqual(c.Offers.Select(o => o.Kind + "@" + o.Node.Lane).ToList(), read.Offers.Select(o => o.Kind + "@" + o.Node.Lane).ToList());

                // A fight saved part way along the road plays back the same.
                CampaignFight fight = c.StartEncounter(0);
                for (int tick = 0; tick < 60 && !fight.Over; tick++) fight.Tick();
                string mid = CampaignSave.Write(c);
                Campaign back = CampaignSave.Read(mid, easy);
                Assert.IsEmpty(back.LoadNote, "the fight on the road plays back the same");
                Assert.AreEqual(mid, CampaignSave.Write(back));
                return;
            }
            Assert.Fail("the reference pilot never won an easy first fight");
        }
    }
}
