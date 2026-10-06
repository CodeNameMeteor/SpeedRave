using UnityEngine;

namespace SpeedRave
{
    // Single source of truth for where the current run is. Autosplitter moves it between states as scenes
    // load; LiveSplit and the on-screen timer both follow it, so they can't disagree about whether a run is
    // in progress, loading or finished.
    //
    //   Idle --Start--> Running --BeginLoading--> Loading --EndLoading--> Running
    //   Running/Loading --Finish--> Finished;  any --Reset--> Idle
    public static class RunState
    {
        public static bool InProgress { get; private set; }
        public static bool Finished { get; private set; }

        // A trigger (door, ending teleporter, train map) has started a room change.
        public static bool IsLoading { get; private set; }
        // The new room has loaded; the loading pause ends on the next frame.
        public static bool LoadFinished { get; private set; }
        public static float LoadingStartedAt { get; private set; }

        public static bool PlagueEnding { get; private set; }
        public static bool SpaceEnding { get; private set; }
        public static bool TrueEnding { get; private set; }
        public static int EndingCount { get; private set; }

        public static bool AllEndingsReached => PlagueEnding && SpaceEnding && TrueEnding;

        public static void Start()
        {
            ClearRun();
            InProgress = true;
        }

        public static void Reset()
        {
            ClearRun();
            InProgress = false;
        }

        // Returns false if a load is already in progress or the run has finished.
        public static bool BeginLoading()
        {
            if (IsLoading || Finished) return false;
            IsLoading = true;
            LoadFinished = false;
            LoadingStartedAt = Time.realtimeSinceStartup;
            return true;
        }

        public static void MarkLoadFinished()
        {
            LoadFinished = true;
        }

        public static void EndLoading()
        {
            IsLoading = false;
            LoadFinished = false;
        }

        // Records an ending scene. Returns true the first time each ending is reached.
        public static bool RecordEnding(string sceneLower)
        {
            bool isNew = false;
            if (sceneLower == "plaguending" && !PlagueEnding) { PlagueEnding = true; isNew = true; }
            else if (sceneLower == "spaceending" && !SpaceEnding) { SpaceEnding = true; isNew = true; }
            else if (sceneLower == "truending" && !TrueEnding) { TrueEnding = true; isNew = true; }

            if (isNew) EndingCount++;
            return isNew;
        }

        public static void Finish()
        {
            Finished = true;
            EndLoading();
        }

        private static void ClearRun()
        {
            Finished = false;
            EndLoading();
            PlagueEnding = false;
            SpaceEnding = false;
            TrueEnding = false;
            EndingCount = 0;
        }
    }
}
