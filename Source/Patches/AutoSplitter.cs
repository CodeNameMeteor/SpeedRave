using HarmonyLib;
using UnityEngine;

namespace SpeedRave.Patches
{
    static class AutoSplitterPatchs
    {
        [HarmonyPatch(typeof(DoorBehavior), "OnTriggerEnter")]
        [HarmonyPrefix]
        static bool DoorBehaviorOnTriggerEnterPatch(Collider other)
        {
            if (other == null || other.CompareTag("Player"))
            {
                Autosplitter.isLoading = true;
            }
            return true;
        }

        [HarmonyPatch(typeof(DoorBehavior), "Start")]
        [HarmonyPrefix]
        static bool DoorBehaviorStartPatch()
        {
            Autosplitter.isLoading = false;
            return true;
        }
    }
}
