using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The Butcher's 15 combos (Jacob, 29 Oct 2026: "Go, Black Hole Round", then "Yes, go" to the other 14; DESIGN_LOCKS, "The
    /// Pals on the road" and "Four Pal fixes from the model"). One quick test per combo and per pact, only to prove each runs
    /// (Jacob, 12 Oct 2026: build first), then the road: three combos offered from the node's own dice, the piece in its
    /// fighter's slot, the save, the fights that follow and the run's end, and rules 14 left as it was.
    public class ButcherComboTests
    {
        // A fight with the class spells: tough fighters and no big moves unless their side's rules give them, so only the
        // spells and the combos happen.
        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs,
            SideRules theirRules = null, NerveRules nerve = null)
        {
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, nerve ?? NerveRules.Off, 0, 400, null, theirRules,
                FightRules.Campaign));
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        private static UnitSpec U(string id, UnitClass unitClass, Row? row = null)
        {
            return row == null ? new UnitSpec(id, unitClass) : new UnitSpec(id, unitClass, row.Value);
        }

        private static UnitSpec Wearing(string id, UnitClass unitClass, ClassBend combo, Row? row = null)
        {
            return U(id, unitClass, row).WithBend(combo);
        }

        private static UnitSpec Marksman(ClassBend bend)
        {
            return new UnitSpec("a", UnitClass.Marksman).WithBend(bend);
        }

        // Three Medics in their front row: they never fight back, so every aim runs its time.
        private static List<UnitSpec> ThreeMedics()
        {
            return Side(U("m1", UnitClass.Medic, Row.Front), U("m2", UnitClass.Medic, Row.Front), U("m3", UnitClass.Medic, Row.Front));
        }

        private static readonly string[] MedicRow = { "m1", "m2", "m3" };

        // The ids beside this one in a row of ids.
        private static List<string> Beside(string[] row, string id)
        {
            int at = Array.IndexOf(row, id);
            return row.Where((x, i) => Math.Abs(i - at) == 1).ToList();
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

        private static bool Is(BattleEvent e, BattleEventKind kind, ClassBend combo)
        {
            return e.Kind == kind && e.Detail == ClassSpells.Name(combo);
        }

        private static Campaign Ready(ulong seed, CampaignTuning tuning)
        {
            Campaign c = Campaign.New(seed, tuning);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }

        // ------------------------------------------------------------ the pacts

        [Test]
        public void HeavyPutsAQuarterMoreTimeBetweenHisAims()
        {
            // Right after a shot his spell clock starts again: a quarter longer with the combo's pact.
            var heavy = Fight("heavy", 4, Side(Marksman(ClassBend.BlackHoleRound)), ThreeMedics());
            var plain = Fight("heavy", 4, Side(Marksman(ClassBend.None)), ThreeMedics());
            Assert.GreaterOrEqual(RunUntil(heavy, 4000, e => e.Kind == BattleEventKind.Hit && e.ActorId == "a" &&
                e.Detail.StartsWith(ClassSpells.AimedDetail)), 0);
            Assert.GreaterOrEqual(RunUntil(plain, 4000, e => e.Kind == BattleEventKind.Hit && e.ActorId == "a" &&
                e.Detail.StartsWith(ClassSpells.AimedDetail)), 0);
            int longer = ClassSpells.AimEveryTicks * ClassSpells.HeavyPactPercent / 100 - ClassSpells.AimEveryTicks;
            Assert.Greater(longer, 0);
            Assert.AreEqual(longer, heavy.FindById("a").SpellTicks - plain.FindById("a").SpellTicks);

            // And between her strikes, on a Skirmisher's Gravity Well combo.
            var collapse = Fight("heavy.k", 4, Side(Wearing("k", UnitClass.Skirmisher, ClassBend.Collapse)), ThreeMedics());
            var strike = Fight("heavy.k", 4, Side(U("k", UnitClass.Skirmisher)), ThreeMedics());
            Assert.GreaterOrEqual(RunUntil(collapse, 4000, e => e.ActorId == "k" && e.Detail.StartsWith(ClassSpells.StrikeDetail)), 0);
            Assert.GreaterOrEqual(RunUntil(strike, 4000, e => e.ActorId == "k" && e.Detail.StartsWith(ClassSpells.StrikeDetail)), 0);
            Assert.AreEqual(ClassSpells.StrikeEveryTicks * ClassSpells.HeavyPactPercent / 100 - ClassSpells.StrikeEveryTicks,
                collapse.FindById("k").SpellTicks - strike.FindById("k").SpellTicks);
        }

        [Test]
        public void UnnervingAndBorrowedTimeAreTakenWhenTheFightStarts()
        {
            // Borrowed time: an eighth less health.
            var plain = Fight("pact", 4, Side(U("k", UnitClass.Skirmisher), U("a", UnitClass.Marksman)), ThreeMedics(), null, NerveRules.On());
            var worn = Fight("pact", 4, Side(Wearing("k", UnitClass.Skirmisher, ClassBend.FrozenFlurry),
                Wearing("a", UnitClass.Marksman, ClassBend.AroundTheCorner)), ThreeMedics(), null, NerveRules.On());
            Assert.AreEqual(plain.FindById("k").MaxHp * ClassSpells.BorrowedTimePermille / 1000, worn.FindById("k").MaxHp);
            Assert.AreEqual(worn.FindById("k").MaxHp, worn.FindById("k").Hp, "she starts full, at the lower health");
            Assert.AreEqual(plain.FindById("a").MaxHp, worn.FindById("a").MaxHp, "a Fold Door combo costs no health");

            // Unnerving: with no nerve of their own the side's bar loses an eighth of the wearer's share (one of two).
            int bar = plain.Nerve(0);
            Assert.AreEqual(bar - bar / 2 / ClassSpells.UnnervingEighths, worn.Nerve(0));
            Assert.AreEqual(plain.Nerve(1), worn.Nerve(1), "theirs is untouched");
        }

        // ------------------------------------------------------------ Gravity Well

        [Test]
        public void BlackHoleRoundCrushesTheEnemiesBesideItsMark()
        {
            var sim = Fight("blackhole", 4, Side(Marksman(ClassBend.BlackHoleRound)), ThreeMedics());
            int pullAt = RunUntil(sim, 4000, e => e.Kind == BattleEventKind.Pull);
            Assert.GreaterOrEqual(pullAt, 0, "no hole ever opened");
            BattleEvent pull = sim.Events[pullAt];
            Assert.AreEqual("a", pull.ActorId);
            Assert.AreEqual(ClassSpells.Name(ClassBend.BlackHoleRound), pull.Detail);

            // It opens on the mark of the Aimed Shot that just landed, on the same tick.
            BattleEvent shot = sim.Events.Take(pullAt).Last(e => e.Kind == BattleEventKind.Hit && e.ActorId == "a");
            StringAssert.StartsWith(ClassSpells.AimedDetail, shot.Detail);
            Assert.AreEqual(shot.TargetId, pull.TargetId);
            Assert.AreEqual(shot.Tick, pull.Tick);

            // Everyone beside the mark in its row is dragged in and crushed: two for the middle one, one for an end.
            List<string> beside = Beside(MedicRow, pull.TargetId);
            Assert.AreEqual(beside.Count, pull.Amount, "how many it dragged in");
            List<BattleEvent> crushes = sim.Events.Skip(pullAt + 1).Take(pull.Amount).ToList();
            int crush = Math.Max(1, sim.FindById("a").Power * ClassSpells.AimedShotPercent / 100 * ClassSpells.BlackHoleCrushPercent / 100);
            foreach (BattleEvent e in crushes)
            {
                Assert.AreEqual(BattleEventKind.Hit, e.Kind);
                Assert.AreEqual(ClassSpells.BlackHoleDetail, e.Detail);
                Assert.AreEqual(pull.Tick, e.Tick);
                Assert.AreEqual(crush, e.Amount, "half the shot's own weight");
            }
            CollectionAssert.AreEquivalent(beside, crushes.Select(e => e.TargetId).ToList());

            // Without the combo the same fight opens no hole.
            var plain = Fight("blackhole", 4, Side(Marksman(ClassBend.None)), ThreeMedics());
            Assert.Less(RunUntil(plain, 4000, e => e.Kind == BattleEventKind.Pull || e.Detail == ClassSpells.BlackHoleDetail), 0);
        }

        // Their melee spring our Trapper's trap, so the Marksman finds snared enemies in a row of three.
        private static List<UnitSpec> ThreeWardens()
        {
            return Side(U("x1", UnitClass.Warden, Row.Front), U("x2", UnitClass.Warden, Row.Front), U("x3", UnitClass.Warden, Row.Front));
        }

        [Test]
        public void BlackHoleRoundCrushesWithTheCritWhenItsMarkIsSnared()
        {
            // The hooks (29 Oct 2026): a shot that cashed a Snare carries its crit into the crush.
            var sim = Fight("blackhole.snare", 4, Side(U("w", UnitClass.Warden), U("t", UnitClass.Trapper), Marksman(ClassBend.BlackHoleRound)),
                ThreeWardens());
            int at = -1;
            for (int from = 0; ; from = at + 1)
            {
                at = RunUntil(sim, 9000, e => Is(e, BattleEventKind.Pull, ClassBend.BlackHoleRound) && e.Amount > 0, from);
                Assert.GreaterOrEqual(at, 0, "no black hole opened on a snared mark with anyone beside it");
                if (sim.Events.Take(at).Last(e => e.Kind == BattleEventKind.Hit && e.ActorId == "a").Detail == ClassSpells.AimedPinDetail) break;
            }
            BattleEvent pull = sim.Events[at];
            List<BattleEvent> crushes = sim.Events.Skip(at + 1).Where(e => e.Tick == pull.Tick && e.Detail == ClassSpells.BlackHoleDetail).ToList();
            Assert.AreEqual(pull.Amount, crushes.Count);
            int crush = Math.Max(1, sim.FindById("a").Power * ClassSpells.AimedShotPercent / 100 * ClassSpells.BlackHoleCrushPercent / 100);
            foreach (BattleEvent e in crushes) Assert.AreEqual(crush * ClassSpells.PinCritPercent / 100, e.Amount, "the crit carried into the crush");
        }

        [Test]
        public void CollapsePinsHerMarkOnEverySecondStrike()
        {
            var sim = Fight("collapse", 4, Side(Wearing("k", UnitClass.Skirmisher, ClassBend.Collapse)), ThreeMedics());
            int at = RunUntil(sim, 6000, e => Is(e, BattleEventKind.Pull, ClassBend.Collapse));
            Assert.GreaterOrEqual(at, 0, "no black hole dropped");
            int strikes = sim.Events.Take(at).Count(e => e.Kind == BattleEventKind.Hit && e.ActorId == "k" && e.Detail.StartsWith(ClassSpells.StrikeDetail));
            Assert.AreEqual(ClassSpells.CollapseEvery, strikes, "on her second strike that landed");
            BattleUnit mark = sim.FindById(sim.Events[at].TargetId);
            Assert.AreEqual(ClassSpells.CollapseTicks - 1, mark.PinTicks, "pinned flat (one tick has run)");
            Assert.AreEqual(0, mark.SnareTicks, "and no Snare mark");
        }

        [Test]
        public void CollapseDropsTheBlackHoleOnEveryStrikeThatRipsAWound()
        {
            // The hooks (29 Oct 2026): our Trapper's snares leave bleeding wounds, and every strike of hers that rips one open
            // drops the black hole at once, odd strikes too.
            var sim = Fight("collapse.rip", 4, Side(U("w", UnitClass.Warden), U("t", UnitClass.Trapper), Wearing("k", UnitClass.Skirmisher, ClassBend.Collapse)),
                ThreeWardens());
            while (!sim.IsFinished && sim.CurrentTick < 9000) sim.Tick();
            List<BattleEvent> rips = sim.Events.Where(e => e.Kind == BattleEventKind.Cash && e.ActorId == "k" && e.Detail == ClassSpells.RipDetail).ToList();
            Assert.GreaterOrEqual(rips.Count, 2, "her strike ripped wounds open");
            foreach (BattleEvent rip in rips)
                Assert.IsTrue(sim.Events.Any(e => Is(e, BattleEventKind.Pull, ClassBend.Collapse) && e.Tick == rip.Tick && e.TargetId == rip.TargetId),
                    "a black hole on the strike that ripped at tick " + rip.Tick);
            int odd = rips.Count(r => sim.Events.Count(e => e.Kind == BattleEventKind.Hit && e.ActorId == "k" &&
                e.Detail.StartsWith(ClassSpells.StrikeDetail) && e.Tick <= r.Tick) % ClassSpells.CollapseEvery != 0);
            Assert.Greater(odd, 0, "a rip on a strike that was not her second, so the rip dropped it");
        }

        [Test]
        public void EventHorizonsFourthSwingCleavesTheEnemiesBesideHisMark()
        {
            var sim = Fight("horizon", 4, Side(Wearing("w", UnitClass.Warden, ClassBend.EventHorizon)), ThreeMedics());
            int at = RunUntil(sim, 6000, e => Is(e, BattleEventKind.Pull, ClassBend.EventHorizon));
            Assert.GreaterOrEqual(at, 0, "his bash never came");
            BattleEvent pull = sim.Events[at];
            int swings = sim.Events.Take(at).Count(e => e.ActorId == "w" && (e.Kind == BattleEventKind.Hit || e.Kind == BattleEventKind.Miss));
            Assert.AreEqual(ClassSpells.EventHorizonEvery - 1, swings, "three plain swings first");
            Assert.AreEqual(Beside(MedicRow, pull.TargetId).Count, pull.Amount);
            int power = sim.FindById("w").Power * ClassSpells.EventHorizonPercent / 100;
            List<BattleEvent> cleaved = sim.Events.Skip(at + 1).Where(e => e.Tick == pull.Tick && e.Detail == ClassSpells.EventHorizonDetail).ToList();
            Assert.AreEqual(pull.Amount, cleaved.Count);
            foreach (BattleEvent e in cleaved) Assert.AreEqual(power, e.Amount, "twice his power");
            CollectionAssert.AreEquivalent(Beside(MedicRow, pull.TargetId), cleaved.Select(e => e.TargetId).ToList());

            // The hooks (29 Oct 2026): everyone the cleave hits is left bleeding, a Bleed mark of his.
            foreach (BattleEvent e in cleaved)
            {
                Assert.IsTrue(sim.Events.Skip(at + 1).Any(m => m.Kind == BattleEventKind.Mark && m.Detail == ClassSpells.BleedMark &&
                    m.ActorId == "w" && m.TargetId == e.TargetId && m.Tick == pull.Tick), e.TargetId + " is left bleeding");
                Assert.Greater(sim.FindById(e.TargetId).BleedTicks, 0);
            }
        }

        [Test]
        public void SingularityNetSnaresTheEnemiesBesideWhoeverSpringsHerTrap()
        {
            string[] row = { "x1", "x2", "x3" };
            var sim = Fight("net", 4, Side(U("w", UnitClass.Warden), Wearing("t", UnitClass.Trapper, ClassBend.SingularityNet)), ThreeWardens());
            int at = RunUntil(sim, 6000, e => Is(e, BattleEventKind.Pull, ClassBend.SingularityNet));
            Assert.GreaterOrEqual(at, 0, "nobody sprang her trap");
            BattleEvent pull = sim.Events[at];
            Assert.Greater(sim.FindById(pull.TargetId).SnareTicks, 0, "whoever sprang it is snared");
            List<string> beside = Beside(row, pull.TargetId);
            Assert.AreEqual(beside.Count, pull.Amount);

            // The hooks (29 Oct 2026): everyone it drags in is snared too, the Snare mark and all, not only pinned.
            foreach (string id in beside)
            {
                Assert.Greater(sim.FindById(id).SnareTicks, 0, id + " is dragged in and snared");
                Assert.IsTrue(sim.Events.Skip(at + 1).Any(e => e.Kind == BattleEventKind.Mark && e.Detail == ClassSpells.SnareMark &&
                    e.ActorId == "t" && e.TargetId == id), id + " carries her Snare mark");
            }
        }

        [Test]
        public void MercyWellHealsTheAlliesBesideHerPatient()
        {
            // Their Wardens each go for whoever stands across (side rules turn the lanes on), so the whole row gets hurt.
            string[] row = { "a1", "a2", "a3" };
            var sim = Fight("mercy", 4, Side(U("a1", UnitClass.Warden, Row.Front), U("a2", UnitClass.Warden, Row.Front),
                U("a3", UnitClass.Warden, Row.Front), Wearing("h", UnitClass.Medic, ClassBend.MercyWell)),
                Side(U("x1", UnitClass.Warden), U("x2", UnitClass.Warden), U("x3", UnitClass.Warden)), new SideRules(1, 0, false));
            int at = RunUntil(sim, 9000, e => e.Kind == BattleEventKind.Heal && e.Detail == ClassSpells.MercyDetail);
            Assert.GreaterOrEqual(at, 0, "nobody beside a patient was healed");
            BattleEvent pull = sim.Events.Take(at).Last(e => Is(e, BattleEventKind.Pull, ClassBend.MercyWell));
            BattleEvent treat = sim.Events.Take(at).Last(e => e.Kind == BattleEventKind.Heal && e.Detail == ClassSpells.TreatDetail);
            Assert.AreEqual(pull.TargetId, treat.TargetId, "around her patient");
            CollectionAssert.Contains(Beside(row, pull.TargetId), sim.Events[at].TargetId, "beside them");
        }

        [Test]
        public void MercyWellPoursWhatTheAlliesBesideDoNotNeedBackIntoHerPatient()
        {
            // One strong Warden hits only the middle of their row, so the allies beside her patient stand unhurt and all their
            // share flows back into the patient (rethought 29 Oct 2026).
            var sim = Fight("mercy.spare", 4, Side(U("a1", UnitClass.Warden, Row.Front), U("a2", UnitClass.Warden, Row.Front),
                U("a3", UnitClass.Warden, Row.Front), Wearing("h", UnitClass.Medic, ClassBend.MercyWell)),
                Side(new UnitSpec("x", UnitClass.Warden, Row.Front, UnitRole.Fighter, 300, 400)), new SideRules(1, 0, false));
            int at = RunUntil(sim, 9000, e => Is(e, BattleEventKind.Pull, ClassBend.MercyWell));
            Assert.GreaterOrEqual(at, 0, "she never treated anyone");
            BattleEvent pull = sim.Events[at];
            Assert.AreEqual("a2", pull.TargetId);
            Assert.AreEqual(2, pull.Amount, "two allies beside");
            BattleEvent treat = sim.Events.Take(at).Last(e => e.Kind == BattleEventKind.Heal && e.Detail == ClassSpells.TreatDetail);
            BattleEvent spare = sim.Events[at + 1];
            Assert.AreEqual(BattleEventKind.Heal, spare.Kind, "neither ally beside needed a drop");
            Assert.AreEqual(ClassSpells.MercyDetail, spare.Detail);
            Assert.AreEqual("a2", spare.TargetId, "so it all went back into her patient");
            Assert.AreEqual(2 * (treat.Amount * ClassSpells.MercyWellPercent / 100), spare.Amount);
        }

        [Test]
        public void MercyWellHealsAFifthMoreForEveryBleedingEnemy()
        {
            // The hooks (29 Oct 2026): our Trapper's snares leave their melee bleeding, and each bleeding enemy feeds her
            // treatment a fifth more, up to three.
            var sim = Fight("mercy.blood", 4, Side(U("w", UnitClass.Warden), U("t", UnitClass.Trapper), Wearing("h", UnitClass.Medic, ClassBend.MercyWell)),
                ThreeWardens());
            int at = RunUntil(sim, 9000, e => e.Kind == BattleEventKind.Cash && e.ActorId == "h" && e.Detail == ClassSpells.MercyDetail);
            Assert.GreaterOrEqual(at, 0, "no bleeding enemy ever fed her treatment");
            BattleEvent blood = sim.Events[at];
            BattleEvent before = sim.Events[at - 1];
            int amount = sim.FindById("h").Power;
            if (before.Kind == BattleEventKind.Cash && before.Detail == ClassSpells.CoveredDetail && before.Tick == blood.Tick) amount += before.Amount;
            CollectionAssert.Contains(Enumerable.Range(1, ClassSpells.MercyBleedingMost).Select(n => amount * ClassSpells.MercyBleedPercent * n / 100).ToList(),
                blood.Amount, "a fifth more for each bleeding enemy");
            Assert.AreEqual(BattleEventKind.Heal, sim.Events[at + 1].Kind);
            Assert.AreEqual(ClassSpells.TreatDetail, sim.Events[at + 1].Detail);

            // With no Trapper nobody bleeds, and her treatment is never fed.
            var dry = Fight("mercy.blood", 4, Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher), Wearing("h", UnitClass.Medic, ClassBend.MercyWell)),
                ThreeWardens());
            Assert.Less(RunUntil(dry, 3000, e => e.Kind == BattleEventKind.Cash && e.Detail == ClassSpells.MercyDetail), 0);
        }

        // ------------------------------------------------------------ Fold Door

        [Test]
        public void AroundTheCornerShootsTheirHealerFromBehindPastTheirGuard()
        {
            // Their Warden Guards their Medic, but no Guard takes a shot through a portal from behind, and at a healer it hits
            // twice as hard (rethought 29 Oct 2026).
            var sim = Fight("corner", 4, Side(Wearing("a", UnitClass.Marksman, ClassBend.AroundTheCorner)),
                Side(U("x", UnitClass.Warden), U("h", UnitClass.Medic, Row.Back)));
            int at = RunUntil(sim, 4000, e => Is(e, BattleEventKind.Portal, ClassBend.AroundTheCorner));
            Assert.GreaterOrEqual(at, 0, "no portal opened");
            Assert.AreEqual("h", sim.Events[at].TargetId, "at their healer, with no Focus Fire and nobody snared");
            Assert.AreEqual("h", sim.Events.Take(at).Last(e => e.Kind == BattleEventKind.Aim && e.ActorId == "a").TargetId);
            int shot = RunUntil(sim, 4000, e => e.Kind == BattleEventKind.Hit && e.ActorId == "a" &&
                (e.Detail.StartsWith(ClassSpells.AimedDetail) || e.Detail == BattleSimulation.GuardedDetail), at);
            Assert.GreaterOrEqual(shot, 0, "the shot never landed");
            Assert.AreEqual("h", sim.Events[shot].TargetId, "no Guard took it");
            StringAssert.StartsWith(ClassSpells.AimedDetail, sim.Events[shot].Detail);
            int lightest = sim.FindById("a").Power * 85 / 100 * ClassSpells.AimedShotPercent / 100;
            Assert.GreaterOrEqual(sim.Events[shot].Amount, lightest * ClassSpells.CornerCritPercent / 100, "harder from behind");

            // Your Focus Fire comes first: focused on their Warden, the shot goes at him.
            var focused = Fight("corner.focus", 4, Side(Wearing("a", UnitClass.Marksman, ClassBend.AroundTheCorner)),
                Side(U("x", UnitClass.Warden), U("h", UnitClass.Medic, Row.Back)));
            var session = new SimSession(focused);
            while (focused.CurrentTick < 10) session.Tick();
            Assert.IsTrue(session.TryIssue(Directives.CommandKind, "0", "x", 0, 0, "FocusFire", out string why), why);
            int aim = RunUntil(focused, 4000, e => e.Kind == BattleEventKind.Aim && e.ActorId == "a", focused.Events.Count);
            Assert.GreaterOrEqual(aim, 0);
            Assert.AreEqual("x", focused.Events[aim].TargetId, "he keeps to your Focus Fire");
        }

        [Test]
        public void AroundTheCornerTakesASnaredEnemyForTriple()
        {
            // The hooks (29 Oct 2026): a snared enemy shot from behind takes triple, not double.
            var sim = Fight("corner.snare", 4, Side(U("w", UnitClass.Warden), U("t", UnitClass.Trapper), Wearing("a", UnitClass.Marksman, ClassBend.AroundTheCorner)),
                ThreeWardens());
            int at = RunUntil(sim, 9000, e => e.Kind == BattleEventKind.Hit && e.ActorId == "a" && e.Detail == ClassSpells.AimedPinDetail);
            Assert.GreaterOrEqual(at, 0, "he never shot a snared enemy");
            BattleEvent cash = sim.Events[at - 1];
            Assert.AreEqual(BattleEventKind.Cash, cash.Kind);
            Assert.AreEqual(ClassSpells.PinDetail, cash.Detail);
            Assert.AreEqual(cash.Amount * 3, sim.Events[at].Amount * 2, "the shot's own weight and twice more on top");
        }

        [Test]
        public void ExecutionCorridorHitsAgainFromBehindWhenHerStrikeCashesAnOpening()
        {
            var sim = Fight("corridor", 4, Side(U("a", UnitClass.Marksman), Wearing("k", UnitClass.Skirmisher, ClassBend.ExecutionCorridor)),
                ThreeMedics());
            int at = RunUntil(sim, 9000, e => Is(e, BattleEventKind.Portal, ClassBend.ExecutionCorridor));
            Assert.GreaterOrEqual(at, 0, "her strike never cashed an Opening");
            BattleEvent portal = sim.Events[at];
            Assert.AreEqual(ClassSpells.StrikeOpenDetail, sim.Events.Take(at).Last(e => e.Kind == BattleEventKind.Hit && e.ActorId == "k").Detail,
                "on the strike that cashed an Opening");
            BattleEvent behind = sim.Events[at + 1];
            Assert.AreEqual(BattleEventKind.Hit, behind.Kind);
            Assert.AreEqual(ClassSpells.CorridorDetail, behind.Detail);
            Assert.AreEqual(portal.TargetId, behind.TargetId);
            Assert.AreEqual(sim.FindById("k").Power * ClassSpells.StrikePercent / 100 * ClassSpells.CorridorPercent / 100, behind.Amount);
        }

        [Test]
        public void WrongDoorTakesBlowsOnTheAlliesBesideHisWardOntoHisShield()
        {
            // He Guards the most hurt of his two Skirmishers; their Wardens each go for whoever stands across (side rules turn
            // the lanes on).
            var sim = Fight("wrongdoor", 4, Side(Wearing("w", UnitClass.Warden, ClassBend.WrongDoor), U("s1", UnitClass.Skirmisher, Row.Front),
                U("s2", UnitClass.Skirmisher, Row.Front)), Side(U("x1", UnitClass.Warden), U("x2", UnitClass.Warden), U("x3", UnitClass.Warden)),
                new SideRules(1, 0, false));
            int at = RunUntil(sim, 6000, e => Is(e, BattleEventKind.Portal, ClassBend.WrongDoor));
            Assert.GreaterOrEqual(at, 0, "no blow came through his portal");
            BattleEvent portal = sim.Events[at];
            Assert.AreEqual("w", portal.ActorId);
            StringAssert.StartsWith("s", portal.TargetId, "a blow meant for an ally");
            Assert.AreNotEqual(sim.FindById("w").GuardTargetId, portal.TargetId, "not his ward, the ally beside them");
            Assert.AreEqual(BattleEventKind.Guard, sim.Events[at + 1].Kind);
            Assert.AreEqual(BattleEventKind.Hit, sim.Events[at + 2].Kind);
            Assert.AreEqual("w", sim.Events[at + 2].TargetId, "it lands on his shield");

            // The hooks (29 Oct 2026): the door's edge leaves whoever struck through it bleeding.
            string striker = sim.Events[at + 2].ActorId;
            Assert.IsTrue(sim.Events.Skip(at + 3).Any(e => e.Kind == BattleEventKind.Mark && e.Detail == ClassSpells.BleedMark &&
                e.ActorId == "w" && e.TargetId == striker && e.Tick == portal.Tick), striker + " is left bleeding");
        }

        [Test]
        public void DoorwayLetsShootersSpringHerTrap()
        {
            var sim = Fight("doorway", 4, Side(U("w", UnitClass.Warden), Wearing("t", UnitClass.Trapper, ClassBend.Doorway)),
                Side(U("x1", UnitClass.Marksman), U("x2", UnitClass.Marksman)));
            int at = RunUntil(sim, 6000, e => Is(e, BattleEventKind.Portal, ClassBend.Doorway));
            Assert.GreaterOrEqual(at, 0, "no shooter fell into her trap");
            Assert.AreEqual(BattleEventKind.TrapSprung, sim.Events[at - 1].Kind);
            Assert.IsTrue(sim.FindById(sim.Events[at].TargetId).Stats.Ranged, "a shooter sprang it");

            // Without the combo only melee spring it, and theirs are all shooters.
            var plain = Fight("doorway", 4, Side(U("w", UnitClass.Warden), U("t", UnitClass.Trapper)),
                Side(U("x1", UnitClass.Marksman), U("x2", UnitClass.Marksman)));
            Assert.Less(RunUntil(plain, 6000, e => e.Kind == BattleEventKind.TrapSprung), 0);
        }

        [Test]
        public void EvacPullsAFallingFighterOutAliveOnceAFight()
        {
            // A frail Warden in front of her: their hard hitting Wardens can only reach him, faster than she heals.
            var frail = new UnitSpec("w", UnitClass.Warden, Row.Front, UnitRole.Fighter, 100, 20);
            var sim = Fight("evac", 4, Side(frail, Wearing("h", UnitClass.Medic, ClassBend.Evac)),
                Side(new UnitSpec("x1", UnitClass.Warden, Row.Front, UnitRole.Fighter, 300, 100),
                    new UnitSpec("x2", UnitClass.Warden, Row.Front, UnitRole.Fighter, 300, 100),
                    new UnitSpec("x3", UnitClass.Warden, Row.Front, UnitRole.Fighter, 300, 100)));
            int at = RunUntil(sim, 9000, e => Is(e, BattleEventKind.Portal, ClassBend.Evac));
            Assert.GreaterOrEqual(at, 0, "nobody of ours was about to fall");
            BattleEvent portal = sim.Events[at];
            Assert.AreEqual("w", portal.TargetId);
            Assert.AreEqual(BattleEventKind.Hit, sim.Events[at - 1].Kind, "right after the blow that would have killed him");

            // The hooks (29 Oct 2026): whoever struck that blow is left with her Opening.
            BattleEvent opened = sim.Events[at + 1];
            Assert.AreEqual(BattleEventKind.Mark, opened.Kind);
            Assert.AreEqual(ClassSpells.OpeningMark, opened.Detail);
            Assert.AreEqual("h", opened.ActorId);
            Assert.AreEqual(sim.Events[at - 1].ActorId, opened.TargetId);
            BattleUnit w = sim.FindById("w");
            Assert.IsTrue(w.Alive);
            Assert.GreaterOrEqual(w.Hp, w.MaxHp * ClassSpells.EvacHealthPermille / 1000);
            Assert.Greater(w.EvacTicks, 0, "out of reach");
            int hitsBefore = sim.Events.Count(e => e.Kind == BattleEventKind.Hit && e.TargetId == "w");
            while (!sim.IsFinished && w.EvacTicks > 0) sim.Tick();
            Assert.AreEqual(hitsBefore, sim.Events.Count(e => e.Kind == BattleEventKind.Hit && e.TargetId == "w"), "nothing reached him meanwhile");
            Assert.IsTrue(sim.FindById("h").EvacSpent, "once a fight");
        }

        // ------------------------------------------------------------ Time Stop

        [Test]
        public void StoppedBulletsHangsTwoShotsThenFiresAllThree()
        {
            var sim = Fight("stopped", 4, Side(Marksman(ClassBend.StoppedBullets)), ThreeMedics());

            // A bullet that hangs frozen in its mark freezes the mark and leaves its Opening (rethought 29 Oct 2026).
            int hang = RunUntil(sim, 9000, e => Is(e, BattleEventKind.Freeze, ClassBend.StoppedBullets));
            Assert.GreaterOrEqual(hang, 0, "no bullet ever hung");
            BattleEvent opened = sim.Events[hang + 1];
            Assert.AreEqual(BattleEventKind.Mark, opened.Kind);
            Assert.AreEqual(ClassSpells.OpeningMark, opened.Detail);
            Assert.AreEqual(sim.Events[hang].TargetId, opened.TargetId);
            Assert.AreEqual(ClassSpells.StoppedFreezeTicks - 1, sim.FindById(opened.TargetId).PinTicks, "frozen with it (one tick has run)");

            // The third fires all three at once, and time stopped around it, so it never misses.
            int at = RunUntil(sim, 9000, e => e.Kind == BattleEventKind.Hit && e.ActorId == "a" && e.Detail.StartsWith(ClassSpells.AimedDetail));
            Assert.GreaterOrEqual(at, 0, "no shot ever fired");
            List<int> frozen = sim.Events.Take(at).Where(e => Is(e, BattleEventKind.Freeze, ClassBend.StoppedBullets)).Select(e => e.Amount).ToList();
            CollectionAssert.AreEqual(new[] { 1, 2 }, frozen, "two shots hang frozen first, and the third never misses");
            int lightest = sim.FindById("a").Power * 85 / 100 * ClassSpells.AimedShotPercent / 100;
            Assert.GreaterOrEqual(sim.Events[at].Amount, lightest * ClassSpells.StoppedShots, "all three at once");
        }

        [Test]
        public void FrozenFlurryKeepsEveryHitUntilItsMarkThaws()
        {
            var sim = Fight("flurry", 4, Side(Wearing("k", UnitClass.Skirmisher, ClassBend.FrozenFlurry), U("a", UnitClass.Marksman)), ThreeMedics());
            int at = RunUntil(sim, 9000, e => Is(e, BattleEventKind.Freeze, ClassBend.FrozenFlurry));
            Assert.GreaterOrEqual(at, 0, "her strike never froze anyone");
            BattleUnit mark = sim.FindById(sim.Events[at].TargetId);
            Assert.Greater(mark.PinTicks, 0, "frozen");
            int kept = 0;
            while (!sim.IsFinished && mark.FlurryTicks > 0)
            {
                kept = Math.Max(kept, mark.FlurryStore);
                sim.Tick();
            }
            Assert.Greater(kept, 0, "hits on it waited");
            List<BattleEvent> landed = sim.Events.Skip(at + 1).Where(e => e.Kind == BattleEventKind.Hit && e.TargetId == mark.Id).ToList();
            Assert.AreEqual(1, landed.Count, "nothing landed until the thaw, then all of it at once");
            Assert.AreEqual(ClassSpells.FlurryDetail, landed[0].Detail);
            Assert.GreaterOrEqual(landed[0].Amount, kept);
        }

        [Test]
        public void FrozenFlurrysThawCashesAnOpeningForTwiceTheKeptHit()
        {
            // The hooks (29 Oct 2026): an Aimed Shot on her frozen mark leaves an Opening, and the thaw cashes it.
            var sim = Fight("flurry.open", 4, Side(Wearing("k", UnitClass.Skirmisher, ClassBend.FrozenFlurry), U("a", UnitClass.Marksman)), ThreeMedics());
            while (!sim.IsFinished && sim.CurrentTick < 9000) sim.Tick();
            int cashed = 0;
            for (int i = 1; i < sim.Events.Count; i++)
            {
                BattleEvent thaw = sim.Events[i], cash = sim.Events[i - 1];
                if (thaw.Kind != BattleEventKind.Hit || thaw.Detail != ClassSpells.FlurryDetail) continue;
                if (cash.Kind != BattleEventKind.Cash || cash.Detail != ClassSpells.OpeningMark || cash.Tick != thaw.Tick) continue;
                Assert.AreEqual("k", cash.ActorId);
                Assert.AreEqual(thaw.TargetId, cash.TargetId);
                Assert.AreEqual(2 * cash.Amount, thaw.Amount, "the whole kept hit, twice");
                cashed++;
            }
            Assert.Greater(cashed, 0, "no thaw found an Opening");
        }

        [Test]
        public void StillWallFreezesWhoeverHitsHisShield()
        {
            var sim = Fight("stillwall", 4, Side(Wearing("w", UnitClass.Warden, ClassBend.StillWall)),
                Side(U("x1", UnitClass.Warden), U("x2", UnitClass.Warden)));
            int at = RunUntil(sim, 6000, e => Is(e, BattleEventKind.Freeze, ClassBend.StillWall));
            Assert.GreaterOrEqual(at, 0, "nobody hit his shield");
            BattleEvent freeze = sim.Events[at], hit = sim.Events[at - 1];
            Assert.AreEqual(BattleEventKind.Hit, hit.Kind);
            Assert.AreEqual("w", hit.TargetId);
            Assert.AreEqual(hit.ActorId, freeze.TargetId, "whoever struck his shield");
            Assert.Greater(sim.FindById(freeze.TargetId).PinTicks, 0);

            // The hooks (29 Oct 2026): and is left with his Opening.
            BattleEvent opened = sim.Events[at + 1];
            Assert.AreEqual(BattleEventKind.Mark, opened.Kind);
            Assert.AreEqual(ClassSpells.OpeningMark, opened.Detail);
            Assert.AreEqual("w", opened.ActorId);
            Assert.AreEqual(freeze.TargetId, opened.TargetId);
            int next = RunUntil(sim, 9000, e => Is(e, BattleEventKind.Freeze, ClassBend.StillWall), at + 1);
            Assert.GreaterOrEqual(next, 0, "it comes round again");
            Assert.GreaterOrEqual(sim.Events[next].Tick - freeze.Tick, ClassSpells.StillWallEveryTicks, "once every 6 s at most");
        }

        [Test]
        public void TimeSnareSlowsEveryBigMoveOfWhoeverSpringsHerTrap()
        {
            // For 6 s after the hold every heavy swing he starts winds up twice as long (rethought 29 Oct 2026).
            var sim = Fight("timesnare", 4, Side(U("w", UnitClass.Warden), Wearing("t", UnitClass.Trapper, ClassBend.TimeSnare)),
                Side(U("x", UnitClass.Warden)), new SideRules(0, 0, false, HeavyRules.EveryTicks));
            int at = RunUntil(sim, 6000, e => Is(e, BattleEventKind.Freeze, ClassBend.TimeSnare));
            Assert.GreaterOrEqual(at, 0, "her trap never sprang");
            BattleEvent slow = sim.Events[at];
            Assert.AreEqual("x", slow.TargetId);
            Assert.AreEqual(ClassSpells.SnareTicks + ClassSpells.TimeSnareTicks, slow.Amount, "the hold, then 6 s more");
            Assert.AreEqual(slow.Amount - 1, sim.FindById("x").SlowTicks, "one tick has run");

            // The hooks (29 Oct 2026): the snare's wound bleeds the whole slow, not the usual 5 s.
            BattleEvent wound = sim.Events.Skip(at + 1).First(e => e.Kind == BattleEventKind.Mark && e.Detail == ClassSpells.BleedMark && e.TargetId == "x");
            Assert.AreEqual(slow.Tick, wound.Tick);
            Assert.AreEqual(slow.Amount, wound.Amount, "it bleeds as long as it is slowed");
            Assert.Greater(slow.Amount, BleedRules.Ticks);
            while (!sim.IsFinished && sim.CurrentTick < slow.Tick + slow.Amount + 400) sim.Tick();

            // Her trap arms again and he springs it again, so every slow counts.
            List<BattleEvent> slows = sim.Events.Where(e => Is(e, BattleEventKind.Freeze, ClassBend.TimeSnare) && e.TargetId == "x").ToList();
            Func<BattleEvent, bool> inside = w => slows.Any(s => w.Tick >= s.Tick && w.Tick < s.Tick + s.Amount);
            List<BattleEvent> windups = sim.Events.Skip(at + 1).Where(e => e.Kind == BattleEventKind.Windup && e.ActorId == "x").ToList();
            Assert.IsNotEmpty(windups.Where(inside), "he swung heavy inside the slow");
            Assert.IsNotEmpty(windups.Where(w => !inside(w)), "and outside it");
            foreach (BattleEvent e in windups.Where(inside))
                Assert.AreEqual(HeavyRules.WindupTicks * ClassSpells.TimeSnarePercent / 100, e.Amount, "every one twice as slow");
            foreach (BattleEvent e in windups.Where(w => !inside(w)))
                Assert.AreEqual(HeavyRules.WindupTicks, e.Amount, "and back to normal after");
        }

        [Test]
        public void TimeSnareHalvesTheSpeedOfWhoeverSpringsHerTrap()
        {
            // Once the hold lets go he moves at half speed: his turn comes closer on every other tick only.
            var sim = Fight("halfspeed", 4, Side(U("w", UnitClass.Warden), Wearing("t", UnitClass.Trapper, ClassBend.TimeSnare)),
                Side(U("x", UnitClass.Warden)));
            int at = RunUntil(sim, 6000, e => Is(e, BattleEventKind.Freeze, ClassBend.TimeSnare));
            Assert.GreaterOrEqual(at, 0, "her trap never sprang");
            BattleUnit x = sim.FindById("x");
            while (!sim.IsFinished && x.SnareTicks > 0) sim.Tick();
            int stayed = 0, closer = 0;
            for (int i = 0; i < 40 && !sim.IsFinished && x.SlowTicks > 1; i++)
            {
                int before = x.Cooldown;
                sim.Tick();
                if (x.Cooldown == before) stayed++;
                else if (x.Cooldown == before - 1) closer++;
            }
            Assert.Greater(closer, 10);
            Assert.LessOrEqual(Math.Abs(stayed - closer), 2, "half the ticks bring his turn no closer");
        }

        [Test]
        public void RewindGivesTheMostHurtAllyBackTheirLastThreeSeconds()
        {
            var sim = Fight("rewind", 4, Side(U("w", UnitClass.Warden), Wearing("h", UnitClass.Medic, ClassBend.Rewind)),
                Side(U("x1", UnitClass.Marksman), U("x2", UnitClass.Marksman)));
            int at = RunUntil(sim, 9000, e => Is(e, BattleEventKind.Freeze, ClassBend.Rewind));
            Assert.GreaterOrEqual(at, 0, "she never rewound anyone");
            BattleEvent freeze = sim.Events[at];
            Assert.AreEqual(0, (freeze.Tick + 1) % ClassSpells.RewindEveryTicks, "on her clock");
            BattleEvent heal = sim.Events[at + 1];
            Assert.AreEqual(BattleEventKind.Heal, heal.Kind);
            Assert.AreEqual(ClassSpells.RewindDetail, heal.Detail);
            Assert.AreEqual(freeze.TargetId, heal.TargetId);
            Assert.AreEqual(freeze.Amount, heal.Amount);
            long from = (freeze.Tick / SimClock.TicksPerSecond - (ClassSpells.RewindSeconds - 1)) * SimClock.TicksPerSecond;
            int lost = sim.Events.Take(at).Where(e => e.Kind == BattleEventKind.Hit && e.TargetId == freeze.TargetId && e.Tick >= from).Sum(e => e.Amount);
            Assert.Greater(freeze.Amount, 0);
            Assert.LessOrEqual(freeze.Amount, lost, "no more than they lost in those seconds");

            // The hooks (29 Oct 2026): whoever last struck that ally is rewound mid swing and left with her Opening.
            string striker = sim.Events.Take(at).Last(e => e.Kind == BattleEventKind.Hit && e.TargetId == freeze.TargetId && e.ActorId.StartsWith("x")).ActorId;
            BattleEvent opened = sim.Events[at + 2];
            Assert.AreEqual(BattleEventKind.Mark, opened.Kind);
            Assert.AreEqual(ClassSpells.OpeningMark, opened.Detail);
            Assert.AreEqual("h", opened.ActorId);
            Assert.AreEqual(striker, opened.TargetId);
        }

        // ------------------------------------------------------------ the road

        [Test]
        public void TheButchersRoadFightOffersThreeCombosForTheRun()
        {
            var easy = new CampaignTuning { EnemyScaleTier1 = 40 };
            for (ulong seed = 5100; seed < 5200; seed++)
            {
                Campaign c = Ready(seed, easy);
                c.StartRun();
                int index = c.Offers.ToList().FindIndex(o => o.Kind != EncounterKind.Ambush && o.Node != null && o.Node.Pal == Pal.Butcher);
                if (index < 0) continue;
                RoadNode node = c.Offers[index].Node;
                Assert.IsTrue(c.PalPickOffered(node), "the road says the Butcher will offer");
                int before = c.Materials;
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(index), new CampaignPilot(PilotStyle.Reference, seed));
                if (!r.Won) continue;

                // Three of his combos, for classes in the squad, not his three button favours.
                // From rules 19 the tribe's gear is on the same card (fix 5, PalRoadTests); before it a Pal fight drops none.
                if (c.PalRoadRun) Assert.IsNotNull(r.SpoilsOffered, "the gear beside his combos");
                else Assert.IsNull(r.SpoilsOffered, "a Pal fight drops no gear");
                Assert.AreEqual(Pal.Butcher, c.FavourOffer.Pal);
                IReadOnlyList<Favour> offered = c.FavourOffer.Choices;
                Assert.AreEqual(FavourRules.Offered, offered.Count);
                Assert.AreEqual(offered.Count, offered.Distinct().Count());
                foreach (Favour f in offered)
                {
                    Assert.IsTrue(FavourRules.IsCombo(f), f + " is a combo");
                    Assert.IsTrue(c.Reserve.Any(x => x.Class == ClassSpells.ClassOf(FavourRules.ComboBend(f))), f + " fits the squad");
                    Assert.AreEqual(0, c.FavourRank(f));
                }

                // The same campaign plays the same node to the same three: they come from the node's own dice.
                Campaign again = Ready(seed, easy);
                again.StartRun();
                CampaignPilot.PlayFight(again, again.StartEncounter(index), new CampaignPilot(PilotStyle.Reference, seed));
                CollectionAssert.AreEqual(offered, again.FavourOffer.Choices);

                string saved = CampaignSave.Write(c);
                Assert.AreEqual(saved, CampaignSave.Write(CampaignSave.Read(saved, easy)), "the offer saves");

                // Taking one costs the fight's materials and puts it in its fighter's one slot, with no stat.
                Favour taken = offered[0];
                ClassBend bend = FavourRules.ComboBend(taken);
                Assert.IsTrue(c.TryTakeFavour(0, out string why), why);
                Assert.AreEqual(before, c.Materials);
                Assert.AreEqual(1, c.FavourRank(taken));
                Assert.IsEmpty(c.Favours, "a combo is no button favour");
                CampaignGear piece = c.Gear.Single(g => g.Bend == bend);
                Assert.AreEqual(Campaign.ComboGearId(bend), piece.Id);
                Assert.AreEqual(0, piece.Bonus);
                Assert.IsTrue(piece.Fits(ClassSpells.ClassOf(bend)));
                CampaignFighter wearer = c.Reserve.First(f => f.Class == ClassSpells.ClassOf(bend));
                Assert.AreEqual(piece.Id, wearer.GearId, "the first of its class wears it at once");

                // The next fight carries it, and a save mid fight plays back the same.
                var picker = new CampaignPilot(PilotStyle.Reference, seed);
                CampaignFight next = c.StartEncounter(picker.PickOffer(c));
                Assert.AreEqual(bend, next.Setup.Side(0).Single(s => s.Id == wearer.Id).Bend);
                Campaign read = CampaignSave.Read(CampaignSave.Write(c), easy);
                Assert.IsEmpty(read.LoadNote, "the fight with the combo plays back the same");
                Assert.AreEqual(piece.Id, read.Reserve.First(f => f.Id == wearer.Id).GearId);
                CampaignPilot.PlayFight(c, next, new CampaignPilot(PilotStyle.Reference, seed * 31UL));

                // Play the run out, taking every combo he offers: never one the run holds at its top rank (from rules 18 one
                // it holds below that can come again, to rank up; DESIGN_LOCKS "The Butcher's theme"), and they fire in its fights.
                int combos = next.Sim.Events.Count(e => e.Kind == BattleEventKind.Pull || e.Kind == BattleEventKind.Portal || e.Kind == BattleEventKind.Freeze);
                for (int n = 1; c.Stage == CampaignStage.InRun && n < 80; n++)
                {
                    if (c.FavourOffer != null && c.FavourOffer.Pal == Pal.Butcher)
                    {
                        foreach (Favour f in c.FavourOffer.Choices) Assert.Less(c.FavourRank(f), MarkRules.MaxStacks, f + " is at its top rank already");
                        Assert.IsTrue(c.TryTakeFavour(0, out why), why);
                    }
                    CampaignFight fight = c.StartEncounter(picker.PickOffer(c));
                    CampaignPilot.PlayFight(c, fight, new CampaignPilot(PilotStyle.Reference, seed * 31UL + (ulong)n));
                    combos += fight.Sim.Events.Count(e => e.Kind == BattleEventKind.Pull || e.Kind == BattleEventKind.Portal || e.Kind == BattleEventKind.Freeze);
                }
                Assert.Greater(combos, 0, "the combos fired in the run's fights");

                // They go when the run does, with the rest of the run's gear.
                Assert.AreNotEqual(CampaignStage.InRun, c.Stage, "the run ended");
                Assert.IsFalse(c.Gear.Any(g => ClassSpells.IsCombo(g.Bend)));
                Assert.AreEqual(0, c.FavourRank(taken));
                return;
            }
            Assert.Fail("no Butcher fight was won in a hundred campaigns");
        }

        [Test]
        public void UnderRules14TheButcherStillOffersHisThreeFavours()
        {
            var old = new CampaignTuning { EnemyScaleTier1 = 40, RulesCap = Campaign.ButcherComboRules - 1 };
            for (ulong seed = 5100; seed < 5200; seed++)
            {
                Campaign c = Ready(seed, old);
                c.StartRun();
                int index = c.Offers.ToList().FindIndex(o => o.Kind != EncounterKind.Ambush && o.Node != null && o.Node.Pal == Pal.Butcher);
                if (index < 0) continue;
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(index), new CampaignPilot(PilotStyle.Reference, seed));
                if (!r.Won) continue;
                CollectionAssert.AreEqual(FavourRules.Of(Pal.Butcher), c.FavourOffer.Choices);
                return;
            }
            Assert.Fail("no Butcher fight was won in a hundred campaigns");
        }
    }
}
