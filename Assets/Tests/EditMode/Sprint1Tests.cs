using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Sim.Tests
{
    /// Sprint 1 of the design Jacob chose on 12 and 13 Oct 2026 (his go, 13 Oct): misses, earned points,
    /// Perfect Hold, nerve snaps, the tribes' doctrines and the combo hint. Jacob's rule 8 is build first,
    /// tune later, so each rule gets one quick test that proves it runs. None of these is balance.
    public sealed class Sprint1Tests
    {
        /// Why the balance tests are parked: the 13 Oct rules change every number they check, and Jacob's rule 8
        /// (12 Oct 2026) is build first, tune in the tuning weeks.
        public const string ParkedForTuning = "Parked until the tuning weeks, 23 to 29 Nov 2026 (Jacob's rule 8, 12 Oct 2026: build first, optimise later).";

        private const int Health = 400;

        private static List<UnitSpec> Clarth()
        {
            return new List<UnitSpec>
            {
                new UnitSpec("c.warden", UnitClass.Warden), new UnitSpec("c.skirm", UnitClass.Skirmisher),
                new UnitSpec("c.marks", UnitClass.Marksman), new UnitSpec("c.trap", UnitClass.Trapper),
                new UnitSpec("c.medic", UnitClass.Medic)
            };
        }

        private static List<UnitSpec> Raiders(bool chief, int count = 4)
        {
            var all = new List<UnitSpec>
            {
                new UnitSpec("r.warden", UnitClass.Warden), new UnitSpec("r.skirm", UnitClass.Skirmisher),
                new UnitSpec("r.marks", UnitClass.Marksman), new UnitSpec("r.medic", UnitClass.Medic)
            };
            List<UnitSpec> list = all.Take(count).ToList();
            if (chief) list.Add(new UnitSpec("r.chief", UnitClass.Skirmisher, Row.Back, UnitRole.Chief, 100, 100));
            return list;
        }

        private static BattleSetup Setup(string id, ulong seed, FightRules fight, TacticsProfile theirs = null,
            SideRules ours = null, SideRules theirRules = null, bool chief = false, NerveRules nerve = null,
            int theirCount = 4, int health = Health)
        {
            return new BattleSetup(id, seed, Clarth(), Raiders(chief, theirCount), new TacticsProfile(),
                theirs ?? new TacticsProfile(), BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints,
                nerve ?? NerveRules.On(), SliceRules.RefillTicks, health, ours, theirRules, fight);
        }

        private static bool Issue(SimSession session, string directive, string target = "")
        {
            return session.TryIssue(Directives.CommandKind, "0", target, 0, 0, directive, out _);
        }

        private static List<BattleEvent> Events(BattleSimulation sim, BattleEventKind kind)
        {
            return sim.Events.Where(e => e.Kind == kind).ToList();
        }

        private static BattleEvent PlayUntil(SimSession session, BattleSimulation sim, BattleEventKind kind)
        {
            int seen = sim.Events.Count;
            while (true)
            {
                for (; seen < sim.Events.Count; seen++)
                {
                    if (sim.Events[seen].Kind == kind) return sim.Events[seen];
                }
                if (sim.IsFinished) break;
                session.Tick();
            }
            Assert.Fail("The fight ended before a " + kind + " event.");
            return default;
        }

        [Test]
        public void BlowsCanMissAndTheFightStillReplaysExactly()
        {
            var misses = new FightRules(true, false, false, false);
            var a = new BattleSimulation(Setup("miss", 11, misses));
            a.RunUntilFinished();
            List<BattleEvent> missed = Events(a, BattleEventKind.Miss);
            Assert.Greater(missed.Count, 0, "some blows miss");
            foreach (BattleEvent m in missed)
                Assert.That(m.Amount, Is.InRange(FightRules.MinHitPercent, FightRules.MaxHitPercent), "the chance it had");

            var b = new BattleSimulation(Setup("miss", 11, misses));
            b.RunUntilFinished();
            Assert.AreEqual(StateChecksum.Compute(a.OrderedStateSnapshot), StateChecksum.Compute(b.OrderedStateSnapshot),
                "the same seed plays the same fight");

            var off = new BattleSimulation(Setup("miss", 11, FightRules.Off));
            off.RunUntilFinished();
            Assert.AreEqual(0, Events(off, BattleEventKind.Miss).Count, "without the rule nothing misses");
        }

        [Test]
        public void AKillEarnsAPointAndPointsNeverPassTheCap()
        {
            var sim = new BattleSimulation(Setup("earn", 12, new FightRules(false, true, false, false), nerve: NerveRules.Off,
                health: BattleSetup.NormalHealthPercent));
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "Unleash"));
            Assert.IsTrue(Issue(session, "Hold"));
            Assert.IsTrue(Issue(session, "FallBack"));
            Assert.AreEqual(1, sim.DirectivePoints(0));
            while (!sim.IsFinished)
            {
                session.Tick();
                Assert.LessOrEqual(sim.DirectivePoints(0), BattleSetup.DefaultDirectivePoints);
            }
            Assert.IsTrue(Events(sim, BattleEventKind.Earn).Any(e => e.ActorSide == 0 && e.Detail == FightRules.KillReason),
                "a kill earns a point");
        }

        [Test]
        public void AHoldAtTheLastMomentParriesTheBigHitAndUnleashRipostes()
        {
            var sim = new BattleSimulation(Setup("parry", 13, new FightRules(false, false, true, false),
                ours: new SideRules(0, 0, true), theirRules: new SideRules(2, 60, false), chief: true));
            var session = new SimSession(sim);
            PlayUntil(session, sim, BattleEventKind.Windup);
            while (sim.WindupTicks(1) > 10) session.Tick();
            Assert.IsTrue(Issue(session, "Hold"));
            BattleEvent release = PlayUntil(session, sim, BattleEventKind.Release);

            Assert.AreEqual(FightRules.Parried, release.Detail);
            BattleEvent parry = Events(sim, BattleEventKind.Parry).Single();
            Assert.AreEqual(0, parry.ActorSide);
            Assert.AreEqual("r.chief", parry.TargetId);
            Assert.AreEqual(0, sim.Events.Count(e => e.Kind == BattleEventKind.Hit && e.Detail == Moves.EclipseDetail),
                "nothing lands");
            Assert.AreEqual(0, sim.HoldTicksLeft(0), "the parry ends the Hold, so it costs no more nerve");

            ComboHint hint = sim.NextCombo(0);
            Assert.AreEqual(FightRules.Riposte, hint.Combo);
            Assert.AreEqual(Directive.Unleash, hint.Next);
            Assert.IsTrue(Issue(session, "Unleash"));
            BattleEvent riposte = Events(sim, BattleEventKind.Combo).Single(e => e.Detail == FightRules.Riposte);
            Assert.AreEqual("r.chief", riposte.TargetId);

            // An early Hold only soaks, as before.
            var early = new BattleSimulation(Setup("parry", 13, new FightRules(false, false, true, false),
                ours: new SideRules(0, 0, true), theirRules: new SideRules(2, 60, false), chief: true));
            var earlySession = new SimSession(early);
            PlayUntil(earlySession, early, BattleEventKind.Windup);
            Assert.IsTrue(Issue(earlySession, "Hold"));
            Assert.AreEqual(Moves.Soaked, PlayUntil(earlySession, early, BattleEventKind.Release).Detail);
        }

        [Test]
        public void LosingNerveSnapsAFighterWhoPanicsOrRises()
        {
            var sim = new BattleSimulation(Setup("snap", 14, new FightRules(false, false, false, true)));
            sim.RunUntilFinished();
            List<BattleEvent> snaps = Events(sim, BattleEventKind.Snap);
            Assert.Greater(snaps.Count, 0, "a side that loses a quarter of its nerve snaps");
            for (int side = 0; side < 2; side++)
                Assert.LessOrEqual(snaps.Count(e => e.ActorSide == side), FightRules.SnapsPerFight);
            foreach (BattleEvent s in snaps)
            {
                Assert.That(s.Detail, Is.EqualTo(FightRules.Panic).Or.EqualTo(FightRules.Rise));
                Assert.AreEqual(UnitRole.Fighter, sim.FindById(s.ActorId).Role, "a leader never snaps");
                if (s.Detail != FightRules.Panic) continue;
                Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Hit && e.ActorId == s.ActorId &&
                    e.Tick > s.Tick && e.Tick < s.Tick + FightRules.SnapTicks), s.ActorId + " panicked and fought on");
            }
        }

        [Test]
        public void EachTribeFightsToItsOwnDoctrine()
        {
            // Bloodridge rally when one of theirs falls.
            var blood = new TacticsProfile();
            TribeDoctrines.Apply(CampaignTribes.BloodridgeId, blood);
            var rally = new BattleSimulation(Setup("rally", 15, FightRules.Off, blood, nerve: NerveRules.Off, theirCount: 2));
            rally.RunUntilFinished();
            BattleEvent firstLoss = rally.Events.First(e => e.Kind == BattleEventKind.Kill && rally.FindById(e.TargetId).Side == 1);
            Assert.IsTrue(rally.Events.Any(e => e.Kind == BattleEventKind.Reaction && e.Detail == BattleSimulation.RallyDetail &&
                e.Tick == firstLoss.Tick && e.ActorSide == 1), "they rally the moment one falls");

            // The Clarth fall back out of an Unleash.
            var clarth = new TacticsProfile();
            TribeDoctrines.Apply(CampaignTribes.ClarthId, clarth);
            var kite = new BattleSimulation(Setup("kite", 16, FightRules.Off, clarth));
            var session = new SimSession(kite);
            session.Tick();
            Assert.IsTrue(Issue(session, "Unleash"));
            Assert.Greater(kite.FallBackTicksLeft(1), 0, "they fall back out of your Unleash");
            Assert.IsTrue(kite.Events.Any(e => e.Kind == BattleEventKind.Reaction && e.Detail == BattleSimulation.KiteDetail));

            // Bloodridge Wardens guard whoever casts.
            var guard = new BattleSimulation(Setup("guard", 17, FightRules.Off, blood, theirRules: new SideRules(2, 60, false), chief: true));
            var guardSession = new SimSession(guard);
            PlayUntil(guardSession, guard, BattleEventKind.Windup);
            Assert.AreEqual("r.warden", guard.CoveredBy(guard.FindById("r.chief"))?.Id, "their Warden covers the caster");
        }

        [Test]
        public void TheComboHintNamesTheNextOrderUntilItsWindowCloses()
        {
            var sim = new BattleSimulation(Setup("hint", 18, FightRules.Off, ours: new SideRules(0, 0, true)));
            var session = new SimSession(sim);
            Assert.IsTrue(sim.NextCombo(0).IsNone);
            Assert.IsTrue(Issue(session, "FallBack"));
            ComboHint hint = sim.NextCombo(0);
            Assert.AreEqual(Moves.Ambush, hint.Combo);
            Assert.AreEqual(Directive.Unleash, hint.Next);
            Assert.AreEqual(Moves.AmbushWindowTicks, hint.TicksLeft);
            for (int i = 0; i <= Moves.AmbushWindowTicks; i++) session.Tick();
            Assert.IsTrue(sim.NextCombo(0).IsNone, "the window closed");
        }

        [Test]
        public void CampaignFightsPlayTheNewRulesAndTheirTribesDoctrine()
        {
            Campaign c = Campaign.New(1301);
            CampaignFight tutorial = c.StartTutorial();
            FightRules rules = tutorial.Setup.Fight;
            Assert.IsTrue(rules.Misses && rules.EarnedPoints && rules.PerfectHold && rules.NerveSnaps);
            Assert.IsNotEmpty(tutorial.Sim.Doctrines(1), "the enemy fights to its tribe's doctrine");
        }

        [Test]
        public void AFightRecordedBeforeTheNewRulesStillPlaysBack()
        {
            // One fight with none of the 13 Oct rules and orders given, one with all of them, saved in a campaign.
            Campaign c = Campaign.New(1302);
            BattleSetup plain = Setup("old.rules", 12, FightRules.Off);
            var sim = new BattleSimulation(plain);
            var session = new SimSession(sim);
            for (int t = 0; t < 60; t++) session.Tick();
            Assert.IsTrue(Issue(session, "Unleash"));
            for (int t = 0; t < 200; t++) session.Tick();
            Assert.IsTrue(Issue(session, "Hold"));
            while (!sim.IsFinished) session.Tick();
            c.Archive.Add(plain, session.Log, sim);
            BattleSetup skill = Setup("new.rules", 12, FightRules.Skill);
            var skilled = new BattleSimulation(skill);
            skilled.RunUntilFinished();
            c.Archive.Add(skill, null, skilled);
            string saved = CampaignSave.Write(c);

            // Recorded by the build before 13 Oct: the plain fight plays back and replays exactly; the other
            // could not have been fought then, so it keeps its history but does not play.
            Campaign before = CampaignSave.Read(RecordedUnder(saved, "ks-battle-20261011-3"));
            Assert.AreEqual("ks-battle-20261011-3", before.Archive.Find("old.rules").RulesIdentity);
            Assert.IsTrue(FightPlayback.TryOpen(before.Archive, "old.rules", out FightPlayback p, out string why), why);
            while (p.Step()) { }
            Assert.IsTrue(p.AtEnd && p.Matched, p.Failure);
            Assert.IsTrue(before.Archive.TryReplay("old.rules", out _, out why), why);
            Assert.IsFalse(FightPlayback.TryOpen(before.Archive, "new.rules", out _, out why));
            StringAssert.Contains("different battle rules", why);

            // Any older rules still do not play back.
            Campaign older = CampaignSave.Read(RecordedUnder(saved, "ks-battle-20261011-2"));
            Assert.IsFalse(FightPlayback.TryOpen(older.Archive, "old.rules", out _, out why));
            StringAssert.Contains("different battle rules", why);
        }

        [TestCase(0)]
        [TestCase(2)]
        public void AnOrderThatEndsTheFightOnItsTickStillSavesAndPlaysBack(int ticks)
        {
            // Their nerve is low, so the Ambush (Fall Back, then Unleash) breaks it the moment it is ordered: the fight
            // ends on the tick of an order, as a Riposte can. The game's own validator found such a save could not be
            // read back (13 Oct 2026).
            BattleSetup setup = Setup("ends.on.order", 13, FightRules.Off, ours: new SideRules(0, 0, true),
                nerve: NerveRules.On(NerveRules.Full, 60));
            var sim = new BattleSimulation(setup);
            var session = new SimSession(sim);
            Assert.IsTrue(Issue(session, "FallBack"));
            for (int i = 0; i < ticks; i++) session.Tick();
            Assert.IsFalse(sim.IsFinished);
            Assert.IsTrue(Issue(session, "Unleash"));
            Assert.IsTrue(sim.IsFinished, "the Ambush broke their nerve on the spot");
            Assert.AreEqual(ticks, sim.CurrentTick);
            Campaign c = Campaign.New(1303);
            c.Archive.Add(setup, session.Log, sim);

            Campaign read = CampaignSave.Read(CampaignSave.Write(c));
            Assert.IsTrue(read.Archive.TryReplay("ends.on.order", out _, out string why), why);
            Assert.IsTrue(FightPlayback.TryOpen(read.Archive, "ends.on.order", out FightPlayback p, out why), why);
            string saved = CampaignSave.Write(read);
            for (int pass = 0; pass < 2; pass++)
            {
                if (pass > 0) p.Restart();
                Assert.AreEqual(0, p.Tick);
                Assert.AreEqual(ticks == 0, p.AtEnd);
                Assert.AreEqual(ticks == 0, p.Matched, "zero-length fights verify on open and restart");
                Assert.AreEqual(ticks == 0, p.Sim.IsFinished);
                if (ticks > 0) Assert.IsEmpty(p.Sim.Events, "longer recordings still start before their orders");
                Assert.IsTrue(p.Seek(0), p.Failure);
                if (pass == 0) while (p.Step()) { }
                else Assert.IsTrue(p.Seek(p.Length), p.Failure);
                Assert.IsTrue(p.AtEnd && p.Matched, p.Failure);
                Assert.AreEqual(session.Checksum, StateChecksum.Compute(p.Sim.OrderedStateSnapshot));
                CollectionAssert.AreEqual(sim.Events, p.Sim.Events, "all terminal orders and their history are preserved");
                Assert.IsFalse(p.Step(), "the terminal state must not advance");
                Assert.AreEqual(ticks, p.Tick);
            }
            Assert.AreEqual(saved, CampaignSave.Write(read), "playback never changes the saved campaign");
        }

        // The save with every recorded fight's battle rules identity swapped for another of the same length.
        private static string RecordedUnder(string saved, string identity)
        {
            byte[] now = System.Text.Encoding.UTF8.GetBytes(CampaignSave.Read(saved).Archive.Fights[0].RulesIdentity), then = System.Text.Encoding.UTF8.GetBytes(identity);
            Assert.AreEqual(now.Length, then.Length);
            string[] lines = saved.Split('\n');
            int swapped = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith("archive\t", System.StringComparison.Ordinal)) continue;
                byte[] bytes;
                using (var input = new System.IO.MemoryStream(System.Convert.FromBase64String(lines[i].Substring(8))))
                using (var inflate = new System.IO.Compression.DeflateStream(input, System.IO.Compression.CompressionMode.Decompress))
                using (var output = new System.IO.MemoryStream()) { inflate.CopyTo(output); bytes = output.ToArray(); }
                for (int at = 0; at <= bytes.Length - now.Length; at++)
                {
                    if (!bytes.Skip(at).Take(now.Length).SequenceEqual(now)) continue;
                    System.Array.Copy(then, 0, bytes, at, then.Length);
                    swapped++;
                    break;
                }
                using (var output = new System.IO.MemoryStream())
                {
                    using (var deflate = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionLevel.Fastest, true))
                        deflate.Write(bytes, 0, bytes.Length);
                    lines[i] = "archive\t" + System.Convert.ToBase64String(output.ToArray());
                }
            }
            Assert.AreEqual(2, swapped, "both fights' identities");
            return string.Join("\n", lines);
        }
    }
}
