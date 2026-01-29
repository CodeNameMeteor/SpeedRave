using BepInEx;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks; // Required for async
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedRave
{
    public class Autosplitter : MonoBehaviour
    {
        public bool debug = false;
        public bool gameStarted = false;
        public static bool isLoading = false;

        public bool gotResources = false;
        public bool gotFruit = false;
        public bool gotPizza = false;
        public bool gotMug = false;
        public bool gotPyramid = false;
        public bool gotBottlecap = false;
        public bool gotDuck = false;
        public bool gotKey = false;
        //public static bool Use = true; // Assumed true for logic

        // Config flags
        public static bool twentyResourceSplit;
        public static bool keySplit;
        public static bool twentyFruitSplit;
        public static bool itemSplit;


        // Networking stuff
        public bool IsConnectedToLivesplit { get; private set; } = false;
        private string IpAddress = "127.0.0.1"; 
        private int Port = 16834;
        private TcpClient Client = null;
        private NetworkStream Stream = null;
        private bool _isConnecting = false;

        private bool timerPaused = false;

        // Singleton Instance
        public static Autosplitter Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
        }
        public void Start()
        {
            // Try to connect on startup, but do it silently in the background
            if (Plugin.AutosplitterEnabled.Value)
            {
                ConnectToLiveSplit();
            }
        }

        public async void ConnectToLiveSplit()
        {
            if (_isConnecting || IsConnectedToLivesplit) return;

            _isConnecting = true;
            try
            {
                // Run the blocking connection in a background thread
                Client = new TcpClient();
                await Client.ConnectAsync(IpAddress, Port);

                if (Client.Connected)
                {
                    Stream = Client.GetStream();

                    SendMessageSafe("getcurrenttimerphase");
                    SendMessageSafe("initgametime");

                    IsConnectedToLivesplit = true;
                    Debug.Log("SpeedRave: Connected to LiveSplit!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"SpeedRave: Could not connect to LiveSplit. {ex.Message}");
                Disconnect(); 
            }
            finally
            {
                _isConnecting = false;
            }
        }

        private void Disconnect()
        {
            IsConnectedToLivesplit = false;

            try
            {
                Stream?.Close();
                Stream?.Dispose();
            }
            catch { }

            try
            {
                Client?.Close();
                Client?.Dispose();
            }
            catch { }

            Stream = null;
            Client = null;
        }

        public void AttemptSendCommand(string command)
        {
            if (!IsConnectedToLivesplit) return;

            SendMessageSafe(command);
        }

        private void SendMessageSafe(string message)
        {
            try
            {
                if (Client == null || !Client.Connected || Stream == null)
                {
                    throw new SocketException();
                }

                byte[] data = Encoding.UTF8.GetBytes(message + "\r\n");
                Stream.Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        public void Update()
        {
            if (IsConnectedToLivesplit || debug)
            {
                UpdateAutosplitter();
            }
        }

        public void UpdateAutosplitter()
        {

            string currentScene = SceneManager.GetActiveScene().name;

            //Reset Logic
            if (currentScene == "TitleScreen" && gameStarted)
            {
                AttemptSendCommand("reset");
                gameStarted = false;
            }

            //Start Logic
            if (currentScene == "Sewer_Start" && !gameStarted)
            {
                AttemptSendCommand("unpausegametime");
                AttemptSendCommand("reset");
                AttemptSendCommand("starttimer");


                ResetRunFlags();
                gameStarted = true;
            }

            // Split Logic
            if (gameStarted && ReferenceManager.ActiveFoodControl != null)
            {
                var playerFood = ReferenceManager.ActiveFoodControl;

                if (Plugin.TwentyResourceSplit.Value && !gotResources && (playerFood.cheese + playerFood.fruit >= 20))
                {
                    AttemptSendCommand("split");
                    gotResources = true;
                }

                if (Plugin.TwentyFruitSplit.Value && !gotFruit && playerFood.fruit >= 20)
                {
                    AttemptSendCommand("split");
                    gotFruit = true;
                }

                if (Plugin.KeySplit.Value && !gotKey && playerFood.haveKey)
                {
                    AttemptSendCommand("split");
                    gotKey = true;
                }

                if (playerFood.hasBottlecap && !gotBottlecap && Plugin.ItemSplit.Value) { AttemptSendCommand("split"); gotBottlecap = true; }
                else if (playerFood.hasPyramid && !gotPyramid && Plugin.ItemSplit.Value) { AttemptSendCommand("split"); gotPyramid = true; }
                else if (playerFood.hasMug && !gotMug && Plugin.ItemSplit.Value) { AttemptSendCommand("split"); gotMug = true; }
                else if (playerFood.hasDuck && !gotDuck && Plugin.ItemSplit.Value) { AttemptSendCommand("split"); gotDuck = true; }
                else if (playerFood.hasPizza && !gotPizza && Plugin.ItemSplit.Value) { AttemptSendCommand("split"); gotPizza = true; }

                if (currentScene.Contains("ending"))
                {
                    AttemptSendCommand("split");
                    gameStarted = false;
                }
            }

            // Loading Logic
            if (isLoading && !timerPaused)
            {
                AttemptSendCommand("pausegametime");
                timerPaused = true;
            }
            else if (timerPaused && (!isLoading || currentScene == "TitleScreen"))
            {
                AttemptSendCommand("unpausegametime");
                timerPaused = false;
            }
        }


        private void ResetRunFlags()
        {
            gotBottlecap = false;
            gotFruit = false;
            gotResources = false;
            gotPizza = false;
            gotMug = false;
            gotPyramid = false;
            gotKey = false;
            gotDuck = false;
        }

        public void OnApplicationQuit()
        {
            if (IsConnectedToLivesplit)
            {
                SendMessageSafe("pausegametime");
                Disconnect();
            }
        }
    }
}