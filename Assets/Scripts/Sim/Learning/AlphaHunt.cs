using KindredSiege.Sim.Battle;

namespace KindredSiege.Sim.Learning
{
    /// The hunters (Jacob, 11 Oct 2026, call J2). When the run's last chief breaks they come for
    /// you: one last fight, Fighters of them and their leader, plus PerTribeJoined more for every tribe that
    /// joined you this run (DESIGN_LOCKS: "the more tribes join you during it, the harder the
    /// hunters come"). The leader winds up a big hit you can see coming, like a chief's Eclipse. Break their
    /// nerve and the run is won; they never join. Since 18 Oct 2026 they are their own tribe, the 25th, and the
    /// rival, not the Sovereign's (DESIGN_LOCKS, "The rival and the Sovereign's army"); the code keeps the Alpha
    /// names until the rename is decided. They are not a tribe on the path: not in CampaignTribes.All, so no run
    /// starts at them. They arrive knowing what the
    /// tribes on the path know, to Hold when you Unleash and to cover whoever you focus, and to wait out your
    /// parry (Jacob, 14 Oct 2026: "yes go", after Claude's checks found the parry won every hunt), and those
    /// habits never change (Jacob's calls, 11 Oct 2026: as first built, Unleash spam won 90% of full run
    /// hunts). They fight at tier 3 strength, the tier after Skullpeak's, at CampaignTuning.HuntPercent, and
    /// send the same lineup every time (Jacob, 11 Oct 2026, "fixed per fight": picked with the bots, it lets
    /// Unleash spam win 3% of hunts, the least of the lineups a player can beat). Numbers are starting values.
    /// What grows, from 18 Oct 2026 (Jacob's go on the rival, built like Odyssey's mercenaries), is their company
    /// (HunterCompany): a named hunter from its ladder leads them, with that hunter's traits and levels, the
    /// bounty on your head sends more of them, and from bounty 2 hunters also wait on the road (EncounterKind.Ambush).
    /// From campaign rules 6 (22 Oct 2026, DESIGN_LOCKS "The hunters' boss, the hunt") the captain runs the hunt: a bar of
    /// four steps over one of your fighters (Battle.ChiefKit.Hunt, BattleSimulation.Hunt.cs), with Glory and its grade card.
    public static class AlphaHunt
    {
        public const string Id = "alpha";
        public const int Fighters = 4;
        public const int PerTribeJoined = 1;

        public static readonly CampaignTribe Tribe = new CampaignTribe(Id, "Alpha", 3,
            new[] { UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Marksman, UnitClass.Trapper, UnitClass.Medic },
            UnitClass.Marksman, "the hunt captain", string.Empty, 0, 2,
            new Feat[0], Feat.None, new[] { Habit.CoverFocused, Habit.HoldOnUnleash, Habit.WaitOutParry }, string.Empty, string.Empty)
        {
            HuntLineup = new[] { UnitClass.Medic, UnitClass.Warden, UnitClass.Skirmisher, UnitClass.Marksman, UnitClass.Trapper,
                UnitClass.Medic },
            // The hunt (Jacob, 22 Oct 2026: "yea go i like it").
            Kit = ChiefKit.Hunt
        };
    }
}
