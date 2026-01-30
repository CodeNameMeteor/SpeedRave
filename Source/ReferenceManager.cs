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
        public static FoodControl ActiveFoodControl { get; private set; }

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
            RefreshReferences();

            if (scene.name.ToLower() != "titlescreen" && scene.name.ToLower() != "credits " && ActiveFoodControl == null)
            {
                ActiveFoodControl = GameObject.FindObjectOfType<FoodControl>();
                if (ActiveFoodControl == null)
                {
                    Debug.Log("[SpeedRave] FoodControl missing! Sideloading Sewer_Start...");

                    // Load Sewer_Start additively so we don't leave the current room
                    SceneManager.LoadScene("Sewer_Start", LoadSceneMode.Additive);

                    return;
                }

 
            }
            if (scene.name == "Sewer_Start" && SceneManager.sceneCount > 1)
            {
                CleanUpSideloadedScene(scene);
            }

        }

        private static void RefreshReferences()
        {
            Player = GameObject.FindGameObjectWithTag("Player");

            if (Player != null)
            {
                PlayerController = Player.GetComponent<FirstPersonController>();
                // Safely get camera via reflection or component
                MainCamera = Player.GetComponentInChildren<Camera>();
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


            Debug.Log("[SpeedRave] References Refreshed");
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
            Debug.Log("[SpeedRave] Sewer_Start logic side-loaded and visuals suppressed.");
        }
    }
}