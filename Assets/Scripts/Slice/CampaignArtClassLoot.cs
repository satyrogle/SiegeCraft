using System.Collections.Generic;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;
using UnityEngine;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        CampaignGear TakenSpoils(EncounterResult result)
        {
            foreach (var piece in result.SpoilsChoices)
                if (_campaign.FindGear(piece.Id) != null) return piece;
            return null;
        }

        bool PendingSpoils(EncounterResult result)
        {
            return result.SpoilsOffered != null && _campaign.Spoils != null &&
                result.SpoilsOffered.Id == _campaign.Spoils.Id;
        }

        static string GearClass(CampaignGear piece) => piece.ClassIndex == CampaignGear.AnyClass
            ? "ANY CLASS" : SliceClasses.Name((UnitClass)piece.ClassIndex);

        void GearPickerHint(PxText label, CampaignFighter fighter, CampaignGear piece, int page, int pages)
        {
            var texts = new List<string>();
            var colours = new List<Color32>();
            var lines = new PxLines().Add(FighterName(fighter) + (pages > 1 ? " / Page " + (page + 1) + " of " + pages : ""), PxGold);
            if (piece == null)
                lines.Add("No gear equipped. Point at a piece or select it to read its effect.", PxBone)
                    .Add(PalFloorRun&&fighter.Rank>=PalStoreRules.SecondSlotRank?"Two gear slots. Slot 2 requires an unused piece.":"One piece each. Equipping moves it from its current wearer.", PxMuted);
            else
            {
                lines.Add(piece.Name, PxBone).Add(piece.Describe(), piece.Fits(fighter.Class) ? PxMint : PxMuted);
                if(ClassSpells.IsCombo(piece.Bend)&&ThemePresentation)lines.Add(ComboThemeText(piece.Bend,piece.Rank),ComboColour(piece.Bend));
            }
            for (int i = 0; i < lines.texts.Count; i++)
                foreach (string line in _tinyFont.Wrap(lines.texts[i].ToUpperInvariant(), 636, 1))
                {
                    if (texts.Count > 0) { texts.Add("\n"); colours.Add(PxBone); }
                    texts.Add(line); colours.Add(lines.colours[i]);
                }
            SetPx(label, texts.ToArray(), colours.ToArray());
        }

        void ShowSpoilsChoices()
        {
            if(CombinedRewardPending){ShowRewardChoices();return;}
            if (_result == null || !PendingSpoils(_result)) return;
            // Duels and migrated receipts can both offer one piece. Preview before spending.
            int count = _campaign.SpoilsChoices.Count;
            CloseModal(); Modal("STRIP ONE OR BANK", "");
            PxWrite(_modal, 40, 62, 640, new PxLines()
                .Add("Keep " + _result.MaterialsEarned + " materials, or exchange them for ONE piece.", PxGold)
                .Add(count > 1 ? "The other pieces are left behind. Equip the chosen piece on the squad page."
                    : "Equip the piece on the squad page after taking it.", PxMuted));
            for (int i = 0; i < count; i++)
            {
                int choice = i; var piece = _campaign.SpoilsChoices[i]; int x = (720 - count * 224 + 16) / 2 + i * 224;
                var lines = new PxLines().Add(GearClass(piece) + (piece.ClassIndex == CampaignGear.AnyClass ? "" : " ONLY"), PxGold)
                    .Add(piece.Name, PxBone).Gap();
                if (piece.Bend == ClassBend.None) lines.Add(piece.Describe(), PxMint);
                else lines.Add(piece.Tier + " / " + CampaignGearRules.StatText(piece.Stat, piece.Bonus), PxMint)
                    .Gap().Add(ClassSpells.Name(piece.Bend), PxGold).Add(ClassSpells.Describe(piece.Bend), PxBone);
                bool allowed = _campaign.CanTakeSpoils(choice, out string why);
                if (!allowed) lines.Gap().Add(why, PxRed);
                PxPanel(x, 105, 208, 236, lines, _modal);
                PxButton("strip-" + choice, "STRIP THIS", x + 14, 346, 180, 28, () => TakeClassSpoils(choice), _modal)
                    .interactable = allowed;
            }
            PxButton("bank-spoils", "KEEP MATERIALS", 480, 398, 208, 28, () => { CloseModal(); AfterResult(); }, _modal);
            SelectControl("strip-0");
        }

        void TakeClassSpoils(int choice)
        {
            if (!_campaign.TryTakeSpoils(choice, out string why)) { _notice = why; return; }
            Save(); SetPage(_result!=null?Page.Result:_page);
        }
    }
}
