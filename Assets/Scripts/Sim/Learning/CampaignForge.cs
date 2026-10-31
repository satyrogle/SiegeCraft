namespace KindredSiege.Sim.Learning
{
    /// The Butcher's forge (Jacob, 11 Oct 2026, call J1; DESIGN_LOCKS: the Pal who broke a chief learns that
    /// chief's gear recipe). Breaking the Bloodridge chief teaches the Butcher the Lava Sun Tabard. At camp,
    /// TabardPrice materials forge it, once and for good. Every run after that one fighter you pick wears it,
    /// and that fighter's hits drain TabardFear percent more nerve. The tabard comes off when the run ends; the
    /// forge stays. Numbers are starting values.
    public static class CampaignForge
    {
        public const string TabardName = "Lava Sun Tabard";
        public const int TabardPrice = 50;
        public const int TabardFear = 40;
    }
}
