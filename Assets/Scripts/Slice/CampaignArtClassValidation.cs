using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KindredSiege.Sim;
using KindredSiege.Sim.Battle;
using KindredSiege.Sim.Learning;

namespace KindredSiege.Slice
{
    public sealed partial class CampaignArtLoop
    {
        // Opt-in UI regression fixtures. These are authored test fights, not balance measurements.
        static UnitSpec ClassFixtureUnit(string id, UnitClass c) => new UnitSpec(id, c);

        ArchivedFight ClassFixture(ulong seed, UnitSpec[] ours, UnitSpec[] theirs, bool chief = false, bool casts = false)
        {
            var setup = new BattleSetup("camp." + seed + ".raiders." + (chief ? "chief" : "warparty"),
                seed, ours, theirs, new TacticsProfile(), new TacticsProfile(),
                BattleSetup.DefaultTickLimit, 5, chief ? NerveRules.On() : NerveRules.Off,
                10, 400, null, casts ? new SideRules(2, 3000, false) : null,
                chief ? FightRules.CampaignChief : FightRules.Campaign);
            var sim = new BattleSimulation(setup);
            while (!sim.IsFinished) sim.Tick();
            return _campaign.Archive.Add(setup, null, sim);
        }

        void InspectClassMoment(ArchivedFight record, BattleEvent e)
        {
            WatchFight(record);
            // Events are stamped before Tick increments CurrentTick.
            SeekPlayback(e.Tick);
            _playback.Step(); DrainPlayback(); DrawBattle();
            Check(_playback.Sim.Events.Any(seen => seen.Tick == e.Tick && seen.Kind == e.Kind &&
                seen.ActorId == e.ActorId && seen.TargetId == e.TargetId), "requested event is present at the inspected moment");
        }

        void CheckClassReadouts()
        {
            string before = StateChecksum.Compute(ViewSim.OrderedStateSnapshot);
            DrawBattle(); DrawBattle();
            Check(before == StateChecksum.Compute(ViewSim.OrderedStateSnapshot), "drawing class cues leaves authoritative state unchanged");
            foreach (var f in _figures)
            {
                var r = _classReadouts[f.unit.Id]; var u = f.unit;
                bool visible = !ViewOver && u.Alive && !ChiefCutInVisible;
                bool[] marks = { u.SnareTicks > 0, u.BleedTicks > 0, u.OpeningTicks > 0, ViewSim.CoveredBy(u) != null };
                for (int i = 0; i < marks.Length; i++)
                    Check(r.marks[i].key.StartsWith((visible && marks[i] ? ClassMarkNames[i] : "") + "|", StringComparison.Ordinal),
                        u.Id + " mark follows state: " + ClassMarkNames[i]);
                Check(r.shield.All(p => p.enabled == (visible && marks[3])), u.Id + " shield follows covering Warden");
                if (r.aim != null)
                {
                    bool aiming = visible && u.AimTicks > 0 && ViewSim.FindById(u.AimTargetId)?.Alive == true;
                    Check(r.aim.All(p => p.enabled == aiming), u.Id + " aim line follows live target");
                }
            }
            CheckTribeReadouts();
        }

        IEnumerator ValidateSingleClassLoot(string dir)
        {
            var source = CampaignSave.Read(File.ReadAllText(Path.Combine(dir, "duel-after.txt")));
            var record = source.Archive.Fights.Last();
            for (int bank = 0; bank < 2; bank++)
            {
                _playback = null; _fight = null;
                // Same fixed saved duel, no command search. Only this test's tier-2 fixture is scaled.
                _campaign = CampaignSave.Read(File.ReadAllText(Path.Combine(dir, "duel-before.txt")),
                    new CampaignTuning { EnemyScaleTier2 = 40 });
                int offer = _campaign.Offers.ToList().FindIndex(e => e.Kind == EncounterKind.Duel);
                Check(offer >= 0, "fixed single-piece fixture offers a duel");
                TakeOffer(offer);
                _fight = _campaign.StartEncounter(offer);
                Check(_fight.Sim.ClassSpellsOn, "single-piece duel uses current class rules");
                ReplayResultRecord(record); yield return null;
                Check(_result.Won && _result.SpoilsChoices.Count == 1,
                    "current class duel victory offers exactly one piece");
                var piece = _result.SpoilsOffered;
                int materials = _campaign.Materials, price = _result.MaterialsEarned;
                string before = CampaignSave.Write(_campaign);
                Check(Click("take-spoils"), "single-piece result opens preview"); yield return null;
                Check(_modal != null && _buttons.ContainsKey("strip-0") && !_buttons.ContainsKey("strip-1"),
                    "single-piece preview has exactly one strip choice");
                Check(CampaignSave.Write(_campaign) == before, "opening single-piece preview spends nothing");
                Capture("09-duel-preview");
                Check(Click("modal-close"), "single-piece preview can be cancelled"); yield return null;
                Check(CampaignSave.Write(_campaign) == before, "cancelling single-piece preview changes nothing");
                Save();
                _campaign = CampaignSave.Read(File.ReadAllText(_savePath));
                SetPage(Page.Result); yield return null;
                Check(Click("take-spoils"), "single-piece preview reopens after saved campaign reload"); yield return null;
                if (bank == 1)
                {
                    Check(Click("bank-spoils"), "single-piece preview can keep materials"); yield return null;
                    Check(_campaign.Materials == materials && _campaign.FindGear(piece.Id) == null,
                        "banking single-piece reward keeps materials and grants no gear");
                    continue;
                }
                SelectControl("strip-0"); HandleUiKey(UnityEngine.KeyCode.Return); yield return null;
                Check(_modal == null && _campaign.Materials == materials - price && _campaign.FindGear(piece.Id) != null,
                    "single-piece keyboard purchase exchanges exactly once");
                Check(!_buttons.ContainsKey("take-spoils") && !_buttons.ContainsKey("strip-0") &&
                    !_campaign.CanTakeSpoils(out _),
                    "single-piece purchase cannot be repeated");
                var lines = new PxLines(); RunResultLines(lines, _result);
                Check(string.Join(" ", lines.texts).Contains(piece.Describe()), "single-piece receipt describes actual reward");
                var saved = CampaignSave.Read(File.ReadAllText(_savePath));
                Check(saved.Materials == _campaign.Materials && saved.FindGear(piece.Id)?.Bend == piece.Bend,
                    "single-piece purchase and class bend survive save reload");
                Capture("10-duel-receipt");
            }
        }

        IEnumerator ValidateClassFrontend()
        {
            yield return null;
            string dir = Argument("-ksClassValidate");
            _savePath = Path.Combine(_capture, "class-save.txt");
            _pilotOn = false; _paused = true;
            var source = CampaignSave.Read(File.ReadAllText(Path.Combine(dir, "loot-after.txt")));
            var lootRecord = source.Archive.Fights.Last();
            int offer = int.Parse(File.ReadAllText(Path.Combine(dir, "loot-offer.txt")));
            string beforeFight = File.ReadAllText(Path.Combine(dir, "loot-before.txt"));
            var fixtureTuning = new CampaignTuning { EnemyScaleTier1 = 40 };
            for (int choice = 0; choice < 4; choice++)
            {
                _playback = null;
                _campaign = CampaignSave.Read(beforeFight, fixtureTuning);
                _fight = _campaign.StartEncounter(offer);
                ReplayResultRecord(lootRecord);
                yield return null;
                Check(_result.Won && _result.SpoilsChoices.Count == 3, "fixed class-gear victory offers three choices");
                var pieces = _result.SpoilsChoices.ToArray();
                int materials = _campaign.Materials, price = _result.MaterialsEarned;
                Check(Click("take-spoils"), "open class choices by result control");
                yield return null;
                Check(_modal != null && Enumerable.Range(0, 3).All(i => _buttons["strip-" + i].interactable),
                    "all three strip controls available");
                if (choice == 0) Capture("01-three-class-pieces");
                if (choice == 3)
                {
                    Check(Click("bank-spoils"), "bank materials by modal control");
                    yield return null;
                    Check(_campaign.Materials == materials && pieces.All(p => _campaign.FindGear(p.Id) == null),
                        "bank keeps materials and grants no piece");
                    continue;
                }
                if (choice == 2)
                {
                    SelectControl("strip-2"); HandleUiKey(UnityEngine.KeyCode.Return);
                    Check(_modal == null, "keyboard submits third choice exactly once");
                }
                else Check(Click("strip-" + choice), "strip choice " + choice + " by real control");
                yield return null;
                var selected = pieces[choice];
                Check(_campaign.Materials == materials - price && _campaign.FindGear(selected.Id) != null,
                    "selected piece costs the reward once");
                Check(pieces.Where(p => p.Id != selected.Id).All(p => _campaign.FindGear(p.Id) == null),
                    "other pieces are not granted");
                Check(_campaign.SpoilsChoices.Count == 0 && !_buttons.ContainsKey("take-spoils"),
                    "exchange cannot be repeated");
                var receiptLines = new PxLines(); RunResultLines(receiptLines, _result);
                string receipt = string.Join(" ", receiptLines.texts);
                Check(receipt.Contains(selected.Name), "receipt names actual selected piece");
                var restored = CampaignSave.Read(File.ReadAllText(_savePath));
                Check(restored.FindGear(selected.Id).Bend == selected.Bend &&
                    restored.FindGear(selected.Id).ClassIndex == selected.ClassIndex &&
                    restored.Materials == _campaign.Materials, "selected class and bend persist in saved campaign");
                Capture("02-receipt-" + choice);
                var wrong = _campaign.Reserve.First(f => !selected.Fits(f.Class));
                var right = _campaign.Reserve.First(f => selected.Fits(f.Class));
                _result = null; TakeOffer(0);
                ShowGearPicker(wrong.Id); yield return null;
                Check(!_buttons["equip-" + selected.Id].interactable && !Click("equip-" + selected.Id),
                    "wrong class cannot equip through UI");
                string unchanged = CampaignSave.Write(_campaign);
                Check(!_campaign.TryEquip(wrong.Id, selected.Id, out _) && unchanged == CampaignSave.Write(_campaign),
                    "backend also rejects wrong class without changing save");
                if (choice == 0) Capture("03-wrong-class");
                ShowGearPicker(right.Id); yield return null;
                SelectControl("equip-" + selected.Id); yield return null;
                Check(_keyboardHint.Contains(ClassSpells.Describe(selected.Bend)), "equipment selection exposes complete spell effect");
                if (choice == 0) Capture("03-class-effect");
                Check(Click("equip-" + selected.Id), "matching class equips through UI");
                yield return null;
                Check(_campaign.GearOf(right.Id)?.Id == selected.Id, "matching fighter wears selected piece");
                Check(AboutFighter(right, _campaign.Offers[0]).Contains(ClassSpells.Describe(selected.Bend)),
                    "fighter card explanation includes the spell bend");
                if (choice == 0) Capture("04-equipped");
            }

            var single = ValidateSingleClassLoot(dir);
            while (single.MoveNext()) yield return single.Current;
            _fight = null; _playback = null; _campaign = Campaign.New(1); SetPage(Page.ChooseTribe);
            var aim = ClassFixture(4, new[] { ClassFixtureUnit("clarth.a", UnitClass.Marksman) },
                new[] { ClassFixtureUnit("raiders.m", UnitClass.Medic) });
            var broken = ClassFixture(5, new[] { ClassFixtureUnit("clarth.a", UnitClass.Marksman) },
                new[] { ClassFixtureUnit("raiders.x", UnitClass.Skirmisher) });
            var snare = ClassFixture(3, new[] { ClassFixtureUnit("clarth.w", UnitClass.Warden), ClassFixtureUnit("clarth.t", UnitClass.Trapper) },
                new[] { ClassFixtureUnit("raiders.x", UnitClass.Skirmisher) });
            var rip = ClassFixture(7, new[] { new UnitSpec("clarth.k", UnitClass.Skirmisher, Row.Front, UnitRole.Fighter,
                100, 100, 0, 0, 0, 0, 0, 0, 0, 0, 2) }, new[] { ClassFixtureUnit("raiders.m", UnitClass.Medic) });
            var covered = ClassFixture(8, new[] { ClassFixtureUnit("clarth.w", UnitClass.Warden), ClassFixtureUnit("clarth.k", UnitClass.Skirmisher),
                ClassFixtureUnit("clarth.m", UnitClass.Medic) }, new[] { ClassFixtureUnit("raiders.x", UnitClass.Skirmisher),
                ClassFixtureUnit("raiders.y", UnitClass.Skirmisher) });
            var opening = ClassFixture(20, new[] { ClassFixtureUnit("clarth.a", UnitClass.Marksman), ClassFixtureUnit("clarth.k", UnitClass.Skirmisher) },
                new[] { ClassFixtureUnit("raiders.m", UnitClass.Medic) });
            var pin = ClassFixture(10, new[] { ClassFixtureUnit("clarth.t", UnitClass.Trapper), ClassFixtureUnit("clarth.a", UnitClass.Marksman) },
                new[] { ClassFixtureUnit("raiders.x", UnitClass.Skirmisher) });
            foreach (var record in new[] { aim, broken, snare, rip, covered, opening, pin })
            {
                Check(_campaign.Archive.TryReplay(record.FightId, out var sim, out _), "class fixture replays exactly: " + record.FightId);
                var moment = sim.Events.First(e => record == aim ? e.Kind == BattleEventKind.Aim :
                    record == broken ? e.Kind == BattleEventKind.Broken :
                    record == snare ? e.Kind == BattleEventKind.Mark && e.Detail == ClassSpells.SnareMark :
                    e.Kind == BattleEventKind.Cash && e.Detail == (record == rip ? ClassSpells.RipDetail :
                        record == covered ? ClassSpells.CoveredDetail : record == opening ? ClassSpells.OpeningMark : ClassSpells.PinDetail));
                InspectClassMoment(record, moment);
                yield return null;
                CheckClassReadouts();
                if (record == aim)
                    Check(_playback.Sim.FindById(moment.ActorId).AimTicks > 0, "aim fixture actually shows an active aim");
                if (moment.Kind == BattleEventKind.Cash)
                    Check(_feed.Any(t => t.Contains(" -> ")), "cash-in identifies mark and payoff");
                if (moment.Kind == BattleEventKind.Broken)
                    Check(_feed.Any(t => t.Contains("Aimed shot lost")), "lost aim gets an interruption explanation");
                Capture("05-class-" + record.Setup.Seed);
                if (record == aim)
                {
                    var marked = sim.Events.First(e => e.Kind == BattleEventKind.Mark && e.Detail == ClassSpells.OpeningMark);
                    InspectClassMoment(record, marked);
                    yield return null;
                    Check(_playback.Sim.FindById(marked.TargetId).OpeningTicks > 0, "opening is active after aimed shot");
                    CheckClassReadouts(); Capture("05-opening-mark");
                }
                long tick = _playback.Tick;
                for (int frame = 0; frame < 3; frame++) yield return null;
                Check(_playback.Tick == tick, "paused class cue does not advance simulation");
                Check(Click("playback-end"), "class replay seeks to end");
                yield return null;
                Check(_playback.Matched, "class replay reaches matching end");
                CheckClassReadouts();
                Check(Click("playback-start"), "class replay restarts");
                yield return null;
                Check(_playback.Tick == 0 && _classReadouts.Count == _figures.Count, "restart rebuilds class readout references");
                CheckClassReadouts();
                _playback = null; SetPage(Page.ChooseTribe);
            }

            var chief = ClassFixture(34, new[] { ClassFixtureUnit("clarth.a", UnitClass.Skirmisher),
                ClassFixtureUnit("clarth.b", UnitClass.Skirmisher), ClassFixtureUnit("clarth.c", UnitClass.Marksman) },
                new[] { ClassFixtureUnit("raiders.x", UnitClass.Warden),
                    new UnitSpec("raiders.chief", UnitClass.Skirmisher, Row.Front, UnitRole.Chief, 100, 100),
                    ClassFixtureUnit("raiders.r1", UnitClass.Skirmisher).AsReserve(),
                    ClassFixtureUnit("raiders.r2", UnitClass.Marksman).AsReserve() }, true, true);
            Check(_campaign.Archive.TryReplay(chief.FightId, out var chiefSim, out _), "chief rules fixture replays");
            WatchFight(chief); yield return null;
            Check(_figures.Where(f => f.unit.Benched).All(f => !f.root.gameObject.activeSelf && !f.readoutRoot.gameObject.activeSelf &&
                f.root.localEulerAngles.z == 0), "benched reserves are hidden and never tipped into death poses");
            var phase = chiefSim.Events.First(e => e.Kind == BattleEventKind.Phase);
            InspectClassMoment(chief, phase); yield return null;
            Check(_figures.All(f => !f.unit.Benched && f.root.gameObject.activeSelf), "reserves appear when phase joins them");
            Check(_feed.Any(t => t.Contains("reserves join")), "phase explains joining reserves");
            Capture("06-chief-phase");
            Check(Click("playback-start"), "restart chief replay"); yield return null;
            Check(_figures.Where(f => f.unit.Benched).All(f => !f.root.gameObject.activeSelf),
                "chief restart hides reserves again");
            _playback = null; SetPage(Page.ChooseTribe);
            var surrender = ClassFixture(31, new[] { ClassFixtureUnit("clarth.a", UnitClass.Skirmisher),
                ClassFixtureUnit("clarth.b", UnitClass.Skirmisher), ClassFixtureUnit("clarth.c", UnitClass.Marksman) },
                new[] { new UnitSpec("raiders.chief", UnitClass.Skirmisher, Row.Front, UnitRole.Chief, 100, 100),
                    ClassFixtureUnit("raiders.x", UnitClass.Warden) }, true);
            Check(_campaign.Archive.TryReplay(surrender.FightId, out var yieldSim, out _), "surrender fixture replays");
            var yielded = yieldSim.Events.First(e => e.Kind == BattleEventKind.Yield);
            InspectClassMoment(surrender, yielded); yield return null;
            Check(_classReadouts[yielded.ActorId].surrender.key.StartsWith("YIELDED|", StringComparison.Ordinal) &&
                _classReadouts[yielded.ActorId].surrender.key.Contains("HOLD TO SPARE"),
                "yielded chief shows persistent Hold-to-spare cue");
            var yieldedFigure = FindFigure(yielded.ActorId);
            Check(yieldedFigure.drawnClip == yieldedFigure.idle, "yielded chief stops showing an attack or cast");
            Capture("07-chief-yields");
            _playback = null; SetPage(Page.ChooseTribe);

            // Genuine historical evidence, not a newly generated fight with a changed header.
            _campaign = CampaignSave.Read(File.ReadAllText(Path.Combine(dir, "replay-format4-20261017.txt")));
            var old = _campaign.Archive.Find("camp.2.raiders.warparty");
            WatchFight(old); yield return null;
            Check(!_playback.Sim.ClassSpellsOn && _classReadouts.Count == 0, "historical fight does not acquire class spells or marks");
            Check(Click("playback-end"), "old replay seeks to end after class replay"); yield return null;
            Check(_playback.Matched, "historical replay still matches after new class and chief rules");
            Capture("08-historical-replay");
        }
    }
}
