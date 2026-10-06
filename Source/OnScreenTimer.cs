using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedRave
{
    public class OnScreenTimer : MonoBehaviour
    {
        private static readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        public static float CurrentTime => (float)stopwatch.Elapsed.TotalSeconds;
        public static bool IsRunning { get; private set; } = false;
        public static bool IsRunActive { get; private set; } = false;
        public static bool IsEnded { get; private set; } = false;

        private GUIStyle timerStyle;
        private GUIStyle shadowStyle;

        private int lastFontSize = -1;
        private Font lastFont = null;
        private float cachedDigitWidth = 20f;
        private float cachedColonWidth = 10f;
        private float cachedDotWidth = 10f;

        private void Awake()
        {
            timerStyle = new GUIStyle();
            timerStyle.normal.textColor = Color.white;
            timerStyle.alignment = TextAnchor.MiddleCenter;

            shadowStyle = new GUIStyle();
            shadowStyle.normal.textColor = Color.black;
            shadowStyle.alignment = TextAnchor.MiddleCenter;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string sceneName = scene.name;

            if (sceneName == "Sewer_Start" && mode == LoadSceneMode.Single)
            {
                StartTimer();
            }
            else if (sceneName == "TitleScreen")
            {
                ResetTimer();
            }
            // Endings are handled by Autosplitter.HandleEnding, which decides whether the run is over.
        }

        public static bool IsEndingScene(string sceneLower)
        {
            return sceneLower.Contains("ending") ||
                   sceneLower == "plaguending" ||
                   sceneLower == "spaceending" ||
                   sceneLower == "truending" ||
                   sceneLower == "winroom1" ||
                   sceneLower == "credits";
        }

        private void Update()
        {
            // Resume when the first frame of gameplay actually executes in the new scene
            if (IsRunActive && !IsEnded && Autosplitter.isLoading && Autosplitter.justLoadedScene)
            {
                Autosplitter.justLoadedScene = false;
                Autosplitter.isLoading = false;
                ResumeTimer();
                if (Autosplitter.Instance != null)
                {
                    Autosplitter.Instance.SendUnpauseGameTimeImmediate();
                }
            }
        }

        public static void StartTimer()
        {
            stopwatch.Restart();
            IsRunActive = true;
            IsRunning = true;
            IsEnded = false;
        }

        public static void PauseTimer()
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }
            IsRunning = false;
        }

        public static void ResumeTimer()
        {
            if (IsRunActive && !IsEnded && !stopwatch.IsRunning)
            {
                stopwatch.Start();
                IsRunning = true;
            }
        }

        public static void StopTimer()
        {
            if (IsRunActive || IsRunning)
            {
                stopwatch.Stop();
                IsRunActive = false;
                IsRunning = false;
                IsEnded = true;
            }
        }

        public static void ResetTimer()
        {
            stopwatch.Reset();
            IsRunActive = false;
            IsRunning = false;
            IsEnded = false;
        }

        private void UpdateMetrics(int fontSize, Font font)
        {
            if (fontSize == lastFontSize && font == lastFont) return;

            lastFontSize = fontSize;
            lastFont = font;

            float maxDigit = 0f;
            for (int i = 0; i <= 9; i++)
            {
                float w = timerStyle.CalcSize(new GUIContent(i.ToString())).x;
                if (w > maxDigit) maxDigit = w;
            }

            cachedDigitWidth = Mathf.Ceil(Mathf.Max(maxDigit, fontSize * 0.55f));
            cachedColonWidth = Mathf.Ceil(Mathf.Max(timerStyle.CalcSize(new GUIContent(":")).x * 1.15f, fontSize * 0.3f));
            cachedDotWidth = Mathf.Ceil(Mathf.Max(timerStyle.CalcSize(new GUIContent(".")).x * 1.15f, fontSize * 0.3f));
        }

        private void OnGUI()
        {
            if (!Plugin.ShowOnScreenTimer.Value) return;

            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == "TitleScreen" && !IsRunActive && !IsEnded) return;

            int fontSize = Mathf.RoundToInt(Plugin.TimerFontSize.Value);
            timerStyle.fontSize = fontSize;
            shadowStyle.fontSize = fontSize;

            Font activeFont = InventoryOverlay.GameFont;
            if (activeFont != null)
            {
                timerStyle.font = activeFont;
                shadowStyle.font = activeFont;
            }

            UpdateMetrics(fontSize, activeFont);

            if (IsEnded)
            {
                timerStyle.normal.textColor = new Color(0.2f, 1f, 0.5f);
            }
            else if (Autosplitter.isLoading)
            {
                timerStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
            }
            else
            {
                timerStyle.normal.textColor = Color.white;
            }

            string formattedTime = FormatTime(CurrentTime);

            // Compute fixed total width for the formatted string
            float totalWidth = 0f;
            for (int i = 0; i < formattedTime.Length; i++)
            {
                char c = formattedTime[i];
                totalWidth += (c == ':') ? cachedColonWidth : (c == '.') ? cachedDotWidth : cachedDigitWidth;
            }

            float height = fontSize + 10f;
            float startX = Screen.width - totalWidth - 25f;
            float y = 15f;

            // Render each character in its own fixed slot to eliminate jitter
            float currentX = startX;
            for (int i = 0; i < formattedTime.Length; i++)
            {
                char c = formattedTime[i];
                float slotWidth = (c == ':') ? cachedColonWidth : (c == '.') ? cachedDotWidth : cachedDigitWidth;

                string charStr = GetCharString(c);
                Rect shadowRect = new Rect(currentX + 2, y + 2, slotWidth, height);
                Rect textRect = new Rect(currentX, y, slotWidth, height);

                GUI.Label(shadowRect, charStr, shadowStyle);
                GUI.Label(textRect, charStr, timerStyle);

                currentX += slotWidth;
            }
        }

        private static readonly string[] DigitStrings = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static readonly string ColonString = ":";
        private static readonly string DotString = ".";
        private static readonly string SpaceString = " ";

        private static string GetCharString(char c)
        {
            if (c >= '0' && c <= '9') return DigitStrings[c - '0'];
            if (c == ':') return ColonString;
            if (c == '.') return DotString;
            if (c == ' ') return SpaceString;
            return c.ToString();
        }

        private string FormatTime(float seconds)
        {
            if (seconds < 0f) seconds = 0f;
            TimeSpan ts = TimeSpan.FromSeconds(seconds);
            int hundredths = (int)((seconds % 1f) * 100f);
            if (hundredths < 0) hundredths = 0;
            if (hundredths > 99) hundredths = 99;

            if (ts.TotalHours >= 1)
            {
                return string.Format("{0}:{1:D2}:{2:D2}.{3:D2}", (int)ts.TotalHours, ts.Minutes, ts.Seconds, hundredths);
            }
            return string.Format("{0:D2}:{1:D2}.{2:D2}", ts.Minutes, ts.Seconds, hundredths);
        }
    }
}
