using System.Collections.Generic;
using System.Linq;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Fix 5, a Pal on every fight (Jacob, 31 Oct 2026: "Go, build it"; DESIGN_LOCKS, "Fix 5: a Pal on every fight"; campaign
    /// rules 19). One quick test per piece, only to prove each runs (Jacob, 12 Oct 2026: build first). Nothing here is tuning.
    public class PalRoadTests
    {
        private static Campaign Started(ulong seed, CampaignTuning tuning)
        {
            Campaign c = Campaign.New(seed, tuning);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            c.StartRun();
            return c;
        }

        // Every road node's Pal, row by row.
        private static List<Pal> Pals(Campaign c)
        {
            var pals = new List<Pal>();
            for (int row = 0; row < c.Road.Rows; row++)
                for (int lane = 0; lane < CampaignRoad.Lanes; lane++) pals.Add(c.Road.Node(row, lane).Pal);
            return pals;
        }

        [Test]
        public void ARules19RoadPutsAPalOnEveryFightWithNoTwoTheSameInARow()
        {
            Campaign c = Started(5300, new CampaignTuning());
            Assert.IsTrue(c.PalRoadRun);
            for (int row = 0; row < c.Road.Rows; row++)
            {
                var seen = new List<Pal>();
                for (int lane = 0; lane < CampaignRoad.Lanes; lane++)
                {
                    RoadNode node = c.Road.Node(row, lane);
                    if (node.Kind != RoadNodeKind.Fight)
                    {
                        Assert.AreEqual(Pal.None, node.Pal, "events carry none");
                        continue;
                    }
                    Assert.AreNotEqual(Pal.None, node.Pal, "every fight carries a Pal");
                    CollectionAssert.DoesNotContain(seen, node.Pal, "no two the same in a row");
                    seen.Add(node.Pal);
                }
            }

            // Capped at rules 18 the road keeps one Pal fight a row.
            Campaign old = Started(5300, new CampaignTuning { RulesCap = Campaign.PalRoadRules - 1 });
            Assert.IsFalse(old.PalRoadRun);
            for (int row = 0; row < old.Road.Rows; row++)
            {
                int pals = 0;
                for (int lane = 0; lane < CampaignRoad.Lanes; lane++) if (old.Road.Node(row, lane).Pal != Pal.None) pals++;
                Assert.AreEqual(1, pals, "one Pal fight a row");
            }
        }

        [Test]
        public void AWonFightOffersItsPalsPicksAndTheTribesGearOnOneCard()
        {
            var easy = new CampaignTuning { EnemyScaleTier1 = 40 };
            for (ulong seed = 5300; seed < 5340; seed++)
            {
                Campaign c = Started(seed, easy);
                int index = c.Offers.ToList().FindIndex(o => o.Node != null && o.Node.Pal != Pal.None);
                if (index < 0) continue;
                int before = c.Materials;
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(index), new CampaignPilot(PilotStyle.Reference, seed));
                if (!r.Won || c.FavourOffer == null) continue;

                // Both on the card: the Pal's picks and the tribe's gear, the materials banked until one is taken.
                Assert.IsNotNull(r.FavourOffered);
                Assert.IsNotNull(r.SpoilsOffered);
                Assert.Greater(c.SpoilsChoices.Count, 0);
                Assert.AreEqual(before + r.MaterialsEarned, c.Materials, "banked");

                // A save with both standing reads back the same.
                string saved = CampaignSave.Write(c);
                Campaign back = CampaignSave.Read(saved, easy);
                Assert.IsEmpty(back.LoadNote);
                Assert.AreEqual(saved, CampaignSave.Write(back));

                // Taking a pick clears the gear and raises no bounty.
                int heat = c.Heat;
                Assert.IsTrue(c.TryTakeFavour(0, out string why), why);
                Assert.AreEqual(0, c.SpoilsChoices.Count, "the gear went with it");
                Assert.AreEqual(heat, c.Heat, "a pick raises no bounty");
                Assert.AreEqual(before, c.Materials, "it cost the banked materials");

                // Taking a piece instead clears the picks and raises the bounty.
                Assert.IsTrue(back.TryTakeSpoils(0, out why), why);
                Assert.IsNull(back.FavourOffer, "the picks went with it");
                Assert.AreEqual(heat + BountyRules.HeatPerStrip, back.Heat, "stripping raises the bounty");
                return;
            }
            Assert.Fail("no Pal fight was won in forty campaigns");
        }

        [Test]
        public void TheRunKeepsItsRoadKindSoTheSameRoadComesBack()
        {
            Campaign c = Started(5300, new CampaignTuning());
            string saved = CampaignSave.Write(c);
            StringAssert.Contains("palroad\t1\n", saved);
            Campaign back = CampaignSave.Read(saved);
            Assert.IsEmpty(back.LoadNote);
            Assert.IsTrue(back.PalRoadRun);
            CollectionAssert.AreEqual(Pals(c), Pals(back), "the same road");
        }
    }
}
