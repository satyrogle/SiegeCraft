using System.IO;
using System.Linq;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    public class ReplayCompatibilityTests
    {
        // These fixture files are byte-for-byte historical validation saves, not current recordings
        // relabelled as old. Sources below are relative to C:/Users/jacob/.codex/scratch/:
        // format1: ks-sprint-20261012/r2-sprint/playback-after.txt
        // format3: ks-art-audit-fixes-20261014/default-moving/end-state.txt
        // format4: ks-combined-20261017/sprint/playback-after.txt
        // format7: ks-heavy-frontend-20261018/validation-b/live-save.txt
        // format9: ks-chief-sprint-20261019/heavy2/live-save.txt
        // A later save can contain a mixture of archive formats and rule vintages.
        static string Fixture(string name) => File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath,
            "Tests/EditMode/Fixtures", name));

        [TestCase("replay-format1-20261012.txt", 8, 8)]
        [TestCase("replay-format3-20261014.txt", 2, 2)]
        [TestCase("replay-format4-20261017.txt", 5, 6)]
        [TestCase("replay-format7-20261018.txt", 5, 7)]
        [TestCase("replay-format9-20261019.txt", 6, 6)]
        public void HistoricalRecordingsOnlyOpenWhenTheirStateAndHistoryStillMatch(string file, int playable, int historical)
        {
            string original = Fixture(file);
            var c = CampaignSave.Read(original);
            string before = CampaignSave.Write(c);
            int matched = 0, unavailable = 0;
            foreach (var fight in c.Archive.Fights)
            {
                bool verified = c.Archive.TryReplay(fight.FightId, out _, out string verification);
                bool opened = FightPlayback.TryOpen(c.Archive, fight.FightId, out var p, out string why);
                Assert.AreEqual(verified, opened, fight.FightId + ": " + why);
                if (!verified)
                {
                    unavailable++;
                    Assert.IsNull(p, "never draw a different reconstruction as the recorded fight");
                    StringAssert.Contains("history is kept", why);
                    Assert.IsNotEmpty(verification);
                    continue;
                }
                matched++;
                Assert.IsTrue(p.Seek(p.Length / 2), p.Failure);
                Assert.IsTrue(p.Seek(0), p.Failure);
                Assert.IsTrue(p.Seek(p.Length), p.Failure);
                Assert.IsTrue(p.AtEnd && p.Matched);
                p.Restart();
                while (p.Step()) { }
                Assert.IsTrue(p.Matched, p.Failure);
            }
            Assert.AreEqual(playable, matched);
            Assert.AreEqual(historical, unavailable);
            Assert.AreEqual(before, CampaignSave.Write(c), "watching and refusal cannot change the campaign");
            var migrated = CampaignSave.Read(before);
            Assert.AreEqual(before, CampaignSave.Write(migrated));
            CollectionAssert.AreEqual(original.Split('\n').Where(l => l.StartsWith("archive\t")).ToArray(),
                before.Split('\n').Where(l => l.StartsWith("archive\t")).ToArray(), "retain the original evidence bytes");
            CollectionAssert.AreEqual(original.Split('\n').Where(l => l.StartsWith("names\t")).ToArray(),
                before.Split('\n').Where(l => l.StartsWith("names\t")).ToArray(), "retain the recorded identities");
        }

        // Produced by the last shipped Sprint 3 build, before the rules-identity correction.
        [TestCase("active-stoneveil-20261019.txt")]
        [TestCase("active-thundercoast-20261019.txt")]
        public void PreviousBuildActiveFightSurvivesIdentityMigration(string file)
        {
            string old = Fixture(file);
            string checksum = old.Split('\n').Single(l => l.StartsWith("proof\t")).Split('\t')[3];
            var c = CampaignSave.Read(old);
            Assert.IsEmpty(c.LoadNote);
            Assert.IsNotNull(c.Active);
            Assert.AreEqual(checksum, c.Active.Session.Checksum);
            var again = CampaignSave.Read(CampaignSave.Write(c));
            Assert.IsEmpty(again.LoadNote);
            Assert.AreEqual(checksum, again.Active.Session.Checksum);
            CollectionAssert.AreEqual(c.RunRoute.Select(t => t.Id), again.RunRoute.Select(t => t.Id));
            for (int i = 0; i < 50 && !c.Active.Over; i++) { c.Active.Tick(); again.Active.Tick(); }
            Assert.AreEqual(c.Active.Session.Checksum, again.Active.Session.Checksum);

            // The old identity is not permission to accept a damaged active-state proof.
            string proof = old.Split('\n').Single(l => l.StartsWith("proof\t"));
            string payload = old.Substring(old.IndexOf("kindred-siege-campaign\t", System.StringComparison.Ordinal));
            var damaged = CampaignSave.Read(payload.Replace(proof, proof.Replace(checksum, "0000000000000000")));
            Assert.IsNull(damaged.Active);
            Assert.AreEqual(CampaignSave.FightLostNote, damaged.LoadNote);
            Assert.AreEqual(c.Materials, damaged.Materials);
            Assert.AreEqual(c.RunsStarted, damaged.RunsStarted);
        }

        [Test]
        public void NewRecordingsIdentifyTheCurrentSpellStrengthRules()
        {
            var c = Campaign.New(921);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, 921, 10));
            var recorded = c.Archive.Fights[0];
            Assert.AreEqual("ks-battle-20261018-1", recorded.RulesIdentity);
            var read = CampaignSave.Read(CampaignSave.Write(c));
            Assert.AreEqual(recorded.RulesIdentity, read.Archive.Find(recorded.FightId).RulesIdentity);
            Assert.IsTrue(FightPlayback.TryOpen(read.Archive, recorded.FightId, out var p, out string why), why);
            Assert.IsTrue(p.Seek(p.Length), p.Failure);
            Assert.IsTrue(p.Matched);
        }
    }
}
