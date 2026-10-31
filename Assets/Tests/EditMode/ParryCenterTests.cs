using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The parry at the center (Jacob, 24 Oct 2026: "go, build the parry at the center in the sim behind rules version 8";
    /// DESIGN_LOCKS "The parry at the center"; BattleSimulation.Parry.cs). One quick test per piece, only to prove it runs.
    /// Nothing here is tuning.
    public class ParryCenterTests
    {
        private static UnitSpec U(string id, UnitClass unitClass, int health = 1000)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, 100, health);
        }

        private static UnitSpec Chief(UnitClass chiefClass, int health = 1000)
        {
            return new UnitSpec("chief", chiefClass, SliceClasses.Stats(chiefClass).DefaultRow, UnitRole.Chief, 100, health);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        private static List<UnitSpec> Ours()
        {
            return Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman), U("h", UnitClass.Medic),
                U("t", UnitClass.Trapper));
        }

        // A band of three that swings heavies, with these traits.
        private static List<UnitSpec> Band()
        {
            return Side(U("x.a", UnitClass.Warden), U("x.b", UnitClass.Skirmisher), U("x.c", UnitClass.Warden));
        }

        private static SideRules Heavies(BandTrait band = BandTrait.None)
        {
            return new SideRules(0, 0, false, HeavyRules.EveryTicks, ChiefSpell.Eclipse, band);
        }

        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, SideRules rules, FightRules fight,
            TacticsProfile theirTactics = null, bool parryCenter = true, bool nerve = true)
        {
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), theirTactics ?? new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, nerve ? NerveRules.On(NerveRules.Full, NerveRules.Full) : NerveRules.Off,
                10, 400, new SideRules(1, 0, true), rules, parryCenter ? fight.WithParryCenter() : fight));
        }

        // The Bloodridge chief with only his Cleave, behind nobody.
        private static BattleSimulation Cleaver(string id, ulong seed, List<UnitSpec> ours)
        {
            return Fight(id, seed, ours, Side(U("x.s", UnitClass.Skirmisher), Chief(UnitClass.Warden)),
                new SideRules(2, 0, false, 0, ChiefSpell.Eclipse, kit: ChiefKit.Bloodridge, moves: ChiefMove.Cleave), FightRules.CampaignChiefTactics);
        }

        private static bool Order(SimSession session, string directive, string target = "", int arg0 = 0)
        {
            return session.TryIssue(Directives.CommandKind, "0", target, arg0, 0, directive, out _);
        }

        // Ticks on until a threat of this name on side 0 has its beat this many ticks off, and returns it.
        private static Threat Beat(SimSession session, BattleSimulation sim, string name, int at = 1)
        {
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                foreach (Threat t in sim.Threats(0))
                    if (t.Name == name && t.BeatTicks == at) return t;
                session.Tick();
            }
            Assert.Fail(name + "'s beat never came");
            return Threat.None;
        }

        // Ticks on until this happens, and returns the event.
        private static BattleEvent Next(SimSession session, BattleSimulation sim, Func<BattleEvent, bool> match)
        {
            int seen = sim.Events.Count;
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                session.Tick();
                for (int i = seen; i < sim.Events.Count; i++) if (match(sim.Events[i])) return sim.Events[i];
                seen = sim.Events.Count;
            }
            Assert.Fail("it never happened, tick " + sim.CurrentTick);
            return default;
        }

        // Holds on the tick before every beat coming at side 0, and plays the fight out. Under the older rules, which give no beat
        // ticks, on the tick before anything parryable lands.
        private static void ParryEverything(BattleSimulation sim)
        {
            var session = new SimSession(sim);
            while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                if (sim.AwaitingVerdict) Order(session, "Hold");
                else if (sim.HoldTicksLeft(0) <= 0 && sim.Threats(0).Any(t => t.BeatTicks == 1 ||
                             (!sim.ParryCenterOn && t.TicksLeft == 1 && (t.Answers & ThreatAnswer.Parry) != 0)))
                    Order(session, "Hold");
                session.Tick();
            }
        }

        private static List<string> Parried(BattleSimulation sim)
        {
            return sim.Events.Where(e => e.Kind == BattleEventKind.Parry && e.ActorSide == 0).Select(e => e.Detail).Distinct().ToList();
        }

        private static List<UnitSpec> Beast(BeastKit kit)
        {
            var parts = new List<UnitSpec>();
            IReadOnlyList<BeastPart> order = BeastKits.Parts(kit);
            for (int i = 0; i < order.Count; i++)
                parts.Add(new UnitSpec("beast." + BeastKits.Name(order[i]), UnitClass.Warden, BeastKits.RowOf(order[i]), UnitRole.Fighter, 100,
                    BeastKits.HealthPercent(order[i])));
            return parts;
        }

        // ------------------------------------------------------------ every boss move has a beat

        [Test]
        public void EveryKitsMovesHaveABeatAHoldOnItParries()
        {
            ChiefKit[] kits = { ChiefKit.Bloodridge, ChiefKit.Skullpeak, ChiefKit.Stoneveil, ChiefKit.Thundercoast, ChiefKit.Hunt };
            ChiefSpell[] spells = { ChiefSpell.Eclipse, ChiefSpell.SteelStorm, ChiefSpell.BloodCall, ChiefSpell.Tide, ChiefSpell.Eclipse };
            string[][] expected =
            {
                new[] { ParryRules.HeavyName, ChiefKits.CleaveName, ChiefKits.RingName },
                new[] { ParryRules.HeavyName, ChiefKits.HookName, ChiefKits.RifleName },
                new[] { ParryRules.HeavyName, ChiefKits.LeechName, ChiefKits.TransfusionName },
                new[] { ParryRules.HeavyName, ChiefKits.SluiceName },
                new[] { ParryRules.HeavyName, ChiefKits.Name(HuntStep.Track) }
            };
            for (int k = 0; k < kits.Length; k++)
            {
                var band = Side(U("x.w", UnitClass.Warden), U("x.m", UnitClass.Marksman), U("x.t", UnitClass.Trapper), U("x.h", UnitClass.Medic),
                    U("x.s", UnitClass.Skirmisher), Chief(UnitClass.Warden));
                BattleSimulation sim = Fight("kit." + kits[k], 7, Ours(), band, new SideRules(2, ChiefKits.SpellEveryTicks(kits[k]), false,
                    HeavyRules.EveryTicks, spells[k], flood: kits[k] == ChiefKit.Thundercoast, kit: kits[k], moves: ChiefKits.Pool(kits[k])),
                    FightRules.CampaignChiefTactics);
                ParryEverything(sim);
                CollectionAssert.IsSubsetOf(expected[k], Parried(sim), kits[k] + " parried only " + string.Join(", ", Parried(sim)));
            }

            BattleSimulation shark = Fight("shark", 7, Ours(), Beast(BeastKit.SandShark), new SideRules(0, 0, false, beast: BeastKit.SandShark),
                FightRules.CampaignBeast);
            ParryEverything(shark);
            CollectionAssert.IsSubsetOf(new[] { BeastKits.BiteName, BeastKits.SweepName, BeastKits.VolleyName }, Parried(shark));
            BattleSimulation bear = Fight("bear", 7, Ours(), Beast(BeastKit.CageBear), new SideRules(0, 0, false, beast: BeastKit.CageBear),
                FightRules.CampaignBeast);
            ParryEverything(bear);
            CollectionAssert.IsSubsetOf(new[] { BeastKits.GrabName, BeastKits.RoarName, BeastKits.PulseName }, Parried(bear));
        }

        [Test]
        public void NothingIsUnparryableAnyMoreNotEvenFireOrAWarCry()
        {
            for (int s = 0; s < ChiefSpells.Count; s++)
            {
                var spell = (ChiefSpell)s;
                BattleSimulation sim = Fight("spell." + s, 3, Ours(), Side(U("x.s", UnitClass.Skirmisher), Chief(UnitClass.Marksman)),
                    new SideRules(2, 300, false, 0, spell), FightRules.Campaign);
                ParryEverything(sim);
                string name = spell == ChiefSpell.Eclipse ? Directives.Name(Directive.Eclipse) : ChiefSpells.Name(spell);
                CollectionAssert.Contains(Parried(sim), name, spell + " was never parried");
            }
            // And a berserk band, whose swings could not be parried before: focus their leader under half, then parry a swing.
            BattleSimulation berserk = Fight("berserk", 5, Ours(), Side(U("x.a", UnitClass.Skirmisher), U("x.b", UnitClass.Skirmisher),
                Chief(UnitClass.Warden, 300)), Heavies(BandTrait.Berserk), FightRules.Skill, null, true, false);
            var session = new SimSession(berserk);
            while (berserk.BerserkTicksLeft(1) <= 0 && !berserk.IsFinished && berserk.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                if (berserk.FocusTicksLeft(0) <= 0) Order(session, "FocusFire", "chief");
                session.Tick();
            }
            Assert.Greater(berserk.BerserkTicksLeft(1), 0, "never went berserk");
            while (berserk.BerserkTicksLeft(1) > 0 && !berserk.Threats(0).Any(t => t.Name == HeavyRules.Heavy && t.BeatTicks == 1)) session.Tick();
            Assert.Greater(berserk.BerserkTicksLeft(1), 0, "no swing came while berserk");
            Assert.IsTrue(berserk.HeavyParryable(1));
            Assert.IsTrue(Order(session, "Hold"));
            session.Tick();
            Assert.AreEqual(ParryRules.HeavyName, berserk.Events.Last(e => e.Kind == BattleEventKind.Parry).Detail);
        }

        // ------------------------------------------------------------ the duel for nerve

        [Test]
        public void AParryStaggersAndOpensHimAndEachParryInARowCracksHarder()
        {
            BattleSimulation sim = Cleaver("cleave", 11, Side(U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman), U("h", UnitClass.Medic)));
            var session = new SimSession(sim);
            BattleUnit chief = sim.FindById("chief");
            int[] glory = new int[2];
            for (int n = 0; n < 2; n++)
            {
                Beat(session, sim, ChiefKits.CleaveName);
                Assert.AreEqual(n == 0 ? 100 : 150, sim.NextParryPercent(0));
                int before = sim.Nerve(1);
                Assert.IsTrue(Order(session, "Hold"));
                session.Tick();
                BattleEvent parry = sim.Events.Last(e => e.Kind == BattleEventKind.Parry);
                Assert.AreEqual(0, parry.ActorSide);
                Assert.AreEqual(ChiefKits.CleaveName, parry.Detail);
                Assert.GreaterOrEqual(before - sim.Nerve(1), n == 0 ? 100 : 150, "parry " + (n + 1) + " cracked too little");
                Assert.AreEqual(FightRules.StaggerTicks - 1, sim.ReelTicksLeft(chief), "he reels for the stagger");
                Assert.IsNull(sim.CoveredBy(chief), "nobody covers him while he reels");
                Assert.IsTrue(sim.EnemyMarks(0).Any(m => m.UnitId == "chief" && m.Mark == ClassSpells.OpeningMark && m.Casher == UnitClass.Skirmisher),
                    "a parried enemy gets an Opening for your Skirmisher");
                Assert.AreEqual(n + 1, sim.ParryChain(0));
                glory[n] = sim.Events.Last(e => e.Kind == BattleEventKind.Glory).Amount;
                // The Riposte is open, for the screen.
                Assert.IsTrue(sim.OpenCombos(0).Any(c => c.Combo == FightRules.Riposte && c.Next == Directive.Unleash));
            }
            Assert.AreEqual(glory[0] * 2, glory[1], "a second parry in a row is no longer halved");
        }

        [Test]
        public void AStaleHoldOnlySoaksAndCracksYourOwnNerve()
        {
            BattleSimulation sim = Fight("block", 3, Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher)), Band(), Heavies(), FightRules.Skill);
            var session = new SimSession(sim);
            Beat(session, sim, HeavyRules.Heavy, 20);
            Assert.IsTrue(Order(session, "Hold"));
            BattleEvent release = Next(session, sim, e => e.Kind == BattleEventKind.Release && e.ActorSide == 1);
            Assert.AreNotEqual(FightRules.Parried, release.Detail, "a Hold 20 ticks early never parries");
            BattleEvent block = sim.Events.Last(e => e.Kind == BattleEventKind.Reaction && e.Detail == ParryRules.BlockDetail);
            Assert.AreEqual(0, block.ActorSide, "the side that held pays");
            Assert.AreEqual(HeavyRules.ParryNerve, block.Amount, "as hard as a parry would have cracked his");
        }

        [Test]
        public void TooLateOrNoHoldStartsYourChainOverAndDropsYourGlory()
        {
            BattleSimulation sim = Cleaver("late", 13, Side(U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman), U("h", UnitClass.Medic)));
            var session = new SimSession(sim);
            Beat(session, sim, ChiefKits.CleaveName);
            Assert.IsTrue(Order(session, "Hold"));
            session.Tick();
            Assert.AreEqual(1, sim.ParryChain(0));
            BattleEvent land = Next(session, sim, e => e.Kind == BattleEventKind.Land && e.Detail.StartsWith(ChiefKits.CleaveName + "."));
            Assert.AreEqual(ChiefKits.CleaveName + "." + ChiefKits.Full, land.Detail);
            Assert.AreEqual(0, sim.ParryChain(0), "a move that lands in full starts your chain over");
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Glory && e.Detail == ChiefKits.EatenDetail && e.Tick == land.Tick),
                "and your Glory drops a rank");
        }

        [Test]
        public void ASafeAnswerCracksHisNerveAQuarterOfAParry()
        {
            BattleSimulation sim = Fight("safe", 3, Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher)), Band(), Heavies(), FightRules.Skill);
            var session = new SimSession(sim);
            Beat(session, sim, HeavyRules.Heavy, 3);
            Assert.IsTrue(Order(session, "FallBack"));
            session.Tick();
            session.Tick();
            int before = sim.Nerve(1);
            session.Tick();
            BattleEvent release = sim.Events.Last(e => e.Kind == BattleEventKind.Release && e.ActorSide == 1);
            Assert.AreEqual(Moves.Dodged, release.Detail);
            Assert.AreEqual(HeavyRules.ParryNerve * ParryRules.SafePercent / 100, before - sim.Nerve(1));
        }

        [Test]
        public void AHoldGivenWhilePausedNeverParriesInAnyFight()
        {
            // No heavy swings and no beast here: before, a paused Hold could still parry this.
            BattleSimulation sim = Fight("paused", 5, Ours(), Side(U("x.s", UnitClass.Skirmisher), Chief(UnitClass.Marksman)),
                new SideRules(2, 300, false), FightRules.Campaign);
            var session = new SimSession(sim);
            Beat(session, sim, Directives.Name(Directive.Eclipse));
            Assert.IsTrue(Order(session, "Hold", string.Empty, HeavyRules.PausedOrderArg));
            session.Tick();
            Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Parry), "a paused Hold parried");
            Assert.AreEqual(Moves.Soaked, sim.Events.Last(e => e.Kind == BattleEventKind.Release).Detail);
        }

        // ------------------------------------------------------------ what rivals learn

        [Test]
        public void ARivalWhoWaitsOutYourParryDoesItOncePerBeat()
        {
            var tactics = new TacticsProfile();
            tactics.SetWaitOutParry(5);
            BattleSimulation sim = Fight("wait", 3, Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher)), Band(), Heavies(), FightRules.Skill, tactics);
            var session = new SimSession(sim);
            Beat(session, sim, HeavyRules.Heavy);
            Assert.IsTrue(Order(session, "Hold"));
            session.Tick();
            Assert.AreEqual(BattleSimulation.WaitOutDetail, sim.Events.Last(e => e.Kind == BattleEventKind.Reaction).Detail);
            Assert.AreEqual(LessonFiring.WaitOut, sim.LessonFirings(1).Last().Rule);
            Beat(session, sim, HeavyRules.Heavy);
            Assert.IsTrue(Order(session, "Hold"));
            session.Tick();
            Assert.AreEqual(ParryRules.HeavyName, sim.Events.Last(e => e.Kind == BattleEventKind.Parry).Detail, "the late beat is parried: one wait a beat");
        }

        [Test]
        public void ARivalWhoFeintsThrowsAFakeBeatAndIsOpenWhileHeFakes()
        {
            var tactics = new TacticsProfile();
            tactics.SetFeint(5);
            BattleSimulation sim = Fight("feint", 3, Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher)), Band(), Heavies(), FightRules.Skill, tactics);
            var session = new SimSession(sim);
            Assert.IsTrue(sim.Feints(1));
            Threat swing = Beat(session, sim, HeavyRules.Heavy);
            BattleUnit swinger = sim.FindById(swing.ActorId);
            Assert.IsTrue(Order(session, "Hold"));
            session.Tick();
            Assert.AreEqual(ParryRules.FeintDetail, sim.Events.Last(e => e.Kind == BattleEventKind.Reaction).Detail);
            Assert.AreEqual(ParryRules.FeintTicks - 1, sim.ReelTicksLeft(swinger), "open while he fakes it");
            Assert.AreEqual(LessonFiring.Feint, sim.LessonFirings(1).Last().Rule);
            // The real beat finds your Hold stale.
            BattleEvent release = Next(session, sim, e => e.Kind == BattleEventKind.Release && e.ActorSide == 1);
            Assert.AreNotEqual(FightRules.Parried, release.Detail);
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Reaction && e.Detail == ParryRules.BlockDetail && e.ActorSide == 0));
            OrderResult hold = sim.OrderResults(0).Last(r => r.Order == Directive.Hold);
            if (hold.Miss != OrderMiss.None) Assert.AreEqual(LessonFiring.Feint, sim.LessonBehind(0, hold), "the lesson behind the miss");
        }

        [Test]
        public void HisWardensStayStuckOnTheFirstOneYouFocusAndNeverCoverTheStaggered()
        {
            var tactics = new TacticsProfile();
            tactics.SetCoverFocused(5);
            BattleSimulation sim = Fight("cover", 3, Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman)),
                Side(U("x.a", UnitClass.Warden), U("x.b", UnitClass.Skirmisher), U("x.c", UnitClass.Marksman)), Heavies(), FightRules.Skill, tactics);
            var session = new SimSession(sim);
            // Their swings go round the band: wait for one by someone their Warden can cover.
            BattleUnit swinger = sim.FindById(Beat(session, sim, HeavyRules.Heavy, 10).ActorId);
            while (swinger.Class == UnitClass.Warden)
            {
                session.Tick();
                swinger = sim.FindById(Beat(session, sim, HeavyRules.Heavy, 10).ActorId);
            }
            BattleUnit other = sim.Units.First(u => u.Side == 1 && u.Class != UnitClass.Warden && u.Id != swinger.Id);
            Assert.IsTrue(Order(session, "FocusFire", swinger.Id));
            session.Tick();
            Assert.IsNotNull(sim.CoveredBy(swinger), "the learned cover");
            Assert.IsTrue(Order(session, "FocusFire", other.Id));
            session.Tick();
            Assert.IsNull(sim.CoveredBy(other), "a second Focus Fire finds them stuck");
            Beat(session, sim, HeavyRules.Heavy);
            Assert.IsTrue(Order(session, "Hold"));
            session.Tick();
            Assert.IsTrue(sim.Events.Any(e => e.Kind == BattleEventKind.Parry && e.TargetId == swinger.Id));
            Assert.IsNull(sim.CoveredBy(swinger), "nobody covers a staggered fighter");
        }

        [Test]
        public void TribesLearnToWaitFromYourParriesToFeintAtThreeAndDropShootingPastYourWarden()
        {
            // Under the older rules: no feint, whatever the parries, and the guard lesson is still learned.
            var mind = new TribeMind("t", "Leader");
            BattleSimulation old = Fight("learn.old", 3, Ours(), Band(), Heavies(), FightRules.Skill, null, false);
            ParryEverything(old);
            FightRecord oldRecord = FightRecord.From(old);
            Assert.GreaterOrEqual(oldRecord.Parries(0, out _, out _), LessonRules.FeintParries);
            IReadOnlyList<Lesson> before = LessonRules.Learn(mind, oldRecord, 1);
            Assert.IsFalse(before.Any(l => l.Rule == LessonRules.FeintRule));
            bool guarded = mind.HasHabit(Habit.AvoidGuarded);

            BattleSimulation sim = Fight("learn.new", 3, Ours(), Band(), Heavies(), FightRules.Skill);
            ParryEverything(sim);
            FightRecord record = FightRecord.From(sim);
            Assert.IsTrue(record.ParryCenter);
            IReadOnlyList<Lesson> lessons = LessonRules.Learn(mind, record, 1);
            Assert.IsTrue(lessons.Any(l => l.Rule == LessonRules.Parry), "waits out your parry");
            Lesson feint = lessons.Single(l => l.Rule == LessonRules.FeintRule);
            Assert.IsTrue(mind.HasHabit(Habit.Feint));
            Assert.AreEqual(record.Parries(0, out _, out _), feint.Evidence);
            Assert.AreEqual(feint.Evidence, LessonRules.Evidence(feint.Rule, record, 1, feint.Subject), "a replay bears the lesson out");
            Assert.IsFalse(lessons.Any(l => l.Rule == LessonRules.Guard), "the guard lesson is cut");
            if (guarded) Assert.AreEqual(0, mind.HabitValue(Habit.AvoidGuarded), "and dropped");
            Assert.IsNotEmpty(LessonRules.PlayFor(feint.Rule), "each lesson names the play it opens");
        }

        // ------------------------------------------------------------ the hunters

        [Test]
        public void HuntersLearnARhythmAgainstYourParriesAndTheTraitsCountSeventeen()
        {
            Assert.AreEqual(17, HunterTraits.Count);
            CollectionAssert.AreEqual(new[] { HunterTrait.Unstoppable, HunterTrait.Quickstrike, HunterTrait.Feint },
                HunterTraits.Counters(Directive.Hold, true), "Delay, then Double swing, then Feint");
            CollectionAssert.AreEqual(new[] { HunterTrait.LongReach }, HunterTraits.Counters(Directive.FallBack, true));
            CollectionAssert.AreEqual(new[] { HunterTrait.ThickHide }, HunterTraits.Counters(Directive.FocusFire, true));
            CollectionAssert.AreEqual(new[] { HunterTrait.Unshakable }, HunterTraits.Counters(Directive.Unleash, true));
            CollectionAssert.AreEqual(new[] { HunterTrait.Unstoppable, HunterTrait.Shieldbreaker }, HunterTraits.Counters(Directive.Hold, false),
                "the older rules keep theirs");
            Assert.AreEqual("Delay", HunterTraits.Name(HunterTrait.Unstoppable));
            Assert.AreEqual("Double Swing", HunterTraits.Name(HunterTrait.Quickstrike));
            Assert.AreEqual(Directive.FocusFire, HunterTraits.Answer(HunterTrait.Feint));
        }

        [Test]
        public void ADoubleSwingComesLightThenHeavyAndParryingBothCracksTheSecondTwice()
        {
            BattleSimulation sim = Fight("double", 3, Side(U("w", UnitClass.Warden)), Band(), Heavies(BandTrait.Double), FightRules.Skill);
            var session = new SimSession(sim);
            Beat(session, sim, HeavyRules.Heavy);
            Assert.IsTrue(Order(session, "Hold"));
            session.Tick();
            Assert.AreEqual(ParryRules.DoubleDetail, sim.Events.Last(e => e.Kind == BattleEventKind.Reaction && e.ActorSide == 1).Detail);
            Assert.AreEqual(LessonFiring.Double, sim.LessonFirings(1).Last().Rule);
            Beat(session, sim, HeavyRules.Heavy);
            int before = sim.Nerve(1);
            Assert.IsTrue(Order(session, "Hold"));
            session.Tick();
            Assert.AreEqual(2, sim.Events.Count(e => e.Kind == BattleEventKind.Parry && e.ActorSide == 0));
            int cracked = before - sim.Nerve(1);
            int expected = HeavyRules.ParryNerve * 2 * (100 + ParryRules.ChainStepPercent) / 100;
            Assert.That(cracked, Is.InRange(expected, expected + 10), "the second of a double parried cracks twice, chained");

            // Fall Back clears only the first.
            BattleSimulation dodge = Fight("double.dodge", 3, Side(U("w", UnitClass.Warden)), Band(), Heavies(BandTrait.Double), FightRules.Skill);
            var s2 = new SimSession(dodge);
            Beat(s2, dodge, HeavyRules.Heavy, 3);
            Assert.IsTrue(Order(s2, "FallBack"));
            BattleEvent first = Next(s2, dodge, e => e.Kind == BattleEventKind.Release && e.ActorSide == 1);
            Assert.AreEqual(Moves.Dodged, first.Detail);
            BattleEvent second = Next(s2, dodge, e => e.Kind == BattleEventKind.Release && e.ActorSide == 1);
            Assert.AreNotEqual(Moves.Dodged, second.Detail, "the heavy second swing comes through your Fall Back");
        }

        [Test]
        public void ThickHideBracesTheOneYouFocusSoHeCannotSwing()
        {
            BattleSimulation sim = Fight("hide", 3, Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher)), Band(), Heavies(BandTrait.ThickHide),
                FightRules.Skill);
            var session = new SimSession(sim);
            BattleUnit braced = sim.FindById("x.b");
            session.Tick();
            Assert.IsTrue(Order(session, "FocusFire", braced.Id));
            Assert.IsTrue(sim.Braced(braced));
            Assert.AreEqual(LessonFiring.ThickHide, sim.LessonFirings(1).Last().Rule);
            int from = sim.Events.Count;
            for (int i = 0; i < BattleSimulation.FocusTicks - 2; i++) session.Tick();
            Assert.IsFalse(sim.Events.Skip(from).Any(e => e.ActorId == braced.Id && (e.Kind == BattleEventKind.Hit || e.Kind == BattleEventKind.Windup)),
                "braced, he does not swing");
        }

        [Test]
        public void AHunterKeepsWhereHeLearnedATraitThroughASave()
        {
            Campaign c = Campaign.New(77);
            string text = CampaignSave.Write(c);
            Assert.GreaterOrEqual(CampaignSave.Version, 18, "hunter lessons came in save version 18");
            Assert.IsTrue(text.StartsWith(CampaignSave.Header + "\t" + CampaignSave.Version), "written as the current version");
            List<string> lines = text.Split('\n').ToList();
            int at = lines.FindLastIndex(l => l.StartsWith("hunter\t") && l.Split('\t').Length > 9);
            Assert.GreaterOrEqual(at, 0, "a hunter with a trait");
            string[] f = lines[at].Split('\t');
            lines.Insert(at + 1, "hunterlesson\t" + f[1] + "\t" + f[9] + "\t" + (int)Directive.Hold + "\t7\tcamp.3.raiders.chief");
            Campaign read = CampaignSave.Read(string.Join("\n", lines));
            CampaignHunter hunter = read.Company.Find(f[1]);
            HunterLesson lesson = hunter.LessonFor((HunterTrait)int.Parse(f[9]));
            Assert.IsNotNull(lesson);
            Assert.AreEqual(Directive.Hold, lesson.Button);
            Assert.AreEqual(7, lesson.Times);
            Assert.AreEqual("camp.3.raiders.chief", lesson.FightId);
            StringAssert.Contains("hunterlesson\t" + f[1], CampaignSave.Write(read), "and writes it again");
        }

        // ------------------------------------------------------------ the reads

        [Test]
        public void TheThreatReadGivesEachBeatItsRingAndTheFightersWhoAnswerIt()
        {
            BattleSimulation sim = Cleaver("read", 11, Ours());
            var session = new SimSession(sim);
            Threat cleave = Beat(session, sim, ChiefKits.CleaveName, FightRules.PerfectHoldTicks + 1);
            Assert.IsTrue(cleave.ParryWindow);
            Assert.IsTrue((cleave.Answers & ThreatAnswer.Parry) != 0);
            if ((cleave.Answers & ThreatAnswer.Warden) != 0) CollectionAssert.Contains(sim.ThreatAnswerers(0, cleave), "w");
            Assert.AreEqual(-1, sim.NextCastTicks(1), "he casts nothing on a clock here");
            Assert.AreEqual(-1, new Threat("x", ThreatTier.Basic, "a", "b", 5, 1, ThreatAnswer.Hold, string.Empty).BeatTicks);
        }

        // ------------------------------------------------------------ rules version 8

        [Test]
        public void EveryCampaignFightFromRulesEightPlaysTheParryAtTheCenter()
        {
            Assert.GreaterOrEqual(Campaign.RulesVersion, 8, "rules 9 plays it too");
            Campaign c = Campaign.New(4600, new CampaignTuning { PathFights = 0 });
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, 4600, 10));
            c.ChooseTribe(CampaignTribes.BloodridgeId);
            CampaignFight fight = c.StartEncounter(0);
            Assert.AreEqual(Campaign.RulesVersion, fight.RulesVersion);
            Assert.IsTrue(fight.Setup.Fight.ParryCenter);
            Assert.IsTrue(fight.Sim.ParryCenterOn);
            // It saves and plays back exactly mid fight.
            for (int i = 0; i < 200 && !fight.Sim.IsFinished; i++) fight.Session.Tick();
            string checksum = StateChecksum.Compute(fight.Sim.OrderedStateSnapshot);
            Campaign back = CampaignSave.Read(CampaignSave.Write(c), new CampaignTuning { PathFights = 0 });
            Assert.AreEqual(string.Empty, back.LoadNote);
            Assert.AreEqual(checksum, StateChecksum.Compute(back.Active.Sim.OrderedStateSnapshot));
        }
    }
}
