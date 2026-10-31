using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Duels (Jacob, 30 Oct 2026: "Yes, design it", then "Yes, build it" and "go"; DESIGN_LOCKS "Duels"; FightRules.ChampionDuel,
    /// campaign rules 16). One quick test per piece, only to prove it runs, each beside the same fight before rules 16.
    /// Nothing here is tuning.
    public class DuelTests
    {
        private static UnitSpec U(string id, UnitClass unitClass, int health = 1000)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, 100, health);
        }

        // A road fight, one of ours against their champion, who swings a heavy every 2.5 s: a champion duel, or the same
        // road fight as before rules 16.
        private static BattleSimulation Duel(string id, UnitSpec ours, bool duel, int health = 1000)
        {
            var rules = new FightRules(true, true, true, true, true, roadCombat: true);
            return new BattleSimulation(new BattleSetup(id, 4, new List<UnitSpec> { ours }, new List<UnitSpec> { U("x", UnitClass.Warden, health) },
                new TacticsProfile(), new TacticsProfile(), BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints,
                NerveRules.On(NerveRules.Full, NerveRules.Full), 10, 400, null, new SideRules(0, 0, false, HeavyRules.EveryTicks),
                duel ? rules.WithChampionDuel() : rules));
        }

        private static bool Order(SimSession session, string directive)
        {
            return session.TryIssue(Directives.CommandKind, "0", string.Empty, 0, 0, directive, out _);
        }

        // Ticks on, holding on the tick before every beat coming at ours, until this happens (its index) or the tick limit.
        private static int ParryUntil(SimSession session, BattleSimulation sim, Func<BattleEvent, bool> match, int ticks = BattleSetup.DefaultTickLimit)
        {
            int seen = 0;
            while (!sim.IsFinished && sim.CurrentTick < ticks)
            {
                if (sim.HoldTicksLeft(0) <= 0 && sim.Threats(0).Any(t => t.BeatTicks == 1)) Order(session, "Hold");
                session.Tick();
                for (; seen < sim.Events.Count; seen++) if (match(sim.Events[seen])) return seen;
            }
            return -1;
        }

        [Test]
        public void InADuelAParriedHeavyCracksTheChampionsNerve()
        {
            // Our Medic never attacks, so only a parry can crack his nerve.
            foreach (bool duel in new[] { true, false })
            {
                BattleSimulation sim = Duel("duel.crack", U("h", UnitClass.Medic), duel);
                int parry = ParryUntil(new SimSession(sim), sim, e => e.Kind == BattleEventKind.Parry, 3000);
                Assert.GreaterOrEqual(parry, 0, "his heavy was parried");
                int nerve = sim.FindById("x").UnitNerve;
                if (duel) Assert.Less(nerve, RoadRules.UnitNerve, "a big beat in a duel: his nerve cracks");
                else Assert.AreEqual(RoadRules.UnitNerve, nerve, "before rules 16 a small beat: no crack");
            }
        }

        [Test]
        public void AChainOfParriesMakesTheChampionYield()
        {
            BattleSimulation sim = Duel("duel.yield", U("h", UnitClass.Medic), true);
            ParryUntil(new SimSession(sim), sim, e => false);
            Assert.IsTrue(sim.IsFinished, "the duel ended");
            Assert.AreEqual(0, sim.Winner, "and was won");
            Assert.IsTrue(sim.FindById("x").Fled, "he yielded and left the field");
            Assert.IsTrue(sim.FindById("h").Alive);

            // Before rules 16 nothing she does can make him go.
            BattleSimulation plain = Duel("duel.yield", U("h", UnitClass.Medic), false);
            ParryUntil(new SimSession(plain), plain, e => false);
            Assert.IsFalse(plain.FindById("x").Fled);
        }

        [Test]
        public void AHoldTooEarlyCracksYourDuelistsNerve()
        {
            foreach (bool duel in new[] { true, false })
            {
                BattleSimulation sim = Duel("duel.early", U("h", UnitClass.Medic), duel);
                var session = new SimSession(sim);
                while (!sim.IsFinished && !sim.Threats(0).Any(t => t.Name == HeavyRules.Heavy && t.BeatTicks == FightRules.PerfectHoldTicks + 5))
                    session.Tick();
                Assert.IsTrue(Order(session, "Hold"), "a Hold five ticks too early");
                int before = sim.FindById("h").UnitNerve;
                int seen = sim.Events.Count, block = -1;
                for (int i = 0; i < 40 && block < 0 && !sim.IsFinished; i++)
                {
                    session.Tick();
                    for (; seen < sim.Events.Count && block < 0; seen++)
                        if (sim.Events[seen].Kind == BattleEventKind.Reaction && sim.Events[seen].Detail == ParryRules.BlockDetail) block = seen;
                }
                if (duel)
                {
                    Assert.GreaterOrEqual(block, 0, "the beat met a stale Hold");
                    Assert.Less(sim.FindById("h").UnitNerve, before, "and cracked her nerve, the 24 Oct rule");
                }
                else Assert.Less(block, 0, "before rules 16 a stale Hold only soaks");
            }
        }

        [Test]
        public void TheChampionSpringsTheTrapOnceADuel()
        {
            foreach (bool duel in new[] { true, false })
            {
                BattleSimulation sim = Duel("duel.trap", U("t", UnitClass.Trapper), duel, 3000);
                while (!sim.IsFinished && sim.CurrentTick < 2400) sim.Tick();
                int sprung = sim.Events.Count(e => e.Kind == BattleEventKind.TrapSprung && e.TargetId == "x");
                if (duel)
                {
                    Assert.AreEqual(1, sprung, "once a duel, then he steps around it");
                    Assert.Greater(sim.Events.Count(e => e.Kind == BattleEventKind.TrapArmed && e.ActorSide == 0), 1, "her trap armed again");
                    Assert.IsTrue(sim.FindById("x").SprangTrap);
                }
                else Assert.Greater(sprung, 1, "before rules 16 he springs it every time");
            }
        }

        // The first duel the reference pilot meets on a run, started; null if none comes in the first ten fights of ten seeds.
        private static CampaignFight FirstDuel(CampaignTuning tuning)
        {
            for (ulong seed = 7000; seed < 7010; seed++)
            {
                Campaign c = Campaign.New(seed, tuning);
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) continue;
                c.StartRun();
                var picker = new CampaignPilot(PilotStyle.Reference, seed);
                for (int n = 0; n < 10 && c.Stage == CampaignStage.InRun; n++)
                {
                    int duel = c.Offers.ToList().FindIndex(o => o.Kind == EncounterKind.Duel);
                    if (duel >= 0) return c.StartEncounter(duel);
                    CampaignPilot.PlayFight(c, c.StartEncounter(picker.PickOffer(c)), new CampaignPilot(PilotStyle.Reference, seed * 31UL + (ulong)n));
                }
            }
            return null;
        }

        [Test]
        public void FromRulesSixteenEveryDuelIsAChampionDuel()
        {
            foreach (int cap in new[] { Campaign.DuelRules, Campaign.DuelRules - 1 })
            {
                CampaignFight fight = FirstDuel(new CampaignTuning { EnemyScaleTier1 = 40, RulesCap = cap });
                Assert.IsNotNull(fight, "no duel came up");
                bool on = cap >= Campaign.DuelRules;
                Assert.AreEqual(on, fight.Setup.Fight.ChampionDuel);
                Assert.AreEqual(on ? HeavyRules.EveryTicks : ParryRules.SquadSwingTicks, fight.Setup.Rules(1).HeavyEveryTicks,
                    "his heavy every 2.5 s from rules 16, every 5 s before");
            }
        }

        [Test]
        public void APilotSetToParryParriesTheChampionsBeat()
        {
            foreach (int permille in new[] { 1000, 0 })
            {
                CampaignFight fight = FirstDuel(new CampaignTuning { EnemyScaleTier1 = 40 });
                Assert.IsNotNull(fight, "no duel came up");
                var pilot = new CampaignPilot(PilotStyle.Passive, 1) { DuelParryPermille = permille };
                while (!fight.Over)
                {
                    pilot.Step(fight);
                    fight.Tick();
                }
                int parries = fight.Sim.Events.Count(e => e.Kind == BattleEventKind.Parry);
                if (permille > 0) Assert.Greater(parries, 0, "it parried his beat");
                else Assert.AreEqual(0, parries, "set to 0, a passive pilot never holds");
            }
        }
    }
}
