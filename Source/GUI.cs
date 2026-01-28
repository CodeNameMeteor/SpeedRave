using BepInEx;
using HarmonyLib;
using SpeedRave.Patches;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

namespace SpeedRave
{
    public class GUIComponent : MonoBehaviour
    {
        public static string[] Scenes = {
            "BattleRoom1",
            "BlubRoom",
            "BrickRollRoom1",
            "CaveRoom1",
            "ComplexRoom1",
            "Credits",
            "DeerRoom1",
            "DeerRoom2",
            "EnterCatRoom1",
            "EnterTrainRoom",
            "ExitRoom1",
            "FaultRoom1",
            "FightRoom1",
            "FishingRoom1",
            "FloatRoom1",
            "FogRoom1",
            "FogRoom2",
            "GunStore1",
            "HallRoom1",
            "IceRoom1",
            "JumpRoom1",
            "JumpRoom2",
            "JumpRoom3",
            "JumpRoom4",
            "JumpRoom5",
            "JasmineRoom1",
            "LadySaytennRoom",
            "LampRoom1",
            "LampRoom2",
            "LonelyRoom1",
            "LoseRoom1",
            "MicroGameRoom1",
            "NarrowRoom1",
            "NarrowRoom2",
            "NarrowRoom3",
            "NarrowRoom4",
            "NarrowRoom5",
            "PianoRoom1",
            "PitRoom1",
            "PitRoom2",
            "PitRoom3",
            "PitRoom4",
            "PitRoom5",
            "Plaguending",
            "PossumQueenRoom1",
            "PunchRoom1",
            "RapBattleRoom1",
            "RapBattleRoom2",
            "ScienceRoom1",
            "Sewer_Start",
            "SpaceRoom1",
            "Spaceending",
            "StretchRoom1",
            "StretchRoom2",
            "StretchRoom3",
            "StudyRoom1",
            "TallRoom1",
            "TallRoom2",
            "TallRoom3",
            "TheatreRoom1",
            "TiltRoom1",
            "TiltRoom2",
            "TiltRoom3",
            "TitleScreen",
            "Truending",
            "UpgradeRoom1",
            "WinRoom1"
        };

        public static int selectedScene = 0;
        public const int X = 20;
        public const int Y = 20;
        public const int WIDTH = 275;
        public const int HEIGHT = 550;
        public static bool showGUI = false;
        public static bool sceneSelectorShowGUI = false;

        private static bool configShowGUI = false;
        private Vector2 configScroll = Vector2.zero;


        //private string DesiredScene = "";

        private Scene currentScene;


        private const int MAIN_WINDOW_ID = 0;
        private const int SCENE_WINDOW_ID = 1;

        private static Rect configWinRect = new Rect(X + WIDTH + 20, Y, 300, 450);
        private static Rect winRect = new(X, Y, WIDTH, HEIGHT);
        private static Rect sceneWinRect = new(
            winRect.x + winRect.width + 10, // 10px padding to the right
            winRect.y,
            1100,
            300
        );

        public static bool Use;

        public static bool locked = false;

        public static string addCheeseBind = "u";
        public static string removeCheeseBind = "i";
        public static string addFruitBind = "o";
        public static string removeFruitBind = "p";
        public static string lockBind = "l";
        public static string storePositionBind = "z";
        public static string restorePositionBind = "x";
        public static string openTrainerBind = "insert";


        //private Autosplitter autosplitter;

        private Vector3 storedPosition;
        private Quaternion storedCharacterRot;
        private Quaternion storedCameraRot;
        private FieldInfo cameraField;
        private FieldInfo mouseLookField;
        private FieldInfo characterTargetRotField;
        private FieldInfo cameraTargetRotField;

        private string seedInput = "";
        private int parsedSeed = 0;


        private void Update()
        {
            if (Use)
            {
                if (Input.GetKeyDown(openTrainerBind.ToLower()))
                {
                    showGUI = !showGUI;
                    if (sceneSelectorShowGUI)
                    {
                        sceneSelectorShowGUI = false;
                    }
                    if(configShowGUI)
                    {
                        configShowGUI = false;
                    }
                }

                if (Input.GetKeyDown(storePositionBind.ToLower()))
                {
                    StorePlayerPosition();
                }

                if (Input.GetKeyDown(restorePositionBind.ToLower()))
                {
                    RestorePlayerPosition();
                }


                if (ReferenceManager.ActiveFoodControl != null)
                {
                    if (Input.GetKeyDown(addCheeseBind.ToLower()))
                    {
                        ModifyCheese(1);
                    }
                    if (Input.GetKeyDown(removeCheeseBind.ToLower()))
                    {
                        ModifyCheese(-1);
                    }
                    if (Input.GetKeyDown(addFruitBind.ToLower()))
                    {
                        ModifyFruit(1);
                    }
                    if (Input.GetKeyDown(removeFruitBind.ToLower()))
                    {
                        ModifyFruit(-1);
                    }
                }

                if (Input.GetKeyDown(lockBind.ToLower()))
                {
                    ToggleSceneLock();
                }
            }
        }

        private void Start()
        {
            if (Autosplitter.Instance != null && Autosplitter.Use)
            {
                Autosplitter.Instance.ConnectToLiveSplit();
            }
            else
            {
                Debug.LogError("Autosplitter component not found");
            }
        }
        private void OnGUI()
        {
            if (showGUI)
            {
                winRect = GUI.Window(MAIN_WINDOW_ID, winRect, WinProc, $"{Plugin.modName} {Plugin.modVersion}");
            }
            if (sceneSelectorShowGUI)
            {
                sceneWinRect = GUI.Window(SCENE_WINDOW_ID, sceneWinRect, SceneWinProc, "Room Selector");
            }
            if(configShowGUI)
            {
                configWinRect = GUI.Window(2, configWinRect, ConfigWinProc, "SpeedRave Config");
            }
        }
        private void ConfigWinProc(int id)
        {
            configScroll = GUILayout.BeginScrollView(configScroll);
            GUILayout.Label("<b>Patches</b>");
            QuickStartPatch.Use = GUILayout.Toggle(QuickStartPatch.Use, " Quick Start");
            QuitToMenuPatch.Use = GUILayout.Toggle(QuitToMenuPatch.Use, " Quit to Menu");
            RemoveMusicPatch.Use = GUILayout.Toggle(RemoveMusicPatch.Use, " Remove Music");


            GUILayout.Label("<b>Autosplitter</b>");

            Autosplitter.Use = GUILayout.Toggle(Autosplitter.Use, " Enable Autosplitter");
            Autosplitter.twentyResourceSplit = GUILayout.Toggle(Autosplitter.twentyResourceSplit, " Split on 20 Resources");
            Autosplitter.keySplit = GUILayout.Toggle(Autosplitter.keySplit, " Split on Key");
            Autosplitter.twentyFruitSplit = GUILayout.Toggle(Autosplitter.twentyFruitSplit, " Split on 20 Fruit");
            Autosplitter.itemSplit = GUILayout.Toggle(Autosplitter.itemSplit, " Split on Item Pickup");

            GUILayout.Space(10);

            GUILayout.Label("<b>Seed Control</b>");

            SetSeedPatchs.Use = GUILayout.Toggle(SetSeedPatchs.Use, " Enable Seeds");

            GUILayout.Label("<b>Inventory Overlay</b>");

            // Boolean Toggle
            InventoryOverlay.showInventory = GUILayout.Toggle(InventoryOverlay.showInventory, " Show Inventory");
            InventoryOverlay.useIcons = GUILayout.Toggle(InventoryOverlay.useIcons, " Use Icons");
            InventoryOverlay.verticalIcons = GUILayout.Toggle(InventoryOverlay.verticalIcons, " Vertical Item Icons");

            // Float Slider for Icon Size
            GUILayout.Label($"Icon Size: {InventoryOverlay.iconSize:F0}");
            InventoryOverlay.iconSize = GUILayout.HorizontalSlider(InventoryOverlay.iconSize, 20f, 150f);

            // Float Slider for Text Height
            GUILayout.Label($"Text Size: {InventoryOverlay.textHeight:F0}");
            InventoryOverlay.textHeight = GUILayout.HorizontalSlider(InventoryOverlay.textHeight, 20f, 150f);

            // Float Slider for Logo Padding
            GUILayout.Label($"Logo Padding: {InventoryOverlay.padding:F0}");
            InventoryOverlay.padding = GUILayout.HorizontalSlider(InventoryOverlay.padding, 10f, 150f);

            GUILayout.Space(10);
            GUILayout.Label("<b>Trainer</b>");

            GUIComponent.Use = GUILayout.Toggle(GUIComponent.Use, " Enable Trainer");

            GUILayout.Label("<b>Binds (Press Enter to apply)</b>");
            GUIComponent.addCheeseBind = GUILayout.TextField(GUIComponent.addCheeseBind);
            GUIComponent.removeCheeseBind = GUILayout.TextField(GUIComponent.removeCheeseBind);
            GUIComponent.addFruitBind = GUILayout.TextField(GUIComponent.addFruitBind);
            GUIComponent.removeFruitBind = GUILayout.TextField(GUIComponent.removeFruitBind);
            GUIComponent.lockBind = GUILayout.TextField(GUIComponent.lockBind);
            GUIComponent.storePositionBind = GUILayout.TextField(GUIComponent.storePositionBind);
            GUIComponent.restorePositionBind = GUILayout.TextField(GUIComponent.restorePositionBind);


            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        private void SceneWinProc(int id)
        {
            GUILayout.Label("Select a Scene:");

            selectedScene = GUILayout.SelectionGrid(selectedScene, Scenes, 10);

            if (GUILayout.Button("Go To Scene"))
            {
                SceneManager.LoadScene(Scenes[selectedScene]);
            }

            GUI.DragWindow();
        }

        private void WinProc(int id)
        {
            if(Autosplitter.Use)
            {
                // Autosplitter 
                GUILayout.Label("<b>Autosplitter</b>");
                GUILayout.Label($"Connected: {Autosplitter.Instance.IsConnectedToLivesplit}");
                if (!Autosplitter.Instance.IsConnectedToLivesplit)
                {
                    if (GUILayout.Button("Connect to LiveSplit"))
                        Autosplitter.Instance.ConnectToLiveSplit();
                }

                GUILayout.Space(5);
            }

            // Seed Control
            if(SetSeedPatchs.Use)
            {
                GUILayout.Label("<b>Seed Control</b>");
                GUILayout.Label($"Current: {Patches.SetSeedPatchs.Seed}");
                seedInput = GUILayout.TextField(seedInput, 11);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Set Seed"))
                {
                    if (int.TryParse(seedInput, out parsedSeed))
                    {
                        Patches.SetSeedPatchs.Seed = parsedSeed;
                        UnityEngine.Random.InitState(parsedSeed);
                        Patches.SetSeedPatchs.randomSeed = false;
                    }
                }
                if (GUILayout.Button("Last Random"))
                {
                    if (Patches.SetSeedPatchs.lastRandomSeed != 0)
                    {
                        Patches.SetSeedPatchs.Seed = Patches.SetSeedPatchs.lastRandomSeed;
                        Patches.SetSeedPatchs.randomSeed = false;
                    }
                }
                GUILayout.EndHorizontal();
                Patches.SetSeedPatchs.randomSeed = GUILayout.Toggle(Patches.SetSeedPatchs.randomSeed, " Use Random Seed");
            }
            
            if(GUIComponent.Use)
            {
                // Room Selector
                GUILayout.Label("<b>Scene Selector</b>");
                GUILayout.Label($"Current Room: {SceneManager.GetActiveScene().name}");
                if (GUILayout.Button(sceneSelectorShowGUI ? "Close Selector" : "Open Room Selector"))
                {
                    sceneSelectorShowGUI = !sceneSelectorShowGUI;
                }

                GUILayout.Space(5);

                // Room Locking
                GUILayout.Label("<b>Room Lock</b>");
                string lockStatus = locked ? "<color=red>LOCKED</color>" : "<color=green>UNLOCKED</color>";
                GUILayout.Label($"Status: {lockStatus}");
                if (GUILayout.Button(locked ? "Unlock (L)" : "Lock (L)"))
                {
                    locked = !locked;
                    if (locked) Patches.SceneLock.lockedScene = SceneManager.GetActiveScene().name;
                }

                // Trainer
                GUILayout.Label("<b>Trainer </b>");

                // Cheese Row
                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"Add Cheese ({addCheeseBind.ToUpper()})")) ModifyCheese(1);
                if (GUILayout.Button($"Sub Cheese ({removeCheeseBind.ToUpper()})")) ModifyCheese(-1);
                GUILayout.EndHorizontal();

                // Fruit Row
                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"Add Fruit ({addFruitBind.ToUpper()})")) ModifyFruit(1);
                if (GUILayout.Button($"Sub Fruit ({removeFruitBind.ToUpper()})")) ModifyFruit(-1);
                GUILayout.EndHorizontal();

                // Position Row
                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"Store Pos ({storePositionBind.ToUpper()})")) StorePlayerPosition();
                if (GUILayout.Button($"Restore Pos ({restorePositionBind.ToUpper()})")) RestorePlayerPosition();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(configShowGUI ? "Close Config" : "Open Config UI"))
            {
                configShowGUI = !configShowGUI;
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void ToggleSceneLock()
        {
            if (!locked)
            {
                Patches.SceneLock.lockedScene = SceneManager.GetActiveScene().name;
                locked = true;
            }
            else
            {
                locked = false;
            }
        }
        private void ModifyFruit(int amount)
        {
            if (ReferenceManager.ActiveFoodControl != null)
            {
                ReferenceManager.ActiveFoodControl.fruit += amount;
            }
        }
        private void ModifyCheese(int amount)
        {
            if (ReferenceManager.ActiveFoodControl != null)
            {
                ReferenceManager.ActiveFoodControl.cheese += amount;
            }
        }
        private void StorePlayerPosition()
        {
            if (ReferenceManager.Player != null && ReferenceManager.PlayerController != null)
            {
                storedPosition = ReferenceManager.PlayerController.transform.position;

                object mouseLookObj = ReferenceManager.MouseLookField.GetValue(ReferenceManager.PlayerController);

                if (mouseLookObj != null)
                {
                    object charRotObj = ReferenceManager.CharacterTargetRotField.GetValue(mouseLookObj);
                    object camRotObj = ReferenceManager.CameraTargetRotField.GetValue(mouseLookObj);

                    if (charRotObj is Quaternion charRot && camRotObj is Quaternion camRot)
                    {
                        storedCharacterRot = charRot;
                        storedCameraRot = camRot;
                    }
                }
            }
        }
        private void RestorePlayerPosition()
        {
            if (ReferenceManager.Player != null && ReferenceManager.PlayerController != null)
            {
                ReferenceManager.PlayerController.transform.position = storedPosition;
                object mouseLookObj = ReferenceManager.MouseLookField.GetValue(ReferenceManager.PlayerController);
                if (mouseLookObj != null)
                {
                    ReferenceManager.CharacterTargetRotField.SetValue(mouseLookObj, storedCharacterRot);
                    ReferenceManager.CameraTargetRotField.SetValue(mouseLookObj, storedCameraRot);
                    var camera = ReferenceManager.MainCamera;
                    if (camera != null)
                    {
                        ReferenceManager.Player.transform.localRotation = storedCharacterRot;
                        camera.transform.localRotation = storedCameraRot;
                    }
                }
            }
        }
    }
}