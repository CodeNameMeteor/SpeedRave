using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

namespace SpeedRave
{
    public static class ReferenceManager
    {
        // Public Accessors
        public static GameObject Player { get; private set; }
        public static FirstPersonController PlayerController { get; private set; }
        public static FoodControl ActiveFoodControl
        {
            get
            {
                RetryFoodControlLookup();
                return activeFoodControl;
            }
            private set => activeFoodControl = value;
        }
        private static FoodControl activeFoodControl;
        private static float nextFoodControlLookup = 0f;
        private static bool sideloadPending = false;

        public static GameObject ActiveInventory { get; set;  }
        public static Camera MainCamera { get; private set; }

        // Reflection Fields (Cache these once globally)
        public static FieldInfo MouseLookField { get; private set; }
        public static FieldInfo CharacterTargetRotField { get; private set; }
        public static FieldInfo CameraTargetRotField { get; private set; }
        public static FieldInfo CameraField { get; private set; }

        // Initialization
        public static void Initialize()
        {
            // Cache Reflection fields once on startup
            MouseLookField = AccessTools.Field(typeof(FirstPersonController), "m_MouseLook");
            CharacterTargetRotField = AccessTools.Field(typeof(MouseLook), "m_CharacterTargetRot");
            CameraTargetRotField = AccessTools.Field(typeof(MouseLook), "m_CameraTargetRot");
            CameraField = AccessTools.Field(typeof(FirstPersonController), "m_Camera");

            // Subscribe to scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Initial fetch
            RefreshReferences();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Our own sideloaded Sewer_Start: hide its visuals first (before looking up the player, so its
            // player rig is already inactive) and never trigger another sideload from it.
            if (mode == LoadSceneMode.Additive && scene.name == "Sewer_Start")
            {
                sideloadPending = false;
                CleanUpSideloadedScene(scene);
                RefreshReferences();
                return;
            }

            // A new room replaces any earlier sideload (and recovers if one never completed).
            if (mode == LoadSceneMode.Single)
            {
                sideloadPending = false;
            }

            RefreshReferences();

            string sceneLower = scene.name.ToLowerInvariant();
            if (sceneLower != "titlescreen" && sceneLower != "credits" && ActiveFoodControl == null)
            {
                ActiveFoodControl = GameObject.FindObjectOfType<FoodControl>();
                if (ActiveFoodControl == null && !sideloadPending)
                {
                    if (Plugin.Debug.Value)
                    {
                        Debug.Log("[SpeedRave] FoodControl missing! Sideloading Sewer_Start...");
                    }
                    // Load Sewer_Start additively so we don't leave the current room. Only one at a time.
                    sideloadPending = true;
                    SceneManager.LoadScene("Sewer_Start", LoadSceneMode.Additive);
                }
            }
        }

        // FoodControl may only appear a frame or more after a sideload (PersistControl sets it up on Start),
        // so look it up again lazily, at most once per second.
        private static void RetryFoodControlLookup()
        {
            if (activeFoodControl != null || Time.unscaledTime < nextFoodControlLookup) return;
            nextFoodControlLookup = Time.unscaledTime + 1f;

            string sceneLower = SceneManager.GetActiveScene().name.ToLowerInvariant();
            if (sceneLower == "titlescreen" || sceneLower == "credits") return;
            activeFoodControl = GameObject.FindObjectOfType<FoodControl>();
        }

        // Prefer the player in the active scene, then any player not in a sideloaded Sewer_Start.
        private static GameObject FindPlayer()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            if (players.Length == 0) return null;

            Scene active = SceneManager.GetActiveScene();
            foreach (GameObject candidate in players)
            {
                if (candidate.scene == active) return candidate;
            }
            foreach (GameObject candidate in players)
            {
                if (candidate.scene.name != "Sewer_Start") return candidate;
            }
            return players[0];
        }

        private static void RefreshReferences()
        {
            Player = FindPlayer();

            if (Player != null)
            {
                PlayerController = Player.GetComponent<FirstPersonController>();
                // Safely get camera via reflection or component
                MainCamera = Player.GetComponentInChildren<Camera>();
            }
            else
            {
                PlayerController = null;
                MainCamera = null;
            }

            ActiveInventory = GameObject.FindGameObjectWithTag("Inventory");

            if(ActiveInventory != null)
            {
                ActiveFoodControl = ActiveInventory.GetComponent<FoodControl>();
            }
            else
            {
                ActiveFoodControl = null;
            }

            if (Plugin.Debug.Value)
            {
                Debug.Log("[SpeedRave] References Refreshed");
            }
        }
        private static void CleanUpSideloadedScene(Scene scene)
        {
            foreach (GameObject obj in scene.GetRootGameObjects())
            {
                // don't disable what we need
                if (obj.GetComponent<PersistControl>() || obj.GetComponent<FoodControl>())
                    continue;

                // disable cameras, lights, and meshes so they don't interfere with the current room
                Camera cam = obj.GetComponentInChildren<Camera>();
                if (cam != null) cam.enabled = false;

                AudioListener listener = obj.GetComponentInChildren<AudioListener>();
                if (listener != null) listener.enabled = false;

                // hide walls
                obj.SetActive(false);
            }
            if (Plugin.Debug.Value)
            {
                Debug.Log("[SpeedRave] Sewer_Start logic side-loaded and visuals suppressed.");
            }
        }
    }
}