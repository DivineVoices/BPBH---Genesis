using UnityEngine;

public class ProgressionTracker
{
    public static Progress progressionLevel = Progress.Started;
}

public enum Progress
{
    Started,
    Game1,
    Game2,
}