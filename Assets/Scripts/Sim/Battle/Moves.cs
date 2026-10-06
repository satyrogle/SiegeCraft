using System;

namespace KindredSiege.Sim.Battle
{
    /// The slice's big moves (Jacob, 4 Oct 2026). Every number here is a placeholder for tuning.
    ///
    /// Eclipse: the raider chief's signature. It winds up for a moment you can see, then hits the
    /// enemy front row for a share of each unit's health and shakes their nerve. Hold soaks half
    /// of it, Fall Back dodges it, and hitting the caster hard enough during the windup breaks it
    /// off and doubles your next order. Break the chief and it is yours: its grade (how hard it
    /// hits) comes from how cleanly you won.
    ///
    /// Frenzy: the Pal transforms once a fight, for free. The Pal hits twice as hard and every
    /// ally a little harder while it lasts.
    ///
    /// Combos: certain orders in a row fuse into one big attack, each once a fight. Ambush is Fall
    /// Back then Unleash: the enemy back row is caught in the open. Pyre is Focus Fire, Unleash,
    /// then Eclipse: when the Eclipse goes off, the focused target burns.
    public static class Moves
    {
        public const int EclipseWindupTicks = 90;

        /// Measured on 4 Oct 2026: at 100 nerve and 15/20/25 percent, a player who never answered
        /// the chief's Eclipse won as often as one who did, so it now hurts enough to answer.
        public const int EclipseNerveCost = 200;
        public const int EclipseHoldPercent = 50;
        public const int MaxEclipseGrade = 3;

        /// A caster that loses this percent of its max health during the windup is interrupted.
        /// Measured on 4 Oct 2026: at 10, and at 6, focus fire on the chief never broke his cast
        /// (about 60 damage reached him in a windup). At this it can, unless his Warden has
        /// learned to cover whoever you focus.
        public const int InterruptPercent = 4;

        /// An empowered order runs this percent as long, or hits this percent as hard.
        public const int EmpoweredPercent = 200;

        public const int FrenzyTicks = 240;
        public const int FrenzyPalPowerPercent = 200;
        public const int FrenzyAllyPowerPercent = 130;

        public const string Ambush = "Ambush";
        public const int AmbushWindowTicks = 90;
        public const int AmbushPercent = 10;
        public const int AmbushNerveCost = 80;

        public const string Pyre = "Pyre";
        public const int PyreWindowTicks = 180;
        public const int PyrePercent = 30;
        public const int PyreNerveCost = 150;

        /// Hit details for the moves' damage, so lessons that count plain hits leave them out.
        public const string EclipseDetail = "eclipse";
        public const string AmbushDetail = "ambush";
        public const string PyreDetail = "pyre";

        /// Break and finish reason when a side's Pal or Chief drops.
        public const string LeaderDetail = "leader";

        /// Release outcomes.
        public const string Full = "full";
        public const string Soaked = "soaked";
        public const string Dodged = "dodged";

        /// Percent of each target's max health an Eclipse of this grade hits for. 0 for no Eclipse.
        public static int EclipsePercent(int grade)
        {
            switch (grade)
            {
                case 1: return 15;
                case 2: return 20;
                case 3: return 25;
                default: return 0;
            }
        }
    }

    /// What a side can do beyond the five orders: its Eclipse and grade (0 for none), how often
    /// its chief casts Eclipse on his own (0 for never; the player casts by order), and whether
    /// its orders fuse into combos. None is the old rules.
    public sealed class SideRules
    {
        public static readonly SideRules None = new SideRules(0, 0, false);

        public SideRules(int eclipseGrade, int autoEclipseTicks, bool combos)
        {
            EclipseGrade = Math.Max(0, Math.Min(Moves.MaxEclipseGrade, eclipseGrade));
            AutoEclipseTicks = Math.Max(0, autoEclipseTicks);
            Combos = combos;
        }

        public int EclipseGrade { get; }
        public int AutoEclipseTicks { get; }
        public bool Combos { get; }
        public bool IsNone => EclipseGrade == 0 && AutoEclipseTicks == 0 && !Combos;

        public void AppendTo(SnapshotBuilder b, string prefix)
        {
            b.Field(prefix + "eclipseGrade", EclipseGrade);
            b.Field(prefix + "autoEclipse", AutoEclipseTicks);
            b.Field(prefix + "combos", Combos);
        }
    }
}
