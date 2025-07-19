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

        public const int X = 20;
        public const int Y = 20;
        public const int WIDTH = 275;
        public const int HEIGHT = 550;
        public static bool showGUI = false;

        private bool trainerToggle = false;

        //private string DesiredScene = "";

        private Scene currentScene;

        private int sceneIndex = 1;

        private Rect winRect = new(X, Y, WIDTH, HEIGHT);

        public static bool Use;

        public static bool locked = false;


        private Autosplitter autosplitter;

        private Vector3 storedPosition;
        private Quaternion storedCharacterRot;
        private Quaternion storedCameraRot;
        private FieldInfo cameraField;
        private FieldInfo mouseLookField;
        private FieldInfo characterTargetRotField;
        private FieldInfo cameraTargetRotField;

        private string seedInput = "";         // the string typed by user
        private int parsedSeed = 0;            // the parsed seed (optional)


        private void Update()
        {
            if (Use)
            {

                if (Input.GetKeyDown(KeyCode.Insert))
                {
                    showGUI = !showGUI;      
                }
                if(trainerToggle)
                {
                    if (Input.GetKeyDown(KeyCode.J) )
                    {
                        if(sceneIndex >= 68)
                        {
                            sceneIndex = 0;
                            SceneManager.LoadScene(sceneIndex);
                        }
                        else
                        {
                            sceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                            SceneManager.LoadScene(sceneIndex);
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.K) )
                    {
                        if(sceneIndex <= 0)
                        {
                            sceneIndex = 68;
                            SceneManager.LoadScene(sceneIndex);
                        }
                        else
                        {
                            sceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
                            SceneManager.LoadScene(sceneIndex);
                        }
                    }
                    var FoodControlArray = FindObjectsOfType<FoodControl>();
                    if (FoodControlArray.Length > 1)
                    {
                        for (int i = FoodControlArray.Length - 1; i > 0; i--)
                        {
                            Destroy(FoodControlArray[i]);
                        }
                    }
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (Input.GetKeyDown(KeyCode.Z) && player != null)
                    {
                        var playerControls = player.GetComponent<FirstPersonController>();
                        storedPosition = player.transform.position;

                        var mouseLook = mouseLookField.GetValue(playerControls);
                        if (mouseLook != null)
                        {
                            object charRotObj = characterTargetRotField.GetValue(mouseLook);
                            object camRotObj = cameraTargetRotField.GetValue(mouseLook);

                            if (charRotObj is Quaternion charRot && camRotObj is Quaternion camRot)
                            {
                                storedCharacterRot = charRot;
                                storedCameraRot = camRot;
                            }
                        }

                    }
                    if (Input.GetKeyDown(KeyCode.X) && player != null)
                    {
                        var playerControls = player.GetComponent<FirstPersonController>();
                        player.transform.position = storedPosition;
                        var mouseLook = mouseLookField.GetValue(playerControls);
                        if (mouseLook != null)
                        {
                            characterTargetRotField.SetValue(mouseLook, storedCharacterRot);
                            cameraTargetRotField.SetValue(mouseLook, storedCameraRot);

                            // Also set actual transforms to match the target rots
                            var camera = (Camera)cameraField.GetValue(playerControls);                            
                            if (camera != null)
                            {
                                player.transform.localRotation = storedCharacterRot;
                                camera.transform.localRotation = storedCameraRot;
                            }

                            

                        }
                    }
                    if (Input.GetKeyDown(KeyCode.U))
                    {
                        FoodControlArray[0].cheese++;
                    }
                    if (Input.GetKeyDown(KeyCode.I))
                    {
                        FoodControlArray[0].cheese--;
                    }
                    if (Input.GetKeyDown(KeyCode.O))
                    {
                        FoodControlArray[0].fruit++;
                    }
                    if (Input.GetKeyDown(KeyCode.P))
                    {
                        FoodControlArray[0].fruit--;
                    }
                    if(Input.GetKeyDown(KeyCode.L) && !locked )
                    {
                        Patches.SceneLock.lockedScene = SceneManager.GetActiveScene().name;
                        locked = true;
                    }
                    else if(Input.GetKeyDown(KeyCode.L) && locked)
                    {
                        locked = false;
                    }
                    
                }        
              
            }
        }

        private void Start()
        {
            mouseLookField = AccessTools.Field(typeof(FirstPersonController), "m_MouseLook");
            characterTargetRotField = AccessTools.Field(typeof(MouseLook), "m_CharacterTargetRot");
            cameraTargetRotField = AccessTools.Field(typeof(MouseLook), "m_CameraTargetRot");
            cameraField = AccessTools.Field(typeof(FirstPersonController), "m_Camera");
            autosplitter = FindObjectOfType<Autosplitter>();
            if (autosplitter != null)
            {
                autosplitter.ConnectToLiveSplit();
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
                winRect = GUI.Window(0, winRect, WinProc, $"{Plugin.modName} {Plugin.modVersion}");
            }
        }
        
        private void WinProc(int id)
        {
            currentScene = SceneManager.GetActiveScene();
            GUILayout.Label("Connected To LiveSplit:" + autosplitter.IsConnectedToLivesplit);
            if (GUILayout.Button("Connect") && !autosplitter.IsConnectedToLivesplit)
            {
                autosplitter.ConnectToLiveSplit();
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
            GUILayout.Space(5);
            trainerToggle = GUILayout.Toggle(trainerToggle, "Enable Trainer");
            if (trainerToggle)
            {
                GUILayout.Label("Current Scene is:");
                GUILayout.Label(currentScene.name);
                GUILayout.Label("Press J To Go To The Next Scene");
                GUILayout.Label("Press K To Go To The Previous Scene");
                GUILayout.Label("Press U To Add Cheese");
                GUILayout.Label("Press I To Subtract Cheese");
                GUILayout.Label("Press O To Add Fruit");
                GUILayout.Label("Press P To Subtract Fruit");
                GUILayout.Label("Press Z To Store Position");
                GUILayout.Label("Press X To Restore Position");
                GUILayout.Label("Press L To Lock The Scene");
                GUILayout.Label("Scene locked: " + locked);
            }
            

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