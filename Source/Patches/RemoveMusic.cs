using HarmonyLib;
using UnityEngine;

namespace SpeedRave.Patches
{
    static class RemoveMusicPatch
    {
        public static bool Use;

        
        [HarmonyPatch(typeof(global::LoadPlayerUpgrades), "Start")]
        [HarmonyPostfix]
        static void LoadPlayerUpgradesStartPatch(global::LoadPlayerUpgrades __instance)
        {
            if(Use)
            {
                
                foreach (AudioSource audioSource in UnityEngine.Object.FindObjectsOfType(typeof(AudioSource)) as AudioSource[])
                {
                    if (audioSource.isPlaying && audioSource.loop)
                    {
                        audioSource.volume = 0f;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(global::TitleScreenControler), "Start")]
        [HarmonyPostfix]
        static void TitleScreenControlerStartPatch(global::TitleScreenControler __instance)
        {
            if (Use)
            {

                foreach (AudioSource audioSource in UnityEngine.Object.FindObjectsOfType(typeof(AudioSource)) as AudioSource[])
                {
                    if (audioSource.isPlaying && audioSource.loop)
                    {
                        audioSource.volume = 0f;
                    }
                }
            }
        }
    }
}
