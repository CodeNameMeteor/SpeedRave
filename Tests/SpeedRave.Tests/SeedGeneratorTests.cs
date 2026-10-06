using SpeedRave.Core;
using Xunit;

namespace SpeedRave.Tests
{
    public class SeedGeneratorTests
    {
        // The original inline algorithm from SetSeed.Init, kept here to prove the extraction didn't change it.
        private static int Original(long ticks)
        {
            int newSeed = (int)ticks;
            for (int i = 0; i < 4; ++i)
            {
                newSeed = newSeed * 0x6C078965 + 1;
            }
            return newSeed;
        }

        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(638640000000000000L)]
        [InlineData(long.MaxValue)]
        public void FromTicks_MatchesOriginalAlgorithm(long ticks)
        {
            Assert.Equal(Original(ticks), SeedGenerator.FromTicks(ticks));
        }

        [Fact]
        public void FromTicks_IsDeterministic_AndSpreadsNearbyInputs()
        {
            Assert.Equal(SeedGenerator.FromTicks(12345), SeedGenerator.FromTicks(12345));
            Assert.NotEqual(SeedGenerator.FromTicks(12345), SeedGenerator.FromTicks(12346));
        }
    }
}
