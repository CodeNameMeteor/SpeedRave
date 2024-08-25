using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedRave.Patches
{
    static class QuitToMenuPatch
    {
        public static bool Use;

        [HarmonyPatch(typeof(FoodControl), "Update")]
        [HarmonyPrefix]
        static void FoodControlUpdatePatch(FoodControl __instance)
        {
            if(Use)
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
                    SceneManager.LoadScene("TitleScreen");
                }
            }
        }

        [HarmonyPatch(typeof(global::TitleScreenControler), "Start")]
        [HarmonyPostfix]
        static void TitleScreenControlerStartPatch(global::TitleScreenControler __instance)
        {
            if(Use)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

        }

        [HarmonyPatch(typeof(global::TitleScreenControler), "StartGame")]
        [HarmonyPostfix]
        static void TitleScreenControlerStartGamePatch(global::TitleScreenControler __instance)
        {
            if(Use)
            {
                __instance.ClearSaveData();
            }
        }
    }
}