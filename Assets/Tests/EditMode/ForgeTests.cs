using System.Collections.Generic;
using System.Globalization;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Call J1 (Jacob, 11 Oct 2026): breaking the Bloodridge chief teaches the Butcher the Lava Sun Tabard.
    /// At camp it costs CampaignForge.TabardPrice materials to forge, once and for good. Every run after that
    /// one fighter wears it, and their hits drain CampaignForge.TabardFear percent more nerve. It comes off
    /// when the run ends; the forge stays.
    public class ForgeTests
    {
        private static Campaign Ready(ulong seed)
        {
            Campaign c = Campaign.New(seed);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }

        /// A campaign at camp with this many materials, knowing the recipe or not, read from an edited save.
        private static Campaign AtCamp(ulong seed, bool recipe, int materials)
        {
            Campaign c = Ready(seed);
            string saved = CampaignSave.Write(c);
            string edited = saved
                .Replace("\ncamp\t" + c.Materials.ToString(CultureInfo.InvariantCulture) + "\n",
                    "\ncamp\t" + materials.ToString(CultureInfo.InvariantCulture) + "\n")
                .Replace("\nforge\t0\t0\n", "\nforge\t" + (recipe ? "1" : "0") + "\t0\n");
            Campaign read = CampaignSave.Read(edited);
            Assert.IsEmpty(read.LoadNote);
            Assert.AreEqual(materials, read.Materials);
            Assert.AreEqual(recipe, read.TabardRecipeKnown);
            Assert.IsFalse(read.TabardForged);
            return read;
        }

        private static Campaign Forged(ulong seed)
        {
            Campaign c = AtCamp(seed, true, CampaignForge.TabardPrice);
            Assert.IsTrue(c.TryForgeTabard(out string failure), failure);
            return c;
        }

        private static EncounterResult PlayUntilChiefBeaten(Campaign c, string tribeId, ulong seed)
        {
            var picker = new CampaignPilot(PilotStyle.Reference, seed);
            for (ulong n = 0; c.Stage == CampaignStage.InRun; n++)
            {
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(picker.PickOffer(c)),
                    new CampaignPilot(PilotStyle.Reference, seed * 31UL + n));
                if (r.Won && r.Encounter.Kind == EncounterKind.Chief && r.Encounter.Tribe.Id == tribeId) return r;
            }
            return null;
        }

        private static void PlayOut(Campaign c, PilotStyle style, ulong seed)
        {
            var picker = new CampaignPilot(style, seed);
            for (ulong n = 0; c.Stage == CampaignStage.InRun; n++)
                CampaignPilot.PlayFight(c, c.StartEncounter(picker.PickOffer(c)), new CampaignPilot(style, seed * 17UL + n));
        }

        private static CampaignFighter Fighter(Campaign c, string id)
        {
            for (int i = 0; i < c.Reserve.Count; i++) if (c.Reserve[i].Id == id) return c.Reserve[i];
            return null;
        }

        /// Every fighter in the fight carries their tribe's fear, and the wearer the tabard's on top.
        private static void AssertFear(CampaignFight fight, string wearer)
        {
            bool seen = false;
            for (int i = 0; i < fight.Squad.Count; i++)
            {
                CampaignFighter f = fight.Squad[i];
                BattleUnit u = fight.Sim.FindById(f.Id);
                int expected = TribeTraits.FearPercent(f.TribeId) + (f.Id == wearer ? CampaignForge.TabardFear : 0);
                Assert.AreEqual(expected, fight.Sim.FearPercent(u), f.Id);
                if (f.Id == wearer) seen = true;
            }
            Assert.IsTrue(seen, "the wearer is not in this fight");
        }

        [Test]
        public void BreakingTheBloodridgeChiefTeachesTheTabard()
        {
            for (ulong seed = 40; seed < 80; seed++)
            {
                Campaign c = Ready(seed);
                Assert.IsFalse(c.TabardRecipeKnown);
                c.StartRun();
                EncounterResult r = PlayUntilChiefBeaten(c, CampaignTribes.BloodridgeId, seed);
                if (r == null) continue;
                Assert.IsTrue(r.RecipeLearned);
                Assert.IsTrue(c.TabardRecipeKnown);
                Assert.IsFalse(c.TabardForged, "learning it forges nothing");
                CollectionAssert.Contains(c.Codex, "Recipe: " + CampaignForge.TabardName);
                Assert.IsFalse(c.CanForgeTabard(out string why), "no forging in the middle of a run");
                StringAssert.Contains("camp", why);

                PlayOut(c, PilotStyle.Reference, seed);
                Assert.AreEqual(CampaignStage.ChooseTribe, c.Stage);
                Assert.IsTrue(c.TabardRecipeKnown, "the recipe outlives the run");
                Assert.GreaterOrEqual(c.Materials, CampaignForge.TabardPrice, "a run that broke the Bloodridge chief pays for the forge");
                Assert.IsTrue(CampaignSave.Read(CampaignSave.Write(c)).TabardRecipeKnown, "and closing the game");
                return;
            }
            Assert.Fail("the reference pilot never beat the Bloodridge chief in forty campaigns");
        }

        [Test]
        public void TheForgeCostsItsPriceAtCampAndIsForGood()
        {
            Campaign none = AtCamp(41, false, 200);
            Assert.IsFalse(none.TryForgeTabard(out string noRecipe));
            StringAssert.Contains("recipe", noRecipe);
            Assert.AreEqual(200, none.Materials);

            Campaign poor = AtCamp(41, true, CampaignForge.TabardPrice - 1);
            Assert.IsFalse(poor.TryForgeTabard(out string tooPoor));
            StringAssert.Contains(CampaignForge.TabardPrice.ToString(CultureInfo.InvariantCulture), tooPoor);
            Assert.AreEqual(CampaignForge.TabardPrice - 1, poor.Materials);
            Assert.IsFalse(poor.TabardForged);

            Campaign c = AtCamp(41, true, CampaignForge.TabardPrice + 7);
            Assert.IsTrue(c.CanForgeTabard(out _));
            Assert.IsTrue(c.TryForgeTabard(out string failure), failure);
            Assert.IsTrue(c.TabardForged);
            Assert.AreEqual(7, c.Materials);
            CollectionAssert.Contains(c.Codex, "Forged: " + CampaignForge.TabardName);
            Assert.IsFalse(c.TryForgeTabard(out string twice));
            StringAssert.Contains("already", twice);
            Assert.AreEqual(7, c.Materials);

            // The forge outlives runs, won or lost, and closing the game.
            for (ulong run = 0; run < 2; run++) CampaignPilot.PlayRun(c, CampaignTribes.BloodridgeId, PilotStyle.Random, 41UL + run);
            Assert.IsTrue(c.TabardForged);
            string saved = CampaignSave.Write(c);
            Campaign read = CampaignSave.Read(saved);
            Assert.IsTrue(read.TabardForged);
            Assert.AreEqual(saved, CampaignSave.Write(read));
        }

        [Test]
        public void TheWearerTakesTheTabardIntoTheFightAndOnlyThey()
        {
            Campaign c = AtCamp(42, true, CampaignForge.TabardPrice);
            Assert.IsFalse(c.CanWearTabard(c.Reserve[0].Id, out string notYet), "nobody wears it before it is forged");
            StringAssert.Contains("not forged", notYet);
            Assert.IsTrue(c.TryForgeTabard(out string failure), failure);
            Assert.IsEmpty(c.TabardWearer, "forging puts it on nobody yet");

            c.StartRun();
            string first = c.TabardWearer;
            Assert.IsNotEmpty(first, "a run starts with someone wearing it");
            Assert.AreEqual(UnitClass.Warden, Fighter(c, first).Class, "until you choose, the first Warden wears it");

            // Hand it to someone else, then take both into a fight.
            string other = null;
            for (int i = 0; i < c.Reserve.Count && other == null; i++) if (c.Reserve[i].Id != first) other = c.Reserve[i].Id;
            Assert.IsFalse(c.TryWearTabard("nobody", out string stranger));
            StringAssert.Contains("not one of your fighters", stranger);
            Assert.IsTrue(c.TryWearTabard(other, out failure), failure);
            Assert.AreEqual(other, c.TabardWearer);

            int index = 0;
            while (c.Offers[index].Kind == EncounterKind.Duel) index++;
            CampaignFight fight = c.StartEncounter(index, new List<string> { first, other });
            AssertFear(fight, other);
            Assert.IsFalse(c.CanWearTabard(first, out string busy), "it cannot change hands in a fight");
            StringAssert.Contains("fight", busy);
        }

        [Test]
        public void TheTabardComesOffWhenTheRunEndsButTheForgeStays()
        {
            Campaign c = Forged(43);
            c.StartRun();
            Assert.IsNotEmpty(c.TabardWearer);
            PlayOut(c, PilotStyle.Random, 43);
            Assert.AreEqual(CampaignStage.ChooseTribe, c.Stage);
            Assert.IsEmpty(c.TabardWearer, "the tabard comes off with the run");
            Assert.IsTrue(c.TabardForged, "the forge stays");

            // At camp you can hand it to one of the squad the next run starts with.
            string pick = c.Reserve[c.Reserve.Count - 1].Id;
            Assert.IsTrue(c.TryWearTabard(pick, out string failure), failure);
            c.StartRun();
            Assert.AreEqual(pick, c.TabardWearer, "the run keeps the one you picked at camp");
        }

        [Test]
        public void AWearerWhoFallsPassesTheTabardOn()
        {
            // The tabard goes round the squad, a fight each, until its wearer falls in a fight the run survives.
            for (ulong seed = 44; seed < 104; seed++)
            {
                Campaign c = Forged(seed);
                // Under the 13 Oct roster as lives, which rules 11 replaced and old saves still play: a fallen wearer stays down.
                CampaignTuning oldRules = c.Tuning;
                oldRules.RulesCap = Campaign.RaidChiefRules;
                c = CampaignSave.Read(CampaignSave.Write(c), oldRules);
                c.StartRun();
                var picker = new CampaignPilot(PilotStyle.Reference, seed);
                for (int n = 0; c.Stage == CampaignStage.InRun; n++)
                {
                    int pick = picker.PickOffer(c);
                    List<CampaignFighter> squad = c.DefaultSquad(c.Offers[pick].SquadSize);
                    string wearer = squad[n % squad.Count].Id;
                    Assert.IsTrue(c.TryWearTabard(wearer, out string failure), failure);
                    EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(pick),
                        new CampaignPilot(PilotStyle.Reference, seed * 13UL + (ulong)n));
                    bool fell = false;
                    for (int i = 0; i < r.Fallen.Count; i++) if (r.Fallen[i].Id == wearer) fell = true;
                    if (!fell || r.RunOver) continue;
                    Assert.IsNotEmpty(c.TabardWearer, "somebody picks it up");
                    Assert.AreNotEqual(wearer, c.TabardWearer);
                    Assert.IsNotNull(Fighter(c, c.TabardWearer), "and they are one of yours");
                    return;
                }
            }
            Assert.Fail("no wearer fell in a fight the run survived, in sixty campaigns");
        }

        [Test]
        public void TheTabardAndItsWearerSurviveClosingTheGame()
        {
            Campaign c = Forged(45);
            c.StartRun();
            string pick = c.Reserve[2].Id;
            Assert.IsTrue(c.TryWearTabard(pick, out string failure), failure);
            string saved = CampaignSave.Write(c);
            Campaign read = CampaignSave.Read(saved);
            Assert.IsEmpty(read.LoadNote);
            Assert.AreEqual(pick, read.TabardWearer);
            Assert.AreEqual(saved, CampaignSave.Write(read));

            // Halfway through a fight with the wearer in it, the reopened fight plays back the same.
            CampaignFight fight = c.StartEncounter(0, new List<string> { pick });
            var pilot = new CampaignPilot(PilotStyle.Reference, 45);
            while (!fight.Over && fight.Sim.CurrentTick < 400)
            {
                pilot.Step(fight);
                fight.Tick();
            }
            string halfway = CampaignSave.Write(c);
            Campaign back = CampaignSave.Read(halfway);
            Assert.IsEmpty(back.LoadNote, "the fight with the tabard in it did not play back");
            Assert.IsNotNull(back.Active);
            Assert.AreEqual(fight.Session.Checksum, back.Active.Session.Checksum);
            AssertFear(back.Active, pick);
            Assert.AreEqual(halfway, CampaignSave.Write(back));
        }

        [Test]
        public void ASaveFromBeforeTheForgeStillOpens()
        {
            Campaign c = Campaign.New(46, new CampaignTuning { RulesCap = 12 });
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, 46, 10));
            c.StartRun();
            string now = CampaignSave.Write(c);
            Assert.IsTrue(now.Contains("\nforge\t0\t0\n"));
            string old = now.Replace("\nforge\t0\t0\n", "\n")
                .Replace("clearroad\t0\n", "")
                .Replace(CampaignSave.Header + "\t" + CampaignSave.Version, CampaignSave.Header + "\t4");
            Assert.AreNotEqual(now, old);
            Campaign read = CampaignSave.Read(old);
            Assert.IsEmpty(read.LoadNote);
            Assert.AreEqual(CampaignStage.InRun, read.Stage);
            Assert.IsFalse(read.TabardRecipeKnown);
            Assert.IsFalse(read.TabardForged);
            Assert.AreEqual(now, CampaignSave.Write(read));
        }

        [Test]
        public void ATabardOnSomeoneWhoIsNotThereCostsTheRunNotTheCamp()
        {
            Campaign c = Forged(47);
            c.StartRun();
            string saved = CampaignSave.Write(c);
            string broken = saved.Replace("\ngear\t" + c.TabardWearer + "\n", "\ngear\tnobody\n");
            Assert.AreNotEqual(saved, broken);
            Campaign read = CampaignSave.Read(broken);
            Assert.AreEqual(CampaignStage.ChooseTribe, read.Stage, "an unreadable run opens at camp");
            Assert.IsNotEmpty(read.LoadNote);
            Assert.IsTrue(read.TabardForged, "the forge is camp, and camp is kept");
            Assert.IsEmpty(read.TabardWearer);

            string unforged = saved.Replace("\nforge\t1\t1\n", "\nforge\t1\t0\n");
            Assert.AreNotEqual(saved, unforged);
            Campaign odd = CampaignSave.Read(unforged);
            Assert.IsNotEmpty(odd.LoadNote, "a tabard worn but never forged is refused");
            Assert.IsFalse(CampaignSave.TryRead(saved.Replace("\nforge\t1\t1\n", "\nforge\t0\t1\n"), null, out _, out string why),
                "forged without the recipe is not a save this game writes");
            StringAssert.Contains("forge", why);
        }
    }
}
