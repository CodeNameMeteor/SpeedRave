using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedRave.Patches
{
    static class QuitToMenuPatch
    {
        private static readonly MethodInfo SaveGameMethod = typeof(FoodControl).GetMethod("SaveGame", BindingFlags.NonPublic | BindingFlags.Instance);

        // The FoodControl most recently destroyed as a duplicate. Other FoodControl.Start postfixes (e.g. the
        // seed text) check this, because Harmony still runs postfixes when a prefix skips the original.
        public static FoodControl DestroyedDuplicate { get; private set; }

        [HarmonyPatch(typeof(FoodControl), "Start")]
        [HarmonyPrefix]
        static bool FoodControlStartPatch(FoodControl __instance)
        {
            var foodControls = UnityEngine.Object.FindObjectsOfType<FoodControl>();
            // The way food_control is programmed is that it is initialised on the start of sewer_start by PersistControl.
            // When returning to sewer_start, destroy duplicate instances.
            if (foodControls.Length <= 1)
            {
                return true;
            }

            DestroyDuplicatePersistControls(__instance.gameObject.scene);
            DestroyedDuplicate = __instance;
            UnityEngine.Object.Destroy(__instance.gameObject);
            // Don't run the original Start (loading save data etc.) on an instance that is being destroyed.
            return false;
        }

        // FindObjectsOfType has no guaranteed order, so pick the duplicates by scene: the original
        // PersistControl lives in another scene (DontDestroyOnLoad), the duplicate in the newly loaded one.
        private static void DestroyDuplicatePersistControls(Scene duplicateScene)
        {
            var persistControls = UnityEngine.Object.FindObjectsOfType<PersistControl>();
            if (persistControls.Length <= 1) return;

            bool originalExistsElsewhere = false;
            foreach (var persistControl in persistControls)
            {
                if (persistControl.gameObject.scene != duplicateScene)
                {
                    originalExistsElsewhere = true;
                    break;
                }
            }

            if (!originalExistsElsewhere)
            {
                // Can't tell them apart by scene; keep the previous behaviour.
                UnityEngine.Object.Destroy(persistControls[persistControls.Length - 1].gameObject);
                return;
            }

            foreach (var persistControl in persistControls)
            {
                if (persistControl.gameObject.scene == duplicateScene)
                {
                    UnityEngine.Object.Destroy(persistControl.gameObject);
                }
            }
        }

        [HarmonyPatch(typeof(FoodControl), "Update")]
        [HarmonyPrefix]
        static bool FoodControlUpdatePatch(FoodControl __instance)
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
                        Log.Error("SaveGame method not found!");
                    }

                    UnityEngine.Object.Destroy(__instance.gameObject);
                    var persistControl = UnityEngine.Object.FindObjectOfType<PersistControl>();
                    if (persistControl != null)
                    {
                        UnityEngine.Object.Destroy(persistControl.gameObject);
                    }
                    
                    SceneManager.LoadScene("TitleScreen");
                    // The object is being destroyed; don't run the original Update on it this frame.
                    return false;
                }
            }
            return true;
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