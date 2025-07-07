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
        public const string modVersion = "0.8.0";

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
        }
    }
}
