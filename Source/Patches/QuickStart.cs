using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpeedRave.Patches
{
    static class QuickStartPatch
    {
        // Set once Space has started the game, so repeated presses during the start transition can't call
        // StartGame again (which would also roll a new random seed). Cleared on each new title screen.
        private static bool startRequested = false;

        [HarmonyPatch(typeof(TitleScreenControler), "Start")]
        [HarmonyPostfix]
        static void TitleScreenControlerStartPatch()
        {
            startRequested = false;
        }

        [HarmonyPatch(typeof(TitleScreenControler), "Update")]
        [HarmonyPostfix]
        static void TitleScreenControlerUpdatePatch(TitleScreenControler __instance)
        {
            if (Plugin.QuickStart.Value && !GUIComponent.IsTyping && !startRequested)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    startRequested = true;
                    __instance.StartGame();
                }
            }
        }
    }
}
