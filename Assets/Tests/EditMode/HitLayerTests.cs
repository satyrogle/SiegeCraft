using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// The damage layers (Jacob, 30 Oct 2026: "Yes, start step 2"; DESIGN_LOCKS "Builds: one theme per Pal", step 2; Hits.cs,
    /// FightRules.Layers, campaign rules 17). One quick test per piece, only to prove it runs. Nothing here is tuning.
    public class HitLayerTests
    {
        [Test]
        public void BonusesOfOneLayerAddAndTheLayersMultiply()
        {
            // Two +50s on one layer make +100: twice the base.
            Assert.AreEqual(200, new Hit(100).Add(HitLayer.Increased, 50).Add(HitLayer.Increased, 50).Points);
            // On two layers they multiply: half again, then half again.
            Assert.AreEqual(225, new Hit(100).Add(HitLayer.Increased, 50).Add(HitLayer.Combo, 50).Points);
            // A cut is a negative bonus, and no layer goes below nothing.
            Assert.AreEqual(50, new Hit(100).Add(HitLayer.Armor, -50).Points);
            Assert.AreEqual(0, new Hit(100).Add(HitLayer.Guard, -150).Points);
        }

        [Test]
        public void AHitRoundsOnceAndKeepsTheLeftover()
        {
            long left = 0;
            // 10 with +15 is 11.5: 11 now, the half kept.
            Assert.AreEqual(11, new Hit(10).Add(HitLayer.Orders, 15).Resolve(ref left));
            Assert.AreEqual(Hit.Unit / 2, left);
            // The next 11.5 and the half kept make 12.
            Assert.AreEqual(12, new Hit(10).Add(HitLayer.Orders, 15).Resolve(ref left));
            Assert.AreEqual(0, left);
            // A hit worth under a point still lands for 1, and takes the leftover with it.
            left = Hit.Unit / 4;
            Assert.AreEqual(1, Hit.Percent(1, 30).Resolve(ref left));
            Assert.AreEqual(0, left);
            // A heal worth under a point waits in its leftover.
            long healLeft = 0;
            Assert.AreEqual(0, Hit.Percent(1, 60).ResolveHeal(ref healLeft));
            Assert.AreEqual(1, Hit.Percent(1, 60).ResolveHeal(ref healLeft));
        }

        [Test]
        public void FromRules17EveryCampaignFightPlaysTheLayers()
        {
            var c = Campaign.New(4600);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, 4600, 10));
            c.ChooseTribe(CampaignTribes.BloodridgeId);
            CampaignFight fight = c.StartEncounter(0);
            Assert.GreaterOrEqual(fight.RulesVersion, Campaign.LayersRules);
            Assert.IsTrue(fight.Sim.LayersOn);
            CampaignPilot.PlayFight(c, fight, new CampaignPilot(PilotStyle.Reference, 4600));
            Assert.IsTrue(fight.Sim.IsFinished, "it plays to its end");

            // Capped at rules 16 the same fight plays without them.
            var capped = Campaign.New(4600, new CampaignTuning { RulesCap = 16 });
            Assert.IsTrue(CampaignPilot.PlayTutorial(capped, PilotStyle.Reference, 4600, 10));
            capped.ChooseTribe(CampaignTribes.BloodridgeId);
            Assert.IsFalse(capped.StartEncounter(0).Sim.LayersOn);
        }
    }
}
