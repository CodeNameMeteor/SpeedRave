using SpeedRave.Core;
using Xunit;

namespace SpeedRave.Tests
{
    public class EndingScenesTests
    {
        [Theory]
        [InlineData("plaguending")]
        [InlineData("spaceending")]
        [InlineData("truending")]
        public void TheThreeEndings_AreEndings(string scene)
        {
            Assert.True(EndingScenes.IsEnding(scene));
        }

        [Theory]
        [InlineData("winroom1")]
        [InlineData("credits")]
        [InlineData("sewer_start")]
        [InlineData("titlescreen")]
        [InlineData("someendingroom")]
        public void OtherScenes_AreNotEndings(string scene)
        {
            // Regression for B-10: WinRoom1, Credits and anything merely containing "ending" used to count.
            Assert.False(EndingScenes.IsEnding(scene));
        }
    }
}
