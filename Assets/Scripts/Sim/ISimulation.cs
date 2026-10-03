namespace KindredSiege.Sim
{
    /// The authoritative simulation. Only two ways in: a command, or a tick.
    /// Presentation reads OrderedStateSnapshot and public getters. It never writes.
    public interface ISimulation
    {
        long CurrentTick { get; }

        void Tick();

        bool TryApply(SimCommand command, out string failure);

        /// Every authoritative field, in a fixed order. The checksum is taken over this.
        string OrderedStateSnapshot { get; }
    }
}
