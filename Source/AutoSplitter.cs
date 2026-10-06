using BepInEx;
using System;
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
        public static float LoadingStartedAt { get; private set; }

        public static void NotifyLoadingStarted()
        {
            if (isLoading || runFinished) return;
            isLoading = true;
            justLoadedScene = false;
            LoadingStartedAt = Time.realtimeSinceStartup;
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
        // Writes happen on Unity's main thread. Without a timeout a peer that stops reading could freeze the game.
        private const int SendTimeoutMs = 1000;
        private TcpClient client = null;
        private NetworkStream stream = null;
        private readonly object streamLock = new object();
        private static readonly byte[] PauseGameTimeBytes = Encoding.UTF8.GetBytes("pausegametime\r\n");
        private static readonly byte[] UnpauseGameTimeBytes = Encoding.UTF8.GetBytes("unpausegametime\r\n");
        private bool isConnecting = false;
        // Each connection gets a number. The background reader reports which connection the remote closed,
        // and Update disconnects only if that is still the current one.
        private int connectionGeneration = 0;
        private volatile int closedGeneration = -1;
        private CancellationTokenSource netCts;

        private bool timerPaused = false;
        private string currentSceneName = "";

        // Singleton Instance
        public static Autosplitter Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
            currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Plugin.AutosplitterEnabled.SettingChanged += OnAutosplitterEnabledChanged;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Plugin.AutosplitterEnabled.SettingChanged -= OnAutosplitterEnabledChanged;
            Disconnect();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Additive loads (e.g. the Sewer_Start sideload in ReferenceManager) are not room changes.
            if (mode == LoadSceneMode.Additive) return;

            currentSceneName = scene.name;
            string sceneLower = scene.name.ToLowerInvariant();

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

        // Turning the autosplitter off drops the LiveSplit connection, so nothing more is sent.
        private void OnAutosplitterEnabledChanged(object sender, EventArgs e)
        {
            if (!Plugin.AutosplitterEnabled.Value)
            {
                Disconnect();
            }
        }

        public async void ConnectToLiveSplit()
        {
            if (!Plugin.AutosplitterEnabled.Value || isConnecting || IsConnectedToLivesplit) return;

            isConnecting = true;
            try
            {
                Disconnect();

                netCts = new CancellationTokenSource();
                client = new TcpClient();
                client.SendTimeout = SendTimeoutMs;
                await client.ConnectAsync(ipAddress, port);

                if (client.Connected)
                {
                    stream = client.GetStream();
                    IsConnectedToLivesplit = true;
                    Debug.Log("[SpeedRave] Connected to LiveSplit!");

                    // Start background reader to drain LiveSplit responses. Capture the stream and token now:
                    // the lambda runs later on a thread-pool thread, when the fields may already be cleared or
                    // belong to a newer connection.
                    NetworkStream readStream = stream;
                    CancellationToken readToken = netCts.Token;
                    int generation = ++connectionGeneration;
                    _ = Task.Run(() => ReadLoopAsync(readStream, readToken, generation));

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
            return Core.TimeFormat.LiveSplit(time);
        }

        private async Task ReadLoopAsync(NetworkStream netStream, CancellationToken ct, int generation)
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
                // Stream closed or canceled on disconnect
            }

            if (!ct.IsCancellationRequested)
            {
                // LiveSplit closed the connection. Disconnect on the main thread (see Update).
                closedGeneration = generation;
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
            catch (Exception ex) { LogDisconnectError(ex); }
            netCts = null;

            try
            {
                stream?.Close();
                stream?.Dispose();
            }
            catch (Exception ex) { LogDisconnectError(ex); }
            stream = null;

            try
            {
                client?.Close();
                client?.Dispose();
            }
            catch (Exception ex) { LogDisconnectError(ex); }
            client = null;
        }

        private static void LogDisconnectError(Exception ex)
        {
            if (Plugin.Debug.Value)
            {
                Debug.LogWarning($"[SpeedRave] Error while closing the LiveSplit connection: {ex.Message}");
            }
        }

        // All LiveSplit commands go through here and are written immediately, in the order they are issued.
        // (They used to sit in a queue that sent one command per frame, which delayed bursts and let the
        // pause/unpause writes overtake queued commands.) Writes are bounded by the socket's send timeout.
        public void AttemptSendCommand(string command)
        {
            if (string.IsNullOrEmpty(command)) return;
            WriteLine(Encoding.UTF8.GetBytes(command + "\r\n"));
        }

        public void SendPauseGameTimeImmediate()
        {
            if (WriteLine(PauseGameTimeBytes))
            {
                timerPaused = true;
            }
        }

        // A loading pause turned out to have no load behind it (see OnScreenTimer.Update): resume LiveSplit
        // game time and give back the time it was wrongly paused for.
        public void ResumeAfterFalsePause()
        {
            SendUnpauseGameTimeImmediate();
            AttemptSendCommand("setgametime " + FormatLiveSplitTime(OnScreenTimer.Elapsed));
        }

        public void SendUnpauseGameTimeImmediate()
        {
            if (WriteLine(UnpauseGameTimeBytes))
            {
                timerPaused = false;
            }
        }

        // Returns true if the data was written.
        private bool WriteLine(byte[] data)
        {
            if (!IsConnectedToLivesplit) return false;

            try
            {
                lock (streamLock)
                {
                    if (stream != null && stream.CanWrite)
                    {
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (Plugin.Debug.Value)
                {
                    Debug.LogWarning($"[SpeedRave] Write to LiveSplit failed: {ex.Message}");
                }
                Disconnect();
            }
            return false;
        }

        private float lastReconnectAttempt = 0f;
        private const float ReconnectInterval = 6f;

        public void Update()
        {
            if (IsConnectedToLivesplit && closedGeneration == connectionGeneration)
            {
                Debug.Log("[SpeedRave] LiveSplit closed the connection.");
                Disconnect();
            }

            if (Plugin.AutosplitterEnabled.Value && Plugin.LiveSplitAutoReconnect.Value && !IsConnectedToLivesplit && !isConnecting)
            {
                if (Time.unscaledTime - lastReconnectAttempt >= ReconnectInterval)
                {
                    lastReconnectAttempt = Time.unscaledTime;
                    ConnectToLiveSplit();
                }
            }

            // Always evaluated, even while disconnected, so split flags track the game. Otherwise every split
            // that became due during a disconnect would fire in a burst when the connection returns.
            UpdateAutosplitter();
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
                // An ending that was already split: treat it like any other room so the loading pause ends.
                justLoadedScene = true;
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

            // Split Logic
            if (gameStarted && !runFinished && ReferenceManager.ActiveFoodControl != null)
            {
                var playerFood = ReferenceManager.ActiveFoodControl;

                if (Plugin.TwentyResourceSplit.Value && !gotResources && (playerFood.cheese + playerFood.fruit >= 20))
                {
                    Split("20 resources");
                    gotResources = true;
                }

                if (Plugin.TwentyFruitSplit.Value && !gotFruit && playerFood.fruit >= 20)
                {
                    Split("20 fruit");
                    gotFruit = true;
                }

                if (Plugin.KeySplit.Value && !gotKey && playerFood.haveKey)
                {
                    Split("key");
                    gotKey = true;
                }

                if (Plugin.ItemSplit.Value)
                {
                    if (playerFood.hasBottlecap && !gotBottlecap) { Split("bottlecap"); gotBottlecap = true; }
                    if (playerFood.hasPyramid && !gotPyramid) { Split("pyramid"); gotPyramid = true; }
                    if (playerFood.hasMug && !gotMug) { Split("mug"); gotMug = true; }
                    if (playerFood.hasDuck && !gotDuck) { Split("duck"); gotDuck = true; }
                    if (playerFood.hasPizza && !gotPizza) { Split("pizza"); gotPizza = true; }
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

        // Sends a split now, or records that it was missed while disconnected (it is not replayed later).
        private void Split(string reason)
        {
            if (!IsConnectedToLivesplit)
            {
                if (Plugin.Debug.Value)
                {
                    Debug.LogWarning($"[SpeedRave] Split for {reason} missed: not connected to LiveSplit.");
                }
                return;
            }
            AttemptSendCommand("split");
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