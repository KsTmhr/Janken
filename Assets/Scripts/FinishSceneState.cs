public enum FinishOutcome
{
    YouWin,
    YouLose,
    Draw
}

public static class FinishSceneState
{
    public static int PlayerAliveFingers = 0;
    public static int OpponentAliveFingers = 0;
    public static FinishOutcome Outcome = FinishOutcome.Draw;

    public static void Set(int playerAlive, int opponentAlive, FinishOutcome outcome)
    {
        PlayerAliveFingers = playerAlive;
        OpponentAliveFingers = opponentAlive;
        Outcome = outcome;
    }
}
