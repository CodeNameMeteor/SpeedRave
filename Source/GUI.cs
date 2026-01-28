using BepInEx;
using HarmonyLib;
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


        //private string DesiredScene = "";

        private Scene currentScene;


        private const int MAIN_WINDOW_ID = 0;
        private const int SCENE_WINDOW_ID = 1;

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
                }

                if (ReferenceManager.Player != null && ReferenceManager.PlayerController != null)
                {
                    if (Input.GetKeyDown(storePositionBind.ToLower()))
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

                    if (Input.GetKeyDown(restorePositionBind.ToLower()))
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


                if (ReferenceManager.ActiveFoodControl != null)
                {
                    if (Input.GetKeyDown(addCheeseBind.ToLower()))
                    {
                        ReferenceManager.ActiveFoodControl.cheese++;
                    }
                    if (Input.GetKeyDown(removeCheeseBind.ToLower()))
                    {
                        ReferenceManager.ActiveFoodControl.cheese--;
                    }
                    if (Input.GetKeyDown(addFruitBind.ToLower()))
                    {
                        ReferenceManager.ActiveFoodControl.fruit++;
                    }
                    if (Input.GetKeyDown(removeFruitBind.ToLower()))
                    {
                        ReferenceManager.ActiveFoodControl.fruit--;
                    }
                }

                if (Input.GetKeyDown(lockBind.ToLower()))
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
            }
        }

        private void Start()
        {
            if (Autosplitter.Instance != null)
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
            if(sceneSelectorShowGUI)
            {
                sceneWinRect = GUI.Window(SCENE_WINDOW_ID, sceneWinRect, SceneWinProc,"Scene" );
            }
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
            currentScene = SceneManager.GetActiveScene();
            GUILayout.Label("Connected To LiveSplit:" + Autosplitter.Instance.IsConnectedToLivesplit);
            if (GUILayout.Button("Connect") && !Autosplitter.Instance.IsConnectedToLivesplit)
            {
                Autosplitter.Instance.ConnectToLiveSplit();
            }
            GUILayout.Label("Seed:" + Patches.SetSeedPatchs.Seed);
            GUILayout.Label("Enter Seed:");
            seedInput = GUILayout.TextField(seedInput, 11);
            if (GUILayout.Button("Set Seed"))
            {
                if (int.TryParse(seedInput, out parsedSeed))
                {
                    Patches.SetSeedPatchs.Seed = parsedSeed;
                    UnityEngine.Random.InitState(parsedSeed);
                    Patches.SetSeedPatchs.randomSeed = false;
                    Debug.Log($"[SpeedRave] Seed set to {parsedSeed}");
                }
                else
                {
                    Debug.LogWarning("[SpeedRave] Invalid seed input!");
                }
            }
            if (GUILayout.Button("Set Last Random Seed") && Patches.SetSeedPatchs.lastRandomSeed != 0)
            {
                Patches.SetSeedPatchs.Seed = Patches.SetSeedPatchs.lastRandomSeed;
                Patches.SetSeedPatchs.randomSeed = false;
            }
            Patches.SetSeedPatchs.randomSeed = GUILayout.Toggle(Patches.SetSeedPatchs.randomSeed, "Use Random Seed");
            if (GUILayout.Button("Open Scene Selector"))
            {
                sceneSelectorShowGUI = !sceneSelectorShowGUI;
            }
            GUILayout.Label("Press U To Add Cheese");
            GUILayout.Label("Press I To Subtract Cheese");
            GUILayout.Label("Press O To Add Fruit");
            GUILayout.Label("Press P To Subtract Fruit");
            GUILayout.Label("Press Z To Store Position");
            GUILayout.Label("Press X To Restore Position");
            GUILayout.Label("Press L To Lock The Scene");
            GUILayout.Label("Scene locked: " + locked);
            

            /*
            DesiredScene = GUILayout.TextField("EnterScene", 100);
            if (GUILayout.Button("Enter"))
            {
                SceneManager.LoadScene(DesiredScene);
            }
            
            if (GUILayout.Button("Test") )
            {
                SceneManager.LoadScene(sceneIndex);
                sceneIndex++;
            }
            */
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}