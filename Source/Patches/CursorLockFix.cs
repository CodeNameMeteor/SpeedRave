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

        // Cursor state from just before the trainer opened, restored when it closes. Forcing a lock on close
        // would hide the cursor while a game menu (e.g. the inventory) that needs it is open.
        private static bool previousCursorIsLocked = true;
        private static CursorLockMode previousLockState = CursorLockMode.Locked;
        private static bool previousCursorVisible = false;

        [HarmonyPatch(typeof(MouseLook), "InternalLockUpdate")]
        [HarmonyPrefix]
        static bool InternalLockUpdatePatch(MouseLook __instance)
        {
            if (GUIComponent.showGUI)
            {
                if (!wasGuiShown)
                {
                    wasGuiShown = true;
                    previousCursorIsLocked = CursorIsLockedField?.GetValue(__instance) as bool? ?? true;
                    previousLockState = Cursor.lockState;
                    previousCursorVisible = Cursor.visible;
                }
                CursorIsLockedField?.SetValue(__instance, false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return false;
            }

            if (wasGuiShown)
            {
                wasGuiShown = false;
                CursorIsLockedField?.SetValue(__instance, previousCursorIsLocked);
                Cursor.lockState = previousLockState;
                Cursor.visible = previousCursorVisible;
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
