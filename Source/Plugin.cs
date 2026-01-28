using BepInEx.Logging;
using BepInEx;
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

        public Plugin()
        {
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
        }

         void Awake()
         {
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
    }
}
