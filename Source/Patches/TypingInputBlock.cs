using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace SpeedRave.Patches
{
    // While a trainer text field has focus, keep keystrokes from moving or jumping the player.
    [HarmonyPatch(typeof(FirstPersonController), "GetInput")]
    static class BlockMovementWhileTyping
    {
        private static readonly FieldInfo InputField = AccessTools.Field(typeof(FirstPersonController), "m_Input");

        static bool Prepare()
        {
            return AccessTools.Method(typeof(FirstPersonController), "GetInput") != null;
        }

        static bool Prefix(FirstPersonController __instance, ref float speed)
        {
            if (!GUIComponent.IsTyping) return true;

            speed = 0f;
            InputField?.SetValue(__instance, Vector2.zero);
            return false;
        }
    }

    [HarmonyPatch(typeof(FirstPersonController), "Update")]
    static class BlockJumpWhileTyping
    {
        private static readonly FieldInfo JumpField = AccessTools.Field(typeof(FirstPersonController), "m_Jump");

        static bool Prepare()
        {
            return JumpField != null;
        }

        static void Postfix(FirstPersonController __instance)
        {
            if (GUIComponent.IsTyping)
            {
                JumpField.SetValue(__instance, false);
            }
        }
    }
}
