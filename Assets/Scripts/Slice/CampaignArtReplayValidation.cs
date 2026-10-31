using System.Collections;
using System.IO;
using System.Linq;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Opt-in player checks with historical test fixtures and isolated saves only.
        IEnumerator ValidateReplayCompatibility()
        {
            yield return null;
            string dir = Argument("-ksReplayValidate");
            _fight = null; _playback = null; _pilotOn = false; _paused = true;
            _campaign = CampaignSave.Read(File.ReadAllText(Path.Combine(dir, "replay-format4-20261017.txt")));
            SetPage(Page.ChooseTribe);
            string before = CampaignSave.Write(_campaign);
            var incompatible = _campaign.Archive.Find("camp.3.raiders.trial");
            Check(incompatible != null, "preserved pre-strength spell recording loaded");
            ShowArchivedFight(incompatible); yield return null;
            Check(Click("replay-watch"), "historical Watch Fight raycast"); yield return null;
            Check(_playback == null && _page != Page.Playback && _modal != null,
                "mismatched replay stays in recorded history without drawing reconstructed combat");
            Check(!FightPlayback.TryOpen(_campaign.Archive, incompatible.FightId, out _, out string why) &&
                why.Contains("history is kept"), "unavailable playback explains retained history");
            Capture("01-unavailable-history");
            Check(Click("replay-check"), "historical Replay Check raycast"); yield return null;
            Check(_playback == null && _modal != null, "failed verification preserves the history screen");
            CloseModal();

            var compatible = _campaign.Archive.Find("camp.2.raiders.warparty");
            ShowArchivedFight(compatible); yield return null;
            Check(Click("replay-watch"), "compatible old Watch Fight raycast"); yield return null;
            Check(_page == Page.Playback && _paused && _playback.Tick == 0, "compatible old fight opens paused");
            Check(Click("pause"), "old replay plays through its control");
            for (int i = 0; i < 20; i++) yield return null;
            Check(_playback.Tick > 0, "old replay advances in player update");
            Check(Click("pause"), "old replay pauses through its control"); yield return null;
            Capture("02-old-replay");
            Check(Click("playback-next"), "old replay seeks ahead"); yield return null;
            Check(Click("playback-prev"), "old replay seeks back"); yield return null;
            Check(Click("playback-end"), "old replay seeks to end"); yield return null;
            Check(_playback.AtEnd && _playback.Matched && _playback.Failure.Length == 0,
                "old replay finishes with matching state and history");
            Capture("03-old-replay-matched");
            Check(Click("playback-start"), "old replay restarts"); yield return null;
            Check(_playback.Tick == 0 && !_playback.Matched, "restart returns to first tick");
            Check(Click("playback-back"), "old replay returns to history"); yield return null;
            Check(before == CampaignSave.Write(_campaign), "watching and refusing old fights leave the campaign unchanged");
            CloseModal();

            _campaign = Campaign.New(921);
            Check(CampaignPilot.PlayTutorial(_campaign, PilotStyle.Reference, 921, 10), "recorded current tutorial fixture");
            SetPage(Page.ChooseTribe);
            var current = _campaign.Archive.Fights.Last();
            Check(current.RulesIdentity == "ks-battle-20261018-1", "new recording carries corrected rules identity");
            ShowArchivedFight(current); yield return null;
            Check(Click("replay-watch"), "current Watch Fight raycast"); yield return null;
            Check(_playback != null && _page == Page.Playback, "current recording opens");
            Check(Click("playback-end"), "current replay seeks to end"); yield return null;
            Check(_playback.Matched, "current recording replays exactly in built player");
            Capture("04-current-replay-matched");
            Check(Click("playback-back"), "current replay returns to history"); yield return null;
            CloseModal();
            Save();
        }
    }
}
