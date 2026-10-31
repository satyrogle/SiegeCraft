using System.Collections.Generic;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The questions the screen asks the fight (C5, 11 Oct 2026): who may strike whom and why not, who covers
    /// whom, how a fight ended, and whether the campaign counts it as won. They come from the rules the fight
    /// itself uses, so the frontend never keeps its own copy, and asking them changes nothing.
    public class ExplanationQueryTests
    {
        private static BattleSimulation Fight(params UnitSpec[] theirs)
        {
            var ours = new[]
            {
                new UnitSpec("a.warden", UnitClass.Warden),
                new UnitSpec("a.skirmisher", UnitClass.Skirmisher),
                new UnitSpec("a.marksman", UnitClass.Marksman)
            };
            return new BattleSimulation(new BattleSetup("queries", 7UL, ours, theirs, null, null));
        }

        private static Campaign Ready(ulong seed)
        {
            Campaign c = Campaign.New(seed);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }

        [Test]
        public void WhileTheirFrontStandsOnlyRangedAndSkirmishersReachTheBackRow()
        {
            BattleSimulation sim = Fight(new UnitSpec("b.warden", UnitClass.Warden), new UnitSpec("b.medic", UnitClass.Medic));
            BattleUnit warden = sim.FindById("a.warden");
            BattleUnit medic = sim.FindById("b.medic");
            Assert.AreEqual(StrikeBlock.FrontStands, sim.CanStrike(warden, medic));
            Assert.AreEqual(StrikeBlock.None, sim.CanStrike(sim.FindById("a.skirmisher"), medic));
            Assert.AreEqual(StrikeBlock.None, sim.CanStrike(sim.FindById("a.marksman"), medic));
            Assert.AreEqual(StrikeBlock.None, sim.CanStrike(warden, sim.FindById("b.warden")));
            Assert.AreEqual(StrikeBlock.SameSide, sim.CanStrike(warden, sim.FindById("a.marksman")));
            Assert.AreEqual(2, sim.StrikersOn(medic, out int living));
            Assert.AreEqual(3, living);
        }

        [Test]
        public void WithNoFrontInTheWayEveryoneReachesTheBackRow()
        {
            BattleSimulation sim = Fight(new UnitSpec("b.medic", UnitClass.Medic), new UnitSpec("b.marksman", UnitClass.Marksman));
            Assert.AreEqual(StrikeBlock.None, sim.CanStrike(sim.FindById("a.warden"), sim.FindById("b.medic")));
            Assert.AreEqual(3, sim.StrikersOn(sim.FindById("b.medic"), out _));
        }

        [Test]
        public void FallBackStopsMeleeOnly()
        {
            for (ulong seed = 300; seed < 340; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                int index = -1;
                for (int i = 0; i < c.Offers.Count && index < 0; i++) if (!c.Offers[i].NoFallBack) index = i;
                if (index < 0) continue;
                CampaignFight fight = c.StartEncounter(index);
                for (int i = 0; i < 40 && !fight.Over; i++) fight.Tick();
                if (fight.Over || !fight.TryOrder(Directive.FallBack, null, out _)) continue;
                fight.Tick();
                BattleSimulation sim = fight.Sim;
                if (fight.Over || sim.FallBackTicksLeft(0) <= 0) continue;
                BattleUnit enemy = null;
                foreach (BattleUnit u in sim.Units) if (enemy == null && u.Alive && u.Side == 1) enemy = u;
                bool sawMelee = false;
                foreach (BattleUnit u in sim.Units)
                {
                    if (!u.Alive || u.Side != 0) continue;
                    StrikeBlock block = sim.CanStrike(u, enemy);
                    if (u.Stats.Ranged) Assert.AreNotEqual(StrikeBlock.FallBack, block, u.Id);
                    else
                    {
                        Assert.AreEqual(StrikeBlock.FallBack, block, u.Id);
                        sawMelee = true;
                    }
                }
                if (sawMelee) return;
            }
            Assert.Fail("no fight in forty campaigns had a melee fighter falling back");
        }

        /// Every plain blow in real campaign fights: if the answers said a pair could not strike before the
        /// tick, the block must have lifted during it (their front fell, or Fall Back ran out).
        [Test]
        public void NoBlowLandsWhereTheAnswersSayItCannot()
        {
            int hits = 0;
            for (ulong seed = 40; seed < 46; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                CampaignFight fight = c.StartEncounter(0);
                var pilot = new CampaignPilot(PilotStyle.Reference, seed);
                BattleSimulation sim = fight.Sim;
                var blocked = new List<string>();
                while (!fight.Over)
                {
                    blocked.Clear();
                    foreach (BattleUnit a in sim.Units)
                        foreach (BattleUnit b in sim.Units)
                        {
                            StrikeBlock why = sim.CanStrike(a, b);
                            if (why == StrikeBlock.FrontStands || why == StrikeBlock.FallBack) blocked.Add(a.Id + ">" + b.Id);
                        }
                    int from = sim.Events.Count;
                    pilot.Step(fight);
                    fight.Tick();
                    for (int i = from; i < sim.Events.Count; i++)
                    {
                        BattleEvent e = sim.Events[i];
                        if (e.Kind != BattleEventKind.Hit || e.Detail.Length > 0) continue;
                        hits++;
                        string pair = e.ActorId + ">" + e.TargetId;
                        if (!blocked.Contains(pair)) continue;
                        StrikeBlock after = sim.CanStrike(sim.FindById(e.ActorId), sim.FindById(e.TargetId));
                        Assert.IsTrue(after != StrikeBlock.FrontStands && after != StrikeBlock.FallBack, pair + " on tick " + e.Tick);
                    }
                }
                c.Finish(fight);
            }
            Assert.Greater(hits, 100, "too few blows checked");
        }

        [Test]
        public void EndingAndFightWonAgreeWithTheCampaignsOwnResult()
        {
            var seen = new int[5];
            int fights = 0;
            PilotStyle[] styles = { PilotStyle.Reference, PilotStyle.Passive, PilotStyle.UnleashSpam };
            for (ulong seed = 1; seed <= 12; seed++)
            {
                PilotStyle style = styles[(int)(seed % 3)];
                Campaign c = Campaign.New(seed);
                for (int t = 0; t < 5 && c.Stage == CampaignStage.Tutorial; t++)
                    fights += Check(c, c.StartTutorial(), new CampaignPilot(PilotStyle.Reference, seed + (ulong)t), seen);
                if (c.Stage == CampaignStage.Tutorial) continue;
                c.StartRun();
                var picker = new CampaignPilot(PilotStyle.Reference, seed);
                for (int n = 0; n < 8 && c.Stage == CampaignStage.InRun; n++)
                    fights += Check(c, c.StartEncounter(picker.PickOffer(c)), new CampaignPilot(style, seed * 31UL + (ulong)n), seen);
            }
            TestContext.WriteLine("fights " + fights + ": breaks " + seen[(int)FightEnd.NerveBreak] + ", leaders fell " +
                seen[(int)FightEnd.LeaderFell] + ", wipes " + seen[(int)FightEnd.Wipe] + ", time ups " + seen[(int)FightEnd.TimeUp]);
            Assert.Greater(fights, 30, "too few fights checked");
        }

        private static int Check(Campaign c, CampaignFight fight, CampaignPilot pilot, int[] seen)
        {
            while (!fight.Over)
            {
                pilot.Step(fight);
                fight.Tick();
            }
            BattleSimulation sim = fight.Sim;
            bool predicted = c.FightWon(fight);
            FightEnding end = sim.Ending;
            EncounterKind kind = fight.Encounter.Kind;
            EncounterResult result = c.Finish(fight);

            Assert.AreNotEqual(FightEnd.None, end.How, kind + " has an ending");
            Assert.AreEqual(result.Won, predicted, kind + ": FightWon matches Finish");
            Assert.AreEqual(result.TimedOut, end.How == FightEnd.TimeUp, kind + ": time up matches Finish");
            Assert.AreEqual(sim.EndTick, end.Tick);
            BattleEvent last = sim.Events[sim.Events.Count - 1];
            Assert.AreEqual(BattleEventKind.End, last.Kind);
            switch (end.How)
            {
                case FightEnd.NerveBreak:
                case FightEnd.LeaderFell:
                    Assert.AreEqual(sim.BrokenSide, end.Side);
                    Assert.AreEqual(end.How == FightEnd.LeaderFell, last.Detail == Moves.LeaderDetail);
                    Assert.AreEqual(sim.AliveCount(end.Side), end.Standing);
                    break;
                case FightEnd.Wipe:
                    Assert.AreEqual("wipe", last.Detail);
                    if (end.Side >= 0) Assert.AreEqual(0, sim.AliveCount(end.Side));
                    break;
                case FightEnd.TimeUp:
                    Assert.AreEqual("time", last.Detail);
                    break;
            }
            seen[(int)end.How]++;
            return 1;
        }

        /// Two identical fights, one asked every question every tick: same events, same state.
        [Test]
        public void AskingTheQuestionsChangesNothing()
        {
            UnitSpec[] Theirs() => new[]
            {
                new UnitSpec("b.warden", UnitClass.Warden), new UnitSpec("b.skirmisher", UnitClass.Skirmisher),
                new UnitSpec("b.trapper", UnitClass.Trapper), new UnitSpec("b.medic", UnitClass.Medic)
            };
            BattleSimulation quiet = Fight(Theirs());
            BattleSimulation asked = Fight(Theirs());
            for (int t = 0; t < 4000 && !quiet.IsFinished; t++)
            {
                quiet.Tick();
                Ask(asked);
                asked.Tick();
                Ask(asked);
            }
            Assert.IsTrue(quiet.IsFinished, "the fight never ended");
            Assert.IsTrue(asked.IsFinished);
            Assert.AreEqual(quiet.EventHash, asked.EventHash);
            Assert.AreEqual(StateChecksum.Compute(quiet.OrderedStateSnapshot), StateChecksum.Compute(asked.OrderedStateSnapshot));
        }

        private static void Ask(BattleSimulation sim)
        {
            foreach (BattleUnit a in sim.Units)
            {
                sim.CoveredBy(a);
                sim.StrikersOn(a, out _);
                foreach (BattleUnit b in sim.Units) sim.CanStrike(a, b);
            }
            Assert.IsNotNull(sim.Ending.How.ToString());
        }
    }
}
