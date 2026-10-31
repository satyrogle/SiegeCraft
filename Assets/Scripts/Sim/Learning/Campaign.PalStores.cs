using System;
using System.Collections.Generic;
using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// The Pals' stores (Jacob, 31 Oct 2026: "Go, sprint it"; DESIGN_LOCKS, "The floor under the three Pals"; campaign rules 23):
    /// each Pal's picks this run in its own place, each with its own save lines (CampaignSave): the Glutton's lent talents, the
    /// Shade's routes at their forms, the Whisperer's habit for each class and the squad's pair, and each fighter's rank and
    /// second slot (CampaignFighter.Rank and GearId2). They go when the run does. From rules 24 (Jacob, 31 Oct 2026: "Go, build
    /// her"; DESIGN_LOCKS, "Habits come with the game, the Whisperer upgrades them") the habits are set at camp and kept, and the
    /// Whisperer's road card fills her part for the run: a rank up, a pair or a habit upgrade.
    public sealed partial class Campaign
    {
        private readonly List<HeldTalent> _talents = new List<HeldTalent>();
        private readonly List<HeldRoute> _routes = new List<HeldRoute>();
        private readonly List<HeldHabit> _habits = new List<HeldHabit>();

        /// The talents the Glutton lent this run, in the order lent.
        public IReadOnlyList<HeldTalent> Talents => _talents;

        /// The Shade's routes this run, at their forms, in the order first taken.
        public IReadOnlyList<HeldRoute> ShadeRoutes => _routes;

        /// The Whisperer's habits, one a class at most, in the order set. From rules 24 they are set at camp and kept between runs,
        /// and an upgrade lasts the run.
        public IReadOnlyList<HeldHabit> Habits => _habits;

        /// The squad's pair this run, None for none.
        public SquadPair Pair { get; private set; }

        // Whether this run plays the floor: its rules version, under the tuning's cap.
        private bool FloorRun => Math.Min(RulesVersion, _tuning.RulesCap) >= FloorRules;

        /// Whether this campaign plays the Whisperer (rules 24): its rules version, under the tuning's cap.
        public bool WhispererRun => Math.Min(RulesVersion, _tuning.RulesCap) >= WhispererRules;

        /// The Glutton lends this talent for the run. False when the run holds it already, or plays rules below 23.
        public bool TryLendTalent(GluttonTalent talent)
        {
            if (!FloorRun || talent <= GluttonTalent.None || (int)talent > PalStoreRules.TalentCount || FindTalent(talent) != null)
                return false;
            _talents.Add(new HeldTalent(talent, false));
            return true;
        }

        /// Feed doubles one talent the run holds, once a run.
        public bool TryFeedTalent(GluttonTalent talent)
        {
            HeldTalent held = FindTalent(talent);
            if (held == null) return false;
            for (int i = 0; i < _talents.Count; i++) if (_talents[i].Fed) return false;
            held.Fed = true;
            return true;
        }

        /// The Shade's pick: a route the run holds gets its next form, up to ShadeRules.MaxForm; a new route takes its button
        /// if the button is plain and the run enhances fewer than two.
        public bool TryTakeRoute(ShadeRoute route)
        {
            if (!FloorRun || route <= ShadeRoute.None || (int)route >= ShadeRules.RouteCount) return false;
            HeldRoute held = FindRoute(route);
            if (held != null)
            {
                if (held.Form >= ShadeRules.MaxForm) return false;
                held.Form++;
                return true;
            }
            if (_routes.Count >= ShadeRules.MaxButtons) return false;
            Directive button = ShadeRules.ButtonOf(route);
            for (int i = 0; i < _routes.Count; i++) if (ShadeRules.ButtonOf(_routes[i].Route) == button) return false;
            _routes.Add(new HeldRoute(route, 1));
            return true;
        }

        /// The Whisperer's habit for its class, in place of the class's old one (28 Oct flip). From rules 24 habits come with the
        /// game: you set them at camp, free to switch, never in a run, and Gatekeeper is out of the slice.
        public bool TrySetHabit(WhispererHabit habit)
        {
            if (!FloorRun || habit <= WhispererHabit.None || (int)habit > PalStoreRules.HabitCount) return false;
            if (WhispererRun && (Stage == CampaignStage.InRun || habit == WhispererHabit.Gatekeeper)) return false;
            UnitClass cls = PalStoreRules.ClassOf(habit);
            for (int i = 0; i < _habits.Count; i++)
                if (_habits[i].Class == cls)
                {
                    if (_habits[i].Habit == habit) return false;
                    _habits[i] = new HeldHabit(habit);
                    return true;
                }
            _habits.Add(new HeldHabit(habit));
            return true;
        }

        /// The squad's pair, in place of the old one.
        public bool TrySetPair(SquadPair pair)
        {
            if (!FloorRun || pair <= SquadPair.None || (int)pair > PalStoreRules.PairCount || Pair == pair) return false;
            Pair = pair;
            return true;
        }

        /// A rank up for one fighter, up to PalStoreRules.MaxRank.
        public bool TryRankUp(string fighterId)
        {
            CampaignFighter f = FindFighter(fighterId);
            if (!FloorRun || f == null || f.Rank >= PalStoreRules.MaxRank) return false;
            f.Rank++;
            return true;
        }

        /// A rank 3 fighter's second slot: a piece of the run's gear his class can wear, worn by nobody, his first slot
        /// included. Not in the middle of a fight.
        public bool TryEquipSecond(string fighterId, string gearId, out string why)
        {
            why = string.Empty;
            CampaignFighter f = FindFighter(fighterId);
            CampaignGear piece = FindGear(gearId);
            if (!FloorRun) why = "The second slot comes with campaign rules " + FloorRules + ".";
            else if (_active != null) why = "Not in the middle of a fight.";
            else if (f == null) why = (fighterId ?? string.Empty) + " is not one of your fighters.";
            else if (f.Rank < PalStoreRules.SecondSlotRank) why = "Only a rank " + PalStoreRules.SecondSlotRank + " fighter has a second slot.";
            else if (piece == null) why = "That piece is not in the run's gear.";
            else if (!piece.Fits(f.Class)) why = "Only a " + SliceClasses.Name((UnitClass)piece.ClassIndex) + " can wear that.";
            else
                for (int i = 0; i < _reserve.Count; i++)
                    if (_reserve[i].GearId == piece.Id || _reserve[i].GearId2 == piece.Id) why = "Someone already wears that piece.";
            if (why.Length > 0) return false;
            f.GearId2 = piece.Id;
            return true;
        }

        // What your side brings into a fight from the stores: the Shade's routes at their forms, and from rules 24 the habits,
        // upgraded or not, and the pair. No talent sets the cheaper hook yet.
        private PalKit RunPalKit(int rules)
        {
            var routes = new List<KeyValuePair<ShadeRoute, int>>();
            for (int i = 0; i < _routes.Count; i++) routes.Add(new KeyValuePair<ShadeRoute, int>(_routes[i].Route, _routes[i].Form));
            var kit = new PalKit(routes, 0);
            if (rules < WhispererRules) return kit;
            var habits = new int[PalStoreRules.HabitCount + 1];
            for (int i = 0; i < _habits.Count; i++) habits[(int)_habits[i].Habit] = _habits[i].Upgraded ? 2 : 1;
            return kit.WithWhisper(habits, Pair);
        }

        // Each of yours in the fight brings his rank (rules 24).
        private void RankSpecs(List<UnitSpec> side)
        {
            for (int i = 0; i < side.Count; i++)
            {
                CampaignFighter f = FindFighter(side[i].Id);
                if (f != null && f.Rank > 1) side[i] = side[i].WithRank(f.Rank);
            }
        }

        // ------------------------------------------------------------ the Whisperer's road card (rules 24)

        // The road node's own dice for her three picks: a stream of its seed nothing else draws from.
        private const ulong WhisperOfferStream = 16UL;

        /// What a pick weighs in her offer: a rank up for a fighter already ranked weighs this, every other pick 1, so leaning
        /// on one star pays (the Butcher's ComboRankUpWeight). A starting value.
        public const int RankUpWeight = 2;

        // Her card: three of every rank up for one of your fighters below the top rank, every pair your squad can field that the
        // run does not hold, and an upgrade for every habit set and not yet upgraded whose class is in your squad, drawn by
        // weight from the node's own dice, in that order. Null when there is nothing, and the fight drops gear as usual.
        private FavourOffer WhisperOffer(RoadNode node)
        {
            var pool = new List<WhisperPick>();
            var weights = new List<int>();
            for (int i = 0; i < _reserve.Count; i++)
            {
                CampaignFighter f = _reserve[i];
                if (f.Rank >= PalStoreRules.MaxRank) continue;
                pool.Add(new WhisperPick(Favour.WhisperRankUp, f.Id, WhispererHabit.None, SquadPair.None));
                weights.Add(f.Rank > 1 ? RankUpWeight : 1);
            }
            for (int p = 1; p <= PalStoreRules.PairCount; p++)
            {
                var pair = (SquadPair)p;
                if (pair == Pair || !SquadHas(WhisperRules.FirstOf(pair)) || !SquadHas(WhisperRules.AnswerOf(pair))) continue;
                pool.Add(new WhisperPick(Favour.WhisperPair, string.Empty, WhispererHabit.None, pair));
                weights.Add(1);
            }
            for (int i = 0; i < _habits.Count; i++)
            {
                if (_habits[i].Upgraded || !SquadHas(_habits[i].Class)) continue;
                pool.Add(new WhisperPick(Favour.WhisperHabitUp, string.Empty, _habits[i].Habit, SquadPair.None));
                weights.Add(1);
            }
            var dice = new DeterministicRng(node.Seed, WhisperOfferStream);
            var taken = new bool[pool.Count];
            for (int n = 0; n < FavourRules.Offered && n < pool.Count; n++)
            {
                int total = 0;
                for (int i = 0; i < pool.Count; i++) if (!taken[i]) total += weights[i];
                int roll = dice.Next(total);
                for (int i = 0; i < pool.Count; i++)
                {
                    if (taken[i]) continue;
                    if (roll < weights[i])
                    {
                        taken[i] = true;
                        break;
                    }
                    roll -= weights[i];
                }
            }
            var choices = new List<Favour>();
            var picks = new List<WhisperPick>();
            for (int i = 0; i < pool.Count; i++)
            {
                if (!taken[i]) continue;
                choices.Add(pool[i].Kind);
                picks.Add(pool[i]);
            }
            return choices.Count == 0 ? null : new FavourOffer(Pal.Whisperer, choices, picks);
        }

        // Takes one of her picks; its name for the codex.
        private string TakeWhisper(WhisperPick pick)
        {
            switch (pick.Kind)
            {
                case Favour.WhisperRankUp:
                    TryRankUp(pick.FighterId);
                    return "Rank up";
                case Favour.WhisperPair:
                    TrySetPair(pick.Pair);
                    return FavourRules.PairName(pick.Pair);
                default:
                    for (int i = 0; i < _habits.Count; i++) if (_habits[i].Habit == pick.Habit) _habits[i].Upgraded = true;
                    return FavourRules.HabitName(pick.Habit) + " upgraded";
            }
        }

        // A lost fight from rules 24: each of yours who fell loses his top rank, and a rank 3 fighter dropping to 2 takes his
        // second piece off (it stays in the run's gear). The ones who lost a rank.
        private List<CampaignFighter> DropRanks(IReadOnlyList<CampaignFighter> fallen)
        {
            var lost = new List<CampaignFighter>();
            for (int i = 0; i < fallen.Count; i++)
            {
                CampaignFighter f = fallen[i];
                if (f.Rank <= 1) continue;
                f.Rank--;
                if (f.Rank < PalStoreRules.SecondSlotRank) f.GearId2 = string.Empty;
                lost.Add(f);
            }
            return lost;
        }

        // A captured fighter from rules 24 loses his rank, and the winners take his second piece with the first.
        private void Captured(CampaignFighter f)
        {
            CampaignGear second = FindGear(f.GearId2);
            if (second != null) _gear.Remove(second);
            f.GearId2 = string.Empty;
            f.Rank = 1;
        }

        private HeldTalent FindTalent(GluttonTalent talent)
        {
            for (int i = 0; i < _talents.Count; i++) if (_talents[i].Talent == talent) return _talents[i];
            return null;
        }

        private HeldRoute FindRoute(ShadeRoute route)
        {
            for (int i = 0; i < _routes.Count; i++) if (_routes[i].Route == route) return _routes[i];
            return null;
        }

        // The stores go when the run does (ClearGearAndGrudges). From rules 24 the habits stay, set at camp, and only their
        // upgrades go.
        private void ClearPalStores()
        {
            _talents.Clear();
            _routes.Clear();
            if (WhispererRun)
                for (int i = 0; i < _habits.Count; i++) _habits[i].Upgraded = false;
            else _habits.Clear();
            Pair = SquadPair.None;
            for (int i = 0; i < _reserve.Count; i++)
            {
                _reserve[i].Rank = 1;
                _reserve[i].GearId2 = string.Empty;
            }
        }

        // ------------------------------------------------------------ for CampaignSave

        internal bool RestoreTalent(GluttonTalent talent, bool fed)
        {
            if (talent <= GluttonTalent.None || (int)talent > PalStoreRules.TalentCount || FindTalent(talent) != null) return false;
            if (fed) for (int i = 0; i < _talents.Count; i++) if (_talents[i].Fed) return false;
            _talents.Add(new HeldTalent(talent, fed));
            return true;
        }

        internal bool RestoreRoute(ShadeRoute route, int form)
        {
            if (route <= ShadeRoute.None || (int)route >= ShadeRules.RouteCount || form < 1 || form > ShadeRules.MaxForm ||
                FindRoute(route) != null || _routes.Count >= ShadeRules.MaxButtons) return false;
            for (int i = 0; i < _routes.Count; i++) if (ShadeRules.ButtonOf(_routes[i].Route) == ShadeRules.ButtonOf(route)) return false;
            _routes.Add(new HeldRoute(route, form));
            return true;
        }

        internal bool RestoreHabit(WhispererHabit habit)
        {
            if (habit <= WhispererHabit.None || (int)habit > PalStoreRules.HabitCount) return false;
            for (int i = 0; i < _habits.Count; i++) if (_habits[i].Class == PalStoreRules.ClassOf(habit)) return false;
            _habits.Add(new HeldHabit(habit));
            return true;
        }

        // A habit the run upgraded (version 27): it must be set.
        internal bool RestoreHabitUp(WhispererHabit habit)
        {
            for (int i = 0; i < _habits.Count; i++)
                if (_habits[i].Habit == habit && !_habits[i].Upgraded)
                {
                    _habits[i].Upgraded = true;
                    return true;
                }
            return false;
        }

        // One of her picks on offer (version 27), in its place: it must match the offer's choice there.
        internal bool RestoreWhisperPick(int index, Favour kind, string fighterId, WhispererHabit habit, SquadPair pair)
        {
            FavourOffer offer = _favourOffer;
            if (offer == null || offer.Pal != Pal.Whisperer || index != offer.Picks.Count || index >= offer.Choices.Count ||
                offer.Choices[index] != kind) return false;
            if (kind == Favour.WhisperRankUp && FindFighter(fighterId) == null) return false;
            if (kind == Favour.WhisperPair && (pair <= SquadPair.None || (int)pair > PalStoreRules.PairCount)) return false;
            if (kind == Favour.WhisperHabitUp && (habit <= WhispererHabit.None || (int)habit > PalStoreRules.HabitCount)) return false;
            offer.AddPick(new WhisperPick(kind, fighterId ?? string.Empty, habit, pair));
            return true;
        }

        internal bool RestorePair(SquadPair pair)
        {
            if (pair <= SquadPair.None || (int)pair > PalStoreRules.PairCount || Pair != SquadPair.None) return false;
            Pair = pair;
            return true;
        }

        internal bool RestoreRank(string fighterId, int rank)
        {
            CampaignFighter f = FindFighter(fighterId);
            if (f == null || f.Rank != 1 || rank < 2 || rank > PalStoreRules.MaxRank) return false;
            f.Rank = rank;
            return true;
        }

        internal bool RestoreSecond(string fighterId, string gearId)
        {
            CampaignFighter f = FindFighter(fighterId);
            CampaignGear piece = FindGear(gearId);
            if (f == null || piece == null || f.Rank < PalStoreRules.SecondSlotRank || !piece.Fits(f.Class) || f.GearId2.Length > 0) return false;
            for (int i = 0; i < _reserve.Count; i++) if (_reserve[i].GearId == piece.Id || _reserve[i].GearId2 == piece.Id) return false;
            f.GearId2 = piece.Id;
            return true;
        }
    }
}
