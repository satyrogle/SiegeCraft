using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The Butcher's theme (Jacob, 30 Oct 2026: "Yes, build step 3"; DESIGN_LOCKS, "The Butcher's theme (builds step 3)";
    /// FightRules.Marks, campaign rules 18). One quick test per piece, only to prove each runs (Jacob, 12 Oct 2026: build
    /// first). Nothing here is tuning.
    public class ButcherThemeTests
    {
        private static readonly FightRules Marks = FightRules.Campaign.WithLayers().WithMarks();

        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, FightRules rules = null)
        {
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.Off, 0, 400, null, null, rules ?? Marks));
        }

        private static List<UnitSpec> Wearing(string id, UnitClass unitClass, ClassBend combo, int rank = 1)
        {
            return new List<UnitSpec> { new UnitSpec(id, unitClass).WithBend(combo).WithComboRank(rank) };
        }

        // Medics in their front row never fight back, so every aim runs its time. Three for a pull to drag in, one so every
        // blow lands on him.
        private static List<UnitSpec> Medics(int count)
        {
            var medics = new List<UnitSpec>();
            for (int i = 1; i <= count; i++) medics.Add(new UnitSpec("m" + i, UnitClass.Medic, Row.Front));
            return medics;
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

        [Test]
        public void EachComboLaysItsFlavoursMark()
        {
            // Black Hole Round, a pull: one stack of Crush on each enemy it drags in, and nothing bursts at one.
            var sim = Fight("crush", 4, Wearing("a", UnitClass.Marksman, ClassBend.BlackHoleRound), Medics(3));
            Assert.IsTrue(sim.MarksOn);
            int at = RunUntil(sim, 4000, e => e.Kind == BattleEventKind.Mark && e.Detail == MarkRules.Crush);
            Assert.GreaterOrEqual(at, 0, "no Crush was laid");
            BattleEvent mark = sim.Events[at];
            Assert.AreEqual("a", mark.ActorId);
            Assert.AreEqual(MarkRules.Ticks, mark.Amount);
            Status crush = sim.FindById(mark.TargetId).Statuses[StatusKind.Crush];
            Assert.AreEqual(1, crush.Stacks);
            Assert.AreEqual("a", crush.SourceId);
            Assert.IsFalse(sim.Events.Any(e => e.Kind == BattleEventKind.Burst));

            // The flavour picks the mark: a portal lays Exposed, time lays Stasis.
            Assert.AreEqual(StatusKind.Crush, MarkRules.Of(ClassBend.MercyWell));
            Assert.AreEqual(StatusKind.Exposed, MarkRules.Of(ClassBend.AroundTheCorner));
            Assert.AreEqual(StatusKind.Stasis, MarkRules.Of(ClassBend.Rewind));

            // Below rules 18 the same fight lays none.
            var old = Fight("crush", 4, Wearing("a", UnitClass.Marksman, ClassBend.BlackHoleRound), Medics(3), FightRules.Campaign.WithLayers());
            Assert.IsFalse(old.MarksOn);
            Assert.AreEqual(-1, RunUntil(old, 4000, e => e.Kind == BattleEventKind.Mark && e.Detail == MarkRules.Crush));
        }

        [Test]
        public void ACrushBurstsAtThreeStacks()
        {
            // At rank 3 one hole lays all three: Crush bursts at once for the Marksman's power, and pins.
            var sim = Fight("crush3", 4, Wearing("a", UnitClass.Marksman, ClassBend.BlackHoleRound, 3), Medics(3));
            int at = RunUntil(sim, 4000, e => e.Kind == BattleEventKind.Burst);
            Assert.GreaterOrEqual(at, 0, "nothing burst");
            BattleEvent burst = sim.Events[at];
            Assert.AreEqual(MarkRules.Crush, burst.Detail);
            Assert.AreEqual(sim.FindById("a").Power, burst.Amount);
            BattleEvent hit = sim.Events.Skip(at + 1).First(e => e.Kind == BattleEventKind.Hit && e.TargetId == burst.TargetId);
            Assert.AreEqual(MarkRules.CrushedDetail, hit.Detail);
            Assert.AreEqual(burst.Tick, hit.Tick);
            BattleUnit victim = sim.FindById(burst.TargetId);
            Assert.AreEqual(0, victim.Statuses[StatusKind.Crush].Stacks, "it clears");
            if (victim.Alive) Assert.Greater(victim.PinTicks, 0, "and he is pinned");
        }

        [Test]
        public void AnExposedBurstLeavesHimWideOpenToTheNextBlow()
        {
            // Around the Corner at rank 3: the healer his shot finds bursts Exposed, and the next blow on him from this side
            // spends it, its crit doubled.
            var sim = Fight("exposed3", 4, Wearing("a", UnitClass.Marksman, ClassBend.AroundTheCorner, 3), Medics(1));
            int at = RunUntil(sim, 4000, e => e.Kind == BattleEventKind.Burst);
            Assert.GreaterOrEqual(at, 0, "nothing burst");
            BattleEvent burst = sim.Events[at];
            Assert.AreEqual(MarkRules.Exposed, burst.Detail);
            Assert.AreEqual(MarkRules.WideOpenTicks, burst.Amount);
            BattleUnit victim = sim.FindById(burst.TargetId);
            Assert.IsTrue(victim.Statuses[StatusKind.WideOpen].On);
            int cash = RunUntil(sim, 8000, e => e.Kind == BattleEventKind.Cash && e.Detail == MarkRules.WideOpenDetail, at + 1);
            Assert.GreaterOrEqual(cash, 0, "no blow spent it");
            Assert.AreEqual(burst.TargetId, sim.Events[cash].TargetId);
            Assert.Greater(sim.Events[cash].Amount, 0, "the crit doubled the blow");
            Assert.IsFalse(victim.Statuses[StatusKind.WideOpen].On, "spent");
        }

        [Test]
        public void AStasisBurstFreezesHim()
        {
            // Stopped Bullets at rank 3: the first bullet that hangs bursts Stasis on its mark, who freezes.
            var sim = Fight("stasis3", 4, Wearing("a", UnitClass.Marksman, ClassBend.StoppedBullets, 3), Medics(1));
            int at = RunUntil(sim, 4000, e => e.Kind == BattleEventKind.Burst);
            Assert.GreaterOrEqual(at, 0, "nothing burst");
            BattleEvent burst = sim.Events[at];
            Assert.AreEqual(MarkRules.Stasis, burst.Detail);
            Assert.AreEqual(MarkRules.StasisFreezeTicks, burst.Amount);
            BattleUnit victim = sim.FindById(burst.TargetId);
            Assert.AreEqual(0, victim.Statuses[StatusKind.Stasis].Stacks, "it clears");
            Assert.Greater(victim.PinTicks, 0, "frozen");
        }

        [Test]
        public void ARankedComboRidesIntoTheFightSavesAndRanksUpWhenTaken()
        {
            var easy = new CampaignTuning { EnemyScaleTier1 = 40 };
            Campaign c = Campaign.New(5100, easy);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, 5100, 10), "the tutorial was never won");
            c.StartRun();

            // The pull combo of the squad's first fighter's class, already at rank 2, and the Butcher offering it again, as a
            // save would hold them.
            UnitClass cls = c.Reserve[0].Class;
            ClassBend bend = Enumerable.Range((int)ClassBend.BlackHoleRound, 15).Select(i => (ClassBend)i)
                .First(b => ClassSpells.IsFor(b, cls) && ClassSpells.AugmentOf(b) == Augment.GravityWell);
            Favour favour = FavourRules.Combos(Pal.Butcher).First(f => FavourRules.ComboBend(f) == bend);
            var lines = CampaignSave.Write(c).Split('\n').ToList();
            int at = lines.FindLastIndex(l => l.StartsWith("fighter\t"));
            lines.Insert(at + 1, string.Join("\t", "piece", Campaign.ComboGearId(bend), CampaignTribes.ClarthId, "0", "0", "0",
                ClassSpells.Name(bend), ((int)cls).ToString(), ((int)bend).ToString(), "2"));
            lines.Insert(at + 2, string.Join("\t", "favouroffer", ((int)Pal.Butcher).ToString(), "0", ((int)favour).ToString()));
            Campaign r = CampaignSave.Read(string.Join("\n", lines), easy);
            Assert.IsEmpty(r.LoadNote);
            Assert.AreEqual(2, r.FavourRank(favour));
            Assert.AreNotEqual(0, r.HeldPals() & (1 << (int)Pal.Butcher), "the run holds a Butcher pick");

            // His offer weights: the held one 2 while it can rank up, another pull combo 1 and one more for the pull held,
            // a portal combo 1.
            Assert.AreEqual(Campaign.ComboRankUpWeight, r.ComboOfferWeight(bend));
            Assert.AreEqual(2, r.ComboOfferWeight(ClassBend.Collapse == bend ? ClassBend.EventHorizon : ClassBend.Collapse));
            Assert.AreEqual(1, r.ComboOfferWeight(ClassBend.AroundTheCorner));

            // Taking it again ranks it up, to the top, and then he never offers it again.
            Assert.IsTrue(r.TryTakeFavour(0, out string why), why);
            Assert.AreEqual(MarkRules.MaxStacks, r.FavourRank(favour));
            Assert.AreEqual(0, r.ComboOfferWeight(bend));
            Assert.AreEqual(1, r.Gear.Count(g => g.Bend == bend), "still one piece");

            // It rides into the next fight on its wearer, and the fight plays the marks.
            CampaignFighter wearer = r.Reserve.First(f => f.Class == cls);
            Assert.IsTrue(r.TryEquip(wearer.Id, Campaign.ComboGearId(bend), out why), why);
            CampaignFight fight = r.StartEncounter(new CampaignPilot(PilotStyle.Reference, 5100).PickOffer(r));
            UnitSpec spec = fight.Setup.Side(0).SingleOrDefault(s => s.Id == wearer.Id);
            Assert.IsNotNull(spec, "the wearer fights");
            Assert.AreEqual(bend, spec.Bend);
            Assert.AreEqual(MarkRules.MaxStacks, spec.ComboRank);
            Assert.IsTrue(fight.Sim.MarksOn);

            // And a save mid fight keeps the rank and plays back the same.
            string saved = CampaignSave.Write(r);
            StringAssert.Contains("\t" + (int)bend + "\t" + MarkRules.MaxStacks + "\n", saved);
            Campaign back = CampaignSave.Read(saved, easy);
            Assert.IsEmpty(back.LoadNote, "the fight plays back the same");
            Assert.AreEqual(saved, CampaignSave.Write(back));
        }

        [Test]
        public void TheRoadsPalsLeanTowardWhatTheRunHolds()
        {
            int butcher = 1 << (int)Pal.Butcher, glutton = 1 << (int)Pal.Glutton;
            foreach (Pal p in FavourRules.Pals) Assert.AreEqual(FavourRules.PlainPalWeight, FavourRules.PalWeight(p, 0));
            Assert.AreEqual(FavourRules.HeldPalWeight, FavourRules.PalWeight(Pal.Butcher, butcher));
            Assert.AreEqual(FavourRules.PlainPalWeight, FavourRules.PalWeight(Pal.Shade, butcher));
            Assert.AreEqual(FavourRules.HeldPalWeight, FavourRules.PalWeight(Pal.Glutton, butcher | glutton));
            Assert.AreEqual(FavourRules.OutsidePalWeight, FavourRules.PalWeight(Pal.Whisperer, butcher | glutton));
            Assert.AreEqual(3 * FavourRules.PlainPalWeight, FavourRules.HeldPalWeight, "three times as likely");
            Assert.AreEqual(3 * FavourRules.OutsidePalWeight, FavourRules.PlainPalWeight, "a third");

            // A run's road keeps the Pals it was made with, so the same road comes back when it loads.
            var easy = new CampaignTuning { EnemyScaleTier1 = 40 };
            Campaign c = Campaign.New(5100, easy);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, 5100, 10), "the tutorial was never won");
            c.StartRun();
            Assert.AreEqual(0, c.RoadPals, "a run's first road holds nothing");
            var lines = CampaignSave.Write(c).Split('\n').ToList();
            int at = lines.FindIndex(l => l.StartsWith("clearroad\t"));
            lines.Insert(at + 1, "roadpals\t" + butcher);
            Campaign r = CampaignSave.Read(string.Join("\n", lines), easy);
            Assert.IsEmpty(r.LoadNote);
            Assert.AreEqual(butcher, r.RoadPals);
            StringAssert.Contains("roadpals\t" + butcher + "\n", CampaignSave.Write(r));
        }
    }
}
