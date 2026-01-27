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
        public static bool Use = true; // Assumed true for logic

        // Config flags
        public static bool twentyResourceSplit;
        public static bool keySplit;
        public static bool twentyFruitSplit;
        public static bool itemSplit;

        public FoodControl[] FoodControlArray;

        // Networking
        public bool IsConnectedToLivesplit { get; private set; } = false;
        private string IpAddress = "127.0.0.1"; // Use IP instead of "localhost" to avoid DNS lookup lag
        private int Port = 16834;
        private TcpClient Client = null;
        private NetworkStream Stream = null;
        private bool _isConnecting = false;

        private bool timerPaused = false;

        public void Start()
        {
            // Try to connect on startup, but do it silently in the background
            if (Use)
            {
                ConnectToLiveSplit();
            }
        }

        // FIX 1: Async Connection to prevent startup lag
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

                    // Optional: Handshake logic (simplified for stability)
                    // Sending initialization command safely
                    SendMessageSafe("getcurrenttimerphase");
                    SendMessageSafe("initgametime");

                    IsConnectedToLivesplit = true;
                    Debug.Log("SpeedRave: Connected to LiveSplit!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"SpeedRave: Could not connect to LiveSplit. {ex.Message}");
                Disconnect(); // Ensure cleanup on failure
            }
            finally
            {
                _isConnecting = false;
            }
        }

        // FIX 2: Proper Disconnection to prevent socket leaks
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

        // FIX 3: Safe Message Sending
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
                // If writing fails, assume connection is dead and clean up
                Disconnect();
            }
        }

        public void Update()
        {
            // Only run logic if connected or debugging
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

                // Cache this array once per run, not every frame
                FoodControlArray = FindObjectsOfType<FoodControl>();

                ResetRunFlags(); // Extracted helper method
                gameStarted = true;
            }

            // Split Logic
            if (gameStarted && FoodControlArray != null && FoodControlArray.Length > 0)
            {
                var playerFood = FoodControlArray[0];

                if (twentyResourceSplit && !gotResources && (playerFood.cheese + playerFood.fruit >= 20))
                {
                    AttemptSendCommand("split");
                    gotResources = true;
                }

                if (twentyFruitSplit && !gotFruit && playerFood.fruit >= 20)
                {
                    AttemptSendCommand("split");
                    gotFruit = true;
                }

                if (keySplit && !gotKey && playerFood.haveKey)
                {
                    AttemptSendCommand("split");
                    gotKey = true;
                }

                if (playerFood.hasBottlecap && !gotBottlecap) { AttemptSendCommand("split"); gotBottlecap = true; }
                else if (playerFood.hasPyramid && !gotPyramid) { AttemptSendCommand("split"); gotPyramid = true; }
                else if (playerFood.hasMug && !gotMug) { AttemptSendCommand("split"); gotMug = true; }
                else if (playerFood.hasDuck && !gotDuck) { AttemptSendCommand("split"); gotDuck = true; }
                else if (playerFood.hasPizza && !gotPizza) { AttemptSendCommand("split"); gotPizza = true; }

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
                // Try to pause, then kill connection
                SendMessageSafe("pausegametime");
                Disconnect();
            }
        }
    }
}