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

        [HarmonyPatch(typeof(TitleScreenControler), "Update")]
        [HarmonyPostfix]
        static void TitleScreenControlerUpdatePatch(TitleScreenControler __instance)
        {
            if (Plugin.QuickStart.Value)
            {
                if (Input.GetButtonDown("Jump"))
                {
                    __instance.StartGame();
                }
            }
        }
    }
}
