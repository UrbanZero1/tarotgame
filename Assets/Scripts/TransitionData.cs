//public static class TransitionData {
//public static int currentGoal = 100;
//}

// Restructure for reward system for chip currency

public enum GameDifficulty
{
    Easy,
    Hard,
    Impossible
}
public static class TransitionData
{
    public static int currentGoal = 100;
    public static GameDifficulty currentDifficulty = GameDifficulty.Easy;

    public static int GetChipReward()
    {
        switch (currentDifficulty)
         {
             case GameDifficulty.Easy:
                return 300;


            case GameDifficulty.Hard:
                return 500;

            case GameDifficulty.Impossible:
                return 800;

            default:
                return 0;
        }

    }
}
