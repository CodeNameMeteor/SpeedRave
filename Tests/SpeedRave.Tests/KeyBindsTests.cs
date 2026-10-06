using System.Globalization;
using UnityEngine;
using Xunit;

namespace SpeedRave.Tests
{
    public class KeyBindsTests
    {
        [Theory]
        [InlineData("U", "U")]
        [InlineData("F6", "F6")]
        [InlineData("INSERT", "Insert")]
        [InlineData("insert", "Insert")]
        [InlineData(" f6 ", "F6")]
        [InlineData("left shift", "LeftShift")]
        [InlineData("right ctrl", "RightControl")]
        [InlineData("up", "UpArrow")]
        [InlineData("page down", "PageDown")]
        [InlineData("1", "Alpha1")]
        [InlineData("[1]", "Keypad1")]
        [InlineData("[+]", "KeypadPlus")]
        [InlineData("joystick button 7", "JoystickButton7")]
        [InlineData("mouse 0", "Mouse0")]
        // Expected key as a string: xUnit inspects theory argument types, and the Unity reference assemblies'
        // attributes throw when instantiated outside Unity.
        public void TryParse_AcceptsKeyCodeAndUnityNames(string bind, string expected)
        {
            Assert.True(SpeedRave.KeyBinds.TryParse(bind, out KeyCode key));
            Assert.Equal(expected, key.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        [InlineData("banana")]
        [InlineData("123")]
        [InlineData("-1")]
        [InlineData("None")]
        public void TryParse_RejectsInvalidNames(string bind)
        {
            Assert.False(SpeedRave.KeyBinds.TryParse(bind, out KeyCode key));
            Assert.Equal(KeyCode.None, key);
        }

        [Fact]
        public void TryParse_IsCultureIndependent()
        {
            // Regression for B-11: under Turkish culture "I".ToLower() is a dotless i.
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                Assert.True(SpeedRave.KeyBinds.TryParse("I", out KeyCode i));
                Assert.Equal(KeyCode.I, i);
                Assert.True(SpeedRave.KeyBinds.TryParse("INSERT", out KeyCode insert));
                Assert.Equal(KeyCode.Insert, insert);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }
    }
}
