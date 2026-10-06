namespace SpeedRave.Core
{
    public static class SeedGenerator
    {
        // Scrambles a time-based value (e.g. DateTime.Now.Ticks) into a random-looking seed with a few rounds
        // of a linear congruential step. Deterministic for a given input.
        public static int FromTicks(long ticks)
        {
            int seed = unchecked((int)ticks);
            for (int i = 0; i < 4; ++i)
            {
                seed = unchecked(seed * 0x6C078965 + 1);
            }
            return seed;
        }
    }
}
