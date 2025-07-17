using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedRave.Patches
{
    static class SceneLock
    {
        public static String lockedScene;
        [HarmonyPatch(typeof(DoorBehavior), "OnTriggerEnter")]
        [HarmonyPrefix]
        static bool DoorBehaviorOnTriggerEnterPatch(DoorBehavior __instance)
        {
           if(GUIComponent.locked)
            {
                __instance.sceneSelection = lockedScene;
            }
            return true;
        }
    }
}
