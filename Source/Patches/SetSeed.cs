using HarmonyLib;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SpeedRave.Patches
{
    static class SetSeedPatchs
    {
        public static bool Use;
        public static int Seed;
        public static bool randomSeed = true;

        static Random.State state = Random.state;
        static Random.State messyState = Random.state;

        [HarmonyPatch(typeof(AppearChance), "Start")]
        [HarmonyPatch(typeof(Billboard_Random), "Start")]
        [HarmonyPatch(typeof(DoorBehavior), "Start")]
        [HarmonyPatch(typeof(EndingTeleporter), "Start")]
        [HarmonyPatch(typeof(GetDialogue), "Start")]
        [HarmonyPatch(typeof(MaterialChangeScript), "Start")]
        [HarmonyPatch(typeof(RatColorScript), "Start")]
        [HarmonyPatch(typeof(SelectNPCScript), "Start")]
        [HarmonyPatch(typeof(SpawnPointScript), "Start")]
        [HarmonyPatch(typeof(WalkUpDialogue), "Start")]
        [HarmonyPrefix]
        public static void StoreState()
        {
            if (Use)
            {
                messyState = Random.state;
                Random.state = state;
            }
        }

        [HarmonyPatch(typeof(AppearChance), "Start")]
        [HarmonyPatch(typeof(Billboard_Random), "Start")]
        [HarmonyPatch(typeof(DoorBehavior), "Start")]
        [HarmonyPatch(typeof(EndingTeleporter), "Start")]
        [HarmonyPatch(typeof(GetDialogue), "Start")]
        [HarmonyPatch(typeof(MaterialChangeScript), "Start")]
        [HarmonyPatch(typeof(RatColorScript), "Start")]
        [HarmonyPatch(typeof(SelectNPCScript), "Start")]
        [HarmonyPatch(typeof(SpawnPointScript), "Start")]
        [HarmonyPatch(typeof(WalkUpDialogue), "Start")]
        [HarmonyPostfix]
        public static void RestoreState()
        {
            if (Use)
            {
                state = Random.state;
                Random.state = messyState;
            }
        }

        [HarmonyPatch(typeof(TitleScreenControler), "StartGame")]
        [HarmonyPrefix]
        public static void Init()
        {
            if (Use)
            {
                if(randomSeed)
                {
                    uint[] state = new uint[4];
                    Seed = (int)DateTime.Now.Ticks;
                    for (int i = 0; i < 4; ++i)
                    {
                        state[i] =  (uint) Seed;
                        Seed = Seed * 0x6C078965 + 1;
                    }
                    Debug.Log($"[SpeedRave] Seed set to {Seed}");
                    StoreState();
                    Random.InitState(Seed);
                    RestoreState();
                }
                else
                {
                    StoreState();
                    Random.InitState(Seed);
                    RestoreState();
                }
            }

        }
        [HarmonyPatch(typeof(FoodControl), "RefreshValues")]
        [HarmonyPostfix]
        public static void addSeedText(FoodControl __instance)
        {
            if (Use)
            {
                __instance.inventoryText.Text = "CHEESE: " + __instance.cheese.ToString() + "\nFRUIT: " + __instance.fruit.ToString() + "\nSEED: " + Seed.ToString();
            }
        }
    }
}
