using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace SpeedRave.Patches
{
    static class CursorLockFix
    {
        private static readonly FieldInfo CursorIsLockedField = AccessTools.Field(typeof(MouseLook), "m_cursorIsLocked");
        private static bool wasGuiShown = false;

        [HarmonyPatch(typeof(MouseLook), "InternalLockUpdate")]
        [HarmonyPrefix]
        static bool InternalLockUpdatePatch(MouseLook __instance)
        {
            if (GUIComponent.showGUI)
            {
                wasGuiShown = true;
                CursorIsLockedField?.SetValue(__instance, false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return false;
            }

            if (wasGuiShown)
            {
                wasGuiShown = false;
                CursorIsLockedField?.SetValue(__instance, true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            return true;
        }

        [HarmonyPatch(typeof(FirstPersonController), "RotateView")]
        [HarmonyPrefix]
        static bool RotateViewPatch()
        {
            if (GUIComponent.showGUI)
            {
                return false;
            }
            return true;
        }
    }
}
