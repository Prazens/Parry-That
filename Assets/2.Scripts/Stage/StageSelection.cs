using System.Collections;
public enum Difficulty
{
    Easy = 0,
    Hard = 1,
}

public static class StageSelection
{
    public static int SelectedStageId { get; private set; } = 0;
    public static Difficulty SelectedDifficulty { get; private set; } = Difficulty.Easy;

    public static void SetSelection(int stageId, Difficulty difficulty)
    {
        SelectedStageId = stageId;
        SelectedDifficulty = difficulty;
    }

    public static bool HasValidSelection()
    {
        return SelectedStageId >= 0;
    }

    public static void ResetSelection()
    {
        SelectedStageId = -1;
        SelectedDifficulty = Difficulty.Easy;
    }
}
