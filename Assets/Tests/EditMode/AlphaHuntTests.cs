using System;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Call J2 (Jacob, 11 Oct 2026): after the run's last chief, one last fight against the Sovereign's
    /// hunters. AlphaHunt.Fighters of them and their captain, plus AlphaHunt.PerTribeJoined more for every
    /// tribe that joined you this run. The captain winds up a big hit you can see coming. Breaking their nerve
    /// wins the run; they never join. Whether the fight is fair is the bots' numbers, not these tests: the
    /// rules here are checked with weaker hunters where a win is needed.
    public class AlphaHuntTests
    {
        private const string B = CampaignTribes.BloodridgeId;
        private const string S = CampaignTribes.SkullpeakId;

        private static Campaign Ready(ulong seed, CampaignTuning tuning = null)
        {
            Campaign c = Campaign.New(seed, tuning);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            return c;
        }

        /// Every enemy at half strength, the hunters too, so the reference pilot reaches the hunt often.
        /// These tests check the rules, not the balance.
        private static CampaignTuning EasyPath()
        {
            return new CampaignTuning { EnemyScaleTier1 = 50, EnemyScaleTier2 = 50 };
        }

        /// The reference pilot plays the run on until the hunt is the only offer, and returns the result
        /// that brought it there. Null if the run ended first.
        private static EncounterResult PlayToTheHunt(Campaign c, ulong seed)
        {
            var picker = new CampaignPilot(PilotStyle.Reference, seed);
            for (ulong n = 0; c.Stage == CampaignStage.InRun; n++)
            {
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(picker.PickOffer(c)),
                    new CampaignPilot(PilotStyle.Reference, seed * 31UL + n));
                if (c.Stage == CampaignStage.InRun && c.Target == AlphaHunt.Tribe) return r;
            }
            return null;
        }

        private static int Count(System.Collections.Generic.IReadOnlyList<UnitSpec> specs, UnitRole role)
        {
            int count = 0;
            for (int i = 0; i < specs.Count; i++) if (specs[i].Role == role) count++;
            return count;
        }

        [Test]
        public void AfterTheLastChiefTheSovereignsHuntersCome()
        {
            for (ulong seed = 210; seed < 260; seed++)
            {
                Campaign c = Ready(seed, EasyPath());
                c.StartRun();
                EncounterResult chief = PlayToTheHunt(c, seed);
                if (chief == null) continue;
                Assert.AreEqual(EncounterKind.Chief, chief.Encounter.Kind);
                Assert.AreEqual(2, chief.Encounter.Tribe.Tier, "the hunters come after the last tier's chief");
                Assert.IsFalse(chief.RunOver, "breaking the last chief no longer ends the run");
                Assert.IsFalse(chief.RunWon);
                Assert.AreEqual(0, c.RunsWon);
                Assert.AreSame(AlphaHunt.Tribe, chief.NextTribe);
                Assert.IsTrue(chief.HuntComing);
                Assert.AreSame(AlphaHunt.Tribe, c.Target);
                CollectionAssert.Contains(c.Codex, "Tribe: " + AlphaHunt.Tribe.Name);

                Assert.AreEqual(1, c.Offers.Count, "the hunt is one fight");
                Encounter hunt = c.Offers[0];
                Assert.AreEqual(EncounterKind.AlphaHunt, hunt.Kind);
                // A raid chief (rules 10) can fall before he kneels, and then his tribe never joins.
                int joined = 0;
                foreach (CampaignTribe t in c.RunRoute) if (c.HasJoined(t.Id)) joined++;
                Assert.AreEqual(AlphaHunt.Fighters + joined * AlphaHunt.PerTribeJoined, hunt.EnemyFighters, "one more for each route tribe that joined");
                Assert.AreEqual(hunt.EnemyFighters + 1, hunt.EnemyUnits, "and their captain");
                Assert.IsTrue(hunt.EnemyCasts);
                StringAssert.Contains(hunt.EnemyFighters.ToString(), hunt.Text);

                CampaignFight fight = c.StartEncounter(0);
                Assert.AreEqual(hunt.EnemyFighters, Count(fight.Setup.Side(1), UnitRole.Fighter));
                Assert.AreEqual(1, Count(fight.Setup.Side(1), UnitRole.Chief), "one captain");
                Assert.AreEqual(hunt.EnemyUnits, fight.Setup.Side(1).Count);
                Assert.Greater(fight.Setup.Rules(1).AutoEclipseTicks, 0);
                Assert.IsNotNull(fight.Assist, "your chiefs' orders come to the hunt");
                return;
            }
            Assert.Fail("the reference pilot never reached the hunt in fifty campaigns");
        }

        [Test]
        public void TheHuntersComeHarderForEveryTribeThatJoined()
        {
            // A run that starts at Skullpeak has one tribe join before the hunt, not two.
            for (ulong seed = 260; seed < 310; seed++)
            {
                Campaign c = Ready(seed, EasyPath());
                c.ChooseTribe(S);
                if (PlayToTheHunt(c, seed) == null) continue;
                Assert.AreEqual(AlphaHunt.Fighters + AlphaHunt.PerTribeJoined, c.Offers[0].EnemyFighters);
                return;
            }
            Assert.Fail("the reference pilot never reached the hunt from Skullpeak in fifty campaigns");
        }

        [Test]
        public void TheCaptainWindsUpABigHitYouCanSeeComing()
        {
            for (ulong seed = 310; seed < 360; seed++)
            {
                Campaign c = Ready(seed, EasyPath());
                c.ChooseTribe(S);
                if (PlayToTheHunt(c, seed) == null) continue;
                CampaignFight fight = c.StartEncounter(0);
                while (!fight.Over && fight.Sim.WindupTicks(1) == 0) fight.Tick();
                // Under road combat a strong squad can end the hunt before his first wind up: try the next one.
                if (fight.Sim.WindupTicks(1) == 0) continue;
                BattleUnit caster = fight.Sim.Leader(1);
                Assert.IsNotNull(caster);
                Assert.AreEqual(UnitRole.Chief, caster.Role, "it is the captain who winds up");
                return;
            }
            Assert.Fail("the captain never wound up in a hunt in fifty campaigns");
        }

        [Test]
        public void BreakingTheHuntersWinsTheRunAndNobodyJoins()
        {
            // Weak hunters, so the rule is tested and not the balance.
            CampaignTuning weak = EasyPath();
            for (ulong seed = 360; seed < 420; seed++)
            {
                Campaign c = Ready(seed, weak);
                c.ChooseTribe(S);
                if (PlayToTheHunt(c, seed) == null) continue;
                int materials = c.Materials;
                int reserve = c.Reserve.Count;
                int reward = weak.MaterialsPerChief + HunterRules.Reward(c.Company.Rank(c.Offers[0].Hunter));
                EncounterResult r = CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Cheater, seed));
                if (!r.Won) continue;
                Assert.IsTrue(r.RunOver);
                Assert.IsTrue(r.RunWon);
                Assert.AreEqual(1, c.RunsWon);
                Assert.IsFalse(r.Joined, "the hunters never join");
                Assert.AreEqual(0, r.JoinedFighters.Count);
                Assert.IsNull(r.AssistUnlocked);
                Assert.IsNull(r.NextTribe);
                // From rules 6 the hunt grades you as a chief with his kit does (22 Oct 2026, "The hunters' boss, the hunt").
                Assert.IsTrue(r.Graded);
                int graded = reward * (100 + ChiefKits.GradeMaterialsPercent * (int)r.Grade) / 100;
                // From rules 12 the streak multiplies it (Campaign.Streak).
                Assert.AreEqual(graded * Math.Max(1, r.Streak), r.MaterialsEarned, "the hunt pays chief materials plus its captain's rank reward, more for a better grade");
                Assert.AreEqual(materials + graded * Math.Max(1, r.Streak), c.Materials);
                Assert.AreEqual(0, r.EnemyLessons.Count, "the hunters keep no memory");
                Assert.AreEqual(0, c.Mind(AlphaHunt.Id).Lessons.Count);
                Assert.AreEqual(CampaignStage.ChooseTribe, c.Stage, "a won run ends back at camp");
                Assert.IsNull(c.Target);
                Assert.IsFalse(c.HasJoined(AlphaHunt.Id));
                Assert.IsFalse(c.HasJoined(S), "the run's tribes reset with it");
                Assert.Greater(reserve, 0);
                return;
            }
            Assert.Fail("the hunters were never broken in sixty campaigns");
        }

        [Test]
        public void LosingToTheHuntersCostsFightersUntilYouCannotFieldThree()
        {
            // The 13 Oct roster as lives, which rules 11 replaced and old saves still play.
            CampaignTuning oldRules = EasyPath();
            oldRules.RulesCap = Campaign.RaidChiefRules;
            for (ulong seed = 420; seed < 470; seed++)
            {
                Campaign c = Ready(seed, oldRules);
                c.ChooseTribe(S);
                if (PlayToTheHunt(c, seed) == null) continue;

                // The hunters at full strength, and nobody giving orders. Since 13 Oct 2026 your roster is your lives:
                // each loss costs the fallen and one captured, and the hunters come again until you cannot field
                // Campaign.MinSquad.
                c = CampaignSave.Read(CampaignSave.Write(c), new CampaignTuning { RulesCap = Campaign.RaidChiefRules });
                EncounterResult r = null;
                for (int tries = 0; tries < 20 && c.Stage == CampaignStage.InRun; tries++)
                {
                    int before = c.Reserve.Count;
                    r = CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.Passive, seed + (ulong)tries));
                    if (r.Won) break;
                    Assert.Less(r.FightersLeft, before, "a lost fight costs at least one fighter");
                    if (!r.RunOver) Assert.AreEqual(EncounterKind.AlphaHunt, c.Offers[0].Kind, "the hunters come again");
                }
                if (r == null || r.Won) continue;
                Assert.IsTrue(r.RunOver);
                Assert.Less(r.FightersLeft, Campaign.MinSquad);
                Assert.IsFalse(r.RunWon);
                Assert.AreEqual(0, c.RunsWon);
                Assert.AreEqual(CampaignStage.ChooseTribe, c.Stage);
                CollectionAssert.AreEqual(new[] { CampaignTribes.ClarthId }, c.Joined);
                return;
            }
            Assert.Fail("giving no orders never lost to the hunters in fifty campaigns");
        }

        [Test]
        public void AHuntClosedHalfwayPlaysOnFromTheSameTick()
        {
            for (ulong seed = 470; seed < 520; seed++)
            {
                Campaign c = Ready(seed, EasyPath());
                c.ChooseTribe(S);
                if (PlayToTheHunt(c, seed) == null) continue;

                // Waiting for the hunt.
                string waiting = CampaignSave.Write(c);
                Campaign read = CampaignSave.Read(waiting, EasyPath());
                Assert.IsEmpty(read.LoadNote);
                Assert.AreSame(AlphaHunt.Tribe, read.Target);
                Assert.AreEqual(EncounterKind.AlphaHunt, read.Offers[0].Kind);
                Assert.AreEqual(c.Offers[0].EnemyFighters, read.Offers[0].EnemyFighters);
                Assert.AreEqual(waiting, CampaignSave.Write(read));

                // Halfway through it.
                CampaignFight fight = c.StartEncounter(0);
                var pilot = new CampaignPilot(PilotStyle.Reference, seed);
                while (!fight.Over && fight.Sim.CurrentTick < 600)
                {
                    pilot.Step(fight);
                    fight.Tick();
                }
                string halfway = CampaignSave.Write(c);
                Campaign back = CampaignSave.Read(halfway, EasyPath());
                Assert.IsEmpty(back.LoadNote, "the hunt did not play back");
                Assert.IsNotNull(back.Active);
                Assert.AreEqual(fight.Session.Checksum, back.Active.Session.Checksum);
                Assert.AreEqual(halfway, CampaignSave.Write(back));
                return;
            }
            Assert.Fail("the reference pilot never reached the hunt in fifty campaigns");
        }

        [Test]
        public void TheHuntersArriveKnowingWhatTheTribesKnowAndLearnNothing()
        {
            Campaign c = Ready(522);
            foreach (Campaign k in new[] { c, CampaignSave.Read(CampaignSave.Write(c)) })
            {
                TribeMind hunters = k.Mind(AlphaHunt.Id);
                Assert.IsNotNull(hunters);
                Assert.IsTrue(hunters.HasHabit(Habit.HoldOnUnleash));
                Assert.IsTrue(hunters.HasHabit(Habit.CoverFocused));
                Assert.IsTrue(hunters.HasHabit(Habit.PressOnFallBack));
            }
            string before = c.Mind(AlphaHunt.Id).Snapshot();
            CampaignPilot.PlayRun(c, B, PilotStyle.Reference, 522);
            Assert.AreEqual(before, c.Mind(AlphaHunt.Id).Snapshot(), "the hunters come back the same every run");
        }

        [Test, Category("Balance"), Ignore(Sprint1Tests.ParkedForTuning)]
        public void UnleashSpamDoesNotBeatTheHunt()
        {
            // Jacob, 11 Oct 2026: as first built, Unleash spam won 90% of full run hunts. Now the hunters
            // arrive knowing to Hold when you Unleash.
            int played = 0, spamWon = 0;
            for (ulong seed = 600; seed < 700 && played < 30; seed++)
            {
                Campaign c = Ready(seed);
                c.StartRun();
                if (PlayToTheHunt(c, seed) == null) continue;
                played++;
                if (CampaignPilot.PlayFight(c, c.StartEncounter(0), new CampaignPilot(PilotStyle.UnleashSpam, seed)).Won) spamWon++;
            }
            TestContext.WriteLine("Unleash spam won " + spamWon + " of " + played + " full run hunts.");
            Assert.GreaterOrEqual(played, 10, "the reference pilot rarely reached the hunt");
            Assert.LessOrEqual(spamWon * 100 / played, 25, "Unleash spam won " + spamWon + " of " + played + " hunts");
        }

        [Test]
        public void NoRunStartsAtTheHunters()
        {
            Assert.IsNull(CampaignTribes.Find(AlphaHunt.Id), "the hunters are not a tribe on the path");
            CollectionAssert.DoesNotContain(CampaignTribes.All, AlphaHunt.Tribe);
            Assert.AreNotSame(AlphaHunt.Tribe, Campaign.NextTribe(CampaignTribes.Skullpeak));
            Campaign c = Ready(520);
            Assert.Throws<ArgumentException>(() => c.ChooseTribe(AlphaHunt.Id));
            Assert.IsNotNull(c.Mind(AlphaHunt.Id), "screens can ask what the hunters remember: nothing");
        }

        [Test]
        public void ASavedHuntAgainstTheWrongSideIsRefused()
        {
            Campaign c = Ready(521);
            c.StartRun();
            string saved = CampaignSave.Write(c);
            int at = saved.IndexOf("\noffer\t", StringComparison.Ordinal);
            Assert.Greater(at, 0);
            int end = saved.IndexOf('\n', at + 1);
            string broken = saved.Substring(0, at) + "\noffer\t" + ((int)EncounterKind.AlphaHunt) + saved.Substring(end);
            Campaign read = CampaignSave.Read(broken);
            Assert.AreEqual(CampaignStage.ChooseTribe, read.Stage, "Bloodridge cannot offer the hunt");
            Assert.IsNotEmpty(read.LoadNote);

            string hunted = saved.Replace("\nrun\t" + c.Target.Id + "\t", "\nrun\t" + AlphaHunt.Id + "\t");
            Assert.AreNotEqual(saved, hunted);
            Campaign odd = CampaignSave.Read(hunted);
            Assert.AreEqual(CampaignStage.ChooseTribe, odd.Stage, "the hunters only offer the hunt");
            Assert.IsNotEmpty(odd.LoadNote);
        }
    }
}
