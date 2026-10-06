namespace KindredSiege.Sim.Learning
{
    /// The slice's full rules (Jacob, 2 and 3 Oct 2026): the nerve bar, orders that refill during
    /// the fight so the player stays busy, and tougher units so a fight lasts about a minute.
    ///
    /// Placeholders, measured on 3 Oct 2026 against untaught raiders, 100 fights a cell. With
    /// units at 400 percent health and a point back every 120 ticks (4 seconds), fights ran from
    /// about 29 seconds (focus their Medic and Hold whenever possible) to 58 seconds (no orders);
    /// focusing and Unleashing whenever possible ran 39 to 51 seconds, and a player who orders
    /// whenever a point is free gave 16 to 25 orders a minute. Under the nerve bar alone the same
    /// fights ran 7 to 15 seconds.
    public static class SliceRules
    {
        /// A directive point comes back this often, in ticks (30 ticks a second).
        public const int RefillTicks = 120;

        /// Every unit's health, in percent of its class's health.
        public const int HealthPercent = 400;

        /// A slice world under the full rules.
        public static SliceWorld CreateWorld(ulong worldSeed)
        {
            return SliceWorld.Create(worldSeed, true, RefillTicks, HealthPercent);
        }
    }
}
