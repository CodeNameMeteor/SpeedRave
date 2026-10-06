using BepInEx;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedRave
{
    public class Autosplitter : MonoBehaviour
    {
        public bool debug = false;
        public bool gameStarted = false;
        public static bool isLoading = false;
        public static bool justLoadedScene = false;
        public static int endingCount = 0;
        public static bool runFinished = false;

        public static void NotifyLoadingStarted()
        {
            if (isLoading || runFinished) return;
            isLoading = true;
            justLoadedScene = false;
            OnScreenTimer.PauseTimer();
            if (Instance != null)
            {
                Instance.SendPauseGameTimeImmediate();
            }
        }

        public static void NotifyLoadingFinished()
        {
            justLoadedScene = true;
        }

        public bool gotResources = false;
        public bool gotFruit = false;
        public bool gotPizza = false;
        public bool gotMug = false;
        public bool gotPyramid = false;
        public bool gotBottlecap = false;
        public bool gotDuck = false;
        public bool gotKey = false;

        public static bool plagueEnding = false;
        public static bool spaceEnding = false;
        public static bool trueEnding = false;

        // Networking
        public bool IsConnectedToLivesplit { get; private set; } = false;
        private readonly string ipAddress = "127.0.0.1"; 
        private readonly int port = 16834;
        private TcpClient client = null;
        private NetworkStream stream = null;
        private readonly object streamLock = new object();
        private static readonly byte[] PauseGameTimeBytes = Encoding.UTF8.GetBytes("pausegametime\r\n");
        private static readonly byte[] UnpauseGameTimeBytes = Encoding.UTF8.GetBytes("unpausegametime\r\n");
        private bool isConnecting = false;
        private CancellationTokenSource netCts;
        private readonly ConcurrentQueue<string> sendQueue = new ConcurrentQueue<string>();
        private bool isSending = false;

        private bool timerPaused = false;
        private string currentSceneName = "";

        // Singleton Instance
        public static Autosplitter Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
            currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Disconnect();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentSceneName = scene.name;
            string sceneLower = scene.name.ToLower();

            // Only start a run when none is in progress. Instant Restart and the title screen reset the run
            // first; loading Sewer_Start mid-run (level loader, Room Lock) must not reset LiveSplit.
            if (scene.name == "Sewer_Start" && mode == LoadSceneMode.Single && !gameStarted)
            {
                StartRun();
            }
            else if (scene.name == "TitleScreen")
            {
                ResetRun();
            }
            else if (gameStarted && OnScreenTimer.IsEndingScene(sceneLower))
            {
                HandleEnding(sceneLower);
            }
            else
            {
                justLoadedScene = true;
            }
        }

        public void Start()
        {
            // Try to connect on startup silently in the background
            if (Plugin.AutosplitterEnabled.Value)
            {
                ConnectToLiveSplit();
            }
        }

        public async void ConnectToLiveSplit()
        {
            if (isConnecting || IsConnectedToLivesplit) return;

            isConnecting = true;
            try
            {
                Disconnect();

                netCts = new CancellationTokenSource();
                client = new TcpClient();
                await client.ConnectAsync(ipAddress, port);

                if (client.Connected)
                {
                    stream = client.GetStream();
                    IsConnectedToLivesplit = true;
                    Debug.Log("[SpeedRave] Connected to LiveSplit!");

                    // Start background reader to drain LiveSplit responses
                    _ = Task.Run(() => ReadLoopAsync(stream, netCts.Token));

                    SyncRunStateAfterConnect();
                }
            }
            catch (Exception ex)
            {
                if (Plugin.Debug.Value)
                {
                    Debug.LogWarning($"[SpeedRave] Could not connect to LiveSplit: {ex.Message}");
                }
                Disconnect(); 
            }
            finally
            {
                isConnecting = false;
            }
        }

        // Called after (re)connecting. Never zeroes game time: mid-run, LiveSplit is synced to the
        // on-screen timer and the current loading state instead.
        private void SyncRunStateAfterConnect()
        {
            if (!gameStarted || runFinished) return;

            AttemptSendCommand("setgametime " + FormatLiveSplitTime(OnScreenTimer.Elapsed));
            if (isLoading)
            {
                SendPauseGameTimeImmediate();
            }
            else
            {
                SendUnpauseGameTimeImmediate();
            }
        }

        // LiveSplit Server accepts h:mm:ss.ff for setgametime.
        private static string FormatLiveSplitTime(TimeSpan time)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}.{3:00}",
                (int)time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds / 10);
        }

        private async Task ReadLoopAsync(NetworkStream netStream, CancellationToken ct)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (!ct.IsCancellationRequested && netStream != null && netStream.CanRead)
                {
                    int bytesRead = await netStream.ReadAsync(buffer, 0, buffer.Length, ct);
                    if (bytesRead == 0) break; // Socket closed
                }
            }
            catch
            {
                // Ignored - stream closed or canceled on disconnect
            }
        }

        public void Disconnect()
        {
            IsConnectedToLivesplit = false;

            try
            {
                netCts?.Cancel();
                netCts?.Dispose();
            }
            catch { }
            netCts = null;

            try
            {
                stream?.Close();
                stream?.Dispose();
            }
            catch { }
            stream = null;

            try
            {
                client?.Close();
                client?.Dispose();
            }
            catch { }
            client = null;

            // Clear send queue
            while (sendQueue.TryDequeue(out _)) { }
            isSending = false;
        }

        public void AttemptSendCommand(string command)
        {
            if (!IsConnectedToLivesplit || string.IsNullOrEmpty(command)) return;

            sendQueue.Enqueue(command);
            if (!isSending)
            {
                _ = ProcessSendQueueAsync();
            }
        }

        public void SendPauseGameTimeImmediate()
        {
            if (!IsConnectedToLivesplit) return;

            try
            {
                lock (streamLock)
                {
                    if (stream != null && stream.CanWrite)
                    {
                        stream.Write(PauseGameTimeBytes, 0, PauseGameTimeBytes.Length);
                        stream.Flush();
                        timerPaused = true;
                    }
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        public void SendUnpauseGameTimeImmediate()
        {
            if (!IsConnectedToLivesplit) return;

            try
            {
                lock (streamLock)
                {
                    if (stream != null && stream.CanWrite)
                    {
                        stream.Write(UnpauseGameTimeBytes, 0, UnpauseGameTimeBytes.Length);
                        stream.Flush();
                        timerPaused = false;
                    }
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private async Task ProcessSendQueueAsync()
        {
            if (isSending) return;
            isSending = true;

            try
            {
                while (sendQueue.TryDequeue(out string message))
                {
                    if (stream == null || client == null || !client.Connected)
                    {
                        Disconnect();
                        break;
                    }

                    byte[] data = Encoding.UTF8.GetBytes(message + "\r\n");
                    lock (streamLock)
                    {
                        if (stream != null && stream.CanWrite)
                        {
                            stream.Write(data, 0, data.Length);
                            stream.Flush();
                        }
                    }
                    await Task.Yield();
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
            finally
            {
                isSending = false;
            }
        }

        private float lastReconnectAttempt = 0f;
        private const float ReconnectInterval = 6f;

        public void Update()
        {
            if (Plugin.AutosplitterEnabled.Value && Plugin.LiveSplitAutoReconnect.Value && !IsConnectedToLivesplit && !isConnecting)
            {
                if (Time.unscaledTime - lastReconnectAttempt >= ReconnectInterval)
                {
                    lastReconnectAttempt = Time.unscaledTime;
                    ConnectToLiveSplit();
                }
            }

            if (IsConnectedToLivesplit || debug)
            {
                UpdateAutosplitter();
            }
        }

        public void StartRun()
        {
            AttemptSendCommand("unpausegametime");
            AttemptSendCommand("reset");
            AttemptSendCommand("starttimer");
            AttemptSendCommand("initgametime");

            ResetRunFlags();
            timerPaused = false;
            isLoading = false;
            justLoadedScene = false;
            gameStarted = true;
            OnScreenTimer.StartTimer();
        }

        public void ResetRun()
        {
            AttemptSendCommand("reset");
            //AttemptSendCommand("setgametime 0");
            ResetRunFlags();
            timerPaused = false;
            isLoading = false;
            justLoadedScene = false;
            gameStarted = false;
            OnScreenTimer.ResetTimer();
        }

        public void HandleEnding(string sceneLower)
        {
            if (!gameStarted || runFinished) return;

            bool shouldSplit = false;
            if (sceneLower == "plaguending" && !plagueEnding)
            {
                plagueEnding = true;
                shouldSplit = true;
            }
            else if (sceneLower == "spaceending" && !spaceEnding)
            {
                spaceEnding = true;
                shouldSplit = true;
            }
            else if (sceneLower == "truending" && !trueEnding)
            {
                trueEnding = true;
                shouldSplit = true;
            }

            if (!shouldSplit)
            {
                if (sceneLower == "plaguending" || sceneLower == "spaceending" || sceneLower == "truending")
                {
                    // An ending that was already split: treat it like any other room so the loading pause ends.
                    justLoadedScene = true;
                }
                else
                {
                    OnScreenTimer.StopTimer();
                }
                return;
            }

            endingCount++;
            AttemptSendCommand("split");

            bool runComplete = !Plugin.AllEndings.Value || (plagueEnding && spaceEnding && trueEnding);
            if (runComplete)
            {
                FinishRun();
            }
            else
            {
                // More endings to go: the run continues, so let the loading pause end as for a normal room.
                justLoadedScene = true;
            }
        }

        // Ends the run: both timers stop and stay stopped until the next reset or start.
        private void FinishRun()
        {
            runFinished = true;
            OnScreenTimer.StopTimer();
            isLoading = false;
            justLoadedScene = false;
            SendPauseGameTimeImmediate();
        }

        public void UpdateAutosplitter()
        {
            string currentScene = currentSceneName;

            // Fallback reset if on TitleScreen
            if (currentScene == "TitleScreen" && gameStarted)
            {
                ResetRun();
            }

            // Fallback start if Sewer_Start loaded before connect
            if (currentScene == "Sewer_Start" && !gameStarted)
            {
                StartRun();
            }

            // Split Logic
            if (ReferenceManager.ActiveFoodControl != null)
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

                if (Plugin.ItemSplit.Value)
                {
                    if (playerFood.hasBottlecap && !gotBottlecap) { AttemptSendCommand("split"); gotBottlecap = true; }
                    if (playerFood.hasPyramid && !gotPyramid) { AttemptSendCommand("split"); gotPyramid = true; }
                    if (playerFood.hasMug && !gotMug) { AttemptSendCommand("split"); gotMug = true; }
                    if (playerFood.hasDuck && !gotDuck) { AttemptSendCommand("split"); gotDuck = true; }
                    if (playerFood.hasPizza && !gotPizza) { AttemptSendCommand("split"); gotPizza = true; }
                }
            }

            // Loading Logic (once the run is finished, game time stays paused)
            if (runFinished)
            {
                return;
            }
            if (isLoading && !timerPaused)
            {
                SendPauseGameTimeImmediate();
            }
            else if (timerPaused && (!isLoading || currentScene == "TitleScreen"))
            {
                SendUnpauseGameTimeImmediate();
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
            plagueEnding = false;
            spaceEnding = false;
            trueEnding = false;
            endingCount = 0;
            runFinished = false;
        }

        public void OnApplicationQuit()
        {
            if (IsConnectedToLivesplit)
            {
                AttemptSendCommand("pausegametime");
                Disconnect();
            }
        }
    }
}