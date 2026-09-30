using HarmonyLib;
using UnityEngine;

namespace SpeedRave.Patches
{
    static class RemoveMusicPatch
    {
        private static void MuteLoopingAudio()
        {
            if (!Plugin.RemoveMusic.Value) return;

            AudioSource[] audioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource != null && audioSource.isPlaying && audioSource.loop)
                {
                    audioSource.volume = 0f;
                }
            }
        }

        [HarmonyPatch(typeof(global::LoadPlayerUpgrades), "Start")]
        [HarmonyPostfix]
        static void LoadPlayerUpgradesStartPatch(global::LoadPlayerUpgrades __instance)
        {
            MuteLoopingAudio();
        }

        [HarmonyPatch(typeof(global::TitleScreenControler), "Start")]
        [HarmonyPostfix]
        static void TitleScreenControlerStartPatch(global::TitleScreenControler __instance)
        {
            MuteLoopingAudio();
        }
    }
}
