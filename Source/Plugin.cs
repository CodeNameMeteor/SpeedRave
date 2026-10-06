using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SpeedRave.Patches;
using UnityEngine;

namespace SpeedRave
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "SpeedRave";
        public const string modName = "SpeedRave";
        public const string modVersion = "1.2.0";

        private GameObject _mod;

        private readonly Harmony harmony = new Harmony(modGUID);

        private static Plugin Instance;

        internal ManualLogSource mls;

        // --- Config Entries ---
        public static ConfigEntry<bool> QuitToMenu;
        public static ConfigEntry<bool> ClearSaveOnStart;
        public static ConfigEntry<bool> RemoveMusic;
        public static ConfigEntry<bool> QuickStart;
        public static ConfigEntry<bool> SeedEnabled;
        public static ConfigEntry<float> SeedFlashDuration;

        public static ConfigEntry<bool> TrainerEnabled;

        public static ConfigEntry<bool> AutosplitterEnabled;
        public static ConfigEntry<bool> TwentyResourceSplit;
        public static ConfigEntry<bool> TwentyFruitSplit;
        public static ConfigEntry<bool> KeySplit;
        public static ConfigEntry<bool> ItemSplit;
        public static ConfigEntry<bool> AllEndings;

        public static ConfigEntry<string> AddCheeseBind;
        public static ConfigEntry<string> RemoveCheeseBind;
        public static ConfigEntry<string> AddFruitBind;
        public static ConfigEntry<string> RemoveFruitBind;
        public static ConfigEntry<string> LockBind;
        public static ConfigEntry<string> StorePositionBind;
        public static ConfigEntry<string> RestorePositionBind;
        public static ConfigEntry<string> OpenTrainerBind;
        public static ConfigEntry<string> IncrementSceneBind;
        public static ConfigEntry<string> DecrementSceneBind;
        public static ConfigEntry<string> RestartBind;
        public static ConfigEntry<string> OpenTrainerAltBind;
        public static ConfigEntry<string> RestartAltBind;

        public static ConfigEntry<bool> LiveSplitAutoReconnect;

        public static ConfigEntry<bool> ShowOnScreenTimer;
        public static ConfigEntry<float> TimerFontSize;
        public static ConfigEntry<bool> ShowTimerStateText;

        public static ConfigEntry<bool> InventoryOverlayEnabled;
        public static ConfigEntry<bool> UseIcons;
        public static ConfigEntry<bool> VerticalIcons;
        public static ConfigEntry<float> IconSize;
        public static ConfigEntry<float> TextHeight;
        public static ConfigEntry<float> Padding;

        public static ConfigEntry<bool> VSyncEnabled;

        public static ConfigEntry<float> UIScale;
        public static ConfigEntry<bool> UseGameFont;

        // In your Plugin class
        public static ConfigEntry<int> TargetFPS;

        public static ConfigEntry<bool> Debug;





         void Awake()
         {
            if (Instance == null)
            {
                Instance = this;
            }
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            // --- Binding Values ---
            QuitToMenu = Config.Bind("Patches", "Quit To Menu", true);
            ClearSaveOnStart = Config.Bind("Patches", "Clear Save On Start", true, "Clear the game's save (via the game's own ClearSaveData) when starting a new game or using Instant Restart, so every run starts fresh. Game settings are not affected.");
            RemoveMusic = Config.Bind("Patches", "Remove Music", false, "Mute all looping audio while enabled. This is mostly music, but looping ambience is muted too. Turning it off restores the original volumes.");
            QuickStart = Config.Bind("Patches", "QuickStart", true);

            SeedEnabled = Config.Bind("Seeding", "Set Seed", true);
            SeedFlashDuration = Config.Bind("Seeding", "Seed Flash Duration", 1f, new ConfigDescription("Seconds the seed is shown on screen after Instant Restart. Raise it if you need longer to read or record the seed.", new AcceptableValueRange<float>(0.5f, 10f)));

            TrainerEnabled = Config.Bind("Trainer", "Enable Trainer", true);

            AutosplitterEnabled = Config.Bind("AutoSplitter", "Autosplitter Enabled", true);
            TwentyResourceSplit = Config.Bind("AutoSplitter", "Twenty Resource Split", false);
            TwentyFruitSplit = Config.Bind("AutoSplitter", "Twenty Fruit Split", false);
            KeySplit = Config.Bind("AutoSplitter", "Key Split", false);
            ItemSplit = Config.Bind("AutoSplitter", "Item Split", false);
            AllEndings = Config.Bind("AutoSplitter", "All Endings", false, "Keep the run going after an ending. The run (and the timers) only finish once all three endings have been reached.");

            AddCheeseBind = Config.Bind("Binds", "Add Cheese Bind", "U");
            RemoveCheeseBind = Config.Bind("Binds", "Remove Cheese Bind", "I");
            AddFruitBind = Config.Bind("Binds", "Add Fruit Bind", "O");
            RemoveFruitBind = Config.Bind("Binds", "Remove Fruit Bind", "P");
            LockBind = Config.Bind("Binds", "Scene Lock Bind", "L");
            StorePositionBind = Config.Bind("Binds", "Store Position Bind", "Z");
            RestorePositionBind = Config.Bind("Binds", "Restore Position Bind", "X");
            OpenTrainerBind = Config.Bind("Binds", "Open Trainer Bind", "INSERT");
            IncrementSceneBind = Config.Bind("Binds", "Increment Scene Bind", "J");
            DecrementSceneBind = Config.Bind("Binds", "Decrement Scene Bind", "K");
            RestartBind = Config.Bind("Binds", "Restart Run Bind", "F6", "Instant restart run hotkey");
            OpenTrainerAltBind = Config.Bind("Binds", "Open Trainer Alt Bind", "", "Optional second key or controller button for opening the trainer, e.g. \"joystick button 6\". Empty = unbound.");
            RestartAltBind = Config.Bind("Binds", "Restart Run Alt Bind", "", "Optional second key or controller button for Instant Restart, e.g. \"joystick button 7\". Empty = unbound.");

            LiveSplitAutoReconnect = Config.Bind("AutoSplitter", "Auto Reconnect LiveSplit", false, "Periodically retry connecting to LiveSplit in the background");

            ShowOnScreenTimer = Config.Bind("Timer", "Show On Screen Timer", false, "Display loadless in-game speedrun timer");
            TimerFontSize = Config.Bind("Timer", "Timer Font Size", 40f, "Font size of the on-screen timer");
            ShowTimerStateText = Config.Bind("Timer", "Show Timer State Text", false, "Show LOADING or FINISHED under the timer as well as changing its colour (yellow while loading, green when finished).");

            InventoryOverlayEnabled = Config.Bind("Inventory Overlay", "Enable InventoryOverlay", false);
            UseIcons = Config.Bind("Inventory Overlay", "Use Icons", true);
            VerticalIcons = Config.Bind("Inventory Overlay", "Vertical Icons", true);
            IconSize = Config.Bind("Inventory Overlay", "Icon Size", 60f);
            TextHeight = Config.Bind("Inventory Overlay", "Text Height", 50f);
            Padding = Config.Bind("Inventory Overlay", "Icon Padding", 10f);

            UseGameFont = Config.Bind("Accessibility", "Use Game Font", true, "Draw the timer, inventory overlay and seed flash in the game's decorative font. Turn off for Unity's plain default font, which can be easier to read.");
            UIScale = Config.Bind("Accessibility", "UI Scale", 1f, new ConfigDescription("Size of the trainer windows and the seed flash (1 = normal, 2 = double).", new AcceptableValueRange<float>(0.5f, 3f)));

            TargetFPS = Config.Bind("Performance", "TargetFPS", -1, "Target framerate (-1 for uncapped)");
            VSyncEnabled = Config.Bind("Performance", "VSyncEnabled", true, "Enable or disable V-Sync");
            Debug = Config.Bind("Debug", "Debug", false, "Enable Debug");

            // By default BepInEx rewrites the whole config file on every change, which happens every frame while
            // a Config UI slider is dragged. Save explicitly instead (Save button, closing the trainer, quitting).
            Config.SaveOnConfigSet = false;
            Config.SettingChanged += (sender, args) => configDirty = true;

            QualitySettings.vSyncCount = VSyncEnabled.Value ? 1 : 0;
            Application.targetFrameRate = TargetFPS.Value;

            _mod = new GameObject("SpeedRaveGUI");
            _mod.AddComponent<GUIComponent>();
            _mod.AddComponent<Autosplitter>();
            _mod.AddComponent<InventoryOverlay>();
            _mod.AddComponent<OnScreenTimer>();
            _mod.AddComponent<MusicMuter>();
            GameObject.DontDestroyOnLoad(_mod);
            ReferenceManager.Initialize();

            
            harmony.PatchAll(typeof(QuitToMenuPatch));
            harmony.PatchAll(typeof(RemoveMusicPatch));
            harmony.PatchAll(typeof(TitlePatch));
            harmony.PatchAll(typeof(QuickStartPatch));
            harmony.PatchAll(typeof(AutoSplitterPatchs));
            harmony.PatchAll(typeof(SetSeedPatchs));
            harmony.PatchAll(typeof(SceneLock));
            harmony.PatchAll(typeof(CursorLockFix));
            harmony.PatchAll(typeof(BlockMovementWhileTyping));
            harmony.PatchAll(typeof(BlockJumpWhileTyping));
        }
        private static bool configDirty = false;

        // Saves only if a setting changed since the last save.
        public static void SaveConfigIfChanged()
        {
            if (configDirty)
            {
                SaveConfig();
            }
        }

        private void OnApplicationQuit()
        {
            SaveConfigIfChanged();
        }

        public static bool SaveConfig()
        {
            try
            {
                Instance?.Config?.Save();
                configDirty = false;
                if (Plugin.Debug.Value)
                {
                    UnityEngine.Debug.Log("[SpeedRave] Configuration saved to disk.");
                }
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[SpeedRave] Error saving config: {ex.Message}");
                return false;
            }
        }
    }
}
