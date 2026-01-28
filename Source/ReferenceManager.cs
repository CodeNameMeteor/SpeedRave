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
            if(scene.name.ToLower() == "sewer_start")
            {
                var foodControls = UnityEngine.Object.FindObjectsOfType<FoodControl>();

                //The way the food_control is programmed is that it is initialised on the start of sewer_start
                //in a typical game you can't go back to sewer_start but as we can just have this here as a precaution
                if (foodControls.Length > 1)
                {
                    UnityEngine.Object.Destroy(UnityEngine.Object.FindObjectsOfType<FoodControl>()[1]);
                }
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
                /*
                var foodControls = UnityEngine.Object.FindObjectsOfType<FoodControl>();

                //The way the player is programmed is that it is initialised on the start of sewer_start
                //in a typical game you can't go back to sewer_start but as we can just have this here as a precaution
                if (foodControls.Length > 1)
                {
                    for (int i = foodControls.Length - 1; i > 0; i--)
                    {
                        UnityEngine.Object.Destroy(foodControls[i]);
                    }
                }
                */
            }
            else
            {
                ActiveFoodControl = null;
            }


            Debug.Log("[SpeedRave] References Refreshed");
        }
    }
}