using System;
using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Battle;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The biggest threat read (Jacob, 22 Oct 2026, later: "go, build the biggest threat read in the sim for the screen and hand
    /// it to Codex"; BattleSimulation.Threats). One quick test per piece, only to prove it runs. Nothing here is tuning.
    public class ThreatTests
    {
        // A fighter at this percent of health, in its class's row: tough, so fights last.
        private static UnitSpec U(string id, UnitClass unitClass, int health = 1000, int bleed = 0)
        {
            return new UnitSpec(id, unitClass, SliceClasses.Stats(unitClass).DefaultRow, UnitRole.Fighter, 100, health, 0, 0, 0, 0, 0, 0,
                0, 0, bleed);
        }

        private static List<UnitSpec> Side(params UnitSpec[] units)
        {
            return new List<UnitSpec>(units);
        }

        // Ours: one of each class, no orders given unless a test gives them.
        private static List<UnitSpec> Ours()
        {
            return Side(U("w", UnitClass.Warden), U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman), U("h", UnitClass.Medic),
                U("t", UnitClass.Trapper));
        }

        // A band of every class behind a chief of this class; their blades bleed when asked.
        private static List<UnitSpec> Band(UnitClass chiefClass, int bleed = 0)
        {
            return Side(U("x.w", UnitClass.Warden, bleed: bleed), U("x.m", UnitClass.Marksman, bleed: bleed),
                U("x.t", UnitClass.Trapper, bleed: bleed), U("x.h", UnitClass.Medic, bleed: bleed), U("x.s", UnitClass.Skirmisher, bleed: bleed),
                new UnitSpec("chief", chiefClass, SliceClasses.Stats(chiefClass).DefaultRow, UnitRole.Chief, 100, 1000));
        }

        private static BattleSimulation Fight(string id, ulong seed, List<UnitSpec> ours, List<UnitSpec> theirs, SideRules rules,
            FightRules fight)
        {
            return new BattleSimulation(new BattleSetup(id, seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, BattleSetup.DefaultDirectivePoints, NerveRules.Off, 10, 400, new SideRules(1, 0, true), rules,
                fight));
        }

        // A chief with his whole kit, his spell on its clock, and his band's heavy swings.
        private static BattleSimulation Kit(string id, ulong seed, ChiefKit kit, UnitClass chiefClass, ChiefSpell spell, int bleed = 0,
            bool flood = false)
        {
            var rules = new SideRules(2, ChiefKits.SpellEveryTicks(kit), false, HeavyRules.EveryTicks, spell, flood: flood, kit: kit,
                moves: ChiefKits.Pool(kit));
            return Fight(id, seed, Ours(), Band(chiefClass, bleed), rules, FightRules.CampaignChiefTactics);
        }

        // A beast's parts as the campaign makes them, tough.
        private static List<UnitSpec> Beast(BeastKit kit)
        {
            var parts = new List<UnitSpec>();
            IReadOnlyList<BeastPart> order = BeastKits.Parts(kit);
            for (int p = 0; p < order.Count; p++)
                parts.Add(new UnitSpec("beast." + BeastKits.Name(order[p]), UnitClass.Warden, BeastKits.RowOf(order[p]), UnitRole.Fighter, 100,
                    Math.Max(1, 500 * BeastKits.HealthPercent(order[p]) / 100)));
            return parts;
        }

        // The read's order, as BattleSimulation.Threats documents it.
        private static bool Before(Threat a, Threat b)
        {
            if ((a.Amount > 0) != (b.Amount > 0)) return a.Amount > 0;
            if (a.TicksLeft != b.TicksLeft) return a.TicksLeft < b.TicksLeft;
            return a.Amount > b.Amount;
        }

        private static bool Order(SimSession session, string directive, string target = "")
        {
            return session.TryIssue(Directives.CommandKind, "0", target, 0, 0, directive, out _);
        }

        // ------------------------------------------------------------ one threat, read

        [Test]
        public void TheTopThreatSaysWhatIsComingWhoItHitsHowLongHowHardAndWhatMeetsIt()
        {
            // Only his Cleave, so it is the one threat.
            var rules = new SideRules(2, 0, false, 0, ChiefSpell.Eclipse, kit: ChiefKit.Bloodridge, moves: ChiefMove.Cleave);
            var sim = Fight("threat.cleave", 101, Ours(), Band(UnitClass.Marksman), rules, FightRules.CampaignChiefKit);
            var session = new SimSession(sim);
            Assert.IsTrue(sim.TopThreat(0).IsNone, "nothing coming yet");
            while (sim.CleaveTicks(1) == 0 && !sim.IsFinished) session.Tick();
            Threat top = sim.TopThreat(0);
            Assert.AreEqual(ChiefKits.CleaveName, top.Name);
            Assert.AreEqual(ThreatTier.Move, top.Tier);
            Assert.AreEqual("chief", top.ActorId);
            Assert.AreEqual(sim.CleaveTargetId(1), top.TargetId);
            Assert.AreEqual(sim.CleaveTicks(1), top.TicksLeft);
            Assert.AreEqual(sim.FindById("chief").Power * HeavyRules.DamagePercent / 100, top.Amount);
            Assert.AreEqual(ThreatAnswer.Parry | ThreatAnswer.Warden |
                (sim.FindById(top.TargetId).Row == Row.Front ? ThreatAnswer.Hold : ThreatAnswer.None), top.Answers);
            Assert.AreEqual(string.Empty, top.FocusId);
            Assert.AreEqual(1, sim.Threats(0).Count);
            Assert.AreEqual(0, sim.Threats(1).Count, "nothing comes at his side");

            // Without a Warden of ours, a Warden is no answer.
            var bare = Fight("threat.cleave.bare", 101, Side(U("s", UnitClass.Skirmisher), U("m", UnitClass.Marksman)), Band(UnitClass.Marksman),
                rules, FightRules.CampaignChiefKit);
            var bs = new SimSession(bare);
            while (bare.CleaveTicks(1) == 0 && !bare.IsFinished) bs.Tick();
            Assert.AreEqual(0, (int)(bare.TopThreat(0).Answers & ThreatAnswer.Warden));
        }

        // ------------------------------------------------------------ everything coming, ranked

        [Test]
        public void EveryWarningComingAtYouIsInTheReadRankedWithTheNextHitOnTop()
        {
            var fights = new List<KeyValuePair<string, BattleSimulation>>
            {
                new KeyValuePair<string, BattleSimulation>("Bloodridge", Kit("threat.bloodridge", 102, ChiefKit.Bloodridge, UnitClass.Marksman,
                    ChiefSpell.Eclipse)),
                new KeyValuePair<string, BattleSimulation>("Skullpeak", Kit("threat.skullpeak", 103, ChiefKit.Skullpeak, UnitClass.Trapper,
                    ChiefSpell.SteelStorm)),
                new KeyValuePair<string, BattleSimulation>("Stoneveil", Kit("threat.stoneveil", 104, ChiefKit.Stoneveil, UnitClass.Medic,
                    ChiefSpell.BloodCall, bleed: 2)),
                new KeyValuePair<string, BattleSimulation>("Thundercoast", Kit("threat.thundercoast", 105, ChiefKit.Thundercoast, UnitClass.Warden,
                    ChiefSpell.Tide, flood: true)),
                new KeyValuePair<string, BattleSimulation>("the hunt", Fight("threat.hunt", 106, Ours(), Band(UnitClass.Warden),
                    new SideRules(2, 600, false, HeavyRules.EveryTicks, ChiefSpell.Firebomb, kit: ChiefKit.Hunt), FightRules.CampaignChiefTactics)),
                new KeyValuePair<string, BattleSimulation>("a War Cry", Fight("threat.warcry", 107, Ours(), Band(UnitClass.Warden),
                    new SideRules(2, 450, false, HeavyRules.EveryTicks, ChiefSpell.WarCry), FightRules.Campaign)),
                new KeyValuePair<string, BattleSimulation>("a Dust Shark", Fight("threat.dustshark", 108, Ours(), Band(UnitClass.Warden),
                    new SideRules(2, 450, false, HeavyRules.EveryTicks, ChiefSpell.DustShark), FightRules.Campaign)),
                new KeyValuePair<string, BattleSimulation>("the sand shark", Fight("threat.shark", 109, Ours(), Beast(BeastKit.SandShark),
                    new SideRules(0, 0, false, beast: BeastKit.SandShark), FightRules.CampaignBeast)),
                new KeyValuePair<string, BattleSimulation>("the cage bear", Fight("threat.bear", 110, Ours(), Beast(BeastKit.CageBear),
                    new SideRules(0, 0, false, beast: BeastKit.CageBear), FightRules.CampaignBeast))
            };
            foreach (KeyValuePair<string, BattleSimulation> f in fights)
            {
                BattleSimulation sim = f.Value;
                var session = new SimSession(sim);
                int seen = 0, read = 0;
                var names = new List<string>();
                while (!sim.IsFinished && sim.CurrentTick < BattleSetup.DefaultTickLimit)
                {
                    session.Tick();
                    IReadOnlyList<Threat> threats = sim.Threats(0);
                    for (int i = 1; i < threats.Count; i++)
                        Assert.IsFalse(Before(threats[i], threats[i - 1]), f.Key + ": ranked wrong at " + sim.CurrentTick);
                    Assert.AreEqual(threats.Count > 0 ? threats[0].Name : string.Empty, sim.TopThreat(0).Name);
                    Assert.IsTrue(threats.All(t => t.TicksLeft > 0 && t.Amount >= 0 && !t.IsNone), f.Key);
                    Assert.IsTrue(threats.All(t => ((t.Answers & ThreatAnswer.FocusFire) != 0) == (t.FocusId.Length > 0)), f.Key + ": a focus needs whom");
                    for (; seen < sim.Events.Count; seen++)
                    {
                        BattleEvent e = sim.Events[seen];
                        if (e.ActorSide != 1 || (e.Kind != BattleEventKind.Tell && e.Kind != BattleEventKind.Windup)) continue;
                        if (sim.IsFinished || sim.AwaitingVerdict) continue;
                        // Met on the tick it started (broke on a Warden, plucked at once), or the caged Pulse, which touches nobody.
                        string detail = e.Detail;
                        bool done = sim.Events.Any(x => x.Tick == e.Tick && x.Kind == BattleEventKind.Land &&
                            x.Detail.StartsWith(detail + ".", StringComparison.Ordinal));
                        if (done || (detail == BeastKits.PulseName && sim.BeastCaged)) continue;
                        Assert.IsTrue(threats.Any(t => t.Name == detail), f.Key + ": " + detail + " at " + e.Tick + " is not in the read");
                        read++;
                        if (!names.Contains(detail)) names.Add(detail);
                    }
                }
                Assert.Greater(read, 0, f.Key);
                TestContext.Progress.WriteLine(f.Key + ": " + read + " warnings read, " + string.Join(", ", names));
            }
        }

        // ------------------------------------------------------------ reading changes nothing

        [Test]
        public void ReadingTheThreatsEveryTickChangesNothingInTheFight()
        {
            BattleSimulation Make() => Kit("threat.readonly", 111, ChiefKit.Stoneveil, UnitClass.Medic, ChiefSpell.BloodCall, bleed: 2);
            BattleSimulation quiet = Make(), asked = Make();
            var qs = new SimSession(quiet);
            var asks = new SimSession(asked);
            int read = 0;
            while (!quiet.IsFinished && quiet.CurrentTick < BattleSetup.DefaultTickLimit)
            {
                // Both give the same orders; one asks for the threats on both sides every tick.
                if (quiet.CurrentTick % 300 == 150)
                {
                    Order(qs, "Hold");
                    Order(asks, "Hold");
                }
                qs.Tick();
                asks.Tick();
                read += asked.Threats(0).Count + asked.Threats(1).Count;
                asked.TopThreat(0);
            }
            Assert.Greater(read, 0);
            Assert.AreEqual(quiet.EventHash, asked.EventHash);
            Assert.AreEqual(quiet.OrderedStateSnapshot, asked.OrderedStateSnapshot);
        }
    }
}
