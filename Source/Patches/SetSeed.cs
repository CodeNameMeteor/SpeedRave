using HarmonyLib;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SpeedRave.Patches
{
    static class SetSeedPatchs
    {
        public static int Seed;
        public static int lastRandomSeed = 0;
        public static bool hasLastRandomSeed = false;
        public static bool randomSeed = true;

        public static GameObject seedText;
        public static GameObject foodControlSeedText;
        private static SuperTextMesh titleSeedSTM;
        private static int lastDisplayedTitleSeed = int.MinValue;
        private static bool lastDisplayedTitleSeedEnabled = false;

        private static Random.State state = Random.state;
        private static Random.State messyState = Random.state;
        private static int stateDepth = 0;

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
            if (Plugin.SeedEnabled.Value)
            {
                if (stateDepth == 0)
                {
                    messyState = Random.state;
                    Random.state = state;
                }
                stateDepth++;
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
        // A finalizer rather than a postfix: it also runs when the patched Start() throws, so stateDepth can't
        // leak and leave Unity's global RNG swapped to the seeded stream for the rest of the session.
        [HarmonyFinalizer]
        public static void RestoreState()
        {
            if (Plugin.SeedEnabled.Value)
            {
                stateDepth--;
                if (stateDepth <= 0)
                {
                    stateDepth = 0;
                    state = Random.state;
                    Random.state = messyState;
                }
            }
        }

        [HarmonyPatch(typeof(TitleScreenControler), "StartGame")]
        [HarmonyPrefix]
        public static void Init()
        {
            if (Plugin.SeedEnabled.Value)
            {
                if (randomSeed)
                {
                    Seed = Core.SeedGenerator.FromTicks(DateTime.Now.Ticks);
                    Log.Info($"Seed set to {Seed}");
                    lastRandomSeed = Seed;
                    hasLastRandomSeed = true;
                    
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

        [HarmonyPatch(typeof(FoodControl), "Start")]
        [HarmonyPostfix]
        public static void addSeedText(FoodControl __instance)
        {
            if (__instance == QuitToMenuPatch.DestroyedDuplicate)
            {
                // A duplicate that is being destroyed; keep pointing at the surviving FoodControl's seed text.
                return;
            }
            if (Plugin.SeedEnabled.Value && __instance != null && __instance.inventoryText != null)
            {
                foodControlSeedText = GameObject.Instantiate(__instance.inventoryText.gameObject, __instance.inventoryText.transform);
                foodControlSeedText.name = "seedText";
                SuperTextMesh seedSTM = foodControlSeedText.GetComponent<SuperTextMesh>();
                if (seedSTM != null)
                {
                    seedSTM.text = "Seed: " + Seed;
                    seedTextBaseX = seedSTM.transform.localPosition.x;
                    PositionSeedText(seedSTM.transform);
                }
            }
        }

        [HarmonyPatch(typeof(FoodControl), "RefreshValues")]
        [HarmonyPostfix]
        public static void UpdateSeedText(FoodControl __instance)
        {
            if (Plugin.SeedEnabled.Value && foodControlSeedText != null)
            {
                SuperTextMesh seedSTM = foodControlSeedText.GetComponent<SuperTextMesh>();
                if (seedSTM != null)
                {
                    seedSTM.text = "Seed: " + Seed;
                    if (Screen.width != seedTextScreenWidth || Screen.height != seedTextScreenHeight)
                    {
                        PositionSeedText(seedSTM.transform);
                    }
                }
            }
        }

        // The inventory seed text is placed relative to the screen size, so it is re-placed when the
        // resolution changes rather than staying where the size at FoodControl.Start put it.
        private static float seedTextBaseX;
        private static int seedTextScreenWidth;
        private static int seedTextScreenHeight;

        private static void PositionSeedText(Transform seedTransform)
        {
            seedTextScreenWidth = Screen.width;
            seedTextScreenHeight = Screen.height;
            seedTransform.localPosition = new Vector3(
                (seedTextBaseX - Screen.width / 2f) + 100f,
                -Screen.height / 2f,
                seedTransform.localPosition.z
            );
        }

        [HarmonyPatch(typeof(TitleScreenControler), "Update")]
        [HarmonyPostfix]
        public static void ModifyTitleButtons(TitleScreenControler __instance)
        {
            if (seedText == null) return;

            bool enabled = Plugin.SeedEnabled.Value;
            if (enabled != lastDisplayedTitleSeedEnabled)
            {
                seedText.SetActive(enabled);
                lastDisplayedTitleSeedEnabled = enabled;
            }

            if (enabled && Seed != lastDisplayedTitleSeed)
            {
                if (titleSeedSTM == null)
                {
                    titleSeedSTM = seedText.GetComponent<SuperTextMesh>();
                }
                if (titleSeedSTM != null)
                {
                    titleSeedSTM.text = "Seed: " + Seed;
                    lastDisplayedTitleSeed = Seed;
                }
            }
        }

        [HarmonyPatch(typeof(TitleScreenControler), "Start")]
        [HarmonyPostfix]
        public static void ModifyTitleButtonPosition(TitleScreenControler __instance)
        {
            if (seedText == null && __instance != null && __instance.titleButtons != null)
            {
                Transform startButton = __instance.titleButtons.transform.Find("CreditsButton");
                if (startButton != null)
                {
                    SuperTextMesh originalText = startButton.GetComponentInChildren<SuperTextMesh>();
                    if (originalText != null)
                    {
                        seedText = GameObject.Instantiate(originalText.gameObject, startButton);
                        seedText.name = "SeedText";
                        titleSeedSTM = seedText.GetComponent<SuperTextMesh>();

                        seedText.transform.localPosition -= new Vector3(760f, 0f, 0f);
                        seedText.SetActive(Plugin.SeedEnabled.Value);
                        if (titleSeedSTM != null)
                        {
                            titleSeedSTM.text = "Seed: " + Seed;
                            lastDisplayedTitleSeed = Seed;
                            lastDisplayedTitleSeedEnabled = Plugin.SeedEnabled.Value;
                        }
                    }
                }
            }
        }
    }
}
