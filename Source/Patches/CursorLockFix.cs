using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityStandardAssets.Characters.FirstPerson;
using HarmonyLib;
using UnityEngine;

namespace SpeedRave.Patches
{
    static class CursorLockFix
    {
        [HarmonyPatch(typeof(MouseLook), "InternalLockUpdate")]
        [HarmonyPrefix]
        static bool InternalLockUpdatePatch(MouseLook __instance)
        {
            if(GUIComponent.showGUI)
            {
                var cursorIsLocked = AccessTools.Field(typeof(MouseLook), "m_cursorIsLocked");
                cursorIsLocked.SetValue(__instance, false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return false;
            }
            else
            {
                var cursorIsLocked = AccessTools.Field(typeof(MouseLook), "m_cursorIsLocked");
                cursorIsLocked.SetValue(__instance, true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                return true;
            }
        }
        [HarmonyPatch(typeof(FirstPersonController), "RotateView")]
        [HarmonyPrefix]
        static bool RotateViewPatch()
        {
            if(GUIComponent.showGUI)
            {
                return false;
            }
            return true;
        }
    }
}
