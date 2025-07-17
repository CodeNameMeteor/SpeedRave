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
        public const string modVersion = "0.9.0";

        private GameObject _mod;

        private readonly Harmony harmony = new Harmony(modGUID);

        private static Plugin Instance;

        internal ManualLogSource mls;

        public Plugin()
        {
            QuitToMenuPatch.Use = Config.Bind("Patches", "Quit To Menu", true).Value;
            RemoveMusicPatch.Use = Config.Bind("Patches", "Remove Music", false).Value;
            QuickStartPatch.Use = Config.Bind("Patches", "QuickStart", true).Value;
            GUIComponent.Use = Config.Bind("Trainer", "Room Picker", true).Value;
            SetSeedPatchs.Use = Config.Bind("Seeding", "Set Seed", true).Value;
            Autosplitter.Use = Config.Bind("Autosplitter", "Autosplitter", true).Value;
            Autosplitter.twentyResourceSplit = Config.Bind("AutoSplitter", "Twenty Resource Split", false).Value;
            Autosplitter.twentyFruitSplit = Config.Bind("AutoSplitter", "Twenty Fruit Split", false).Value;
            Autosplitter.keySplit = Config.Bind("AutoSplitter", "Key Split", false).Value;
            Autosplitter.itemSplit = Config.Bind("AutoSplitter", "Item Split", false).Value;
        }

         void Awake()
         {
            _mod = new GameObject("SpeedRaveGUI");
            _mod.AddComponent<GUIComponent>();
            _mod.AddComponent<Autosplitter>();
            GameObject.DontDestroyOnLoad(_mod);
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
