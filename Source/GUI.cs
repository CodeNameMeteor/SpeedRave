﻿using BepInEx;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedRave
{
    public class GUIComponent : MonoBehaviour
    {
        private bool showGUI = false;

        //private string DesiredScene = "";

        private Scene currentScene;

        private int sceneIndex = 1;

        private Rect winRect = new(20, 20, 275, 220);

        public static bool Use;

        private void Update()
        {
            if (Use)
            {
                if (Input.GetKeyDown(KeyCode.Insert))
                {
                    showGUI = !showGUI;
                }
                if(showGUI)
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
                }        
                
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

            GUILayout.Label("Current Scene is:");
            GUILayout.Label(currentScene.name);
            GUILayout.Label("Press J To Go To The Next Scene");
            GUILayout.Label("Press K To Go To The Previous Scene");
            GUILayout.Label("Press U To Add Cheese");
            GUILayout.Label("Press I To Subtract Cheese");
            GUILayout.Label("Press O To Add Fruit");
            GUILayout.Label("Press P To Subtract Fruit");
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