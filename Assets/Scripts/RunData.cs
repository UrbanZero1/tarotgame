public struct TableInfo
{
    public string name;
    public int goal;
    public GameDifficulty difficulty;
}

public static class RunData
{
    public const int TABLE_COUNT = 3;

    private static readonly TableInfo[] tables =
    {
        new TableInfo { name = "Table 1", goal = 100, difficulty = GameDifficulty.Easy },
        new TableInfo { name = "Table 2", goal = 130, difficulty = GameDifficulty.Hard },
        new TableInfo { name = "Table 3", goal = 160, difficulty = GameDifficulty.Impossible }
    };

    public static bool IsActive { get; private set; }
    public static int CurrentTable { get; private set; }
    public static bool IsLastTable => CurrentTable >= TABLE_COUNT - 1;

    public static TableInfo GetTable(int index)
    {
        return tables[index];
    }

    public static void StartNewRun()
    {
        IsActive = true;
        CurrentTable = 0;

        if (ChipManager.Instance != null)
            ChipManager.Instance.ResetChips();

        if (ItemsManager.Instance != null)
            ItemsManager.Instance.ResetInventory();
    }

    public static void PrepareTable()
    {
        TableInfo table = tables[CurrentTable];
        TransitionData.currentGoal = table.goal;
        TransitionData.currentDifficulty = table.difficulty;
    }

    public static void AdvanceTable()
    {
        if (CurrentTable < TABLE_COUNT - 1)
            CurrentTable++;
    }

    public static void EndRun()
    {
        IsActive = false;
        CurrentTable = 0;
    }
}
