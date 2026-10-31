using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The two new tribes (Jacob, 18 Oct 2026: "go, build both tribes as planned and put them on the road now with
    /// placeholder figures"; DESIGN_LOCKS, "The two new tribes"). One quick test each, only to prove they run.
    public class NewTribesTests
    {
        private static Campaign Ready(ulong seed, CampaignTuning tuning)
        {
            Campaign c = Campaign.New(seed, tuning);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }

        [Test]
        public void TheCaveTribeBleedsYouAndItsBloodCallRipsOnlyTheBleeding()
        {
            // On the road in tier 1, with bleeding blades and the blood call.
            Assert.AreEqual(1, CampaignTribes.Stoneveil.Tier);
            Assert.AreEqual(ChiefSpell.BloodCall, CampaignTribes.Stoneveil.Spell);
            Assert.AreEqual(Directive.FallBack, ChiefSpells.Answer(ChiefSpell.BloodCall));
            Campaign c = Ready(6100, new CampaignTuning());
            c.ChooseTribe(CampaignTribes.StoneveilId);
            CampaignFight road = c.StartEncounter(0);
            foreach (UnitSpec u in road.Setup.Side(1)) Assert.AreEqual(TribeTraits.BloodlettersBleedPercent, u.BleedPercent, u.Id);

            // In a fight: their blows leave wounds that bleed, and the call hits only those still bleeding.
            var mine = new List<UnitSpec>
            {
                new UnitSpec("m.warden", UnitClass.Warden), new UnitSpec("m.skirm", UnitClass.Skirmisher), new UnitSpec("m.marks", UnitClass.Marksman)
            };
            var them = new List<UnitSpec>
            {
                new UnitSpec("t.skirm", UnitClass.Skirmisher, Row.Front, UnitRole.Fighter, 100, 100, 0, 0, 0, 0, 0, 0, 0, 0, 2),
                new UnitSpec("t.skirm2", UnitClass.Skirmisher, Row.Front, UnitRole.Fighter, 100, 100, 0, 0, 0, 0, 0, 0, 0, 0, 2),
                new UnitSpec("t.priest", UnitClass.Medic, Row.Back, UnitRole.Chief, 100, 200, 0, 0, 0, 0, 0, 0, 0, 0, 2)
            };
            var sim = new BattleSimulation(new BattleSetup("cave", 61, mine, them, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.On(), SliceRules.RefillTicks, 400,
                new SideRules(0, 0, true), new SideRules(2, 150, false, spell: ChiefSpell.BloodCall), FightRules.Skill));
            int bleeds = 0, calls = 0, seen = 0;
            while (!sim.IsFinished && sim.CurrentTick < 1800)
            {
                var bleeding = new HashSet<string>();
                foreach (BattleUnit u in sim.Units) if (u.Side == 0 && u.Alive && u.BleedTicks > 0) bleeding.Add(u.Id);
                sim.Tick();
                for (; seen < sim.Events.Count; seen++)
                {
                    BattleEvent e = sim.Events[seen];
                    if (e.Kind != BattleEventKind.Hit) continue;
                    if (e.Detail == BleedRules.Detail) bleeds++;
                    if (e.Detail != ChiefSpells.BloodDetail) continue;
                    calls++;
                    CollectionAssert.Contains(bleeding, e.TargetId, "the blood call hits only fighters who were bleeding");
                }
            }
            Assert.Greater(bleeds, 0, "their blows left wounds that bled");
            Assert.Greater(calls, 0, "the blood call went off on someone bleeding");
        }

        [Test]
        public void TheSeaTribeFloodsTheFightAndTakesNoPrisoners()
        {
            // On the road in tier 2, flooding every fight, with the tide.
            Assert.AreEqual(2, CampaignTribes.Thundercoast.Tier);
            Assert.AreEqual(ChiefSpell.Tide, CampaignTribes.Thundercoast.Spell);
            Assert.IsTrue(CampaignTribes.Thundercoast.Floods);
            Assert.IsTrue(CampaignTribes.Thundercoast.NoPrisoners);

            // The water rises a level every FloodRules.StepTicks up to FloodRules.MaxLevel, and only their side floods.
            Campaign look = Ready(6200, new CampaignTuning());
            look.ChooseTribe(CampaignTribes.ThundercoastId);
            CampaignFight wet = look.StartEncounter(0);
            Assert.IsTrue(wet.Sim.Floods(1));
            Assert.IsFalse(wet.Sim.Floods(0));
            Assert.AreEqual(0, wet.Sim.FloodLevel);
            while (!wet.Over && wet.Sim.CurrentTick < FloodRules.StepTicks) wet.Tick();
            if (!wet.Over) Assert.AreEqual(1, wet.Sim.FloodLevel, "the water rose a level");

            // Beaten by them, your most hurt fighter dies instead of being captured, and nobody is held against them.
            // The sea tribe's no prisoners under the 13 Oct roster as lives, which rules 11 replaced and old saves still play.
            var hard = new CampaignTuning { EnemyScaleTier2 = 300, RulesCap = Campaign.RaidChiefRules };
            for (ulong seed = 6300; seed < 6340; seed++)
            {
                Campaign c = Ready(seed, hard);
                c.ChooseTribe(CampaignTribes.ThundercoastId);
                int before = c.Reserve.Count;
                EncounterResult lost = CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Passive, seed));
                if (lost.Won || lost.Slain == null) continue;
                Assert.IsNull(lost.Captured, "they take no prisoners");
                CollectionAssert.DoesNotContain(c.Reserve, lost.Slain);
                Assert.IsEmpty(c.Grudges, "nobody was captured, so no grudge");
                Assert.Less(c.Reserve.Count, before);
                return;
            }
            Assert.Fail("no fight against the sea tribe was lost with someone still standing in forty campaigns");
        }

        [Test]
        public void ASaveFromBeforeTheNewTribesTeachesThemTheirWars()
        {
            // A save written before the new tribes holds no mind for them; read back, they arrive with the habits their
            // wars taught, as in a new campaign.
            Campaign fresh = Ready(6400, new CampaignTuning());
            int taught = fresh.Mind(CampaignTribes.StoneveilId).HabitValue(Habit.HoldOnUnleash);
            Assert.Greater(taught, 0, "a new campaign teaches them their wars");
            var kept = new List<string>();
            bool skipping = false;
            foreach (string line in CampaignSave.Write(fresh).Split('\n'))
            {
                string[] f = line.Split('\t');
                if (f[0] == "mind") skipping = f.Length > 1 && (f[1] == CampaignTribes.StoneveilId || f[1] == CampaignTribes.ThundercoastId);
                else if (f[0] != "habit" && f[0] != "threat" && f[0] != "learned" && f[0] != "lesson") skipping = false;
                if (!skipping) kept.Add(line);
            }
            Campaign read = CampaignSave.Read(string.Join("\n", kept));
            Assert.AreEqual(taught, read.Mind(CampaignTribes.StoneveilId).HabitValue(Habit.HoldOnUnleash));
            Assert.AreEqual(fresh.Mind(CampaignTribes.ThundercoastId).HabitValue(Habit.CoverFocused),
                read.Mind(CampaignTribes.ThundercoastId).HabitValue(Habit.CoverFocused));
        }
    }
}
