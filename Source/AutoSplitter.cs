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

        public bool IsConnectedToLivesplit = false;

        public bool[] SplitArray;

        private string IpAddress = "localhost";
        private int Port = 16834;

        private bool timerPaused = false;

        private TcpClient Client = null;
        private NetworkStream Stream = null;


        private const int BUFFER_SIZE = 1024;




        public void Awake()
        {
        }

        public void Start()
        {
        }



        public void ConnectToLiveSplit()
        {
            try
            {
                if (!IsConnectedToLivesplit)
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


        private void UpdateFields()
        {
        }

        private void CacheExitSequenceMethod()
        {
        }

        public void ExitSequence()
        {
        }



        public void UpdateAutosplitter()
        {

            if (IsConnectedToLivesplit || debug)
            {
                UpdateFields();

                //Debug.Log(SceneManager.GetActiveScene().name);
                //Debug.Log(isLoading);
                Debug.Log(timerPaused);
                //if (SceneManager.GetActiveScene().name == "Sewer_Start" && !gameStarted && !isLoading)
                if (SceneManager.GetActiveScene().name == "TitleScreen" && gameStarted)
                {
                    try
                    {
                        Stream.Write(Encoding.UTF8.GetBytes("reset\r\n"), 0, Encoding.UTF8.GetBytes("reset\r\n").Length);
                        gameStarted = false;
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
                if (SceneManager.GetActiveScene().name == "Sewer_Start" && !gameStarted)
                {
                    try
                    {
                        Stream.Write(Encoding.UTF8.GetBytes("unpausegametime\r\n"), 0, Encoding.UTF8.GetBytes("unpausegametime\r\n").Length);
                        Stream.Write(Encoding.UTF8.GetBytes("reset\r\n"), 0, Encoding.UTF8.GetBytes("reset\r\n").Length);
                        Stream.Write(Encoding.UTF8.GetBytes("starttimer\r\n"), 0, Encoding.UTF8.GetBytes("starttimer\r\n").Length);
                        gameStarted = true;
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
                if (SceneManager.GetActiveScene().name.Contains("ending") && gameStarted)
                {
                    try
                    {
                        Stream.Write(Encoding.UTF8.GetBytes("split\r\n"), 0, Encoding.UTF8.GetBytes("split\r\n").Length);
                        gameStarted = false;
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
                if(isLoading && !timerPaused)
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
                if(timerPaused && (!isLoading || SceneManager.GetActiveScene().name == "TitleScreen"))
                {
                    try
                    {
                        Stream.Write(Encoding.UTF8.GetBytes("unpausegametime\r\n"), 0, Encoding.UTF8.GetBytes("unpausegametime\r\n").Length);
                        timerPaused = false;
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