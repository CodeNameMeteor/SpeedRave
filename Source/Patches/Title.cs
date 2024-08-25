using HarmonyLib;

namespace SpeedRave.Patches
{
    static class TitlePatch
    {

        [HarmonyPatch(typeof(TitleColor), "Start")]
        [HarmonyPostfix]
        static void TitleColorStartPatch(TitleColor __instance)
        {
            // Use reflection to access the private field
            var titleTextField = AccessTools.Field(typeof(TitleColor), "titleText");

            // Get the value of the private field
            var titleText = (SuperTextMesh)titleTextField.GetValue(__instance);

            // Modify the text
            titleText.text = "SEWER RAVE+";
        }

        [HarmonyPatch(typeof(TitleColor), "ChangeColor")]
        [HarmonyPrefix]
        static void TitleColorChangeColorPatch(TitleColor __instance)
        {
            // Use reflection to access the private field
            var titleTextField = AccessTools.Field(typeof(TitleColor), "titleText");

            // Get the value of the private field
            var titleText = (SuperTextMesh)titleTextField.GetValue(__instance);

            // Modify the text
            titleText.text = "SEWER RAVE+";
        }
    }
}
