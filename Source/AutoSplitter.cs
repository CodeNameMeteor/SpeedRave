using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace SpeedRave
{
    public class Autosplitter : MonoBehaviour
    {
        //private static readonly string ConfigPath = Paths.ConfigPath + "\\" + "SpeedrunUtils\\";
        //private readonly string SplitsPath = Path.Combine(ConfigPath, "splits.txt");
        //private readonly string SettingsPath = Path.Combine(ConfigPath, "Settings.txt");

        public bool debug = false;
        public bool gameStarted = false;
        public int itemCount;
        public static bool isLoading = false;
        public bool gotResources = false;
        public bool gotSnake = false;
        public bool gotFruit = false;
        public bool gotPossum = false;
        public FoodControl[] FoodControlArray;

        public bool IsConnectedToLivesplit = false;

        public bool[] SplitArray;

        private string IpAddress = "localhost";
        private int Port = 16834;

        private bool timerPaused = false;

        private TcpClient Client = null;
        private NetworkStream Stream = null;

        public static bool Use;

        private const int BUFFER_SIZE = 1024;




        public void ConnectToLiveSplit()
        {
            try
            {
                if (!IsConnectedToLivesplit && Use)
                {
                    Byte[] data = new Byte[256];
                    String responseData = String.Empty;

                    Client = new TcpClient(IpAddress, Port);
                    Stream = Client.GetStream();

                    Stream.Write(Encoding.UTF8.GetBytes("getcurrenttimerphase\r\n"), 0, Encoding.UTF8.GetBytes("getcurrenttimerphase\r\n").Length);

                    int bytes = Stream.Read(data, 0, data.Length);
                    responseData = Encoding.ASCII.GetString(data, 0, bytes);

                    if (responseData != String.Empty)
                    {
                        Stream.Write(Encoding.UTF8.GetBytes("initgametime\r\n"), 0, Encoding.UTF8.GetBytes("initgametime\r\n").Length);
                        IsConnectedToLivesplit = true;
                    }
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError($"Error connecting to LiveSplit: {ex.Message}");
                IsConnectedToLivesplit = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"An unexpected error occurred: {ex.Message}");
                IsConnectedToLivesplit = false;
            }
        }





        public void UpdateAutosplitter()
        {

            if (IsConnectedToLivesplit || debug)
            {

                //Debug.Log(SceneManager.GetActiveScene().name);
                //Debug.Log(isLoading);
                //Debug.Log(timerPaused);
                //if (SceneManager.GetActiveScene().name == "Sewer_Start" && !gameStarted && !isLoading)
                if (SceneManager.GetActiveScene().name == "TitleScreen" && gameStarted)
                {
                    AttemptSendCommand("reset");
                    gameStarted = false;
                }
                if (SceneManager.GetActiveScene().name == "Sewer_Start" && !gameStarted)
                {
                    AttemptSendCommand("unpausegametime");
                    AttemptSendCommand("reset");
                    AttemptSendCommand("starttimer");
                    //FoodControlArray = FindObjectsOfType<FoodControl>();
                    gameStarted = true;
                }
                if (gameStarted )
                {
                    /*
                    if((FoodControlArray[0].cheese + FoodControlArray[0].fruit >= 20) && !gotResources)
                    {
                        AttemptSendCommand("split");
                        gotResources = true;
                    }
                    
                    else if(FoodControlArray[0].fruit >= 20 && gotResources)
                    {
                        AttemptSendCommand("split");
                        gotFruit = true;
                    }
                    /*
                    else if (gotResources && SceneManager.GetActiveScene().name == "JasmineRoom1" && !gotSnake)
                    {
                        AttemptSendCommand("split");
                        gotSnake = true;
                    }
                    
                    else if (gotFruit && SceneManager.GetActiveScene().name == "PossumQueenRoom1" && !gotPossum )
                    {
                        AttemptSendCommand("split");
                        gotPossum = true;
                    }
                    */
                    if (SceneManager.GetActiveScene().name.Contains("ending") )
                    {
                        AttemptSendCommand("split");
                        gameStarted = false;
                    }

                }
                if (isLoading && !timerPaused)
                {
                    AttemptSendCommand("pausegametime");
                    timerPaused = true;
                }
                else if(timerPaused && (!isLoading || SceneManager.GetActiveScene().name == "TitleScreen"))
                {
                    AttemptSendCommand("unpausegametime");
                    timerPaused = false;
                }
            }
        }




        public void AttemptSendCommand(String command)
        {
            try
            {
                Stream.Write(Encoding.UTF8.GetBytes(command + "\r\n"), 0, Encoding.UTF8.GetBytes(command + "\r\n").Length);
            }
            catch (SocketException ex)
            {
                Debug.LogError($"Error connecting to LiveSplit: {ex.Message}");
                IsConnectedToLivesplit = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"An unexpected error occurred: {ex.Message}");
                IsConnectedToLivesplit = false;
            }
        }

        public void Update()
        {
            if (IsConnectedToLivesplit || debug)
            {
                UpdateAutosplitter();
            }
        }

        public void OnApplicationQuit()
        {
            if (IsConnectedToLivesplit)
            {
                try
                {
                    Stream.Write(Encoding.UTF8.GetBytes("pausegametime\r\n"), 0, Encoding.UTF8.GetBytes("pausegametime\r\n").Length);
                    timerPaused = true;
                }
                catch (SocketException ex)
                {
                    Debug.LogError($"Error connecting to LiveSplit: {ex.Message}");
                    IsConnectedToLivesplit = false;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"An unexpected error occurred: {ex.Message}");
                    IsConnectedToLivesplit = false;
                }
            }
        }
    }
}