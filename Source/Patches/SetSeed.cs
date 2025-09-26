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
        public static int lastRandomSeed = 0;
        public static bool randomSeed = true;

        public static GameObject seedText;
        public static GameObject foodControlSeedText;


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
                    lastRandomSeed = Seed;
                    
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
            if (Use)
            {
                foodControlSeedText = GameObject.Instantiate(__instance.inventoryText.gameObject, __instance.inventoryText.transform);
                foodControlSeedText.name = "seedText";
                SuperTextMesh seedSTM = foodControlSeedText.GetComponent<SuperTextMesh>();
                seedSTM.text = "Seed: " + Seed;
                seedSTM.transform.localPosition = new Vector3(
                //seedSTM.transform.localPosition.x - 850f,
                (seedSTM.transform.localPosition.x - Screen.width/2) + 100, 
                seedSTM.transform.localPosition.y - seedSTM.transform.localPosition.y - Screen.height/2,
                seedSTM.transform.localPosition.z
                );
                //seedSTM.transform.localPosition -= new Vector3(850f, 540f, 0f);
                //seedSTM.transform.localPosition -= new Vector3(0f, seedSTM.transform.localPosition.y, 0f);
                //seedSTM.transform.localPosition += new Vector3(0f, Screen.currentResolution.height-200, 0f);
                //__instance.inventoryText.Text = "CHEESE: " + __instance.cheese.ToString() + "\nFRUIT: " + __instance.fruit.ToString() + "\nSEED: " + Seed.ToString();
            }
        }
        [HarmonyPatch(typeof(FoodControl), "RefreshValues")]
        [HarmonyPostfix]
        public static void UpdateSeedText(FoodControl __instance)
        {
            if (Use)
            {
                SuperTextMesh seedSTM = foodControlSeedText.GetComponent<SuperTextMesh>();
                seedSTM.text = "Seed: " + Seed;
            }
        }
        [HarmonyPatch(typeof(TitleScreenControler), "Update")]
        [HarmonyPostfix]
        public static void ModifyTitleButtons(TitleScreenControler __instance)
        {

            if(Use)
            {
                SuperTextMesh seedSTM = seedText.GetComponent<SuperTextMesh>();
                seedSTM.text = "Seed: " + Seed;
            }
        }
        [HarmonyPatch(typeof(TitleScreenControler), "Start")]
        [HarmonyPostfix]
        public static void ModifyTitleButtonPositon(TitleScreenControler __instance)
        {
            //ModifyButtonPosition(__instance.titleButtons, "ExitButton", new Vector3(-60.0f,0,0));
            // Clone the original button GameObject
            if(seedText == null || Use)
            {
                Transform startButton = __instance.titleButtons.transform.Find("CreditsButton");

                SuperTextMesh originalText = startButton.GetComponentInChildren<SuperTextMesh>();

                seedText = GameObject.Instantiate(originalText.gameObject, startButton);
                seedText.name = "SeedText";

                //moving the text to a better position
                seedText.transform.localPosition -= new Vector3(760f, 0f, 0f);
            }

        }
    }
}
