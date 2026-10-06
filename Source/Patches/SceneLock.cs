using HarmonyLib;
using System;

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
