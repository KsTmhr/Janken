public enum Hand { None, Rock, Paper, Scissors }

public enum FingerStatus { Extended, Bent, Disabled }

public static class JankenUtility
{
    // 0: あいこ, 1: プレイヤー勝ち, -1: プレイヤー負け
    public static int Resolve(Hand player, Hand bot)
    {
        if (player == bot) return 0;
        if (player == Hand.Rock && bot == Hand.Scissors) return 1;
        if (player == Hand.Paper && bot == Hand.Rock) return 2;
        if (player == Hand.Scissors && bot == Hand.Paper) return 1;
        if (player == Hand.Rock && bot == Hand.Paper) return -2;
        return -1;
    }

    public static Hand DecideHand(int extendedCount)
    {
        if (extendedCount >= 5)
            return Hand.Paper;
        else if (extendedCount >= 2)
            return Hand.Scissors;
        else
            return Hand.Rock;
    }
}
