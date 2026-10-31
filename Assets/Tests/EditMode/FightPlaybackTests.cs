using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using NUnit.Framework;

namespace KindredSiege.Sim.Tests
{
    /// Watching a recorded fight (Jacob's go, 12 Oct 2026): FightPlayback plays an archived fight back one
    /// tick at a time, exactly as it went, so a screen can play, pause, restart, change speed and jump to a
    /// lesson's window without ever changing the campaign.
    public class FightPlaybackTests
    {
        // A campaign fight played live with the reference pilot's orders, every tick's state written down.
        private static Campaign Played(ulong seed, out string fightId, out List<string> states)
        {
            Campaign c = Campaign.New(seed);
            Assert.IsTrue(CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10), "the tutorial was never won");
            c.StartRun();
            CampaignFight fight = c.StartEncounter(0);
            var pilot = new CampaignPilot(PilotStyle.Reference, seed);
            states = new List<string> { fight.Session.Checksum };
            while (!fight.Over)
            {
                pilot.Step(fight);
                fight.Tick();
                states.Add(fight.Session.Checksum);
            }
            fightId = fight.Setup.FightId;
            c.Finish(fight);
            return c;
        }

        private static string Checksum(FightPlayback p)
        {
            return StateChecksum.Compute(p.Sim.OrderedStateSnapshot);
        }

        [Test]
        public void EveryTickPlaysBackTheWayItWent()
        {
            foreach (ulong seed in new ulong[] { 1101, 1102, 1103 })
            {
                Campaign c = Played(seed, out string fightId, out List<string> states);
                Assert.Greater(c.Archive.Find(fightId).Log.Count, 0, "the pilot gave orders, so playback has to give them too");
                Assert.IsTrue(FightPlayback.TryOpen(c.Archive, fightId, out FightPlayback p, out string why), why);
                Assert.AreEqual(0, p.Tick);
                Assert.AreEqual(states.Count - 1, p.Length);
                Assert.AreEqual(states[0], Checksum(p));
                for (int t = 1; t < states.Count; t++)
                {
                    Assert.IsTrue(p.Step(), p.Failure);
                    Assert.AreEqual(states[t], Checksum(p), "tick " + t);
                }
                Assert.IsTrue(p.AtEnd);
                Assert.IsTrue(p.Matched, p.Failure);
                Assert.IsFalse(p.Step(), "nothing plays after the end");
            }
        }

        [Test]
        public void SeekGoesToAnyTickForwardOrBack()
        {
            Campaign c = Played(1104, out string fightId, out List<string> states);
            Assert.IsTrue(FightPlayback.TryOpen(c.Archive, fightId, out FightPlayback p, out string why), why);
            long middle = p.Length / 2;
            Assert.IsTrue(p.Seek(middle), p.Failure);
            Assert.AreEqual(middle, p.Tick);
            Assert.AreEqual(states[(int)middle], Checksum(p));
            Assert.IsTrue(p.Seek(3), p.Failure);
            Assert.AreEqual(3, p.Tick);
            Assert.AreEqual(states[3], Checksum(p), "going back starts again and plays on");
            Assert.IsTrue(p.Seek(p.Length), p.Failure);
            Assert.IsTrue(p.Matched);
            p.Restart();
            Assert.AreEqual(0, p.Tick);
            Assert.IsFalse(p.Matched);
            Assert.AreEqual(states[0], Checksum(p));
            Assert.IsFalse(p.Seek(-1));
            Assert.IsFalse(p.Seek(p.Length + 1));
        }

        [Test]
        public void ALessonCanBeWatchedFromItsWindow()
        {
            // The rematch clue names a lesson and the fight that taught it (C7).
            for (ulong seed = 1105; seed < 1145; seed++)
            {
                Campaign c = Campaign.New(seed);
                if (!CampaignPilot.PlayTutorial(c, PilotStyle.Reference, seed, 10)) continue;
                CampaignPilot.PlayRun(c, CampaignTribes.BloodridgeId, PilotStyle.Reference, seed);
                RematchClue clue = RematchClue.For(c, CampaignTribes.BloodridgeId);
                if (clue == null) continue;
                Assert.IsTrue(FightPlayback.TryOpen(c.Archive, clue.FightId, out FightPlayback p, out string why), why);
                Assert.IsTrue(p.Seek(clue.Lesson.FromTick), p.Failure);
                Assert.AreEqual(clue.Lesson.FromTick, p.Tick);
                while (p.Tick < clue.Lesson.ToTick) Assert.IsTrue(p.Step(), p.Failure);
                Assert.IsTrue(p.Seek(p.Length), p.Failure);
                Assert.IsTrue(p.Matched);
                return;
            }
            Assert.Fail("no campaign in forty left Bloodridge with a clue to watch");
        }

        [Test]
        public void WatchingAFightNeverChangesTheCampaign()
        {
            Campaign c = Played(1145, out string fightId, out _);
            string before = CampaignSave.Write(c);
            Assert.IsTrue(FightPlayback.TryOpen(c.Archive, fightId, out FightPlayback p, out string why), why);
            p.Seek(p.Length);
            p.Restart();
            p.Seek(p.Length / 3);
            Assert.AreEqual(before, CampaignSave.Write(c));
            Assert.IsTrue(c.Archive.TryReplay(fightId, out _, out why), why);
        }

        [Test]
        public void AFightThatCannotPlayBackSaysWhy()
        {
            Campaign c = Played(1146, out _, out _);
            Assert.IsFalse(FightPlayback.TryOpen(c.Archive, "no.such.fight", out FightPlayback none, out string why));
            Assert.IsNull(none);
            StringAssert.Contains("no.such.fight", why);

            // A fight saved by a build with other battle rules keeps its history but does not play back.
            ArchivedFight first = c.Archive.Fights[0];
            string saved = CampaignSave.Write(c);
            string line = saved.Split('\n').First(l => l.StartsWith("archive\t"));
            byte[] bytes = Inflate(line.Substring(8));
            int at = Find(bytes, Encoding.UTF8.GetBytes(first.RulesIdentity));
            Assert.GreaterOrEqual(at, 0);
            bytes[at] = (byte)'X';
            Campaign read = CampaignSave.Read(saved.Replace(line, "archive\t" + Deflate(bytes)));
            Assert.IsFalse(FightPlayback.TryOpen(read.Archive, first.FightId, out _, out why));
            StringAssert.Contains("different battle rules", why);
        }

        private static int Find(byte[] bytes, byte[] token)
        {
            for (int i = 0; i <= bytes.Length - token.Length; i++)
                if (bytes.Skip(i).Take(token.Length).SequenceEqual(token)) return i;
            return -1;
        }

        private static byte[] Inflate(string text)
        {
            using (var input = new MemoryStream(Convert.FromBase64String(text)))
            using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                deflate.CopyTo(output);
                return output.ToArray();
            }
        }

        private static string Deflate(byte[] bytes)
        {
            using (var output = new MemoryStream())
            {
                using (var deflate = new DeflateStream(output, CompressionLevel.Fastest, true)) deflate.Write(bytes, 0, bytes.Length);
                return Convert.ToBase64String(output.ToArray());
            }
        }
    }
}
