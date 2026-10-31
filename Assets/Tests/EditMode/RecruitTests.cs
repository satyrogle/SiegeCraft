using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Capture or kill (Jacob, 13 Oct 2026; this replaced call J4 of 11 Oct): break the chief's nerve while he still
    /// stands and his whole band from that fight joins you, fallen ones too, with his order; kill him and nobody
    /// joins, but you take his Princess piece. Since 21 Oct 2026 he kneels when his nerve breaks and you choose
    /// (Battle.ChiefKits): the reference pilot spares him, the cheater executes him.
    public class RecruitTests
    {
        [Test]
        public void ACapturedChiefBringsHisWholeBandAndAKilledOneOnlyHisPiece()
        {
            int captured = 0, killed = 0;
            // The cheater plays half these chief fights and executes him at the kneel, so captures take more seeds.
            for (ulong seed = 40; seed < 240 && captured < 12; seed++)
            {
                Campaign c = Campaign.New(seed);
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) continue;
                c.StartRun();
                var pilot = new CampaignPilot(PilotStyle.Reference, seed);
                for (int n = 0; n < 6 && c.Stage == CampaignStage.InRun; n++)
                {
                    int pick = pilot.PickOffer(c);
                    bool chief = c.Offers[pick].Kind == EncounterKind.Chief;
                    CampaignFight fight = c.StartEncounter(pick);
                    var player = new CampaignPilot(chief && seed % 2 == 0 ? PilotStyle.Cheater : PilotStyle.Reference, seed * 7UL + (ulong)n);
                    while (!fight.Over)
                    {
                        player.Step(fight);
                        fight.Tick();
                    }
                    int band = 0;
                    bool chiefStanding = false;
                    foreach (BattleUnit u in fight.Sim.Units)
                    {
                        if (u.Side != 1) continue;
                        if (u.Role == UnitRole.Fighter && !u.Benched) band++; // a reserve who never came in is not of his band
                        if (u.Role == UnitRole.Chief) chiefStanding = u.Alive;
                    }
                    EncounterResult r = c.Finish(fight);
                    if (!chief || !r.Won) continue;
                    if (chiefStanding)
                    {
                        captured++;
                        Assert.IsTrue(r.ChiefCaptured && r.Joined, "seed " + seed + ": he still stood, so he was captured");
                        // A raid chief fought alone (rules 10): his tribe's chief lineup joins instead.
                        int joins = fight.Setup.Fight.RaidChief && band == 0 ? fight.Encounter.EnemyFighters : band;
                        Assert.AreEqual(joins, r.JoinedFighters.Count, "seed " + seed + ": his whole band joins, fallen ones too");
                        Assert.IsNotNull(r.AssistUnlocked);
                        Assert.IsNull(r.PrincessTaken);
                    }
                    else
                    {
                        killed++;
                        Assert.IsTrue(r.ChiefKilled && !r.Joined, "seed " + seed + ": he fell, so nobody joins");
                        Assert.AreEqual(0, r.JoinedFighters.Count);
                        Assert.IsNull(r.AssistUnlocked);
                        Assert.AreEqual(GearTier.Princess, r.PrincessTaken.Tier);
                        CollectionAssert.Contains(c.Gear, r.PrincessTaken);
                    }
                    break;
                }
            }
            TestContext.WriteLine(captured + " chiefs captured, " + killed + " killed");
            Assert.Greater(captured, 5, "too few captures checked");
            Assert.Greater(killed, 0, "the cheater executes him at the kneel, so some chiefs fall");
        }
    }
}
