using System;
using SpeedRave.Core;
using Xunit;

namespace SpeedRave.Tests
{
    public class TimeFormatTests
    {
        [Theory]
        [InlineData(0, "00:00.00")]
        [InlineData(1234, "00:01.23")]
        [InlineData(83456, "01:23.45")]
        [InlineData(3599990, "59:59.99")]
        [InlineData(3600000, "1:00:00.00")]
        [InlineData(36125670, "10:02:05.67")]
        public void Timer_FormatsMilliseconds(long milliseconds, string expected)
        {
            Assert.Equal(expected, TimeFormat.Timer(TimeSpan.FromMilliseconds(milliseconds)));
        }

        [Fact]
        public void Timer_JustBeforeAWholeSecond_DoesNotJumpAhead()
        {
            // Regression for B-18: 59.9996s used to display as 01:00.99.
            var time = TimeSpan.FromTicks(599996 * TimeSpan.TicksPerMillisecond / 10);
            Assert.Equal("00:59.99", TimeFormat.Timer(time));
        }

        [Fact]
        public void Timer_NegativeTime_ShowsZero()
        {
            Assert.Equal("00:00.00", TimeFormat.Timer(TimeSpan.FromSeconds(-3)));
        }

        [Theory]
        [InlineData(0, "0:00:00.00")]
        [InlineData(83456, "0:01:23.45")]
        [InlineData(3723990, "1:02:03.99")]
        public void LiveSplit_UsesHoursMinutesSecondsHundredths(long milliseconds, string expected)
        {
            Assert.Equal(expected, TimeFormat.LiveSplit(TimeSpan.FromMilliseconds(milliseconds)));
        }
    }
}
