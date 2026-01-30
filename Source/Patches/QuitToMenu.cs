using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedRave.Patches
{
    static class QuitToMenuPatch
    {
        //public static bool Use;

        [HarmonyPatch(typeof(FoodControl), "Start")]
        [HarmonyPrefix]
        static void FoodControlStartPatch(FoodControl __instance)
        {
            var persistControls = UnityEngine.Object.FindObjectsOfType<PersistControl>();
            var foodControls = UnityEngine.Object.FindObjectsOfType<FoodControl>();
            //The way the food_control is programmed is that it is initialised on the start of sewer_start by persist Control
            //in a typical game you can't go back to sewer_start but as we can just have this here as a precaution
            if (foodControls.Length > 1)
            {
                UnityEngine.Object.Destroy(persistControls[1]);
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
        }

        [HarmonyPatch(typeof(FoodControl), "Update")]
        [HarmonyPrefix]
        static void FoodControlUpdatePatch(FoodControl __instance)
        {
            if(Plugin.QuitToMenu.Value)
            {
                if (__instance.display && Input.GetButtonDown("Cancel"))
                {
                    __instance.canvas.SetActive(false);
                    __instance.display = false;

                    MethodInfo saveGameMethod = typeof(FoodControl).GetMethod("SaveGame", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (saveGameMethod != null)
                    {
                        saveGameMethod.Invoke(__instance, null);
                    }
                    else
                    {
                        Debug.LogError("SaveGame method not found!");
                    }

                    UnityEngine.Object.Destroy(__instance.gameObject);
                    UnityEngine.Object.Destroy(UnityEngine.Object.FindObjectOfType<PersistControl>());
                    
                    //Object[] allObjects = Object.FindObjectsOfType(typeof(FoodControl));
                    //foreach (Object obj in allObjects)
                    //{

                   //     UnityEngine.Object.Destroy(obj);
                    //}
                    
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
            if (Plugin.QuitToMenu.Value)
            {
                __instance.ClearSaveData();
            }
        }
    }
}