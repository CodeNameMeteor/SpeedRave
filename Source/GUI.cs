using BepInEx;
using BepInEx.Configuration;
using SpeedRave.Patches;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public const int HEIGHT = 600;
        public static bool showGUI = false;

        // True while a trainer text field has keyboard focus. Hotkeys, QuickStart and player movement are
        // suppressed so typing a seed or a bind name doesn't trigger them.
        public static bool IsTyping => showGUI && GUIUtility.keyboardControl != 0;

        // Bind text that has been edited but not applied yet, so half-typed names never become live binds.
        private readonly Dictionary<ConfigEntry<string>, string> pendingBinds = new Dictionary<ConfigEntry<string>, string>();
        public static bool sceneSelectorShowGUI = false;

        private static bool configShowGUI = false;
        private Vector2 configScroll = Vector2.zero;
        private Vector2 sceneScroll = Vector2.zero;

        private int sceneIndex = 0;

        private const int MAIN_WINDOW_ID = 0;
        private const int SCENE_WINDOW_ID = 1;
        private const int CONFIG_WINDOW_ID = 2;

        private static Rect configWinRect = new Rect(X + WIDTH + 20, Y, 320, 500);
        private static Rect winRect = new(X, Y, WIDTH, HEIGHT);
        private static Rect sceneWinRect = new(
            winRect.x + winRect.width + 10,
            winRect.y,
            1100,
            350
        );

        public static bool locked = false;

        private Vector3 storedPosition;
        private Quaternion storedCharacterRot;
        private Quaternion storedCameraRot;
        private bool hasStoredPosition = false;

        private string seedInput = "";
        private int parsedSeed = 0;
        private string fpsInput = "-1";
        private float saveFeedbackTime = 0f;
        private bool saveSuccess = false;
        private float seedFeedbackTime = 0f;
        private string seedFeedbackMessage = "";

        private static float seedFlashTimer = 0f;
        private static string seedFlashText = "";
        private static string seedFlashMode = "";
        private GUIStyle seedFlashStyle;
        private GUIStyle seedFlashShadowStyle;

        private static bool SafeGetKeyDown(string bind)
        {
            if (string.IsNullOrWhiteSpace(bind)) return false;
            try
            {
                return Input.GetKeyDown(bind.Trim().ToLowerInvariant());
            }
            catch
            {
                return false;
            }
        }

        private void Start()
        {
            // Show the configured cap rather than a hard-coded -1.
            fpsInput = Plugin.TargetFPS.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private void Update()
        {
            if (SafeGetKeyDown(Plugin.OpenTrainerBind.Value))
            {
                showGUI = !showGUI;
                GUIUtility.keyboardControl = 0;
                if (sceneSelectorShowGUI)
                {
                    sceneSelectorShowGUI = false;
                }
                if (configShowGUI)
                {
                    configShowGUI = false;
                }
            }

            if (IsTyping)
            {
                return;
            }

            if (SafeGetKeyDown(Plugin.RestartBind.Value))
            {
                TriggerInstantRestart();
            }

            if (Plugin.TrainerEnabled.Value)
            {

                if (SafeGetKeyDown(Plugin.StorePositionBind.Value))
                {
                    StorePlayerPosition();
                }

                if (SafeGetKeyDown(Plugin.RestorePositionBind.Value))
                {
                    RestorePlayerPosition();
                }

                if (SafeGetKeyDown(Plugin.IncrementSceneBind.Value))
                {
                    sceneIndex = GetCurrentSceneIndex();
                    sceneIndex = (sceneIndex + 1) % Scenes.Length;
                    SceneManager.LoadScene(Scenes[sceneIndex]);
                }
                if (SafeGetKeyDown(Plugin.DecrementSceneBind.Value))
                {
                    sceneIndex = GetCurrentSceneIndex();
                    sceneIndex = (sceneIndex - 1 + Scenes.Length) % Scenes.Length;
                    SceneManager.LoadScene(Scenes[sceneIndex]);
                }
                if (SafeGetKeyDown(Plugin.LockBind.Value))
                {
                    ToggleSceneLock();
                }

                if (ReferenceManager.ActiveFoodControl != null)
                {
                    if (SafeGetKeyDown(Plugin.AddCheeseBind.Value))
                    {
                        ModifyCheese(1);
                    }
                    if (SafeGetKeyDown(Plugin.RemoveCheeseBind.Value))
                    {
                        ModifyCheese(-1);
                    }
                    if (SafeGetKeyDown(Plugin.AddFruitBind.Value))
                    {
                        ModifyFruit(1);
                    }
                    if (SafeGetKeyDown(Plugin.RemoveFruitBind.Value))
                    {
                        ModifyFruit(-1);
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (Time.unscaledTime < seedFlashTimer && !string.IsNullOrEmpty(seedFlashText))
            {
                DrawSeedFlash();
            }

            if (showGUI)
            {
                winRect = GUI.Window(MAIN_WINDOW_ID, winRect, WinProc, $"{Plugin.modName} {Plugin.modVersion}");

                if (sceneSelectorShowGUI)
                {
                    sceneWinRect = GUI.Window(SCENE_WINDOW_ID, sceneWinRect, SceneWinProc, "Room Selector");
                }

                if (configShowGUI)
                {
                    configWinRect = GUI.Window(CONFIG_WINDOW_ID, configWinRect, ConfigWinProc, "SpeedRave Config");
                }
            }
        }

        private void DrawSeedFlash()
        {
            if (seedFlashStyle == null)
            {
                seedFlashStyle = new GUIStyle();
                seedFlashStyle.fontSize = 32;
                seedFlashStyle.alignment = TextAnchor.UpperLeft;
                seedFlashStyle.fontStyle = FontStyle.Bold;

                seedFlashShadowStyle = new GUIStyle();
                seedFlashShadowStyle.fontSize = 32;
                seedFlashShadowStyle.alignment = TextAnchor.UpperLeft;
                seedFlashShadowStyle.fontStyle = FontStyle.Bold;
            }

            if (InventoryOverlay.GameFont != null)
            {
                seedFlashStyle.font = InventoryOverlay.GameFont;
                seedFlashShadowStyle.font = InventoryOverlay.GameFont;
            }

            float timeLeft = seedFlashTimer - Time.unscaledTime;
            float alpha = Mathf.Clamp01(timeLeft / 0.25f);

            Color textColor = new Color(1f, 1f, 1f, alpha);
            Color shadowColor = new Color(0f, 0f, 0f, alpha * 0.85f);

            seedFlashStyle.normal.textColor = textColor;
            seedFlashShadowStyle.normal.textColor = shadowColor;

            float x = 20f;
            float y = 20f;
            float w = 500f;
            float h = 50f;

            GUI.Label(new Rect(x + 2, y + 2, w, h), seedFlashText, seedFlashShadowStyle);
            GUI.Label(new Rect(x, y, w, h), seedFlashText, seedFlashStyle);

            if (!string.IsNullOrEmpty(seedFlashMode))
            {
                float modeY = y + 36f;
                GUI.Label(new Rect(x + 2, modeY + 2, w, h), seedFlashMode, seedFlashShadowStyle);
                GUI.Label(new Rect(x, modeY, w, h), seedFlashMode, seedFlashStyle);
            }
        }

        private void ConfigWinProc(int id)
        {
            configScroll = GUILayout.BeginScrollView(configScroll);

            GUILayout.Label("<b>Patches</b>");
            Plugin.QuickStart.Value = GUILayout.Toggle(Plugin.QuickStart.Value, " Quick Start (Space to Start)");
            Plugin.QuitToMenu.Value = GUILayout.Toggle(Plugin.QuitToMenu.Value, " Quit to Menu (Cancel Key)");
            Plugin.ClearSaveOnStart.Value = GUILayout.Toggle(Plugin.ClearSaveOnStart.Value, " Clear Save on New Game (Speedruns)");
            Plugin.RemoveMusic.Value = GUILayout.Toggle(Plugin.RemoveMusic.Value, " Remove Music");

            GUILayout.Label("<b>Autosplitter</b>");
            Plugin.AutosplitterEnabled.Value = GUILayout.Toggle(Plugin.AutosplitterEnabled.Value, " Enable Autosplitter");
            Plugin.TwentyResourceSplit.Value = GUILayout.Toggle(Plugin.TwentyResourceSplit.Value, " Split on 20 Resources");
            Plugin.KeySplit.Value = GUILayout.Toggle(Plugin.KeySplit.Value, " Split on Key");
            Plugin.TwentyFruitSplit.Value = GUILayout.Toggle(Plugin.TwentyFruitSplit.Value, " Split on 20 Fruit");
            Plugin.ItemSplit.Value = GUILayout.Toggle(Plugin.ItemSplit.Value, " Split on Item Pickup");
            Plugin.AllEndings.Value = GUILayout.Toggle(Plugin.AllEndings.Value, " All Endings (run ends after all 3)");
            Plugin.LiveSplitAutoReconnect.Value = GUILayout.Toggle(Plugin.LiveSplitAutoReconnect.Value, " Auto-Reconnect LiveSplit (Every 6s)");

            GUILayout.Space(10);

            GUILayout.Label("<b>Seed Control</b>");
            Plugin.SeedEnabled.Value = GUILayout.Toggle(Plugin.SeedEnabled.Value, " Enable Seeding");

            GUILayout.Label("<b>Speedrun Timer</b>");
            Plugin.ShowOnScreenTimer.Value = GUILayout.Toggle(Plugin.ShowOnScreenTimer.Value, " Show On-Screen Timer");
            if (Plugin.ShowOnScreenTimer.Value)
            {
                GUILayout.Label($"Timer Font Size: {Plugin.TimerFontSize.Value:F0}");
                Plugin.TimerFontSize.Value = GUILayout.HorizontalSlider(Plugin.TimerFontSize.Value, 20f, 80f);
            }

            GUILayout.Label("<b>Inventory Overlay</b>");
            Plugin.InventoryOverlayEnabled.Value = GUILayout.Toggle(Plugin.InventoryOverlayEnabled.Value, " Enable Inventory Overlay");
            Plugin.UseIcons.Value = GUILayout.Toggle(Plugin.UseIcons.Value, " Use Icons");
            Plugin.VerticalIcons.Value = GUILayout.Toggle(Plugin.VerticalIcons.Value, " Vertical Item Icons");

            GUILayout.Label($"Icon Size: {Plugin.IconSize.Value:F0}");
            Plugin.IconSize.Value = GUILayout.HorizontalSlider(Plugin.IconSize.Value, 20f, 150f);

            GUILayout.Label($"Text Size: {Plugin.TextHeight.Value:F0}");
            Plugin.TextHeight.Value = GUILayout.HorizontalSlider(Plugin.TextHeight.Value, 20f, 150f);

            GUILayout.Label($"Item Padding: {Plugin.Padding.Value:F0}");
            Plugin.Padding.Value = GUILayout.HorizontalSlider(Plugin.Padding.Value, 10f, 150f);

            GUILayout.Label("<b>Performance</b>");
            bool currentVSync = Plugin.VSyncEnabled.Value;
            Plugin.VSyncEnabled.Value = GUILayout.Toggle(Plugin.VSyncEnabled.Value, " Enable V-Sync");

            if (currentVSync != Plugin.VSyncEnabled.Value)
            {
                QualitySettings.vSyncCount = Plugin.VSyncEnabled.Value ? 1 : 0;
            }

            if (!Plugin.VSyncEnabled.Value)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Target FPS:", GUILayout.Width(80));
                fpsInput = GUILayout.TextField(fpsInput, GUILayout.Width(60));

                if (GUILayout.Button("Apply FPS"))
                {
                    if (int.TryParse(fpsInput, out int parsedFPS))
                    {
                        Plugin.TargetFPS.Value = parsedFPS;
                        Application.targetFrameRate = parsedFPS;
                    }
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("<color=yellow>FPS Cap ignored while V-Sync is ON</color>");
            }
            GUILayout.Label("<b>Run Controls</b>");
            GUILayout.Space(10);
            BindField("Restart Run Bind:", Plugin.RestartBind);

            GUILayout.Space(10);
            GUILayout.Label("<b>Trainer</b>");
            Plugin.TrainerEnabled.Value = GUILayout.Toggle(Plugin.TrainerEnabled.Value, " Enable Trainer");

            GUILayout.Label("<b>Trainer Binds</b>");
            BindField("Add Cheese Bind:", Plugin.AddCheeseBind);
            BindField("Remove Cheese Bind:", Plugin.RemoveCheeseBind);
            BindField("Add Fruit Bind:", Plugin.AddFruitBind);
            BindField("Remove Fruit Bind:", Plugin.RemoveFruitBind);
            BindField("Lock Scene Bind:", Plugin.LockBind);
            BindField("Store Position Bind:", Plugin.StorePositionBind);
            BindField("Restore Position Bind:", Plugin.RestorePositionBind);
            BindField("Open Trainer Bind:", Plugin.OpenTrainerBind);
            BindField("Increment Scene Bind:", Plugin.IncrementSceneBind);
            BindField("Decrement Scene Bind:", Plugin.DecrementSceneBind);

            GUILayout.Space(15);
            bool isSavedRecently = Time.unscaledTime < saveFeedbackTime;

            if (isSavedRecently)
            {
                if (saveSuccess)
                {
                    GUI.color = new Color(0.2f, 1f, 0.6f);
                    if (GUILayout.Button("✓ CONFIG SAVED!"))
                    {
                        saveSuccess = Plugin.SaveConfig();
                        saveFeedbackTime = Time.unscaledTime + 2.5f;
                    }
                    GUI.color = Color.white;
                    GUILayout.Label("<color=#55FF55><b>✓ Saved to SpeedRave.cfg!</b></color>");
                }
                else
                {
                    GUI.color = Color.red;
                    if (GUILayout.Button("✗ SAVE FAILED!"))
                    {
                        saveSuccess = Plugin.SaveConfig();
                        saveFeedbackTime = Time.unscaledTime + 2.5f;
                    }
                    GUI.color = Color.white;
                    GUILayout.Label("<color=red><b>✗ Error writing config (check console)</b></color>");
                }
            }
            else
            {
                GUI.color = Color.green;
                if (GUILayout.Button("SAVE TO CONFIG"))
                {
                    saveSuccess = Plugin.SaveConfig();
                    saveFeedbackTime = Time.unscaledTime + 2.5f;
                }
                GUI.color = Color.white;
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        // A bind text field whose edits only take effect when Apply is pressed.
        private void BindField(string label, ConfigEntry<string> entry)
        {
            GUILayout.Label(label);
            GUILayout.BeginHorizontal();
            string current = entry.Value ?? "";
            if (!pendingBinds.TryGetValue(entry, out string text))
            {
                text = current;
            }
            text = GUILayout.TextField(text);
            if (text != current)
            {
                pendingBinds[entry] = text;
                if (GUILayout.Button("Apply", GUILayout.Width(60)))
                {
                    entry.Value = text.Trim();
                    pendingBinds.Remove(entry);
                    GUIUtility.keyboardControl = 0;
                }
            }
            else
            {
                pendingBinds.Remove(entry);
            }
            GUILayout.EndHorizontal();
        }

        private void SceneWinProc(int id)
        {
            sceneScroll = GUILayout.BeginScrollView(sceneScroll);
            GUILayout.Label("Select a Scene:");

            selectedScene = GUILayout.SelectionGrid(selectedScene, Scenes, 10);

            if (GUILayout.Button("Go To Scene"))
            {
                SceneManager.LoadScene(Scenes[selectedScene]);
            }

            GUILayout.EndScrollView();
            GUI.DragWindow();
        }

        private void WinProc(int id)
        {
            if (Plugin.AutosplitterEnabled.Value)
            {
                GUILayout.Label("<b>Autosplitter</b>");
                bool isConnected = Autosplitter.Instance != null && Autosplitter.Instance.IsConnectedToLivesplit;
                GUILayout.Label($"Connected: {isConnected}");
                if (!isConnected)
                {
                    if (GUILayout.Button("Connect to LiveSplit"))
                    {
                        if (Autosplitter.Instance != null)
                        {
                            Autosplitter.Instance.ConnectToLiveSplit();
                        }
                    }
                }
                GUILayout.Space(5);
            }

            // Seed Control
            if (Plugin.SeedEnabled.Value)
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
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Seed"))
                {
                    GUIUtility.systemCopyBuffer = Patches.SetSeedPatchs.Seed.ToString();
                    seedFeedbackMessage = "✓ Seed copied to clipboard!";
                    seedFeedbackTime = Time.unscaledTime + 2.0f;
                }
                if (GUILayout.Button("Paste Seed"))
                {
                    string clip = GUIUtility.systemCopyBuffer;
                    if (int.TryParse(clip, out int pastedSeed))
                    {
                        Patches.SetSeedPatchs.Seed = pastedSeed;
                        Patches.SetSeedPatchs.randomSeed = false;
                        seedInput = pastedSeed.ToString();
                        seedFeedbackMessage = "✓ Seed pasted from clipboard!";
                        seedFeedbackTime = Time.unscaledTime + 2.0f;
                    }
                    else
                    {
                        seedFeedbackMessage = "✗ Clipboard is not a valid number";
                        seedFeedbackTime = Time.unscaledTime + 2.0f;
                    }
                }
                GUILayout.EndHorizontal();

                if (Time.unscaledTime < seedFeedbackTime)
                {
                    GUILayout.Label($"<color=#55FF55><b>{seedFeedbackMessage}</b></color>");
                }

                Patches.SetSeedPatchs.randomSeed = GUILayout.Toggle(Patches.SetSeedPatchs.randomSeed, " Use Random Seed");
            }
            
            // Run Controls
            GUILayout.Label("<b>Run Controls</b>");
            GUILayout.BeginHorizontal();
            string restartBindDisplay = string.IsNullOrEmpty(Plugin.RestartBind.Value) ? "UNBOUND" : Plugin.RestartBind.Value.ToUpperInvariant();
            if (GUILayout.Button($"Instant Restart ({restartBindDisplay})"))
            {
                TriggerInstantRestart();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (Plugin.TrainerEnabled.Value)
            {
                // Room Selector
                GUILayout.Label("<b>Scene Selector</b>");
                GUILayout.Label($"Current Room: {SceneManager.GetActiveScene().name}");
                if (GUILayout.Button(sceneSelectorShowGUI ? "Close Selector" : "Open Room Selector"))
                {
                    sceneSelectorShowGUI = !sceneSelectorShowGUI;
                    if (sceneSelectorShowGUI)
                    {
                        // Place it under the main window when opened; after that it can be dragged freely.
                        sceneWinRect.x = winRect.x;
                        sceneWinRect.y = winRect.y + winRect.height + 10;
                    }
                }

                GUILayout.Space(5);

                // Room Locking
                GUILayout.Label("<b>Room Lock</b>");
                string lockStatus = locked ? "<color=red>LOCKED</color>" : "<color=green>UNLOCKED</color>";
                GUILayout.Label($"Status: {lockStatus}");
                if (GUILayout.Button(locked ? $"Unlock ({Plugin.LockBind.Value.ToUpperInvariant()})" : $"Lock ({Plugin.LockBind.Value.ToUpperInvariant()})"))
                {
                    ToggleSceneLock();
                }

                // Trainer
                GUILayout.Label("<b>Trainer</b>");

                // Cheese Row
                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"Add Cheese ({Plugin.AddCheeseBind.Value.ToUpperInvariant()})")) ModifyCheese(1);
                if (GUILayout.Button($"Sub Cheese ({Plugin.RemoveCheeseBind.Value.ToUpperInvariant()})")) ModifyCheese(-1);
                GUILayout.EndHorizontal();

                // Fruit Row
                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"Add Fruit ({Plugin.AddFruitBind.Value.ToUpperInvariant()})")) ModifyFruit(1);
                if (GUILayout.Button($"Sub Fruit ({Plugin.RemoveFruitBind.Value.ToUpperInvariant()})")) ModifyFruit(-1);
                GUILayout.EndHorizontal();

                // Position Row
                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"Store Pos ({Plugin.StorePositionBind.Value.ToUpperInvariant()})")) StorePlayerPosition();
                if (GUILayout.Button($"Restore Pos ({Plugin.RestorePositionBind.Value.ToUpperInvariant()})")) RestorePlayerPosition();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(configShowGUI ? "Close Config" : "Open Config UI"))
            {
                configShowGUI = !configShowGUI;
                if (configShowGUI)
                {
                    // Place it beside the main window when opened; after that it can be dragged freely.
                    configWinRect.x = winRect.x + winRect.width + 10;
                    configWinRect.y = winRect.y;
                }
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

                object mouseLookObj = ReferenceManager.MouseLookField?.GetValue(ReferenceManager.PlayerController);

                if (mouseLookObj != null && ReferenceManager.CharacterTargetRotField != null && ReferenceManager.CameraTargetRotField != null)
                {
                    object charRotObj = ReferenceManager.CharacterTargetRotField.GetValue(mouseLookObj);
                    object camRotObj = ReferenceManager.CameraTargetRotField.GetValue(mouseLookObj);

                    if (charRotObj is Quaternion charRot && camRotObj is Quaternion camRot)
                    {
                        storedCharacterRot = charRot;
                        storedCameraRot = camRot;
                        hasStoredPosition = true;
                    }
                }
            }
        }

        private void RestorePlayerPosition()
        {
            if (!hasStoredPosition) return;

            if (ReferenceManager.Player != null && ReferenceManager.PlayerController != null)
            {
                ReferenceManager.PlayerController.transform.position = storedPosition;
                object mouseLookObj = ReferenceManager.MouseLookField?.GetValue(ReferenceManager.PlayerController);
                if (mouseLookObj != null && ReferenceManager.CharacterTargetRotField != null && ReferenceManager.CameraTargetRotField != null)
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

        private int GetCurrentSceneIndex()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            for (int i = 0; i < Scenes.Length; i++)
            {
                if (string.Equals(Scenes[i], activeScene, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return 0;
        }

        // Clears the game's save the same way the title screen does. Mid-run there is no TitleScreenControler
        // in the scene, so a temporary inactive one is created just to call ClearSaveData(). This avoids
        // PlayerPrefs.DeleteAll(), which would also wipe the game's settings and other mods' data.
        private static void ClearGameSaveData()
        {
            var titleController = UnityEngine.Object.FindObjectOfType<TitleScreenControler>();
            if (titleController != null)
            {
                titleController.ClearSaveData();
                return;
            }

            // Inactive so that Awake/Start/Update never run on the temporary component.
            var temp = new GameObject("SpeedRaveTempTitleController");
            temp.SetActive(false);
            try
            {
                temp.AddComponent<TitleScreenControler>().ClearSaveData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SpeedRave] Could not clear save data on restart: {ex.Message}");
            }
            finally
            {
                UnityEngine.Object.Destroy(temp);
            }
        }

        public static void TriggerInstantRestart()
        {
            if (Autosplitter.Instance != null)
            {
                Autosplitter.Instance.ResetRun();
            }

            if (Plugin.ClearSaveOnStart.Value)
            {
                ClearGameSaveData();
            }

            if (Plugin.SeedEnabled.Value)
            {
                // Always re-seed: a new seed in random mode, and a rewind of the seeded RNG stream to the
                // same seed in set-seed mode (otherwise the stream continues from the previous attempt).
                Patches.SetSeedPatchs.Init();
                seedFlashText = $"Seed: {Patches.SetSeedPatchs.Seed}";
                seedFlashMode = Patches.SetSeedPatchs.randomSeed ? "Random Seed" : "Set Seed";
                seedFlashTimer = Time.unscaledTime + 1.0f;
            }

            OnScreenTimer.ResetTimer();

            var persist = UnityEngine.Object.FindObjectOfType<PersistControl>();
            if (persist != null)
            {
                UnityEngine.Object.Destroy(persist.gameObject);
            }
            var food = UnityEngine.Object.FindObjectOfType<FoodControl>();
            if (food != null)
            {
                UnityEngine.Object.Destroy(food.gameObject);
            }

            SceneManager.LoadScene("Sewer_Start");
        }
    }
}