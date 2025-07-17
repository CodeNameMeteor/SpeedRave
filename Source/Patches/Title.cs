using HarmonyLib;

namespace SpeedRave.Patches
{
    static class TitlePatch
    {
        //Modify's the default titleText to our new title text
        [HarmonyPatch(typeof(TitleColor), "Start")]
        [HarmonyPostfix]
        static void TitleColorStartPatch(TitleColor __instance)
        {
            var titleTextField = AccessTools.Field(typeof(TitleColor), "titleText");

            var titleText = (SuperTextMesh)titleTextField.GetValue(__instance);

            titleText.text = "SEWER RAVE+";
        }

        //modifies the titleText which appears after clicking the title to change the colour.
        [HarmonyPatch(typeof(TitleColor), "ChangeColor")]
        [HarmonyPrefix]
        static void TitleColorChangeColorPatch(TitleColor __instance)
        {
            var titleTextField = AccessTools.Field(typeof(TitleColor), "titleText");

            var titleText = (SuperTextMesh)titleTextField.GetValue(__instance);

            titleText.text = "SEWER RAVE+";
        }
    }
}
