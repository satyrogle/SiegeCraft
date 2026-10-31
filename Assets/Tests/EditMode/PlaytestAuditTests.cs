using System.Linq;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The rule fix from Claude's playtest for Jacob (14 Oct 2026). The screen fixes are in the Slice and its
    /// validation.
    public class PlaytestAuditTests
    {
        /// In the playtest the Bloodridge Eye Charm was offered after the trial, taken, and offered again after the
        /// duel. A won fight now never offers a piece you already hold: since the class pieces (20 Oct 2026) that is the
        /// same tribe, class, bend, stat and tier, for each of the three on offer.
        [Test]
        public void SpoilsNeverOfferAPieceYouAlreadyHold()
        {
            int offered = 0, wouldRepeat = 0;
            for (ulong seed = 920; seed < 950; seed++)
            {
                Campaign c = Campaign.New(seed, new CampaignTuning { EnemyScaleTier1 = 40 });
                Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
                c.StartRun();
                for (int n = 0; n < 8 && c.Stage == CampaignStage.InRun && c.Offers.Count > 0; n++)
                {
                    var pilot = new CampaignPilot(PilotStyle.Reference, seed * 31 + (ulong)n);
                    // A favour fight drops no gear (18 Oct 2026), so this takes a gear fight when one is on offer.
                    int gear = c.Offers.ToList().FindIndex(o => o.Node == null || o.Node.Pal == Pal.None);
                    CampaignPilot.PlayFight(c, c.StartEncounter(gear >= 0 ? gear : pilot.PickOffer(c)), pilot);
                    for (int k = 0; k < c.SpoilsChoices.Count; k++)
                    {
                        CampaignGear piece = c.SpoilsChoices[k];
                        offered++;
                        var same = c.Gear.Where(g => g.TribeId == piece.TribeId && g.Tier == piece.Tier && g.ClassIndex == piece.ClassIndex &&
                            g.Bend == piece.Bend).ToList();
                        // The stat the fight's seed gives this piece before the rule moves it on (Campaign.MakeSpoils).
                        if ((int)((c.Archive.Fights.Last().Setup.Seed >> (8 * k + 5)) % 4UL) != (int)piece.Stat) wouldRepeat++;
                        if (same.Select(g => g.Stat).Distinct().Count() < 4)
                            Assert.IsFalse(same.Any(g => g.Stat == piece.Stat), piece.Name + " is already held, seed " + seed);
                    }
                    if (c.SpoilsChoices.Count > 0) Assert.IsTrue(c.TryTakeSpoils(out string why), why);
                }
            }
            Assert.Greater(offered, 20, "enough spoils were offered to test");
            Assert.Greater(wouldRepeat, 0, "some fight's own stat was one you held, so the rule was used");
        }

        /// The loose end left by that fix (Jacob, 14 Oct 2026: "fix the loose end"): an event could still give you a piece
        /// you already hold. It now moves on to the next stat too.
        [Test]
        public void EventsNeverGiveAPieceYouAlreadyHold()
        {
            var easy = new CampaignTuning { EnemyScaleTier1 = 40 };
            int given = 0;
            for (ulong seed = 4200; seed < 4320; seed++)
            {
                Campaign c = Campaign.New(seed, easy);
                Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
                c.StartRun();
                for (int n = 0; n < 10 && c.Stage == CampaignStage.InRun; n++)
                {
                    RoadNode ev = c.NextNodes.FirstOrDefault(x => x.Kind == RoadNodeKind.Event && GearChoice(x, c.Materials) >= 0);
                    if (ev != null)
                    {
                        int choice = GearChoice(ev, c.Materials);
                        string tribe = c.Road.TribeId;
                        var before = c.Gear.ToList();
                        Assert.IsTrue(c.TryTakeEvent(ev.Lane, choice, out EventOutcome outcome, out string why), why);
                        if (outcome.Gear == null) continue;
                        given++;
                        // The same piece: tribe, class, bend and tier (class pieces, 20 Oct 2026), and the stat. It is for a
                        // class you have.
                        CampaignGear got = outcome.Gear;
                        Assert.IsTrue(c.Reserve.Any(f => (int)f.Class == got.ClassIndex), got.Name + " fits none of yours");
                        var held = before.Where(g => g.TribeId == tribe && g.Tier == got.Tier && g.ClassIndex == got.ClassIndex && g.Bend == got.Bend)
                            .Select(g => g.Stat).ToList();
                        if (held.Distinct().Count() < 4)
                            Assert.IsFalse(held.Contains(got.Stat), got.Name + " is already held, seed " + seed);
                        continue;
                    }
                    if (c.Offers.Count == 0) break;
                    var pilot = new CampaignPilot(PilotStyle.Reference, seed * 31 + (ulong)n);
                    CampaignPilot.PlayFight(c, c.StartEncounter(pilot.PickOffer(c)), pilot);
                    if (c.Spoils != null) c.TryTakeSpoils(out _);
                }
            }
            Assert.Greater(given, 10, "enough event gear was given to test");
        }

        // The first choice at this event that gives gear and that you can pay for, or -1.
        private static int GearChoice(RoadNode node, int materials)
        {
            for (int i = 0; i < node.Event.Choices.Count; i++)
            {
                RoadChoice choice = node.Event.Choices[i];
                if (choice.Cost <= materials && (choice.Effect == EventEffect.BuyPauper || choice.Effect == EventEffect.BuyMid ||
                    choice.Effect == EventEffect.Pray))
                    return i;
            }
            return -1;
        }
    }
}
