using HarmonyLib;
using System.Reflection;

namespace SpeedRave.Patches
{
    static class TitlePatch
    {
        private static readonly FieldInfo TitleTextField = AccessTools.Field(typeof(TitleColor), "titleText");

        // Modifies the default titleText to our new title text
        [HarmonyPatch(typeof(TitleColor), "Start")]
        [HarmonyPostfix]
        static void TitleColorStartPatch(TitleColor __instance)
        {
            SetTitleText(__instance);
        }

        // Modifies the titleText which appears after clicking the title to change the colour
        [HarmonyPatch(typeof(TitleColor), "ChangeColor")]
        [HarmonyPrefix]
        static void TitleColorChangeColorPatch(TitleColor __instance)
        {
            SetTitleText(__instance);
        }

        private static void SetTitleText(TitleColor instance)
        {
            if (instance == null || TitleTextField == null) return;

            var titleText = TitleTextField.GetValue(instance) as SuperTextMesh;
            if (titleText != null)
            {
                titleText.text = "SEWER RAVE+";
            }
        }
    }
}
