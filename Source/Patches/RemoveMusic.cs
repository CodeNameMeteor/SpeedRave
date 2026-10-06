using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace SpeedRave.Patches
{
    static class RemoveMusicPatch
    {
        // Original volumes of the sources we muted, so turning the option off can restore them.
        private static readonly Dictionary<AudioSource, float> mutedSources = new Dictionary<AudioSource, float>();

        internal static void MuteLoopingAudio()
        {
            if (!Plugin.RemoveMusic.Value) return;

            AudioSource[] audioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource != null && audioSource.isPlaying && audioSource.loop)
                {
                    if (!mutedSources.ContainsKey(audioSource))
                    {
                        mutedSources[audioSource] = audioSource.volume;
                    }
                    audioSource.volume = 0f;
                }
            }
        }

        internal static void RestoreMutedAudio()
        {
            foreach (var pair in mutedSources)
            {
                // Sources destroyed since (e.g. by a scene change) compare equal to null.
                if (pair.Key != null)
                {
                    pair.Key.volume = pair.Value;
                }
            }
            mutedSources.Clear();
        }

        internal static bool HasMutedAudio => mutedSources.Count > 0;

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

    // Re-checks once a second so music that starts after a scene's Start is muted too, and restores the
    // original volumes as soon as Remove Music is turned off.
    public class MusicMuter : MonoBehaviour
    {
        private const float CheckInterval = 1f;
        private float nextCheck = 0f;

        private void Update()
        {
            if (Time.unscaledTime < nextCheck) return;
            nextCheck = Time.unscaledTime + CheckInterval;

            if (Plugin.RemoveMusic.Value)
            {
                RemoveMusicPatch.MuteLoopingAudio();
            }
            else if (RemoveMusicPatch.HasMutedAudio)
            {
                RemoveMusicPatch.RestoreMutedAudio();
            }
        }
    }
}
