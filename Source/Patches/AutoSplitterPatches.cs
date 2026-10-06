using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedRave.Patches
{
    static class AutoSplitterPatches
    {
        private static readonly FieldInfo SelectPauseField = AccessTools.Field(typeof(TrainMapScript), "selectpause");
        private static readonly FieldInfo SelExitField = AccessTools.Field(typeof(TrainMapScript), "selExit");
        private static readonly FieldInfo SelMovieField = AccessTools.Field(typeof(TrainMapScript), "selMovie");
        private static readonly FieldInfo SelPossumField = AccessTools.Field(typeof(TrainMapScript), "selPossum");
        private static readonly FieldInfo SelSnakeField = AccessTools.Field(typeof(TrainMapScript), "selSnake");

        [HarmonyPatch(typeof(SceneManager), "LoadScene", new Type[] { typeof(string) })]
        [HarmonyPrefix]
        static void SceneManagerLoadScenePatch(string sceneName)
        {
            if (sceneName != "TitleScreen")
            {
                Autosplitter.NotifyLoadingStarted();
            }
        }

        [HarmonyPatch(typeof(DoorBehavior), "OnTriggerEnter")]
        [HarmonyPrefix]
        static bool DoorBehaviorOnTriggerEnterPatch(Collider other)
        {
            if (other != null && other.CompareTag("Player"))
            {
                Autosplitter.NotifyLoadingStarted();
            }
            return true;
        }

        [HarmonyPatch(typeof(EndingTeleporter), "OnTriggerEnter")]
        [HarmonyPrefix]
        static bool EndingTeleporterOnTriggerEnterPatch(Collider other)
        {
            if (other != null && other.CompareTag("Player"))
            {
                Autosplitter.NotifyLoadingStarted();
            }
            return true;
        }

        [HarmonyPatch(typeof(DoorBehavior), "Start")]
        [HarmonyPrefix]
        static bool DoorBehaviorStartPatch()
        {
            Autosplitter.NotifyLoadingFinished();
            return true;
        }

        [HarmonyPatch(typeof(TrainMapScript), "OnTriggerStay")]
        [HarmonyPrefix]
        static bool TrainMapScriptOnTriggerStayPatch(TrainMapScript __instance, Collider other)
        {
            if (__instance != null && other != null && other.CompareTag("Player") && Input.GetButtonDown("Fire1"))
            {
                bool selectpause = (bool)(SelectPauseField?.GetValue(__instance) ?? true);
                if (!selectpause)
                {
                    bool selExit = (bool)(SelExitField?.GetValue(__instance) ?? false);
                    bool selMovie = (bool)(SelMovieField?.GetValue(__instance) ?? false);
                    bool selPossum = (bool)(SelPossumField?.GetValue(__instance) ?? false);
                    bool selSnake = (bool)(SelSnakeField?.GetValue(__instance) ?? false);
                    if (selExit || selMovie || selPossum || selSnake)
                    {
                        Autosplitter.NotifyLoadingStarted();
                    }
                }
            }
            return true;
        }
    }
}
