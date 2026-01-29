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
        public const string modVersion = "0.9.9";

        private GameObject _mod;

        private readonly Harmony harmony = new Harmony(modGUID);

        private static Plugin Instance;

        internal ManualLogSource mls;

        // --- Config Entries ---
        public static ConfigEntry<bool> QuitToMenu;
        public static ConfigEntry<bool> RemoveMusic;
        public static ConfigEntry<bool> QuickStart;
        public static ConfigEntry<bool> SeedEnabled;

        public static ConfigEntry<bool> TrainerEnabled;

        public static ConfigEntry<bool> AutosplitterEnabled;
        public static ConfigEntry<bool> TwentyResourceSplit;
        public static ConfigEntry<bool> TwentyFruitSplit;
        public static ConfigEntry<bool> KeySplit;
        public static ConfigEntry<bool> ItemSplit;

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

        public static ConfigEntry<bool> InventoryOverlayEnabled;
        public static ConfigEntry<bool> UseIcons;
        public static ConfigEntry<bool> VerticalIcons;
        public static ConfigEntry<float> IconSize;
        public static ConfigEntry<float> TextHeight;
        public static ConfigEntry<float> Padding;


        public Plugin()
        {
            /*
            QuitToMenuPatch.Use = Config.Bind("Patches", "Quit To Menu", true).Value;

            RemoveMusicPatch.Use = Config.Bind("Patches", "Remove Music", false).Value;

            QuickStartPatch.Use = Config.Bind("Patches", "QuickStart", true).Value;

            SetSeedPatchs.Use = Config.Bind("Seeding", "Set Seed", true).Value;

            GUIComponent.Use = Config.Bind("Trainer", "Enable Trainer", true).Value;

            GUIComponent.addCheeseBind = Config.Bind("Binds", "Add Cheese Bind", "U").Value;
            GUIComponent.removeCheeseBind = Config.Bind("Binds", "Remove Cheese Bind", "I").Value;
            GUIComponent.addFruitBind = Config.Bind("Binds", "Add Fruit Bind", "O").Value;
            GUIComponent.removeFruitBind = Config.Bind("Binds", "Remove Fruit Bind", "P").Value;
            GUIComponent.lockBind = Config.Bind("Binds", "Scene Lock Bind", "L").Value;
            GUIComponent.storePositionBind = Config.Bind("Binds", "Store Position Bind", "Z").Value;
            GUIComponent.restorePositionBind = Config.Bind("Binds", "Restore Position Bind", "X").Value;
            GUIComponent.openTrainerBind = Config.Bind("Binds", "Open Trainer Bind", "INSERT").Value;


            Autosplitter.Use = Config.Bind("AutoSplitter", "Autosplitter Enabled", true).Value;
            Autosplitter.twentyResourceSplit = Config.Bind("AutoSplitter", "Twenty Resource Split", false).Value;
            Autosplitter.twentyFruitSplit = Config.Bind("AutoSplitter", "Twenty Fruit Split", false).Value;
            Autosplitter.keySplit = Config.Bind("AutoSplitter", "Key Split", false).Value;
            Autosplitter.itemSplit = Config.Bind("AutoSplitter", "Item Split", false).Value;

            InventoryOverlay.showInventory = Config.Bind("Inventory Overlay", "Enable InventoryOverlay", false).Value;
            InventoryOverlay.useIcons = Config.Bind("Inventory Overlay", "Use Icons", true).Value;
            InventoryOverlay.verticalIcons = Config.Bind("Inventory Overlay", "Vertical Icons", true).Value;
            InventoryOverlay.textHeight = Config.Bind("Inventory Overlay", "Text Height", 45f).Value;
            InventoryOverlay.iconSize = Config.Bind("Inventory Overlay", "Icon Size", 50f).Value;
            InventoryOverlay.padding = Config.Bind("Inventory Overlay", "Icon Padding", 10f).Value;
            */
        }

         void Awake()
         {
            // --- Binding Values ---
            QuitToMenu = Config.Bind("Patches", "Quit To Menu", true);
            RemoveMusic = Config.Bind("Patches", "Remove Music", false);
            QuickStart = Config.Bind("Patches", "QuickStart", true);

            SeedEnabled = Config.Bind("Seeding", "Set Seed", true);

            TrainerEnabled = Config.Bind("Trainer", "Enable Trainer", true);

            AutosplitterEnabled = Config.Bind("AutoSplitter", "Autosplitter Enabled", true);
            TwentyResourceSplit = Config.Bind("AutoSplitter", "Twenty Resource Split", false);
            TwentyFruitSplit = Config.Bind("AutoSplitter", "Twenty Fruit Split", false);
            KeySplit = Config.Bind("AutoSplitter", "Key Split", false);
            ItemSplit = Config.Bind("AutoSplitter", "Item Split", false);

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

            InventoryOverlayEnabled = Config.Bind("Inventory Overlay", "Enable InventoryOverlay", false);
            UseIcons = Config.Bind("Inventory Overlay", "Use Icons", true);
            VerticalIcons = Config.Bind("Inventory Overlay", "Vertical Icons", true);
            IconSize = Config.Bind("Inventory Overlay", "Icon Size", 60f);
            TextHeight = Config.Bind("Inventory Overlay", "Text Height", 50f);
            Padding = Config.Bind("Inventory Overlay", "Icon Padding", 10f);

            _mod = new GameObject("SpeedRaveGUI");
            _mod.AddComponent<GUIComponent>();
            _mod.AddComponent<Autosplitter>();
            _mod.AddComponent<InventoryOverlay>();
            GameObject.DontDestroyOnLoad(_mod);
            ReferenceManager.Initialize();
            if (Instance == null)
            {
                Instance = this;
            }
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            
            harmony.PatchAll(typeof(QuitToMenuPatch));
            harmony.PatchAll(typeof(RemoveMusicPatch));
            harmony.PatchAll(typeof(TitlePatch));
            harmony.PatchAll(typeof(QuickStartPatch));
            harmony.PatchAll(typeof(AutoSplitterPatchs));
            harmony.PatchAll(typeof(SetSeedPatchs));
            harmony.PatchAll(typeof(SceneLock));
            harmony.PatchAll(typeof(CursorLockFix));
        }
        public static void SaveConfig() => Instance.Config.Save();
    }
}
