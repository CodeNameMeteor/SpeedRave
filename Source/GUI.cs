using BepInEx;
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

        private Rect winRect = new(20, 20, 275, 130);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                showGUI = !showGUI;
            }
            if (Input.GetKeyDown(KeyCode.J) && sceneIndex <= 68)
            {
                SceneManager.LoadScene(sceneIndex);
                sceneIndex++;
            }
            if (Input.GetKeyDown(KeyCode.K) && sceneIndex >= 0)
            {
                SceneManager.LoadScene(sceneIndex);
                sceneIndex--;
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