using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The Whisperer (Jacob, 31 Oct 2026: "Go, build her"; DESIGN_LOCKS, "Habits come with the game, the Whisperer upgrades
    /// them"; FightRules.Whisper, campaign rules 24). One quick test per piece, only to prove each runs (Jacob, 12 Oct 2026:
    /// build first). Nothing here is tuning.
    public class WhispererTests
    {
        private static readonly FightRules Whisper = FightRules.Campaign.WithLayers().WithMarks().WithFloor().WithWhisper();
        private static readonly FightRules RoadWhisper = FightRules.Campaign.WithRoadCombat().WithLayers().WithFloor().WithWhisper();

        private static List<UnitSpec> Side(params UnitSpec[] units) => new List<UnitSpec>(units);

        private static UnitSpec U(string id, UnitClass unitClass, int health = 3000, int power = 100)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, power, health);
        }

        // Medics in their front row never fight back.
        private static List<UnitSpec> Medics(int count, int health = 30000)
        {
            var medics = new List<UnitSpec>();
            for (int i = 1; i <= count; i++) medics.Add(new UnitSpec("x" + i, UnitClass.Medic, Row.Front, UnitRole.Fighter, 100, health));
            return medics;
        }

        // Your side's kit with these habits, each upgraded or not, and this pair.
        private static PalKit Kit(SquadPair pair, params KeyValuePair<WhispererHabit, bool>[] habits)
        {
            var states = new int[WhisperRules.HabitCount + 1];
            for (int i = 0; i < habits.Length; i++) states[(int)habits[i].Key] = habits[i].Value ? 2 : 1;
            return PalKit.None.WithWhisper(states, pair);
        }

        private static KeyValuePair<WhispererHabit, bool> H(WhispererHabit habit, bool upgraded = false)
        {
            return new KeyValuePair<WhispererHabit, bool>(habit, upgraded);
        }

        private static BattleSimulation Fight(string id, List<UnitSpec> ours, List<UnitSpec> theirs, FightRules rules, PalKit kit = null,
            SideRules theirRules = null)
        {
            return new BattleSimulation(new BattleSetup(id, 4, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.Off, 0, 400,
                new SideRules(1, 0, true).WithPals(kit ?? PalKit.None), theirRules, rules));
        }

        // The index of the first event from this index on that matches, ticking on as needed up to this tick; -1 if none.
        private static int RunUntil(BattleSimulation sim, int ticks, Func<BattleEvent, bool> match, int from = 0)
        {
            int seen = from;
            while (true)
            {
                for (; seen < sim.Events.Count; seen++) if (match(sim.Events[seen])) return seen;
                if (sim.IsFinished || sim.CurrentTick >= ticks) return -1;
                sim.Tick();
            }
        }

        // Ticks on until this holds, up to this tick. False if it never did.
        private static bool TickUntil(BattleSimulation sim, int ticks, Func<bool> holds)
        {
            while (!holds())
            {
                if (sim.IsFinished || sim.CurrentTick >= ticks) return false;
                sim.Tick();
            }
            return true;
        }

        private static void RunOut(BattleSimulation sim, int ticks)
        {
            while (!sim.IsFinished && sim.CurrentTick < ticks) sim.Tick();
        }

        // The most hits one fighter's spell landed on a single tick.
        private static int MostSpellHitsOnATick(BattleSimulation sim, string id, string detail)
        {
            return sim.Events.Where(e => e.Kind == BattleEventKind.Hit && e.ActorId == id && e.Detail.StartsWith(detail, StringComparison.Ordinal))
                .GroupBy(e => e.Tick).Select(g => g.Count()).DefaultIfEmpty(0).Max();
        }

        // ------------------------------------------------------------ rank 2

        [Test]
        public void AtRank2TheSkirmisherStrikesTwiceAndTheMarksmanShootsTwicePerAim()
        {
            var ours = Side(U("s", UnitClass.Skirmisher).WithRank(2), U("m", UnitClass.Marksman).WithRank(2),
                U("s1", UnitClass.Skirmisher), U("m1", UnitClass.Marksman));
            BattleSimulation sim = Fight("whisper.rank.blows", ours, Medics(3), Whisper);
            RunOut(sim, 1800);
            Assert.AreEqual(2, MostSpellHitsOnATick(sim, "s", ClassSpells.StrikeDetail), "her strike, twice");
            Assert.AreEqual(1, MostSpellHitsOnATick(sim, "s1", ClassSpells.StrikeDetail), "a rank 1 strike, once");
            Assert.AreEqual(2, MostSpellHitsOnATick(sim, "m", ClassSpells.AimedDetail), "two shots per aim");
            Assert.AreEqual(1, MostSpellHitsOnATick(sim, "m1", ClassSpells.AimedDetail), "a rank 1 aim, one shot");
        }

        [Test]
        public void AtRank2TheWardenGuardsTwoTheTrapSpringsTwiceAndTheMedicKeepsATreatmentInHand()
        {
            var ours = Side(U("w", UnitClass.Warden, 100).WithRank(2), U("t", UnitClass.Trapper, 100).WithRank(2),
                U("med", UnitClass.Medic, 100).WithRank(2), U("m", UnitClass.Marksman, 100));
            var theirs = Side(U("x1", UnitClass.Skirmisher, 6000, 40), U("x2", UnitClass.Warden, 6000, 40), U("x3", UnitClass.Skirmisher, 6000, 40));
            BattleSimulation sim = Fight("whisper.rank.wards", ours, theirs, Whisper);

            // Nobody is hurt yet: she keeps her treatment in hand.
            Assert.GreaterOrEqual(RunUntil(sim, 900, e => e.Kind == BattleEventKind.Whisper && e.ActorId == "med" &&
                e.Detail == WhisperRules.InHandDetail), 0, "a treatment kept in hand");

            // He covers two at once.
            BattleUnit w = sim.FindById("w");
            Assert.IsTrue(TickUntil(sim, 900, () => w.GuardTargetId.Length > 0 && w.GuardTargetId2.Length > 0), "two wards");
            Assert.AreNotEqual(w.GuardTargetId, w.GuardTargetId2);

            // Her trap springs twice before she arms it again.
            int armed = RunUntil(sim, 3000, e => e.Kind == BattleEventKind.TrapArmed && e.ActorId == "t");
            Assert.GreaterOrEqual(armed, 0);
            int first = RunUntil(sim, 3000, e => e.Kind == BattleEventKind.TrapSprung && e.ActorSide == 0, armed + 1);
            Assert.GreaterOrEqual(first, 0, "it springs");
            int next = RunUntil(sim, 3000, e => (e.Kind == BattleEventKind.TrapSprung && e.ActorSide == 0) ||
                (e.Kind == BattleEventKind.TrapArmed && e.ActorId == "t"), first + 1);
            Assert.GreaterOrEqual(next, 0);
            Assert.AreEqual(BattleEventKind.TrapSprung, sim.Events[next].Kind, "a second spring before she rearms");

            // And once someone is hurt she treats twice in a row.
            RunOut(sim, 3000);
            int most = sim.Events.Where(e => e.Kind == BattleEventKind.Heal && e.ActorId == "med" && e.Detail == ClassSpells.TreatDetail)
                .GroupBy(e => e.Tick).Select(g => g.Count()).DefaultIfEmpty(0).Max();
            Assert.GreaterOrEqual(most, 2, "two treatments on one turn");
        }

        // ------------------------------------------------------------ habits

        [Test]
        public void BodyguardCoversTheAimingMarksmanAndTriageWaitsForAQuarter()
        {
            // Bodyguard: while the Marksman aims his Guard is on him, where a plain Warden stays on the Medic.
            var ours = Side(U("w", UnitClass.Warden), U("m", UnitClass.Marksman), U("med", UnitClass.Medic));
            BattleSimulation guarded = Fight("whisper.bodyguard", ours, Medics(2), Whisper, Kit(SquadPair.None, H(WhispererHabit.Bodyguard)));
            BattleUnit w = guarded.FindById("w"), m = guarded.FindById("m");
            Assert.IsTrue(TickUntil(guarded, 1200, () => m.AimTicks > 0 && w.GuardTargetId == "m"), "the Guard on the aiming Marksman");
            BattleSimulation plain = Fight("whisper.bodyguard", Side(U("w", UnitClass.Warden), U("m", UnitClass.Marksman), U("med", UnitClass.Medic)),
                Medics(2), Whisper);
            BattleUnit pw = plain.FindById("w"), pm = plain.FindById("m");
            Assert.IsTrue(TickUntil(plain, 1200, () => pm.AimTicks > 0 && pw.GuardTargetId.Length > 0));
            Assert.AreEqual("med", pw.GuardTargetId, "a plain Warden keeps the Medic");

            // Triage: she only treats someone under a quarter of his health, where a plain Medic treats lighter wounds.
            Func<PalKit, List<int>> treatedAt = kit =>
            {
                BattleSimulation sim = Fight("whisper.triage", Side(U("w", UnitClass.Warden, 100), U("s", UnitClass.Skirmisher, 100),
                    U("med", UnitClass.Medic, 100)), Side(U("x1", UnitClass.Skirmisher, 20000, 40), U("x2", UnitClass.Skirmisher, 20000, 40)), Whisper, kit);
                var before = new List<int>();
                int seen = 0;
                var permille = new Dictionary<string, int>();
                while (!sim.IsFinished && sim.CurrentTick < 3000)
                {
                    foreach (BattleUnit u in sim.Units) permille[u.Id] = u.HpPermille;
                    sim.Tick();
                    for (; seen < sim.Events.Count; seen++)
                    {
                        BattleEvent e = sim.Events[seen];
                        if (e.Kind == BattleEventKind.Heal && e.ActorId == "med" && e.Detail == ClassSpells.TreatDetail) before.Add(permille[e.TargetId]);
                    }
                }
                return before;
            };
            List<int> plainTreats = treatedAt(PalKit.None), triageTreats = treatedAt(Kit(SquadPair.None, H(WhispererHabit.Triage)));
            Assert.IsTrue(plainTreats.Any(p => p >= WhisperRules.TriageBelowPermille), "a plain Medic treats lighter wounds");
            Assert.IsNotEmpty(triageTreats, "Triage treats in the end");
            Assert.IsTrue(triageTreats.All(p => p < WhisperRules.TriageBelowPermille), "only someone under a quarter");
        }

        [Test]
        public void TheUpgradesKeepTheOpeningReachTheFrontRowAndDoubleTheSavedTreatment()
        {
            // An upgraded Spotter's Opening stays the whole fight; a plain one for its usual clock.
            Func<PalKit, int> opening = kit =>
            {
                BattleSimulation sim = Fight("whisper.spotter", Side(U("m", UnitClass.Marksman)), Medics(1), Whisper, kit);
                int at = RunUntil(sim, 1800, e => e.Kind == BattleEventKind.Mark && e.ActorId == "m" && e.Detail == ClassSpells.OpeningMark);
                Assert.GreaterOrEqual(at, 0, "he shot");
                return sim.Events[at].Amount;
            };
            Assert.AreEqual(ClassSpells.OpeningTicks, opening(Kit(SquadPair.None, H(WhispererHabit.Spotter))));
            Assert.Greater(opening(Kit(SquadPair.None, H(WhispererHabit.Spotter, true))), 10 * ClassSpells.OpeningTicks, "until cashed");

            // An upgraded Front First's treatment reaches the whole front row on one turn.
            // Their Wardens only reach your front row, two Skirmishers with no Guard over them.
            var front = Side(U("s1", UnitClass.Skirmisher, 100), U("s2", UnitClass.Skirmisher, 100), U("med", UnitClass.Medic, 100));
            BattleSimulation ff = Fight("whisper.frontfirst", front, Side(U("x1", UnitClass.Warden, 6000, 60), U("x2", UnitClass.Warden, 6000, 60),
                U("x3", UnitClass.Warden, 6000, 60)), Whisper, Kit(SquadPair.None, H(WhispererHabit.FrontFirst, true)));
            RunOut(ff, 2400);
            Assert.IsTrue(ff.Events.Where(e => e.Kind == BattleEventKind.Heal && e.ActorId == "med").GroupBy(e => e.Tick)
                .Any(g => g.Select(e => e.TargetId).Distinct().Count() >= 2), "two of the front row treated on one turn");

            // An upgraded Triage's treatment lands twice as big as a plain Triage's, on the same first patient.
            Func<PalKit, BattleEvent> firstTreat = kit =>
            {
                // Enough health that even under a quarter a doubled treatment fits in what he is missing.
                BattleSimulation sim = Fight("whisper.triage.up", Side(U("s", UnitClass.Skirmisher, 1000), U("w", UnitClass.Warden, 1000),
                    U("med", UnitClass.Medic, 30000)),
                    Side(U("x1", UnitClass.Skirmisher, 9000, 300), U("x2", UnitClass.Skirmisher, 9000, 300)), Whisper, kit);
                int at = RunUntil(sim, 3000, e => e.Kind == BattleEventKind.Heal && e.ActorId == "med" && e.Detail == ClassSpells.TreatDetail);
                Assert.GreaterOrEqual(at, 0, "someone fell under a quarter and she treated him");
                return sim.Events[at];
            };
            BattleEvent plainTreat = firstTreat(Kit(SquadPair.None, H(WhispererHabit.Triage)));
            BattleEvent upTreat = firstTreat(Kit(SquadPair.None, H(WhispererHabit.Triage, true)));
            Assert.AreEqual(plainTreat.Tick, upTreat.Tick);
            Assert.AreEqual(plainTreat.TargetId, upTreat.TargetId);
            Assert.AreEqual(2 * plainTreat.Amount, upTreat.Amount, "twice as big");
        }

        [Test]
        public void AnAmbushersTrapWaitsForAHeavySwingAndUpgradedStopsIt()
        {
            Func<PalKit, BattleSimulation> fight = kit =>
            {
                var ours = Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher), U("t", UnitClass.Trapper));
                var theirs = Side(U("x1", UnitClass.Skirmisher, 9000), U("x2", UnitClass.Warden, 9000));
                return Fight("whisper.ambush", ours, theirs, Whisper, kit, new SideRules(0, 0, false, 4 * SimClock.TicksPerSecond));
            };
            BattleSimulation ambusher = fight(Kit(SquadPair.None, H(WhispererHabit.Ambusher)));
            int sprung = RunUntil(ambusher, 3000, e => e.Kind == BattleEventKind.Whisper && e.ActorSide == 0 && e.Detail == WhisperRules.AmbushDetail);
            Assert.GreaterOrEqual(sprung, 0, "her trap springs on a heavy swing");
            long at = ambusher.Events[sprung].Tick;
            Assert.AreEqual(0, ambusher.Events.Count(e => e.Tick < at && e.Kind == BattleEventKind.TrapSprung && e.ActorSide == 0),
                "no light blow sprang it first");

            BattleSimulation upgraded = fight(Kit(SquadPair.None, H(WhispererHabit.Ambusher, true)));
            int stopped = RunUntil(upgraded, 3000, e => e.Kind == BattleEventKind.Whisper && e.ActorSide == 0 && e.Detail == WhisperRules.StoppedDetail);
            Assert.GreaterOrEqual(stopped, 0, "upgraded, the heavy swing is stopped");
            long tick = upgraded.Events[stopped].Tick;
            Assert.IsFalse(upgraded.Events.Any(e => e.Tick == tick && e.Kind == BattleEventKind.Release && e.ActorSide == 1),
                "the stopped swing never lands");
        }

        // ------------------------------------------------------------ pairs

        [Test]
        public void PinAndShootAnswersTheSnareFreeAndMendAndHoldGuardsThePatientAndTheMedic()
        {
            // The Trapper's snare, and the Marksman answers it at once without spending his own aim.
            Func<PalKit, BattleSimulation> pin = kit => Fight("whisper.pin", Side(U("w", UnitClass.Warden), U("t", UnitClass.Trapper),
                U("m", UnitClass.Marksman)), Side(U("x1", UnitClass.Skirmisher, 9000, 30), U("x2", UnitClass.Skirmisher, 9000, 30)), RoadWhisper, kit);
            BattleSimulation paired = pin(Kit(SquadPair.PinAndShoot));
            int free = RunUntil(paired, 3000, e => e.Kind == BattleEventKind.Whisper && e.ActorId == "m" && e.Detail == WhisperRules.FreeDetail);
            Assert.GreaterOrEqual(free, 0, "a free answer");
            BattleSimulation unpaired = pin(PalKit.None);
            RunOut(unpaired, 3000);
            Assert.IsFalse(unpaired.Events.Any(e => e.Kind == BattleEventKind.Whisper), "no pair, no free answer");

            // Mend and Hold: right after she treats someone, the Warden Guards them and her.
            BattleSimulation mend = Fight("whisper.mend", Side(U("w", UnitClass.Warden, 100), U("s", UnitClass.Skirmisher, 100),
                U("med", UnitClass.Medic, 100)),
                Side(U("x1", UnitClass.Warden, 9000, 60), U("x2", UnitClass.Warden, 9000, 60)), Whisper, Kit(SquadPair.MendAndHold));
            int treated = RunUntil(mend, 3000, e => e.Kind == BattleEventKind.Heal && e.ActorId == "med" && e.Detail == ClassSpells.TreatDetail &&
                e.TargetId != "w" && e.TargetId != "med");
            Assert.GreaterOrEqual(treated, 0, "she treated someone beside the Warden");
            BattleUnit w = mend.FindById("w");
            Assert.AreEqual(mend.Events[treated].TargetId, w.GuardTargetId, "his Guard on her patient");
            Assert.AreEqual("med", w.GuardTargetId2, "and on her");
        }

        // ------------------------------------------------------------ the campaign

        private static Campaign Ready(ulong seed, CampaignTuning tuning)
        {
            Campaign c = Campaign.New(seed, tuning);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }

        [Test]
        public void HabitsAreSetAtCampAndKeptBetweenRuns()
        {
            Campaign c = Ready(5600, new CampaignTuning { EnemyScaleTier1 = 40 });
            Assert.IsTrue(c.TrySetHabit(WhispererHabit.Bodyguard));
            Assert.IsTrue(c.TrySetHabit(WhispererHabit.Triage));
            Assert.IsFalse(c.TrySetHabit(WhispererHabit.Gatekeeper), "out of the slice");
            Campaign back = CampaignSave.Read(CampaignSave.Write(c), new CampaignTuning { EnemyScaleTier1 = 40 });
            Assert.IsEmpty(back.LoadNote);
            Assert.AreEqual(2, back.Habits.Count, "a save keeps them at camp");

            CampaignPilot.PlayRun(back, CampaignTribes.BloodridgeId, PilotStyle.Reference, 5600);
            Assert.AreNotEqual(CampaignStage.InRun, back.Stage);
            Assert.AreEqual(2, back.Habits.Count, "kept when the run ends");
            Assert.IsTrue(back.Habits.All(h => !h.Upgraded));
            back.StartRun();
            Assert.IsFalse(back.TrySetHabit(WhispererHabit.Rescuer), "never in a run");
        }

        [Test]
        public void HerRoadCardRanksUpSetsAPairOrUpgradesAHabitAndTheSaveKeepsIt()
        {
            var easy = new CampaignTuning { EnemyScaleTier1 = 40 };
            bool seen = false;
            for (ulong seed = 5700; seed < 5760 && !seen; seed++)
            {
                Campaign c = Ready(seed, easy);
                Assert.IsTrue(c.TrySetHabit(WhispererHabit.Bodyguard));
                Assert.IsTrue(c.TrySetHabit(WhispererHabit.Spotter));
                c.StartRun();
                int index = c.Offers.ToList().FindIndex(o => o.Kind != EncounterKind.Ambush && o.Node != null && o.Node.Pal == Pal.Whisperer);
                if (index < 0) continue;
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(index), new CampaignPilot(PilotStyle.Reference, seed));
                if (!r.Won || c.FavourOffer == null) continue;
                seen = true;
                FavourOffer offer = c.FavourOffer;
                Assert.AreEqual(Pal.Whisperer, offer.Pal);
                Assert.IsTrue(offer.Choices.All(FavourRules.IsWhisper), "only her road card");
                Assert.AreEqual(offer.Choices.Count, offer.Picks.Count);

                // Saved with her card standing, it comes back the same.
                string saved = CampaignSave.Write(c);
                Campaign back = CampaignSave.Read(saved, easy);
                Assert.IsEmpty(back.LoadNote);
                Assert.AreEqual(saved, CampaignSave.Write(back));

                // Each pick does its one thing.
                for (int choice = 0; choice < back.FavourOffer.Choices.Count; choice++)
                {
                    Campaign take = CampaignSave.Read(saved, easy);
                    WhisperPick pick = take.FavourOffer.Picks[choice];
                    int rankBefore = pick.FighterId.Length > 0 ? take.Reserve.First(f => f.Id == pick.FighterId).Rank : 0;
                    Assert.IsTrue(take.TryTakeFavour(choice, out string why), why);
                    if (pick.Kind == Favour.WhisperRankUp) Assert.AreEqual(rankBefore + 1, take.Reserve.First(f => f.Id == pick.FighterId).Rank);
                    else if (pick.Kind == Favour.WhisperPair) Assert.AreEqual(pick.Pair, take.Pair);
                    else Assert.IsTrue(take.Habits.Single(h => h.Habit == pick.Habit).Upgraded);
                    Campaign kept = CampaignSave.Read(CampaignSave.Write(take), easy);
                    Assert.IsEmpty(kept.LoadNote);
                    Assert.AreEqual(CampaignSave.Write(take), CampaignSave.Write(kept), "and a save keeps it");
                }
            }
            Assert.IsTrue(seen, "no Whisperer fight was won");
        }

        [Test]
        public void AFighterWhoFallsInALostFightLosesHisTopRank()
        {
            bool seen = false;
            for (ulong seed = 600; seed < 640 && !seen; seed++)
            {
                Campaign c = Ready(seed, null);
                c.ChooseTribe(CampaignTribes.BloodridgeId);
                for (int i = 0; i < c.Reserve.Count; i++) Assert.IsTrue(c.TryRankUp(c.Reserve[i].Id));
                var picker = new CampaignPilot(PilotStyle.Reference, seed);
                for (int n = 0; c.Stage == CampaignStage.InRun && !seen; n++)
                {
                    var before = c.Reserve.ToDictionary(f => f.Id, f => f.Rank);
                    EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(picker.PickOffer(c)), new CampaignPilot(PilotStyle.Reference, seed * 31UL + (ulong)n));
                    if (r.Won || r.Fallen.Count == 0 || r.Fallen.All(f => before[f.Id] <= 1)) continue;
                    seen = true;
                    foreach (CampaignFighter f in r.Fallen)
                        Assert.AreEqual(Math.Max(1, before[f.Id] - 1), f.Rank, f.Name + " drops one rank");
                    CollectionAssert.AreEquivalent(r.Fallen.Where(f => before[f.Id] > 1).Select(f => f.Id), r.RanksLost.Select(f => f.Id));
                    foreach (CampaignFighter f in c.Reserve.Where(f => !r.Fallen.Contains(f)))
                        Assert.AreEqual(before[f.Id], f.Rank, "the ones still standing keep theirs");
                }
            }
            Assert.IsTrue(seen, "no lost fight with a ranked fighter down");
        }
    }
}
