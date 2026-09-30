using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedRave.Patches
{
    static class QuitToMenuPatch
    {
        private static readonly MethodInfo SaveGameMethod = typeof(FoodControl).GetMethod("SaveGame", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch(typeof(FoodControl), "Start")]
        [HarmonyPrefix]
        static void FoodControlStartPatch(FoodControl __instance)
        {
            var persistControls = UnityEngine.Object.FindObjectsOfType<PersistControl>();
            var foodControls = UnityEngine.Object.FindObjectsOfType<FoodControl>();
            // The way food_control is programmed is that it is initialised on the start of sewer_start by PersistControl.
            // When returning to sewer_start, destroy duplicate instances.
            if (foodControls.Length > 1)
            {
                if (persistControls.Length > 1)
                {
                    UnityEngine.Object.Destroy(persistControls[persistControls.Length - 1].gameObject);
                }
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
        }

        [HarmonyPatch(typeof(FoodControl), "Update")]
        [HarmonyPrefix]
        static void FoodControlUpdatePatch(FoodControl __instance)
        {
            if (Plugin.QuitToMenu.Value)
            {
                if (__instance.display && Input.GetButtonDown("Cancel"))
                {
                    if (__instance.canvas != null)
                    {
                        __instance.canvas.SetActive(false);
                    }
                    __instance.display = false;

                    if (SaveGameMethod != null)
                    {
                        SaveGameMethod.Invoke(__instance, null);
                    }
                    else
                    {
                        Debug.LogError("SaveGame method not found!");
                    }

                    UnityEngine.Object.Destroy(__instance.gameObject);
                    var persistControl = UnityEngine.Object.FindObjectOfType<PersistControl>();
                    if (persistControl != null)
                    {
                        UnityEngine.Object.Destroy(persistControl.gameObject);
                    }
                    
                    SceneManager.LoadScene("TitleScreen");
                }
            }
        }

        [HarmonyPatch(typeof(global::TitleScreenControler), "Start")]
        [HarmonyPostfix]
        static void TitleScreenControlerStartPatch(global::TitleScreenControler __instance)
        {
            if (Plugin.QuitToMenu.Value)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        [HarmonyPatch(typeof(global::TitleScreenControler), "StartGame")]
        [HarmonyPostfix]
        static void TitleScreenControlerStartGamePatch(global::TitleScreenControler __instance)
        {
            if (Plugin.ClearSaveOnStart.Value)
            {
                __instance.ClearSaveData();
            }
        }
    }
}