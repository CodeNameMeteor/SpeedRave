using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpeedRave.Patches
{
    static class AutoSplitterPatchs
    {
        [HarmonyPatch(typeof(DoorBehavior), "OnTriggerEnter")]
        [HarmonyPrefix]
        static bool DoorBehaviorOnTriggerEnterPatch()
        {
            Autosplitter.isLoading = true;
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
