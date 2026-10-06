using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedRave
{
    public class OnScreenTimer : MonoBehaviour
    {
        private static readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        // Time added back after a loading pause that had no load behind it.
        private static TimeSpan correction = TimeSpan.Zero;
        public static TimeSpan Elapsed => stopwatch.Elapsed + correction;

        // A trigger pauses the timer before the game loads the next room. If no room load follows within
        // this many seconds (for example a door that refused to open), the pause is treated as false.
        private const float FalsePauseTimeout = 15f;
        public static bool IsRunning { get; private set; } = false;
        public static bool IsRunActive { get; private set; } = false;
        public static bool IsEnded { get; private set; } = false;

        private GUIStyle timerStyle;
        private GUIStyle shadowStyle;
        private GUIStyle stateStyle;
        private GUIStyle stateShadowStyle;

        private int lastFontSize = -1;
        private Font lastFont = null;
        private float cachedDigitWidth = 20f;
        private float cachedColonWidth = 10f;
        private float cachedDotWidth = 10f;

        // Cached so OnGUI (called several times per frame) doesn't allocate: the active scene is tracked via
        // activeSceneChanged, and the time string is only rebuilt when the displayed hundredth changes.
        private static bool onTitleScreen = true;
        private long cachedCentiseconds = -1;
        private string cachedTimeText = "";

        private string GetFormattedTime()
        {
            TimeSpan elapsed = Elapsed;
            long centiseconds = elapsed.Ticks / (TimeSpan.TicksPerMillisecond * 10);
            if (centiseconds != cachedCentiseconds)
            {
                cachedCentiseconds = centiseconds;
                // Formats straight from the TimeSpan (see Core.TimeFormat).
                cachedTimeText = Core.TimeFormat.Timer(elapsed);
            }
            return cachedTimeText;
        }

        private static void OnActiveSceneChanged(Scene previous, Scene next)
        {
            onTitleScreen = next.name == "TitleScreen";
        }

        private void Awake()
        {
            onTitleScreen = SceneManager.GetActiveScene().name == "TitleScreen";
            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            timerStyle = new GUIStyle();
            timerStyle.normal.textColor = Color.white;
            timerStyle.alignment = TextAnchor.MiddleCenter;

            shadowStyle = new GUIStyle();
            shadowStyle.normal.textColor = Color.black;
            shadowStyle.alignment = TextAnchor.MiddleCenter;

            stateStyle = new GUIStyle();
            stateStyle.alignment = TextAnchor.UpperRight;
            stateShadowStyle = new GUIStyle();
            stateShadowStyle.normal.textColor = Color.black;
            stateShadowStyle.alignment = TextAnchor.UpperRight;

            // Starting, resetting and stopping the timer is driven by Autosplitter (StartRun, ResetRun,
            // HandleEnding) so the on-screen timer and LiveSplit always agree.
        }

        public static bool IsEndingScene(string sceneLower)
        {
            return Core.EndingScenes.IsEnding(sceneLower);
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void Update()
        {
            // Resume when the first frame of gameplay actually executes in the new scene
            if (IsRunActive && !IsEnded && RunState.IsLoading && RunState.LoadFinished)
            {
                RunState.EndLoading();
                ResumeTimer();
                if (Autosplitter.Instance != null)
                {
                    Autosplitter.Instance.SendUnpauseGameTimeImmediate();
                }
            }
            else if (IsRunActive && !IsEnded && RunState.IsLoading && !RunState.LoadFinished
                     && Time.realtimeSinceStartup - RunState.LoadingStartedAt > FalsePauseTimeout)
            {
                float pausedFor = Time.realtimeSinceStartup - RunState.LoadingStartedAt;
                Debug.LogWarning($"[SpeedRave] Timer was paused for {pausedFor:F1}s without a room load; resuming and adding the time back.");
                RunState.EndLoading();
                correction += TimeSpan.FromSeconds(pausedFor);
                ResumeTimer();
                if (Autosplitter.Instance != null)
                {
                    Autosplitter.Instance.ResumeAfterFalsePause();
                }
            }
        }

        public static void StartTimer()
        {
            correction = TimeSpan.Zero;
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
            correction = TimeSpan.Zero;
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

            if (onTitleScreen && !IsRunActive && !IsEnded) return;

            int fontSize = Mathf.RoundToInt(Plugin.TimerFontSize.Value);
            timerStyle.fontSize = fontSize;
            shadowStyle.fontSize = fontSize;

            // null = Unity's default font (when the game font is off or not found yet).
            Font activeFont = InventoryOverlay.DisplayFont;
            timerStyle.font = activeFont;
            shadowStyle.font = activeFont;

            UpdateMetrics(fontSize, activeFont);

            if (IsEnded)
            {
                timerStyle.normal.textColor = new Color(0.2f, 1f, 0.5f);
            }
            else if (RunState.IsLoading)
            {
                timerStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
            }
            else
            {
                timerStyle.normal.textColor = Color.white;
            }

            string formattedTime = GetFormattedTime();

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

            DrawStateText(startX, y + height, totalWidth, fontSize);
        }

        // Optional text cue under the timer, so the loading/finished state isn't shown by colour alone.
        private void DrawStateText(float x, float y, float width, int timerFontSize)
        {
            if (!Plugin.ShowTimerStateText.Value) return;

            string state = IsEnded ? "FINISHED" : RunState.IsLoading ? "LOADING" : null;
            if (state == null) return;

            int fontSize = Mathf.Max(12, timerFontSize / 2);
            stateStyle.fontSize = fontSize;
            stateShadowStyle.fontSize = fontSize;
            stateStyle.font = timerStyle.font;
            stateShadowStyle.font = timerStyle.font;
            stateStyle.normal.textColor = timerStyle.normal.textColor;

            Rect rect = new Rect(x, y, width, fontSize + 6f);
            GUI.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), state, stateShadowStyle);
            GUI.Label(rect, state, stateStyle);
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
    }
}
