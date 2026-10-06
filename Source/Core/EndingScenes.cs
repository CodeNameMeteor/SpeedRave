namespace SpeedRave.Core
{
    public static class EndingScenes
    {
        // The only scenes that end (or, with All Endings, split) a run. Both LiveSplit and the on-screen
        // timer use this one list so they always stop on the same event. Expects a lower-case scene name.
        public static bool IsEnding(string sceneLower)
        {
            return sceneLower == "plaguending" ||
                   sceneLower == "spaceending" ||
                   sceneLower == "truending";
        }
    }
}
