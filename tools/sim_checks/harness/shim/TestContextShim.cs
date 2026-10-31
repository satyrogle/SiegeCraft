// Harness only: NUnit's TestContext needs a real runner, so this stands in for it here.
namespace KindredSiege.Sim.Tests
{
    internal static class TestContext
    {
        public static void WriteLine(string s) { System.Console.WriteLine("  | " + s); }
        public static void WriteLine(object o) { System.Console.WriteLine("  | " + o); }
        public static System.IO.TextWriter Progress => System.Console.Out;
        public static System.IO.TextWriter Out => System.Console.Out;
        public static Ctx CurrentContext { get; } = new Ctx();
        internal sealed class Ctx { public string WorkDirectory => System.IO.Directory.GetCurrentDirectory(); }
    }
}
